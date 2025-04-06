using Microsoft.EntityFrameworkCore;
namespace Section.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Section.Models.Section> Sections { get; set; }

    }
}
