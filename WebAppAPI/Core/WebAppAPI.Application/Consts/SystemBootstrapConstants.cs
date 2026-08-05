namespace WebAppAPI.Application.Consts
{
    public static class SystemBootstrapConstants
    {
        public const string SystemAdministratorRoleId = "e63314dc-40c1-444d-aaed-f403c99d002d";

        public const string SystemAdministratorRoleName = "SystemAdministrator";

        public const string SystemAdministratorUserId = "9782395d-d478-47e5-9896-41c55ea4a693";

        public static IReadOnlySet<string> ProtectedEndpointCodes { get; } =
            new HashSet<string>(StringComparer.Ordinal)
            {
                "GET.Read.GetRoles",
                "GET.Read.GetAuthorizeDefinitionEndpoints",
                "GET.Read.GetRolesandEndpoints",
                "POST.Write.AssignRolestoEndpoints"
            };
    }
}
