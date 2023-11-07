namespace RESTAPI_DynamoDB.Models
{
    public class Roles
    {
        public static readonly List<string> AllowedRoles = new() { Administrator, Basic };

        public const string Administrator = "admin";
        public const string Basic = "basic";
    }
}
