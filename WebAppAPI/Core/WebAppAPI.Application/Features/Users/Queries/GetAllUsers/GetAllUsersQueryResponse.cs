namespace WebAppAPI.Application.Features.Users.Queries.GetAllUsers
{
    public class GetAllUsersQueryResponse
    {
        public int TotalUserCount { get; set; }
        public object Users { get; set; }
    }
}
