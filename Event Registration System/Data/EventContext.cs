using Microsoft.EntityFrameworkCore;
using Event_Registration_System.Models;

namespace Event_Registration_System.Data
{
    public class EventContext : DbContext
    {
        public EventContext(DbContextOptions<EventContext> options) : base(options)
        {
        }
        public DbSet<Event> Events { get; set; }
        public DbSet<Registration> Registrations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Registration>()
                .HasOne(r => r.Event)
                .WithMany() // Or .WithMany(e => e.Registrations) if you have a collection in Event
                .HasForeignKey(r => r.EventId);
        }
    }
}
