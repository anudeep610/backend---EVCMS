using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace backend.Models;

public partial class EcmsContext : DbContext
{
    public EcmsContext()
    {
    }

    public EcmsContext(DbContextOptions<EcmsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Billing> Billings { get; set; }

    public virtual DbSet<Chargingstation> Chargingstations { get; set; }

    public virtual DbSet<Reservation> Reservations { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("User ID=postgres;Password=2001;Host=localhost;Port=5432;Database=ECMS;Pooling=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Billing>(entity =>
        {
            entity.HasKey(e => e.Billingid).HasName("billing_pkey");

            entity.ToTable("billing");

            entity.Property(e => e.Billingid)
                .ValueGeneratedNever()
                .HasColumnName("billingid");
            entity.Property(e => e.Billingdate).HasColumnName("billingdate");
            entity.Property(e => e.Paymentstatus)
                .HasMaxLength(20)
                .HasColumnName("paymentstatus");
            entity.Property(e => e.Stationid).HasColumnName("stationid");
            entity.Property(e => e.Totalamount)
                .HasPrecision(10, 2)
                .HasColumnName("totalamount");
            entity.Property(e => e.Userid)
                .HasMaxLength(200)
                .HasColumnName("userid");

            entity.HasOne(d => d.Station).WithMany(p => p.Billings)
                .HasForeignKey(d => d.Stationid)
                .HasConstraintName("billing_stationid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Billings)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("billing_userid_fkey");
        });

        modelBuilder.Entity<Chargingstation>(entity =>
        {
            entity.HasKey(e => e.Stationid).HasName("chargingstation_pkey");

            entity.ToTable("chargingstation");

            entity.Property(e => e.Stationid)
                .ValueGeneratedNever()
                .HasColumnName("stationid");
            entity.Property(e => e.Availability)
                .HasMaxLength(20)
                .HasColumnName("availability");
            entity.Property(e => e.Chargingrate)
                .HasPrecision(10, 2)
                .HasColumnName("chargingrate");
            entity.Property(e => e.Location)
                .HasMaxLength(100)
                .HasColumnName("location");
            entity.Property(e => e.Ports).HasColumnName("ports");
            entity.Property(e => e.Userid)
                .HasMaxLength(200)
                .HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.Chargingstations)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("chargingstation_userid_fkey");
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(e => e.Reservationid).HasName("reservations_pkey");

            entity.ToTable("reservations");

            entity.Property(e => e.Reservationid)
                .ValueGeneratedNever()
                .HasColumnName("reservationid");
            entity.Property(e => e.Endtime).HasColumnName("endtime");
            entity.Property(e => e.Starttime).HasColumnName("starttime");
            entity.Property(e => e.Stationid).HasColumnName("stationid");
            entity.Property(e => e.Userid)
                .HasMaxLength(200)
                .HasColumnName("userid");

            entity.HasOne(d => d.Station).WithMany(p => p.Reservations)
                .HasForeignKey(d => d.Stationid)
                .HasConstraintName("reservations_stationid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Reservations)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("reservations_userid_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Username, "users_username_key").IsUnique();

            entity.Property(e => e.Userid)
                .HasMaxLength(200)
                .HasColumnName("userid");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Passwordhash)
                .HasMaxLength(100)
                .HasColumnName("passwordhash");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasColumnName("role");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
