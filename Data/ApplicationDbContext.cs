using ClientApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ClientApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>().ToTable("clientes");

            modelBuilder.Entity<Client>(entity =>
            {
                entity.Property(e => e.ClientId).HasColumnName("id");
                entity.Property(e => e.FirstName).HasColumnName("nombre");
                entity.Property(e => e.LastName).HasColumnName("apellido");
                entity.Property(e => e.CorporateName).HasColumnName("razon_social");
                entity.Property(e => e.CUIT).HasColumnName("cuit");
                entity.Property(e => e.Birthdate).HasColumnName("fecha_nacimiento").HasColumnType("date");
                entity.Property(e => e.CellPhone).HasColumnName("telefono_celular");
                entity.Property(e => e.Email).HasColumnName("email");
            });
        }
    }
}