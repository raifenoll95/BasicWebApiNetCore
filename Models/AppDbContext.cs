using Microsoft.EntityFrameworkCore;
using progressApp.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Define las tablas de tu base de datos (estas son las entidades)
    public DbSet<User> Users { get; set; }

    // Puedes agregar más tablas aquí según lo necesites
}
