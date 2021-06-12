using Microsoft.EntityFrameworkCore;

namespace RATClassLibrary
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; }


        public ApplicationContext()
        {
            //
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=127.0.0.1;Port=5432;Database=RATbd;Username=RAT;Password=RAT");
        }
    }
}