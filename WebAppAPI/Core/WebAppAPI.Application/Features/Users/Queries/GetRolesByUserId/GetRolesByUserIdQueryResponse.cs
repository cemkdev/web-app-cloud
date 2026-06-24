namespace WebAppAPI.Application.Features.Users.Queries.GetRolesByUserId
{
    public class GetRolesByUserIdQueryResponse
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsAssigned { get; set; }
    }
}
