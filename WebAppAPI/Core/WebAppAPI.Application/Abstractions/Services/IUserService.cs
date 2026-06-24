using WebAppAPI.Application.DTOs.User;
using I = WebAppAPI.Domain.Entities.Identity;

namespace WebAppAPI.Application.Abstractions.Services
{
    public interface IUserService
    {
        Task<ListUserDto> GetAllUsersAsync(int page, int size);
        Task<CreateUserResponse> CreateAsync(CreateUser model);
        Task UpdateRefreshTokenAsync(I.AppUser user, string refreshToken, int refreshTokenExpiration, bool isFromRefreshToken = false, bool isLogout = false);
        Task UpdatePasswordAsync(string userId, string resetToken, string newPassword);

        /// <summary>
        /// Retrieves the roles of a user by ID or username.
        /// </summary>
        /// <param name="userIdentifier">The user's ID or username.</param>
        /// <returns>List of role names assigned to the user.</returns>
        Task<List<string>> GetRolesByUserIdentifierAsync(string userIdentifier);
        Task AssignRoleToUserAsync(string userId, string[] roles);
    }
}
