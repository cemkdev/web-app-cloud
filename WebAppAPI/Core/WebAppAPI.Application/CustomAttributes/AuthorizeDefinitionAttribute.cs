using WebAppAPI.Application.Enums;

namespace WebAppAPI.Application.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class AuthorizeDefinitionAttribute : Attribute
    {
        public required string Menu { get; set; }
        public required string Definition { get; set; }
        public required ActionType ActionType { get; set; }
        public bool AdminOnly { get; set; }
    }
}
