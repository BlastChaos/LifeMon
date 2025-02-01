using Microsoft.AspNetCore.Mvc;

namespace MyApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LifeMonController : ControllerBase
    {
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

        // POST: api/myapi
        [HttpPost]
        public IActionResult Post([FromBody] string value)
        {
            return CreatedAtAction(nameof(Get), new { id = 1 }, new { Message = $"You posted: {value}" });
        }

        // PUT: api/myapi/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] string value)
        {
            return NoContent(); // Example: Update logic here
        }

        // DELETE: api/myapi/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            return NoContent(); // Example: Delete logic here
        }
    }
}
