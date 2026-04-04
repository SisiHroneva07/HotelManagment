using Hotel.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Data;

/// <summary>
/// EF Core context for Identity, rooms, guests, and reservations.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Client> Clients => Set<Client>();

    public DbSet<Room> Rooms => Set<Room>();

    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Client>(e =>
        {
            e.Property(c => c.FirstName).HasMaxLength(100).IsRequired();
            e.Property(c => c.LastName).HasMaxLength(100).IsRequired();
            e.Property(c => c.PhoneNumber).HasMaxLength(10).IsRequired();
            e.Property(c => c.Email).HasMaxLength(256).IsRequired();
        });

        builder.Entity<Room>(e =>
        {
            e.Property(r => r.RoomNumber).HasMaxLength(20).IsRequired();
            e.HasIndex(r => r.RoomNumber).IsUnique();
            e.Property(r => r.PriceAdult).HasPrecision(18, 2);
            e.Property(r => r.PriceChild).HasPrecision(18, 2);
        });

        builder.Entity<Reservation>(e =>
        {
            e.Property(r => r.TotalAmount).HasPrecision(18, 2);
            e.HasOne(r => r.Room)
                .WithMany(room => room.Reservations)
                .HasForeignKey(r => r.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(r => r.User)
                .WithMany(u => u.ReservationsCreated)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(r => r.Clients)
                .WithMany(c => c.Reservations);
        });

        builder.Entity<ApplicationUser>(e =>
        {
            e.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            e.Property(u => u.MiddleName).HasMaxLength(100);
            e.Property(u => u.LastName).HasMaxLength(100).IsRequired();
            e.Property(u => u.EGN).HasMaxLength(10).IsRequired();
            e.Property(u => u.PhoneNumber).HasMaxLength(10);
        });
    }
}
