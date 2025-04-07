using Microsoft.EntityFrameworkCore;

namespace Course.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Course.Models.Course> Courses { get; set; }

    }
}
