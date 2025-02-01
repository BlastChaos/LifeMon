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

        // GET: api/myapi/5
        [HttpGet("lifemons/{userId}")]
        public async Task<IActionResult> GetLifeMons(string userId)
        {
            // Validate the user ID
            if (!ObjectId.TryParse(userId, out _))
                return BadRequest("User ID is not a valid ObjectId.");

            var parsedUserId = ObjectId.Parse(userId);

            // Check if the user ID exists in the Users collection
            var usersCollection = _database.GetCollection<User>("Users");
            var userExists = await usersCollection.Find(u => u.Id == parsedUserId).AnyAsync();
            if (!userExists)
                return NotFound("User ID does not exist.");

            // Get the user's LifeMons from the database
            var lifeMonsCollection = _database.GetCollection<LifeMon>("LifeMons");
            var lifeMons = await lifeMonsCollection
                .Find(lm => lm.UserId == parsedUserId)
                .ToListAsync();

            // Map the LifeMons to the output model
            var lifeMonsOutput = lifeMons.Select(lm => new {
                Id = lm.Id.ToString(),
                UserId = lm.UserId.ToString(),
                Name = lm.Name,
            });

            // Return the LifeMons
            return Ok(lifeMonsOutput);
        }

        [HttpPost("lifemons")]
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

        
        [HttpDelete("lifemons/{userId}/{name}")]
        public async Task<IActionResult> DeleteLifeMonAsync(string userId, string name)
        {
            if (!ObjectId.TryParse(userId, out _))
                return BadRequest("User ID is not a valid ObjectId.");

            var userObjectId = ObjectId.Parse(userId);

            var collection = _database.GetCollection<LifeMon>("LifeMons");

            var filter = Builders<LifeMon>.Filter.And(
                Builders<LifeMon>.Filter.Eq(lm => lm.UserId, userObjectId),
                Builders<LifeMon>.Filter.Eq(lm => lm.Name, name)
            );

            var result = await collection.DeleteOneAsync(filter);

            if (result.IsAcknowledged && result.DeletedCount == 1)
                return Ok(new { Message = "LifeMon deleted successfully." });
            else
                return NotFound("LifeMon not found.");
        }

        
        [HttpPost("teams")]
        public async Task<IActionResult> CreateTeamAsync([FromBody] TeamInfo teamInfo)
        {
            if (string.IsNullOrWhiteSpace(teamInfo.Name))
                return BadRequest("Team name is required.");

            if (teamInfo.LifeMonNames == null || teamInfo.LifeMonNames.Length == 0)
                return BadRequest("List of LifeMons is required.");

            var lifeMonsCollection = _database.GetCollection<LifeMon>("LifeMons");
            var lifeMons = await lifeMonsCollection.Find(lm => teamInfo.LifeMonNames.Contains(lm.Name)).ToListAsync();

            if (lifeMons.Count != teamInfo.LifeMonNames.Length)
                return BadRequest("Some LifeMons do not exist.");

            var team = new Team
            {
                UserId = ObjectId.Parse(teamInfo.UserId),
                Name = teamInfo.Name,
                LifeMons = lifeMons.Select(lm => lm.Id).ToList(),
            };

            var teamsCollection = _database.GetCollection<Team>("Teams");

            var filter = Builders<Team>.Filter.And(
                Builders<Team>.Filter.Eq(t => t.UserId, team.UserId),
                Builders<Team>.Filter.Eq(t => t.Name, team.Name)
            );
            var update = Builders<Team>.Update
                .Set(t => t.Name, team.Name)
                .Set(t => t.LifeMons, team.LifeMons);

            await teamsCollection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });

            return Ok(new { Message = "Team created or updated successfully." });
        }

        [HttpGet("teams/{userId}/{name}")]
        public async Task<IActionResult> GetTeamByNameAsync(string userId, string name)
        {
            if (!ObjectId.TryParse(userId, out _))
                return BadRequest("User ID is not a valid ObjectId.");

            var userObjectId = ObjectId.Parse(userId);

            var teamsCollection = _database.GetCollection<Team>("Teams");
            var team = await teamsCollection
                .Find(t => t.UserId == userObjectId && t.Name == name)
                .FirstOrDefaultAsync();

            if (team == null)
                return NotFound("Team not found.");

            var lifeMonsCollection = _database.GetCollection<LifeMon>("LifeMons");
            var lifeMons = await lifeMonsCollection
                .Find(lm => team.LifeMons.Contains(lm.Id))
                .ToListAsync();

            var lifeMonsOutput = lifeMons.Select(lm => new {
                Id = lm.Id.ToString(),
                UserId = lm.UserId.ToString(),
                Name = lm.Name,
            });

            return Ok(lifeMonsOutput);
        }

        [HttpDelete("teams/{userId}/{name}")]
        public async Task<IActionResult> DeleteTeamAsync(string userId, string name)
        {
            if (!ObjectId.TryParse(userId, out _))
                return BadRequest("User ID is not a valid ObjectId.");

            var userObjectId = ObjectId.Parse(userId);

            var teamsCollection = _database.GetCollection<Team>("Teams");

            var filter = Builders<Team>.Filter.And(
                Builders<Team>.Filter.Eq(t => t.UserId, userObjectId),
                Builders<Team>.Filter.Eq(t => t.Name, name)
            );

            var result = await teamsCollection.DeleteOneAsync(filter);

            if (result.IsAcknowledged && result.DeletedCount == 1)
                return Ok(new { Message = "Team deleted successfully." });
            else
                return NotFound("Team not found.");
        }
    }
}
