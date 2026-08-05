using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Consts;
using WebAppAPI.Application.Enums;
using WebAppAPI.Application.Exceptions;
using WebAppAPI.Application.Features.Users.Commands.AssignRoleToUser.DTOs;
using WebAppAPI.Application.Features.Users.Commands.CreateUser.DTOs;
using WebAppAPI.Application.Features.Users.Commands.UpdatePassword.DTOs;
using WebAppAPI.Application.Features.Users.Queries.GetAllUsers.DTOs;
using WebAppAPI.Application.Helpers;
using WebAppAPI.Domain.Entities.Identity;

namespace WebAppAPI.Persistence.Services
{
    public sealed class UserService(
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager) : IUserService
    {
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly RoleManager<AppRole> _roleManager = roleManager;

        public async Task<GetAllUsersDto> GetAllUsersAsync(int page, int size, CancellationToken cancellationToken)
        {
            if (page < 0)
                throw new ArgumentOutOfRangeException(nameof(page), page, "Page cannot be less than zero.");

            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size), size, "Size must be greater than zero.");

            IQueryable<AppUser> usersQuery = _userManager.Users.AsNoTracking();

            int totalUserCount = await usersQuery.CountAsync(cancellationToken);

            List<UserListItemDto> users = await usersQuery
                .OrderBy(user => user.DateCreated)
                .ThenBy(user => user.Id)
                .Skip(page * size)
                .Take(size)
                .Select(user => new UserListItemDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    DateCreated = user.DateCreated,
                    DateUpdated = user.DateUpdated
                })
                .ToListAsync(cancellationToken);

            return new GetAllUsersDto
            {
                TotalUserCount = totalUserCount,
                Users = users
            };
        }

        public async Task<CreateUserResultDto> CreateAsync(CreateUserDto model)
        {
            ArgumentNullException.ThrowIfNull(model);

            string firstName = model.FirstName.Trim();
            string lastName = model.LastName.Trim();

            AppUser user = new()
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = firstName,
                LastName = lastName,
                FullName = $"{firstName} {lastName}",
                UserName = model.Username.Trim(),
                Email = model.Email.Trim(),
                PhoneNumber = model.PhoneNumber.Trim()
            };

            IdentityResult result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                return new CreateUserResultDto
                {
                    Succeeded = true,
                    Message = "The user has been successfully created."
                };
            }

            string errorMessage = string.Join(
                Environment.NewLine,
                result.Errors.Select(error => $"• {error.Code}: {error.Description}"));

            return new CreateUserResultDto
            {
                Succeeded = false,
                Message = errorMessage
            };
        }

        public async Task UpdatePasswordAsync(ResetPasswordDto model)
        {
            ArgumentNullException.ThrowIfNull(model);

            if (string.IsNullOrWhiteSpace(model.UserId) ||
                string.IsNullOrWhiteSpace(model.ResetToken) ||
                string.IsNullOrWhiteSpace(model.NewPassword))
                throw new PasswordChangeFailedException();

            AppUser? user = await _userManager.FindByIdAsync(model.UserId);

            if (user is null)
                throw new PasswordChangeFailedException();

            string decodedResetToken = model.ResetToken.UrlDecode();

            IdentityResult resetResult = await _userManager.ResetPasswordAsync(
                user,
                decodedResetToken,
                model.NewPassword);

            if (!resetResult.Succeeded)
                throw new PasswordChangeFailedException();

            IdentityResult securityStampResult = await _userManager.UpdateSecurityStampAsync(user);

            if (!securityStampResult.Succeeded)
                throw new PasswordChangeFailedException();
        }

        public async Task<List<string>> GetRolesByUserIdentifierAsync(string userIdentifier, UserIdentifierType identifierType)
        {
            if (string.IsNullOrWhiteSpace(userIdentifier))
                throw new ArgumentException("User identifier must be provided.", nameof(userIdentifier));

            AppUser? user = identifierType switch
            {
                UserIdentifierType.Id => await _userManager.FindByIdAsync(userIdentifier),

                UserIdentifierType.Username => await _userManager.FindByNameAsync(userIdentifier),

                _ => throw new ArgumentOutOfRangeException(
                    nameof(identifierType),
                    identifierType,
                    "Unsupported user identifier type.")
            };

            if (user is null)
                throw new NotFoundUserException();

            IList<string> roles = await _userManager.GetRolesAsync(user);

            return roles.ToList();
        }

        public async Task AssignRoleToUserAsync(AssignRolesToUserDto model)
        {
            ArgumentNullException.ThrowIfNull(model);

            AppUser? user = await _userManager.FindByIdAsync(model.UserId);

            if (user is null)
                throw new NotFoundUserException("The user could not be found.");

            HashSet<string> requestedRoles = model.Roles
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Select(role => role.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Keep the bootstrap administrator user assigned to the protected role.
            if (user.Id == SystemBootstrapConstants.SystemAdministratorUserId && !requestedRoles.Contains(SystemBootstrapConstants.SystemAdministratorRoleName))
                throw new InvalidOperationException("The SystemAdministrator role cannot be removed from the bootstrap administrator user.");
            //

            foreach (string roleName in requestedRoles)
                if (!await _roleManager.RoleExistsAsync(roleName))
                    throw new InvalidOperationException($"The role '{roleName}' does not exist.");

            IList<string> currentRoles = await _userManager.GetRolesAsync(user);

            string[] rolesToRemove = currentRoles
                .Where(role => !requestedRoles.Contains(role))
                .ToArray();

            HashSet<string> currentRoleSet = currentRoles.ToHashSet(StringComparer.OrdinalIgnoreCase);

            string[] rolesToAdd = requestedRoles
                .Where(role => !currentRoleSet.Contains(role))
                .ToArray();

            if (rolesToRemove.Length > 0)
            {
                IdentityResult removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

                if (!removeResult.Succeeded)
                    throw new InvalidOperationException("User roles could not be removed.");
            }

            if (rolesToAdd.Length > 0)
            {
                IdentityResult addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);

                if (!addResult.Succeeded)
                    throw new InvalidOperationException("User roles could not be assigned.");
            }
        }
    }
}
