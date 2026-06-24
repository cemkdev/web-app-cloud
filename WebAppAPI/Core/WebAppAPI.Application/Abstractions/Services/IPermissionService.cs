namespace WebAppAPI.Application.Abstractions.Services
{
    public interface IPermissionService
    {
        Task<bool?> GetAdminOnlyByCodeAsync(string code);
        Task<bool> HasRolePermissionAsync(string username, string code);
        Task<bool> HasAdminAccessAsync(string username);
    }
}
