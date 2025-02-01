using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
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

        [HttpPost("addLifeMon")]
        public async Task<IActionResult> AddLifeMonAsync([FromBody] LifeMonInfo lifeMonInfo)
        {
            if (!ObjectId.TryParse(lifeMonInfo.UserId, out _))
                return BadRequest("User ID is not a valid ObjectId.");

            // Check if the user ID exists in the Users collection
            var userObjectId = ObjectId.Parse(lifeMonInfo.UserId);
            var usersCollection = _database.GetCollection<User>("Users");
            var userExists = await usersCollection.Find(u => u.Id == userObjectId).AnyAsync();
            if (!userExists)
                return NotFound("User ID does not exist.");

            var collection = _database.GetCollection<LifeMon>("LifeMons");
            var existingLifeMon = await collection.Find(lm => lm.Name == lifeMonInfo.Name).FirstOrDefaultAsync();
            if (existingLifeMon != null)
                return Conflict("LifeMon with the same name already exists.");

            var lifeMon = new LifeMon
            {
                UserId = userObjectId,
                Name = lifeMonInfo.Name,
            };

            await collection.InsertOneAsync(lifeMon);

            return Ok(new { Message = "LifeMon added successfully." });
        }
    }
}
