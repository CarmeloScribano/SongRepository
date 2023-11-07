using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RESTAPI_DynamoDB.Models;
using RESTAPI_DynamoDB.Utilities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RESTAPI_DynamoDB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserLoginController : Controller
    {
        private readonly IDynamoDBContext _context;
        protected readonly IConfiguration _configuration;

        public UserLoginController(IDynamoDBContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        /// <summary>
        /// Logs the User in and returns the bearer token.
        /// </summary>
        /// <param name="username">Username used to login.</param>
        /// <param name="password">Password used to login.</param>
        /// <returns>The status code of the call.</returns>
        /// <response code="200">Success logging in.</response>
        /// <response code="400">The request was not properly formatted.</response>
        /// <response code="401">Invalid credentials used.</response>
        /// <response code="500">Server is unvalaible.</response>
        /// <response code="503">The server is not properly connecting to the Database.</response>
        [Route("Login/{username}/{password}")]
        [HttpPost]
        public async Task<ActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return StatusCode(400);
            }

            var loggedInUser = await _context.LoadAsync<User>(username.Trim());
            if (loggedInUser is null)
            {
                return StatusCode(409);
            }
            else if (PasswordHasher.SHA512Hasher(password) != loggedInUser.Password)
            {
                return StatusCode(401);
            }

            return Ok(new JwtSecurityTokenHandler().WriteToken(CreateJWTToken(loggedInUser)));
        }


        /// <summary>
        /// Creates a User.
        /// </summary>
        /// <param name="user">The User desired to create.</param>
        /// <returns>The status code of the call.</returns>
        /// <response code="200">Success creating the User.</response>
        /// <response code="400">The request was not properly formatted.</response>
        /// <response code="401">Unauthorized, log in before running this call.</response>
        /// <response code="409">User already exists.</response>
        /// <response code="500">Server is unvalaible.</response>
        /// <response code="503">The server is not properly connecting to the Database.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Roles.Administrator)]
        [Route("CreateUser")]
        [HttpPost]
        public async Task<ActionResult> CreateUser(User user)
        {
            if (string.IsNullOrEmpty(user.Username.Trim()) || string.IsNullOrEmpty(user.Password.Trim()))
            {
                return StatusCode(400);
            }
            else if (await _context.LoadAsync<User>(user.Username.Trim()) != null)
            {
                return StatusCode(409);
            }

            await _context.SaveAsync(new User()
            {
                Username = user.Username,
                Password = PasswordHasher.SHA512Hasher(user.Password),
                Age = 20,
                Role = Roles.AllowedRoles.Contains(user.Role) ? user.Role : Roles.Basic
            });

            return Ok();
        }


        /// <summary>
        /// Gets a list of all Users.
        /// </summary>
        /// <returns>The list of Songs.</returns>
        /// <response code="200">Success getting all the Songs.</response>
        /// <response code="400">The request was not properly formatted.</response>
        /// <response code="401">Unauthorized, log in before running this call.</response>
        /// <response code="500">Server is unvalaible.</response>
        /// <response code="503">The server is not properly connecting to the Database.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Roles.Administrator)]
        [Route("GetAllUsers")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var conditions = new List<ScanCondition>();
            var users = await _context
                .ScanAsync<User>(conditions)
                .GetRemainingAsync();

            return Ok(users);
        }


        /// <summary>
        /// Updates the password of a User.
        /// </summary>
        /// <param name="user">User desired to update.</param>
        /// <param name="newPassword">New password desired.</param>
        /// <returns>The status code of the call.</returns>
        /// <response code="200">Success updating the password.</response>
        /// <response code="400">The request was not properly formatted.</response>
        /// <response code="401">Invalid credentials used.</response>
        /// <response code="404">The User desired to update was not found.</response>
        /// <response code="500">Server is unvalaible.</response>
        /// <response code="503">The server is not properly connecting to the Database.</response>
        [Route("ChangePassword/{newPassword}")]
        [HttpPut]
        public async Task<ActionResult> ChangePassword(UserLogin user, string newPassword)
        {
            if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password) || string.IsNullOrEmpty(newPassword))
            {
                return StatusCode(400);
            }

            var loggedInUser = await _context.LoadAsync<User>(user.Username.Trim());
            if (loggedInUser is null)
            {
                return StatusCode(404);
            }
            else if (PasswordHasher.SHA512Hasher(user.Password) != loggedInUser.Password)
            {
                return StatusCode(401);
            }

            await _context.SaveAsync(new User()
            {
                Username = user.Username,
                Password = PasswordHasher.SHA512Hasher(newPassword)
            });

            return Ok();
        }


        /// <summary>
        /// Deletes a desired User.
        /// </summary>
        /// <param name="username">Username of the User that is desired.</param>
        /// <returns>The status code of the call.</returns>
        /// <response code="204">Success deleting the User.</response>
        /// <response code="400">The request was not properly formatted.</response>
        /// <response code="401">Unauthorized, log in before running this call.</response>
        /// <response code="404">The User desired to delete was not found.</response>
        /// <response code="500">Server is unvalaible.</response>
        /// <response code="503">The server is not properly connecting to the Database.</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Roles.Administrator)]
        [Route("DeleteUser/{username}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteSong(string username)
        {
            var user = await _context.LoadAsync<User>(username.Trim());
            if (user == null)
            {
                return NotFound();
            }

            await _context.DeleteAsync(user);

            return NoContent();
        }


        /// <summary>
        /// Creates a JWT token for the given User.
        /// </summary>
        /// <param name="user">User object desired to create a JWT token for.</param>
        /// <returns>A SecurityToken containing the JWT token.</returns>
        [NonAction]
        public SecurityToken CreateJWTToken(User user)
        {
            var claims = new[]
            {
                    new Claim(ClaimTypes.NameIdentifier, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
            };
            return new JwtSecurityToken
            (
                issuer: _configuration.GetValue<string>("Jwt:Issuer"),
                audience: _configuration.GetValue<string>("Jwt:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddDays(60),
                notBefore: DateTime.UtcNow,
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("Jwt:Key"))),
                    SecurityAlgorithms.HmacSha256)
            );
        } 
    }
}
