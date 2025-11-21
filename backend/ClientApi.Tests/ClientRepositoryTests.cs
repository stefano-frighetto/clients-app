using ClientApi.Data;
using ClientApi.Models;
using ClientApi.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClientApi.Tests
{
    public class ClientRepositoryTests
    {
        private static ApplicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        private static Client GetTestClient(string email, string cuit)
        {
            return new Client { FirstName = "Test", LastName = "User", CorporateName = "Corp", CUIT = cuit, Email = email, CellPhone = "1111111111", Birthdate = DateTime.Now };
        }

        [Fact]
        public async Task AddAsync_ShouldAddClientAndSaveChanges()
        {
            var dbName = Guid.NewGuid().ToString();
            var context = GetDbContext(dbName);
            var repo = new ClientRepository(context);
            var client = GetTestClient("add@test.com", "20-00000000-0");

            await repo.AddAsync(client);

            var count = await context.Clients.CountAsync();
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllClients()
        {
            var dbName = Guid.NewGuid().ToString();
            using (var context = GetDbContext(dbName))
            {
                context.Clients.Add(GetTestClient("a@t.com", "20-1"));
                context.Clients.Add(GetTestClient("b@t.com", "20-2"));
                await context.SaveChangesAsync();
            }

            using (var context = GetDbContext(dbName))
            {
                var repo = new ClientRepository(context);
                var clients = await repo.GetAllAsync();
                Assert.Equal(2, clients.Count());
            }
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnClient_WhenClientExists()
        {
            var dbName = Guid.NewGuid().ToString();
            int clientId;
            using (var context = GetDbContext(dbName))
            {
                context.Clients.Add(GetTestClient("get@t.com", "20-get-0"));
                await context.SaveChangesAsync();
                clientId = context.Clients.First().ClientId;
            }

            using (var context = GetDbContext(dbName))
            {
                var repo = new ClientRepository(context);
                var client = await repo.GetByIdAsync(clientId);
                Assert.NotNull(client);
            }
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateClientAndSaveChanges()
        {
            var dbName = Guid.NewGuid().ToString();
            int clientId;
            using (var context = GetDbContext(dbName))
            {
                context.Clients.Add(GetTestClient("old@t.com", "20-old-0"));
                await context.SaveChangesAsync();
                clientId = context.Clients.First().ClientId;
            }

            using (var context = GetDbContext(dbName))
            {
                var repo = new ClientRepository(context);
                var client = await repo.GetByIdAsync(clientId);

                client!.FirstName = "UpdatedName";

                await repo.UpdateAsync(client);

                var updatedClient = await repo.GetByIdAsync(clientId);
                Assert.Equal("UpdatedName", updatedClient!.FirstName);
            }
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveClientAndSaveChanges()
        {
            var dbName = Guid.NewGuid().ToString();
            int clientId;
            using (var context = GetDbContext(dbName))
            {
                context.Clients.Add(GetTestClient("del@t.com", "20-del-0"));
                await context.SaveChangesAsync();
                clientId = context.Clients.First().ClientId;
            }

            using (var context = GetDbContext(dbName))
            {
                var repo = new ClientRepository(context);
                var clientToDelete = await repo.GetByIdAsync(clientId);
                await repo.DeleteAsync(clientToDelete!);

                var deletedClient = await repo.GetByIdAsync(clientId);
                Assert.Null(deletedClient);
            }
        }

        [Fact]
        public async Task GetConflictAsync_ShouldReturnClient_WhenCuitMatches()
        {
            var dbName = Guid.NewGuid().ToString();
            using (var context = GetDbContext(dbName))
            {
                context.Clients.Add(GetTestClient("unique1@t.com", "20-DUP-C-0"));
                await context.SaveChangesAsync();
            }
            using (var context = GetDbContext(dbName))
            {
                var repo = new ClientRepository(context);
                var conflict = await repo.GetConflictAsync("20-DUP-C-0", "totally-new@t.com");
                Assert.NotNull(conflict);
                Assert.Equal("20-DUP-C-0", conflict.CUIT);
            }
        }

        [Fact]
        public async Task GetConflictAsync_ShouldReturnClient_WhenEmailMatches()
        {
            var dbName = Guid.NewGuid().ToString();
            using (var context = GetDbContext(dbName))
            {
                context.Clients.Add(GetTestClient("DUP@t.com", "20-unique1-0"));
                await context.SaveChangesAsync();
            }
            using (var context = GetDbContext(dbName))
            {
                var repo = new ClientRepository(context);
                var conflict = await repo.GetConflictAsync("20-new-C-0", "DUP@t.com");
                Assert.NotNull(conflict);
                Assert.Equal("DUP@t.com", conflict.Email);
            }
        }

        [Fact]
        public async Task GetConflictAsync_ShouldReturnNull_WhenNoConflict()
        {
            var dbName = Guid.NewGuid().ToString();
            using (var context = GetDbContext(dbName))
            {
                var repo = new ClientRepository(context);
                var conflict = await repo.GetConflictAsync("20-NEWEST-0", "totally-new@t.com");
                Assert.Null(conflict);
            }
        }

        [Fact]
        public async Task EmailExistsForOtherClientAsync_ReturnsTrue_WhenEmailExistsForDifferentClient()
        {
            var dbName = Guid.NewGuid().ToString();
            using (var context = GetDbContext(dbName))
            {
                context.Clients.Add(new Client { ClientId = 1, FirstName = "A", LastName = "B", CorporateName = "C", CUIT = "20-111", Email = "SHARED@test.com", CellPhone = "111", Birthdate = DateTime.Now });
                await context.SaveChangesAsync();
            }
            using (var context = GetDbContext(dbName))
            {
                var repo = new ClientRepository(context);

                var result = await repo.EmailExistsForOtherClientAsync("SHARED@test.com", 2);
                Assert.True(result);
            }
        }

        [Fact]
        public async Task EmailExistsForOtherClientAsync_ReturnsFalse_WhenEmailExistsForSameClient()
        {
            var dbName = Guid.NewGuid().ToString();
            using (var context = GetDbContext(dbName))
            {
                context.Clients.Add(new Client { ClientId = 5, FirstName = "A", LastName = "B", CorporateName = "C", CUIT = "20-111", Email = "SAME@test.com", CellPhone = "111", Birthdate = DateTime.Now });
                await context.SaveChangesAsync();
            }
            using (var context = GetDbContext(dbName))
            {
                var repo = new ClientRepository(context);

                var result = await repo.EmailExistsForOtherClientAsync("SAME@test.com", 5);
                Assert.False(result);
            }
        }

        [Fact]
        public async Task EmailExistsForOtherClientAsync_ReturnsFalse_WhenEmailDoesNotExist()
        {
            var dbName = Guid.NewGuid().ToString();
            using (var context = GetDbContext(dbName))
            {
                var repo = new ClientRepository(context);
                var result = await repo.EmailExistsForOtherClientAsync("NONEXISTENT@test.com", 1);
                Assert.False(result);
            }
        }
    }
}