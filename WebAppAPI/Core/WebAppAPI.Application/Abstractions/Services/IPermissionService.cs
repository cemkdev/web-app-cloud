namespace WebAppAPI.Application.Abstractions.Services
{
    public interface IPermissionService
    {
        Task<bool?> RequiresAdminAccessAsync(string code, CancellationToken cancellationToken);
        Task<bool> HasRolePermissionAsync(string username, string code, CancellationToken cancellationToken);
        Task<bool> HasAdminAccessAsync(string username, CancellationToken cancellationToken);
    }
}
