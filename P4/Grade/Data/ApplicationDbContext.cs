using Microsoft.EntityFrameworkCore;
namespace Grade.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Grade.Models.Grade> Grades { get; set; }

    }
}
