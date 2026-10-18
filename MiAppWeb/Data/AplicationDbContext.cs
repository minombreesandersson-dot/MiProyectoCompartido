using Microsoft.EntityFrameworkCore;
using MiAppWeb.Models;

namespace MiAppWeb.Data;

public class AplicationDbContext : DbContext
{
    public AplicationDbContext(DbContextOptions<AplicationDbContext> options) : base(options) { }

    public DbSet<Producto> Productos { get; set; }
}
