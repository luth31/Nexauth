using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nexauth_server.Models;

namespace nexauth_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthRequestController : ControllerBase
    {
        private readonly AuthContext _context;

        public AuthRequestController(AuthContext context)
        {
            _context = context;
        }

        // GET: api/AuthRequest
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuthRequest>>> GetAuthRequests()
        {
            return await _context.AuthRequests.ToListAsync();
        }

        // GET: api/AuthRequest/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AuthRequest>> GetAuthRequest(long id)
        {
            var authRequest = await _context.AuthRequests.FindAsync(id);

            if (authRequest == null)
            {
                return NotFound();
            }

            return authRequest;
        }

        // PUT: api/AuthRequest/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAuthRequest(long id, AuthRequest authRequest)
        {
            if (id != authRequest.Id)
            {
                return BadRequest();
            }

            _context.Entry(authRequest).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AuthRequestExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/AuthRequest
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<AuthRequest>> PostAuthRequest(AuthRequest authRequest)
        {
            _context.AuthRequests.Add(authRequest);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAuthRequest", new { id = authRequest.Id }, authRequest);
        }

        // DELETE: api/AuthRequest/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuthRequest(long id)
        {
            var authRequest = await _context.AuthRequests.FindAsync(id);
            if (authRequest == null)
            {
                return NotFound();
            }

            _context.AuthRequests.Remove(authRequest);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AuthRequestExists(long id)
        {
            return _context.AuthRequests.Any(e => e.Id == id);
        }
    }
}
