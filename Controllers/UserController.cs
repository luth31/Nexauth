using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexauth.Server.Models;

namespace Nexauth.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase {
        private readonly AuthContext _context;

        public UserController(AuthContext context) {
            _context = context;
        }

        // GET: api/User
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUser() {
            return await _context.User.ToListAsync();
        }

        // GET: api/User/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(string id) {
            var user = await _context.User.Where(u => u.Username == id).FirstOrDefaultAsync();

            if (user == null) {
                return NotFound();
            }

            return user;
        }

        // POST: api/User
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user) {
            var existingUser = await _context.User.Where(u => u.Username == user.Username).FirstOrDefaultAsync();
            if (existingUser != null) {
                return BadRequest();
            }
            _context.User.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id) {
            var user = await _context.User.Where(u => u.Username == id).FirstOrDefaultAsync();
            if (user == null)
                return NotFound();
            _context.User.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool UserExists(long id)
        {
            return _context.User.Any(e => e.Id == id);
        }
    }
}
