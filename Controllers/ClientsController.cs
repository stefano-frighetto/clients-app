using ClientApi.Data;
using ClientApi.DTOs;
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
                .FromSqlRaw("SELECT * FROM search_clients(@p0)", name)
                .ToListAsync();

            if (clients.Count == 0)
            {
                return NotFound("No clients were found");
            }

            return Ok(clients);
        }

        [HttpPost]
        public async Task<ActionResult<Client>> CreateClient(CreateClientDto clientDto)
        {
            var existingConflict = await _context.Clients
                .Where(c => c.CUIT == clientDto.CUIT || c.Email == clientDto.Email)
                .Select(c => new { c.CUIT, c.Email })
                .FirstOrDefaultAsync();

            if (existingConflict != null)
            {
                return Conflict($"Ya existe un cliente registrado con el {(existingConflict.CUIT == clientDto.CUIT ? $"CUIT {clientDto.CUIT}" : $"email {clientDto.Email}")}");
            }

            var newClient = new Client
            {
                FirstName = clientDto.FirstName,
                LastName = clientDto.LastName,
                CorporateName = clientDto.CorporateName,
                CUIT = clientDto.CUIT,
                Birthdate = clientDto.Birthdate,
                CellPhone = clientDto.CellPhone,
                Email = clientDto.Email
            };

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
            existingClient.CorporateName = updatedClient.CorporateName;
            existingClient.CUIT = updatedClient.CUIT;
            existingClient.Birthdate = updatedClient.Birthdate;
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
