using LibraryAdministration.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryAdministration.Database
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
            
        }

        public DbSet<Book> Book => Set<Book>();
        public DbSet<Reader> Reader => Set<Reader>();
        public DbSet<LibraryAdministrator> LibraryAdministrator => Set<LibraryAdministrator>();
    }
}
