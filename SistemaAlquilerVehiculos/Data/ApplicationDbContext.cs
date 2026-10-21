using Microsoft.EntityFrameworkCore;
using SistemaAlquilerVehiculos.Models;

namespace SistemaAlquilerVehiculos.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Marca> Marcas { get; set; }
        public DbSet<ModeloVehiculo> Modelos { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Alquiler> Alquileres { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.Documento)
                .IsUnique();

            modelBuilder.Entity<ModeloVehiculo>()
                .Property(m => m.PrecioDia)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Alquiler>()
                .Property(a => a.PrecioDiaAplicado)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Alquiler>()
                .Property(a => a.DescuentoPorcentaje)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Alquiler>()
                .Property(a => a.CostoTotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ModeloVehiculo>()
                .HasOne(m => m.Marca)
                .WithMany(b => b.Modelos)
                .HasForeignKey(m => m.MarcaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Alquiler>()
                .HasOne(a => a.ModeloVehiculo)
                .WithMany(m => m.Alquileres)
                .HasForeignKey(a => a.ModeloVehiculoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Alquiler>()
                .HasOne(a => a.Cliente)
                .WithMany(c => c.Alquileres)
                .HasForeignKey(a => a.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}