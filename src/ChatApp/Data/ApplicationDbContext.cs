using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<ChatRoom> ChatRooms { get; set; }

        public DbSet<ChatRoomMember> ChatRoomMembers { get; set; }

        public DbSet<ChatMessage> ChatMessages { get; set; }

        public DbSet<ChatRoomAvatar> ChatRoomAvatars { get; set; }

        public DbSet<UserAvatar> UserAvatars { get; set; }

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}
