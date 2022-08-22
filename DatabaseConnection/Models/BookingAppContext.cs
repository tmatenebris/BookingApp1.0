using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Database.Models
{

    public partial class BookingAppContext : DbContext
    {
        public BookingAppContext()
        {
        }

        public BookingAppContext(DbContextOptions<BookingAppContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Booking> Bookings { get; set; } = null!;
        public virtual DbSet<BookingView> Bookingviews { get; set; } = null!;
        public virtual DbSet<Hall> Halls { get; set; } = null!;
        public virtual DbSet<Imagesandesc> Imagesandescs { get; set; } = null!;
        public virtual DbSet<Offer> Offers { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseNpgsql("Host=localhost;Database=BookingApp;Username=postgres;Password=postgres");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.ToTable("bookings");

                entity.Property(e => e.BookingId).HasColumnName("booking_id");

                entity.Property(e => e.FromDate).HasColumnName("from_date");

                entity.Property(e => e.HallId).HasColumnName("hall_id");

                entity.Property(e => e.OwnerId).HasColumnName("owner_id");

                entity.Property(e => e.ToDate).HasColumnName("to_date");

                entity.Property(e => e.TotalPrice).HasColumnName("total_price");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Hall)
                    .WithMany(p => p.Bookings)
                    .HasForeignKey(d => d.HallId)
                    .HasConstraintName("bookings_hall_id_fkey");

                entity.HasOne(d => d.Owner)
                    .WithMany(p => p.BookingOwners)
                    .HasForeignKey(d => d.OwnerId)
                    .HasConstraintName("bookings_owner_id_fkey");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.BookingUsers)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("bookings_user_id_fkey");
            });

            modelBuilder.Entity<BookingView>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("bookingview");

                entity.Property(e => e.FromDate).HasColumnName("from_date");

                entity.Property(e => e.Image).HasColumnName("image");

                entity.Property(e => e.Name)
                    .HasMaxLength(70)
                    .HasColumnName("name");

                entity.Property(e => e.Owner).HasColumnName("owner");

                entity.Property(e => e.OwnerId).HasColumnName("owner_id");

                entity.Property(e => e.ToDate).HasColumnName("to_date");

                entity.Property(e => e.TotalPrice).HasColumnName("total_price");

                entity.Property(e => e.UserId).HasColumnName("user_id");
            });

            modelBuilder.Entity<Hall>(entity =>
            {
                entity.ToTable("halls");

                entity.Property(e => e.HallId).HasColumnName("hall_id");

                entity.Property(e => e.Capacity).HasColumnName("capacity");

                entity.Property(e => e.Image).HasColumnName("image");

                entity.Property(e => e.Location)
                    .HasMaxLength(50)
                    .HasColumnName("location");

                entity.Property(e => e.Name)
                    .HasMaxLength(70)
                    .HasColumnName("name");

                entity.Property(e => e.OwnerId).HasColumnName("owner_id");

                entity.Property(e => e.Price).HasColumnName("price");

                entity.HasOne(d => d.Owner)
                    .WithMany(p => p.Halls)
                    .HasForeignKey(d => d.OwnerId)
                    .HasConstraintName("halls_owner_id_fkey");
            });

            modelBuilder.Entity<Imagesandesc>(entity =>
            {
                entity.HasKey(e => e.ImageId)
                    .HasName("imagesandesc_pkey");

                entity.ToTable("imagesandesc");

                entity.Property(e => e.ImageId).HasColumnName("image_id");

                entity.Property(e => e.Description).HasColumnName("description");

                entity.Property(e => e.HallId).HasColumnName("hall_id");

                entity.Property(e => e.Image).HasColumnName("image");

                entity.HasOne(d => d.Hall)
                    .WithMany(p => p.Imagesandescs)
                    .HasForeignKey(d => d.HallId)
                    .HasConstraintName("imagesandesc_hall_id_fkey");
            });

            modelBuilder.Entity<Offer>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("offer");

                entity.Property(e => e.Capacity).HasColumnName("capacity");

                entity.Property(e => e.Description).HasColumnName("description");

                entity.Property(e => e.Email)
                    .HasMaxLength(50)
                    .HasColumnName("email");

                entity.Property(e => e.FirstName)
                    .HasMaxLength(20)
                    .HasColumnName("first_name");

                entity.Property(e => e.HallId).HasColumnName("hall_id");

                entity.Property(e => e.Image).HasColumnName("image");

                entity.Property(e => e.LastName)
                    .HasMaxLength(40)
                    .HasColumnName("last_name");

                entity.Property(e => e.Location)
                    .HasMaxLength(50)
                    .HasColumnName("location");

                entity.Property(e => e.Name)
                    .HasMaxLength(70)
                    .HasColumnName("name");

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(15)
                    .HasColumnName("phone_number");

                entity.Property(e => e.Price).HasColumnName("price");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.HasIndex(e => e.Username, "users_username_key")
                    .IsUnique();

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.Email)
                    .HasMaxLength(50)
                    .HasColumnName("email");

                entity.Property(e => e.FirstName)
                    .HasMaxLength(20)
                    .HasColumnName("first_name");

                entity.Property(e => e.LastName)
                    .HasMaxLength(40)
                    .HasColumnName("last_name");

                entity.Property(e => e.Password)
                    .HasMaxLength(30)
                    .HasColumnName("password");

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(15)
                    .HasColumnName("phone_number");

                entity.Property(e => e.Role)
                    .HasMaxLength(10)
                    .HasColumnName("role");

                entity.Property(e => e.Username)
                    .HasMaxLength(30)
                    .HasColumnName("username");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}


