using ClientApi.Data;
using ClientApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClientApi.Controllers
{
    [Route("clients")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ClientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Client>>> GetClients()
        {
            var clients = await _context.Clients.ToListAsync();
            return Ok(clients);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Client>> GetClient(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            
            if (client == null)
            {
                return NotFound("No client with the corresponding Id was found");
            }

            return Ok(client);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Client>>> SearchClientsByName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return await GetClients();
            }

            var clients = await _context.Clients
                .Where(c => c.FirstName.Contains(name))
                .ToListAsync();

            if (clients.Count == 0)
            {
                return NotFound("No clients were found");
            }

            return Ok(clients);
        }

        [HttpPost]
        public async Task<ActionResult<Client>> CreateClient(Client newClient)
        {
            _context.Clients.Add(newClient);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetClient), new { id = newClient.ClientId }, newClient);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Client>> UpdateClient(int id, Client updatedClient)
        {
            if (id != updatedClient.ClientId)
            {
                return BadRequest("Id in the URL doesn't match the Id in the client data");
            }

            var existingClient = await _context.Clients.FindAsync(id);

            if (existingClient == null)
            {
                return NotFound("No client with the corresponding Id was found");
            }

            existingClient.FirstName = updatedClient.FirstName;
            existingClient.LastName = updatedClient.LastName;
            existingClient.Birthdate = updatedClient.Birthdate;
            existingClient.CUIT = updatedClient.CUIT;
            existingClient.Address = updatedClient.Address;
            existingClient.CellPhone = updatedClient.CellPhone;
            existingClient.Email = updatedClient.Email;

            await _context.SaveChangesAsync();

            return Ok(existingClient);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteClient(int id)
        {
            var client = await _context.Clients.FindAsync(id);

            if (client == null)
            {
                return NotFound("No client");
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
