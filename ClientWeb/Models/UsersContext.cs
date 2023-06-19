using Microsoft.EntityFrameworkCore;

namespace ClientWeb.Models
{
    public partial class UsersContext : DbContext
    {
        public UsersContext() { }
        public UsersContext(DbContextOptions<UsersContext> options) : base(options) { }

        public virtual DbSet<User> Users { get; set; }

        // public virtual DbSet<Teacher> Teachers { get; set; }
        // public virtual DbSet<Student> Students { get; set; }
    }
}
