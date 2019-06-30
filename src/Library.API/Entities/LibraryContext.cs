using Microsoft.EntityFrameworkCore;

namespace Library.API.Entities
{
    public class LibraryContext : DbContext
    {
        public LibraryContext(DbContextOptions options):base(options){
            //Database.Migrate();
        }

        // Add db tables to the context
        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
    }
}