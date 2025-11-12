using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HBDrop.WebApp.Models;

namespace HBDrop.WebApp.Data;

/// <summary>
/// Application database context with Identity and custom entities
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets for custom entities
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Birthday> Birthdays => Set<Birthday>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<WhatsAppSession> WhatsAppSessions => Set<WhatsAppSession>();
    public DbSet<AdditionalBirthday> AdditionalBirthdays => Set<AdditionalBirthday>();
    public DbSet<CustomEvent> CustomEvents => Set<CustomEvent>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure ApplicationUser
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            
            // One-to-one relationship with WhatsAppSession
            entity.HasOne(e => e.WhatsAppSession)
                .WithOne(e => e.User)
                .HasForeignKey<WhatsAppSession>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-many relationship with Contacts
            entity.HasMany(e => e.Contacts)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-many relationship with Messages
            entity.HasMany(e => e.SentMessages)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Contact
        builder.Entity<Contact>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.PhoneNumber }).IsUnique();
            
            // One-to-many relationship with Birthdays
            entity.HasMany(e => e.Birthdays)
                .WithOne(e => e.Contact)
                .HasForeignKey(e => e.ContactId)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-many relationship with AdditionalBirthdays
            entity.HasMany(e => e.AdditionalBirthdays)
                .WithOne(e => e.Contact)
                .HasForeignKey(e => e.ContactId)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-many relationship with Messages
            entity.HasMany(e => e.ReceivedMessages)
                .WithOne(e => e.Contact)
                .HasForeignKey(e => e.ContactId)
                .OnDelete(DeleteBehavior.Restrict); // Don't delete messages when contact is deleted
        });

        // Configure Birthday
        builder.Entity<Birthday>(entity =>
        {
            entity.HasIndex(e => e.ContactId);
            entity.HasIndex(e => e.BirthDate);
            entity.HasIndex(e => new { e.ContactId, e.IsEnabled });
        });

        // Configure AdditionalBirthday
        builder.Entity<AdditionalBirthday>(entity =>
        {
            entity.HasIndex(e => e.ContactId);
            entity.HasIndex(e => new { e.BirthMonth, e.BirthDay });
            entity.HasIndex(e => new { e.ContactId, e.IsEnabled });
            
            // Relationship with SendToGroup (optional group contact)
            entity.HasOne(e => e.SendToGroup)
                .WithMany()
                .HasForeignKey(e => e.SendToGroupId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Message
        builder.Entity<Message>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ContactId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => new { e.UserId, e.Status, e.CreatedAt });
            entity.HasIndex(e => new { e.ContactId, e.CreatedAt });
        });

        // Configure WhatsAppSession
        builder.Entity<WhatsAppSession>(entity =>
        {
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasIndex(e => e.PhoneNumber);
            entity.HasIndex(e => e.IsActive);
        });

        // Configure enum to string conversion for MessageStatus
        builder.Entity<Message>()
            .Property(e => e.Status)
            .HasConversion<string>();
    }

    /// <summary>
    /// Override SaveChangesAsync to automatically update timestamps
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is Contact contact)
            {
                contact.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is Birthday birthday)
            {
                birthday.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is AdditionalBirthday additionalBirthday)
            {
                additionalBirthday.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is WhatsAppSession session)
            {
                session.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
