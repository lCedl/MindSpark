using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace backend.Controllers
{
    [Route("api/BackendController")]
    [ApiController]
    public class BackendController : ControllerBase
    {
        private readonly BackendContext _context;

        public BackendController(BackendContext context)
        {
            _context = context;
        }

        // GET: api/Backend
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BackendItemDTO>>> GetBackendItem()
        {
            return await _context.BackendItem
                .Select(x => ItemToDTO(x))
                .ToListAsync();
        }

        // GET: api/Backend/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BackendItemDTO>> GetBackendItem(long id)
        {
            var backendItem = await _context.BackendItem.FindAsync(id);

            if (backendItem == null)
            {
                return NotFound();
            }

            return ItemToDTO(backendItem);
        }

        // PUT: api/Backend/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBackendItem(long id, BackendItemDTO backendItemDTO)
        {
            if (id != backendItemDTO.Id)
            {
                return BadRequest();
            }

            _context.Entry(backendItemDTO).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BackendItemExists(id))
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

        // POST: api/Backend
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<BackendItemDTO>> PostBackendItem(BackendItemDTO backendItemDTO)
        {
            var backendItem = new BackendItem
            {
                IsComplete = backendItemDTO.IsComplete,
                Name = backendItemDTO.Name,
            };

            _context.BackendItem.Add(backendItem);
            await _context.SaveChangesAsync();

            //return CreatedAtAction("GetBackendItem", new { id = backendItem.Id }, backendItem);
            return CreatedAtAction(nameof(GetBackendItem), new { id = backendItem.Id }, ItemToDTO(backendItem));
        }

        // DELETE: api/Backend/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBackendItem(long id)
        {
            var backendItem = await _context.BackendItem.FindAsync(id);
            if (backendItem == null)
            {
                return NotFound();
            }

            _context.BackendItem.Remove(backendItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BackendItemExists(long id)
        {
            return _context.BackendItem.Any(e => e.Id == id);
        }
        
        private static BackendItemDTO ItemToDTO(BackendItem backendItem) =>
            new BackendItemDTO
            {
                Id = backendItem.Id,
                Name = backendItem.Name,
                IsComplete = backendItem.IsComplete
            };
    }
}
