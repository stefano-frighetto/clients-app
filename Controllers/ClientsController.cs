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
        private readonly ILogger<ClientsController> _logger;

        public ClientsController(ApplicationDbContext context, ILogger<ClientsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves the complete list of all registered clients.
        /// </summary>
        /// <returns>A list of clients.</returns>
        /// <response code="200">Returns the list of clients.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Client>>> GetClients()
        {
            _logger.LogInformation("Retrieving all clients from the database.");
            var clients = await _context.Clients.ToListAsync();
            return Ok(clients);
        }

        /// <summary>
        /// Retrieves a specific client by their unique ID.
        /// </summary>
        /// <param name="id">The ID of the client to retrieve.</param>
        /// <returns>The client object if found.</returns>
        /// <response code="200">Returns the requested client.</response>
        /// <response code="404">If no client exists with the provided ID.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Client>> GetClient(int id)
        {
            var client = await _context.Clients.FindAsync(id);

            if (client == null)
            {
                _logger.LogWarning("Client with Id {ClientId} not found.", id);
                return NotFound("No client with the corresponding Id was found");
            }

            return Ok(client);
        }

        /// <summary>
        /// Searches for clients whose name partially matches the search term.
        /// </summary>
        /// <remarks>
        /// Uses a case-insensitive and accent-insensitive search (via Stored Procedure).
        /// Matches characters anywhere in the name (central characters).
        /// </remarks>
        /// <param name="name">The name or fragment to search for (e.g., "john").</param>
        /// <returns>A list of matching clients.</returns>
        /// <response code="200">Returns the list of matching clients.</response>
        /// <response code="404">If no clients are found matching the criteria.</response>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Client>>> SearchClientsByName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return await GetClients();
            }

            _logger.LogInformation("Searching clients with name containing: {Name}", name);

            var clients = await _context.Clients
                .FromSqlRaw("SELECT * FROM search_clients(@p0)", name)
                .ToListAsync();

            if (clients.Count == 0)
            {
                _logger.LogInformation("No clients found matching the search criteria: {Name}", name);
                return NotFound("No clients were found");
            }

            return Ok(clients);
        }

        /// <summary>
        /// Creates a new client in the database.
        /// </summary>
        /// <remarks>
        /// Validates that the CUIT and Email are unique before creating.
        /// The ID is automatically generated.
        /// </remarks>
        /// <param name="clientDto">The client data to create.</param>
        /// <returns>The created client with its new ID.</returns>
        /// <response code="201">Client created successfully.</response>
        /// <response code="400">If the input data is invalid (format validations).</response>
        /// <response code="409">If a client with the same CUIT or Email already exists.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<Client>> CreateClient(CreateClientDto clientDto)
        {
            var existingConflict = await _context.Clients
                .Where(c => c.CUIT == clientDto.CUIT || c.Email == clientDto.Email)
                .Select(c => new { c.CUIT, c.Email })
                .FirstOrDefaultAsync();

            if (existingConflict != null)
            {
                string msg = $"There is already a registered customer with the field {(existingConflict.CUIT == clientDto.CUIT ? $"CUIT = {clientDto.CUIT}" : $"email = {clientDto.Email}")}";
                _logger.LogWarning("Conflict when creating client: {Message}", msg);
                return Conflict(msg);
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

            _logger.LogInformation("Created new client with Id {ClientId}", newClient.ClientId);

            return CreatedAtAction(nameof(GetClient), new { id = newClient.ClientId }, newClient);
        }

        /// <summary>
        /// Updates an existing client's data.
        /// </summary>
        /// <param name="id">The ID of the client to update.</param>
        /// <param name="updatedClient">The client object with updated data.</param>
        /// <returns>The updated client.</returns>
        /// <response code="200">Client updated successfully.</response>
        /// <response code="400">If the URL ID does not match the body ID.</response>
        /// <response code="404">If the client does not exist.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
            _logger.LogInformation("Updated client with Id {ClientId}", id);

            return Ok(existingClient);
        }

        /// <summary>
        /// Deletes a client from the system.
        /// </summary>
        /// <param name="id">The ID of the client to delete.</param>
        /// <response code="204">Client deleted successfully.</response>
        /// <response code="404">If the client does not exist.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteClient(int id)
        {
            var client = await _context.Clients.FindAsync(id);

            if (client == null)
            {
                return NotFound("No client");
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Deleted client with Id {ClientId}", id);

            return NoContent();
        }
    }
}