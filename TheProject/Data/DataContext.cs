global using Microsoft.EntityFrameworkCore;
global using TheProject.Models;
using TheProject.Services;

namespace TheProject.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }


        public DbSet<Users> users { get; set; }

    }

}
