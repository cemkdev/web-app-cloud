using WebAppAPI.Application.Enums;
using WebAppAPI.Application.Features.Users.Commands.AssignRoleToUser.DTOs;
using WebAppAPI.Application.Features.Users.Commands.CreateUser.DTOs;
using WebAppAPI.Application.Features.Users.Commands.UpdatePassword.DTOs;
using WebAppAPI.Application.Features.Users.Queries.GetAllUsers.DTOs;

namespace WebAppAPI.Application.Abstractions.Services
{
    public interface IUserService
    {
        Task<GetAllUsersDto> GetAllUsersAsync(int page, int size, CancellationToken cancellationToken);
        Task<CreateUserResultDto> CreateAsync(CreateUserDto model);
        Task UpdatePasswordAsync(ResetPasswordDto model);

        /// <summary>
        /// Retrieves the roles of a user by ID or username.
        /// </summary>
        /// <param name="userIdentifier">The user's ID or username.</param>
        /// <returns>List of role names assigned to the user.</returns>
        Task<List<string>> GetRolesByUserIdentifierAsync(string userIdentifier, UserIdentifierType identifierType);
        Task AssignRoleToUserAsync(AssignRolesToUserDto model);
    }
}
