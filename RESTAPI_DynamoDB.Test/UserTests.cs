using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using RESTAPI_DynamoDB.Models;
using System.Net.Http.Headers;
using System.Text;

namespace RESTAPI_DynamoDB.Test
{
    [TestClass]
    public class UserTests
    {
        private const string defaultValue = "string1";
        private readonly HttpClient _httpClient;



        // User object used as a test value.
        private readonly User testUser = new()
        {
            Username = defaultValue,
            Password = defaultValue,
            Age = 0,
            Role = "admin"
        };
        // User object used as a test value for updates.
        private readonly User updatedUser = new()
        {
            Username = defaultValue,
            Password = defaultValue + "-updated",
            Age = 0,
            Role = "admin"
        };



        public UserTests()
        {
            _httpClient = new WebApplicationFactory<Program>().CreateDefaultClient();

            string token = GetUserToken(new UserLogin()).Result;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }



        /// <summary>
        /// Tries to login to the server.
        /// </summary>
        [TestMethod]
        public void Login_Test()
        {
            Assert.IsNotNull(GetUserToken(new UserLogin()).Result);
        }

        /// <summary>
        /// Tries to create a User.
        /// </summary>
        [TestMethod]
        public async Task CreateUser_Test()
        {
            //Create a new User
            await _httpClient.DeleteAsync($"/api/UserLogin/DeleteUser/{testUser.Username}");
            var postJson = JsonConvert.SerializeObject(testUser);
            var payload = new StringContent(postJson, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/UserLogin/CreateUser", payload);
            Assert.AreEqual("OK", response.StatusCode.ToString());

            //Create a User that already exists
            response = await _httpClient.PostAsync("/api/UserLogin/CreateUser", payload);
            Assert.AreEqual("Conflict", response.StatusCode.ToString());
        }

        /// <summary>
        /// Tries to get all Users.
        /// </summary>
        [TestMethod]
        public async Task GetAllUsers_Test()
        {
            var response = await _httpClient.GetAsync("/api/UserLogin/GetAllUsers");
            var stringResult = await response.Content.ReadAsStringAsync();
            List<User> listUsers = JsonConvert.DeserializeObject<List<User>>(stringResult) ?? new List<User>();

            foreach (User user in listUsers)
            {
                Assert.IsNotNull(user.Username);
                Assert.IsNotNull(user.Password);
                Assert.IsNotNull(user.Age);
                Assert.IsNotNull(user.Role);
            }
        }

        /// <summary>
        /// Tries to update a User password.
        /// </summary>
        [TestMethod]
        public async Task UpdateUser_Test()
        {
            //Update an existing User
            await _httpClient.DeleteAsync($"/api/UserLogin/DeleteUser/{testUser.Username}");
            var postJson = JsonConvert.SerializeObject(testUser);
            var payload = new StringContent(postJson, Encoding.UTF8, "application/json");
            _ = await _httpClient.PostAsync("/api/UserLogin/CreateUser", payload);

            postJson = JsonConvert.SerializeObject(testUser);
            payload = new StringContent(postJson, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PutAsync("/api/UserLogin/ChangePassword/" + updatedUser.Password, payload);
            Assert.AreEqual("OK", response.StatusCode.ToString());

            //Update a User that does not exists
            await _httpClient.DeleteAsync($"/api/UserLogin/DeleteUser/{testUser.Username}");
            postJson = JsonConvert.SerializeObject(testUser);
            payload = new StringContent(postJson, Encoding.UTF8, "application/json");
            response = await _httpClient.PutAsync("/api/UserLogin/ChangePassword/" + updatedUser.Password, payload);
            Assert.AreEqual("NotFound", response.StatusCode.ToString());
        }

        /// <summary>
        /// Tries to delete a User.
        /// </summary>
        [TestMethod]
        public async Task DeleteUser_Test()
        {
            //Creates and delete a User
            var postJson = JsonConvert.SerializeObject(testUser);
            var payload = new StringContent(postJson, Encoding.UTF8, "application/json");
            await _httpClient.PostAsync("/api/UserLogin/CreateUser", payload);
            HttpResponseMessage response = await _httpClient.DeleteAsync($"/api/UserLogin/DeleteUser/{testUser.Username}");
            Assert.AreEqual("NoContent", response.StatusCode.ToString());

            //Try deleting a User that does not exist
            response = await _httpClient.DeleteAsync($"/api/UserLogin/DeleteUser/{testUser.Username}");
            Assert.AreEqual("NotFound", response.StatusCode.ToString());
        }


        //--------------------------------------------------------------------------------------------------------------------------


        /// <summary>
        /// Creates a Post request and returns bearer token of the specified user.
        /// </summary>
        /// <param name="user">User desired to send.</param>
        /// <returns>String containing the token.</returns>
        private async Task<string> GetUserToken(UserLogin user)
        {
            var postJson = JsonConvert.SerializeObject(user);
            var payload = new StringContent(postJson, Encoding.UTF8, "application/json");
            return await _httpClient.PostAsync($"/api/UserLogin/Login/{user.Username}/{user.Password}", payload).Result.Content.ReadAsStringAsync();
        }
    }
}