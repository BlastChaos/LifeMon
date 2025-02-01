using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace MyApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LifeMonController : ControllerBase
    {
        private readonly IMongoDatabase _database;

        public LifeMonController(IMongoDatabase database)
        {
            _database = database;
        }

        // GET: api/myapi
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { Message = "Hello, World!" });
        }

        // GET: api/myapi/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            return Ok(new { Message = $"You requested ID: {id}" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] UserInfo loginRequest)
        {
            var usersCollection = _database.GetCollection<User>("Users");

            // Find user by username
            var user = await usersCollection.Find(u => u.Username == loginRequest.Username).FirstOrDefaultAsync();

            // Check if user exists and password matches
            if (user == null || BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.Password) == false)
            {
                return Unauthorized("Invalid username or password.");
            }

            // Return user ID if authentication is successful
            return Ok(new { UserId = user.Id.ToString() });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserInfo model)
        {
            if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
                return BadRequest("Username and password are required.");

            var collection = _database.GetCollection<User>("Users");

            // Check if the username already exists
            var existingUser = await collection.Find(u => u.Username == model.Username).FirstOrDefaultAsync();
            if (existingUser != null)
                return Conflict("Username already exists.");

            // Hash the password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

            // Create the new user
            var user = new User
            {
                Username = model.Username,
                Password = hashedPassword
            };

            // Insert the user into the database
            await collection.InsertOneAsync(user);

            return Ok(new { Message = "User registered successfully." });
        }

        // DELETE: api/myapi/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            return NoContent(); // Example: Delete logic here
        }
    }
}
