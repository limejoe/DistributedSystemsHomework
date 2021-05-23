using Microsoft.EntityFrameworkCore;

namespace AuthorsService.Models
{
    public class AuthorContext : DbContext
    {
        public AuthorContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Author> Authors { get; set; }
    }
}
