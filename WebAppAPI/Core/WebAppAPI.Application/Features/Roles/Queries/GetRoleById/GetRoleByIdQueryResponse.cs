namespace WebAppAPI.Application.Features.Roles.Queries.GetRoleById
{
    public class GetRoleByIdQueryResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsAdmin { get; set; }
    }
}
