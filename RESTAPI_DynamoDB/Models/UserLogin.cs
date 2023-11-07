namespace RESTAPI_DynamoDB.Models
{
    public class UserLogin
    {
        private string password = "admin";
        private string username = "admin";

        public string Username { get => username; set => username = value; }
        public string Password { get => password; set => password = value; }
    }
}
