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
            var lifeMonsOutput = lifeMons.Select(lm => new
            {
                Id = lm.Id.ToString(),
                UserId = lm.UserId.ToString(),
                Name = lm.Name,
                Attack = lm.Attack,
                Hp = lm.Hp,
                Speed = lm.Speed,
                Defense = lm.Defense,
                SpecialAttack = lm.SpecialAttack,
                SpecialDefense = lm.SpecialDefense,
                Species = lm.Species,
                Description = lm.Description,
                Type = lm.Type,
                Move = lm.Move,
            });

            // Return the LifeMons
            return Ok(lifeMonsOutput);
        }

        [HttpGet("lifemons/{userId}/{name}")]
        public async Task<IActionResult> GetLifeMon(string userId, string name)
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

            // Get the specified LifeMon from the database
            var lifeMonsCollection = _database.GetCollection<LifeMon>("LifeMons");
            var lifeMon = await lifeMonsCollection
                .Find(lm => lm.UserId == parsedUserId && lm.Name == name)
                .FirstOrDefaultAsync();

            if (lifeMon == null)
                return NotFound("LifeMon not found.");

            // Map the LifeMon to the output model
            var lifeMonOutput = new
            {
                Id = lifeMon.Id.ToString(),
                UserId = lifeMon.UserId.ToString(),
                Name = lifeMon.Name,
                Attack = lifeMon.Attack,
                Hp = lifeMon.Hp,
                Speed = lifeMon.Speed,
                Defense = lifeMon.Defense,
                SpecialAttack = lifeMon.SpecialAttack,
                SpecialDefense = lifeMon.SpecialDefense,
                Species = lifeMon.Species,
                Description = lifeMon.Description,
                Type = lifeMon.Type,
                Move = lifeMon.Move,
            };

            // Return the LifeMon
            return Ok(lifeMonOutput);
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
                Attack = lifeMonInfo.Attack,
                Hp = lifeMonInfo.Hp,
                Speed = lifeMonInfo.Speed,
                Defense = lifeMonInfo.Defense,
                SpecialAttack = lifeMonInfo.SpecialAttack,
                SpecialDefense = lifeMonInfo.SpecialDefense,
                Species = lifeMonInfo.Species,
                Description = lifeMonInfo.Description,
                Type = lifeMonInfo.Type,
                Move = lifeMonInfo.Move
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

            if (!result.IsAcknowledged || result.DeletedCount == 0)
                return NotFound("LifeMon not found.");

            // Remove the LifeMon from teams if it is present
            var teamsCollection = _database.GetCollection<Team>("Teams");
            var filterTeams = Builders<Team>.Filter.ElemMatch(t => t.LifeMons, lm => lm == name);
            var updateTeams = Builders<Team>.Update.PullFilter(t => t.LifeMons, lm => lm == name);
            await teamsCollection.UpdateManyAsync(filterTeams, updateTeams);

            return Ok(new { Message = "LifeMon deleted successfully." });
        }


        [HttpPost("teams")]
        public async Task<IActionResult> CreateTeamAsync([FromBody] TeamInfo teamInfo)
        {
            if (string.IsNullOrWhiteSpace(teamInfo.Name))
                return BadRequest("Team name is required.");

            if (teamInfo.LifeMonNames == null || teamInfo.LifeMonNames.Length == 0)
                return BadRequest("List of LifeMons is required.");

            if (!ObjectId.TryParse(teamInfo.UserId, out _))
                return BadRequest("User ID is not a valid ObjectId.");

            var lifeMonsCollection = _database.GetCollection<LifeMon>("LifeMons");
            var filter = Builders<LifeMon>.Filter.And(
                Builders<LifeMon>.Filter.Eq(lm => lm.UserId, ObjectId.Parse(teamInfo.UserId)),
                Builders<LifeMon>.Filter.In(lm => lm.Name, teamInfo.LifeMonNames)
            );
            var lifeMonDocuments = await lifeMonsCollection.Find(filter).ToListAsync();

            if (lifeMonDocuments.Count != teamInfo.LifeMonNames.Length)
                return BadRequest("Some LifeMons do not exist.");

            var team = new Team
            {
                UserId = ObjectId.Parse(teamInfo.UserId),
                Name = teamInfo.Name,
                LifeMons = lifeMonDocuments.Select(lm => lm.Name).ToList(),
            };

            var teamsCollection = _database.GetCollection<Team>("Teams");

            var filterTeams = Builders<Team>.Filter.And(
                Builders<Team>.Filter.Eq(t => t.UserId, team.UserId),
                Builders<Team>.Filter.Eq(t => t.Name, team.Name)
            );
            var update = Builders<Team>.Update
                .Set(t => t.Name, team.Name)
                .Set(t => t.LifeMons, team.LifeMons);

            await teamsCollection.UpdateOneAsync(filterTeams, update, new UpdateOptions { IsUpsert = true });

            return Ok(new { Message = "Team created or updated successfully." });
        }

        [HttpGet("teams/{userId}")]
        public async Task<IActionResult> GetTeamsAsync(string userId)
        {
            if (!ObjectId.TryParse(userId, out _))
                return BadRequest("User ID is not a valid ObjectId.");

            var userObjectId = ObjectId.Parse(userId);

            var teamsCollection = _database.GetCollection<Team>("Teams");
            var teams = await teamsCollection
                .Find(t => t.UserId == userObjectId)
                .ToListAsync();

            var teamsOutput = teams.Select(t => new
            {
                Id = t.Id.ToString(),
                UserId = t.UserId.ToString(),
                Name = t.Name,
                LifeMons = t.LifeMons
            });

            return Ok(teamsOutput);
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

        [HttpDelete("teams/{userId}/{name}/{lifemonName}")]
        public async Task<IActionResult> DeleteLifeMonFromTeamAsync(string userId, string name, string lifemonName)
        {
            if (!ObjectId.TryParse(userId, out _))
                return BadRequest("User ID is not a valid ObjectId.");

            var userObjectId = ObjectId.Parse(userId);

            var teamsCollection = _database.GetCollection<Team>("Teams");
            var filter = Builders<Team>.Filter.And(
                Builders<Team>.Filter.Eq(t => t.UserId, userObjectId),
                Builders<Team>.Filter.Eq(t => t.Name, name)
            );
            var update = Builders<Team>.Update.Pull(t => t.LifeMons, lifemonName);

            var result = await teamsCollection.UpdateOneAsync(filter, update);

            if (result.IsAcknowledged && result.ModifiedCount == 1)
                return Ok(new { Message = "LifeMon deleted successfully from team." });
            else
                return NotFound("Team or LifeMon not found.");
        }
    }
}
