using PlayerDatabase.Models;
using PlayerDatabase.Services;
using Microsoft.AspNetCore.Mvc;

namespace PlayerDatabase.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlayerController : ControllerBase
    {
        public PlayerController()
        {
        }

        // GET all action
        [HttpGet]
        public ActionResult<List<Player>> GetAll() =>
            PlayerService.GetAll();

        /*// GET by Id action
        [HttpGet("{id}")]
        public ActionResult<Player> Get(int id)
        {
            var player = PlayerService.Get(id);

            if (player == null)
                return NotFound();

            return player;

        }*/

        [HttpGet("{name}")]
        public ActionResult<Player> Get(string name)
        {
            var player = PlayerService.Get(name);

            if (player == null)
                return NotFound();

            return player;
        }

        // POST action
        [HttpPut]
        public IActionResult Create(Player player)
        {
            PlayerService.Add(player);
            return CreatedAtAction(nameof(Create), new { id = player.Id }, player);
        }

        // PUT action
        [HttpPut("{id}")]
        public IActionResult Update(int id, Player player)
        {
            if (id != player.Id)
                return BadRequest();

            var existingPlayer = PlayerService.Get(id);
            if (existingPlayer is null)
                return NotFound();

            PlayerService.Update(player);

            return NoContent();
        }

        // DELETE action
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var player = PlayerService.Get(id);

            if (player is null)
                return NotFound();

            PlayerService.Delete(id);

            return NoContent();
        }
    }
}
