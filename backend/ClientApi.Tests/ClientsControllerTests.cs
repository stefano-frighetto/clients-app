using ClientApi.Controllers;
using ClientApi.DTOs;
using ClientApi.Models;
using ClientApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace ClientApi.Tests
{
    public class ClientsControllerTests
    {
        private readonly Mock<IClientRepository> _mockRepo;
        private readonly Mock<ILogger<ClientsController>> _mockLogger;
        private readonly ClientsController _controller;

        public ClientsControllerTests()
        {
            _mockRepo = new Mock<IClientRepository>();
            _mockLogger = new Mock<ILogger<ClientsController>>();
            _controller = new ClientsController(_mockRepo.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetClients_ReturnsOkResult_WithListOfClients()
        {
            var fakeClients = new List<Client>
            {
                new Client { ClientId = 1, FirstName = "Juan", LastName = "Perez", CorporateName = "A", CUIT = "20-11111111-1", Email = "j@t.com", CellPhone = "111", Birthdate = DateTime.Now },
                new Client { ClientId = 2, FirstName = "Maria", LastName = "Gomez", CorporateName = "B", CUIT = "27-22222222-2", Email = "m@t.com", CellPhone = "222", Birthdate = DateTime.Now }
            };

            _mockRepo.Setup(repo => repo.GetAllAsync())
                     .ReturnsAsync(fakeClients);

            var result = await _controller.GetClients();

            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedClients = Assert.IsType<List<Client>>(actionResult.Value);
            Assert.Equal(2, returnedClients.Count);
        }

        [Fact]
        public async Task GetClient_ReturnsOk_WhenClientExists()
        {
            var fakeClient = new Client { ClientId = 1, FirstName = "Test", LastName = "T", CorporateName = "C", CUIT = "20-11111111-1", Email = "t@t.com", CellPhone = "111", Birthdate = DateTime.Now };

            _mockRepo.Setup(repo => repo.GetByIdAsync(1))
                     .ReturnsAsync(fakeClient);

            var result = await _controller.GetClient(1);

            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedClient = Assert.IsType<Client>(actionResult.Value);
            Assert.Equal(1, returnedClient.ClientId);
        }

        [Fact]
        public async Task GetClient_ReturnsNotFound_WhenClientDoesNotExist()
        {
            _mockRepo.Setup(repo => repo.GetByIdAsync(999))
                     .ReturnsAsync((Client?)null);

            var result = await _controller.GetClient(999);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task SearchClients_ReturnsOk_WhenMatchesFound()
        {
            var searchResults = new List<Client>
            {
                new Client { ClientId = 1, FirstName = "Juan", LastName = "Perez", CorporateName = "A", CUIT = "20-11111111-1", Email = "j@t.com", CellPhone = "111", Birthdate = DateTime.Now }
            };

            _mockRepo.Setup(repo => repo.SearchByNameAsync("ua"))
                     .ReturnsAsync(searchResults);

            var result = await _controller.SearchClientsByName("ua");

            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedClients = Assert.IsType<List<Client>>(actionResult.Value);
            Assert.Single(returnedClients);
        }

        [Fact]
        public async Task SearchClients_ReturnsAll_WhenNameIsNull()
        {
            var allClients = new List<Client> { new Client { ClientId = 1, FirstName = "A", LastName = "B", CorporateName = "C", CUIT = "20-1", Email = "e", CellPhone = "1", Birthdate = DateTime.Now } };
            _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(allClients);

            var result = await _controller.SearchClientsByName(null);

            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsType<List<Client>>(actionResult.Value);
            Assert.Single(list);
        }

        [Fact]
        public async Task CreateClient_ReturnsCreated_WhenDataIsValid()
        {
            var newDto = new CreateClientDto
            {
                FirstName = "Nuevo",
                LastName = "C",
                CorporateName = "N",
                CUIT = "20-33333333-3",
                Email = "n@t.com",
                CellPhone = "333",
                Birthdate = DateTime.Now
            };

            _mockRepo.Setup(repo => repo.GetConflictAsync(newDto.CUIT, newDto.Email))
                     .ReturnsAsync((Client?)null);

            var result = await _controller.CreateClient(newDto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);

            _mockRepo.Verify(repo => repo.AddAsync(It.IsAny<Client>()), Times.Once);
        }

        [Fact]
        public async Task CreateClient_ReturnsConflict_WhenConflictExists()
        {
            var newDto = new CreateClientDto
            {
                FirstName = "Duplicado",
                LastName = "D",
                CorporateName = "D",
                CUIT = "20-33333333-3",
                Email = "dup@t.com",
                CellPhone = "333",
                Birthdate = DateTime.Now
            };

            var existingClient = new Client { ClientId = 5, FirstName = "Viejo", LastName = "V", CorporateName = "V", CUIT = "20-33333333-3", Email = "otro@t.com", CellPhone = "111", Birthdate = DateTime.Now };

            _mockRepo.Setup(repo => repo.GetConflictAsync(newDto.CUIT, newDto.Email))
                     .ReturnsAsync(existingClient);

            var result = await _controller.CreateClient(newDto);

            Assert.IsType<ConflictObjectResult>(result.Result);

            _mockRepo.Verify(repo => repo.AddAsync(It.IsAny<Client>()), Times.Never);
        }

        [Fact]
        public async Task CreateClient_ReturnsConflict_WhenEmailExists()
        {
            var newDto = new CreateClientDto
            {
                FirstName = "X",
                LastName = "Y",
                CorporateName = "Z",
                CUIT = "20-11111111-1",
                Email = "duplicado@test.com",
                CellPhone = "111",
                Birthdate = DateTime.Now
            };

            var existingClient = new Client
            {
                ClientId = 1,
                FirstName = "A",
                LastName = "B",
                CorporateName = "C",
                CUIT = "20-99999999-9",
                Email = "duplicado@test.com",
                CellPhone = "222",
                Birthdate = DateTime.Now
            };

            _mockRepo.Setup(repo => repo.GetConflictAsync(newDto.CUIT, newDto.Email))
                     .ReturnsAsync(existingClient);

            var result = await _controller.CreateClient(newDto);

            var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
            Assert.Contains("email", conflictResult.Value?.ToString());
        }
        [Fact]
        public async Task UpdateClient_ReturnsOk_WhenValid()
        {
            var existingClient = new Client { ClientId = 10, FirstName = "Old", LastName = "Old", CorporateName = "Old", CUIT = "20-10101010-1", Email = "old@t.com", CellPhone = "111", Birthdate = DateTime.Now };

            _mockRepo.Setup(repo => repo.GetByIdAsync(10))
                     .ReturnsAsync(existingClient);

            _mockRepo.Setup(repo => repo.EmailExistsForOtherClientAsync("new@t.com", 10))
                     .ReturnsAsync(false);

            var updateData = new Client { ClientId = 10, FirstName = "New", LastName = "New", CorporateName = "New", CUIT = "20-10101010-1", Email = "new@t.com", CellPhone = "111", Birthdate = DateTime.Now };

            var result = await _controller.UpdateClient(10, updateData);

            Assert.IsType<OkObjectResult>(result.Result);
            _mockRepo.Verify(repo => repo.UpdateAsync(It.IsAny<Client>()), Times.Once);
        }

        [Fact]
        public async Task UpdateClient_ReturnsBadRequest_WhenIdsDoNotMatch()
        {
            var updateData = new Client { ClientId = 1, FirstName = "T", LastName = "T", CorporateName = "T", CUIT = "20-111", Email = "t@t.com", CellPhone = "111", Birthdate = DateTime.Now };

            var result = await _controller.UpdateClient(50, updateData);

            Assert.IsType<BadRequestObjectResult>(result.Result);
            _mockRepo.Verify(repo => repo.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task UpdateClient_ReturnsNotFound_WhenClientDoesNotExist()
        {
            var updateData = new Client { ClientId = 99, FirstName = "T", LastName = "T", CorporateName = "T", CUIT = "20-111", Email = "t@t.com", CellPhone = "111", Birthdate = DateTime.Now };

            _mockRepo.Setup(repo => repo.GetByIdAsync(99))
                     .ReturnsAsync((Client?)null);

            var result = await _controller.UpdateClient(99, updateData);

            Assert.IsType<NotFoundObjectResult>(result.Result);
            _mockRepo.Verify(repo => repo.UpdateAsync(It.IsAny<Client>()), Times.Never);
        }

        [Fact]
        public async Task DeleteClient_ReturnsNotFound_WhenClientDoesNotExist()
        {
            _mockRepo.Setup(repo => repo.GetByIdAsync(999))
                     .ReturnsAsync((Client?)null);

            var result = await _controller.DeleteClient(999);

            Assert.IsType<NotFoundObjectResult>(result);
            _mockRepo.Verify(repo => repo.DeleteAsync(It.IsAny<Client>()), Times.Never);
        }

        [Fact]
        public async Task DeleteClient_ReturnsNoContent_WhenClientExists()
        {
            var clientToDelete = new Client { ClientId = 1, FirstName = "Bye", LastName = "Bye", CorporateName = "B", CUIT = "20-11111111-1", Email = "b@t.com", CellPhone = "111", Birthdate = DateTime.Now };

            _mockRepo.Setup(repo => repo.GetByIdAsync(1))
                     .ReturnsAsync(clientToDelete);

            var result = await _controller.DeleteClient(1);

            Assert.IsType<NoContentResult>(result);
            _mockRepo.Verify(repo => repo.DeleteAsync(clientToDelete), Times.Once);
        }
    }
}