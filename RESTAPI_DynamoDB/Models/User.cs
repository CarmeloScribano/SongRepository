using System.Diagnostics.CodeAnalysis;

namespace RESTAPI_DynamoDB.Models
{
    public class User
    {
        private string role = Roles.Basic;
        private string password = "string";
        private string username = "string";

        [NotNull]
        public string Username { get => username; set => username = value; }
        [NotNull]
        public string Password { get => password; set => password = value; }
        public int Age { get; set; }
        public string Role { get => role; set => role = value; }
    }
}
