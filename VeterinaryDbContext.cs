using Microsoft.EntityFrameworkCore;
using MiProyectoOnion.Domain.Entities;

namespace MiProyectoOnion.Infrastructure.Persistence;

/// <summary>
/// DbContext principal de la veterinaria.
/// Configura las entidades, relaciones y restricciones de la base de datos
/// usando EF Core con Fluent API.
/// </summary>
public class VeterinaryDbContext : DbContext
{
    public VeterinaryDbContext(DbContextOptions<VeterinaryDbContext> options)
        : base(options) { }

    // ── DbSets (tablas de la base de datos) ────────────────────────────────
    public DbSet<Owner> Owners => Set<Owner>();
    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<Veterinarian> Veterinarians => Set<Veterinarian>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Treatment> Treatments => Set<Treatment>();
    public DbSet<Medicine> Medicines => Set<Medicine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Configuración de Owner ─────────────────────────────────────────
        modelBuilder.Entity<Owner>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Name).IsRequired().HasMaxLength(150);
            entity.Property(o => o.Document).IsRequired().HasMaxLength(20);
            entity.Property(o => o.Email).IsRequired().HasMaxLength(100);
            entity.Property(o => o.Phone).HasMaxLength(20);

            // Índices únicos para documento y email (regla de negocio)
            entity.HasIndex(o => o.Document).IsUnique();
            entity.HasIndex(o => o.Email).IsUnique();

            // Relación: un propietario tiene muchas mascotas
            entity.HasMany(o => o.Pets)
                  .WithOne(p => p.Owner)
                  .HasForeignKey(p => p.OwnerId)
                  .OnDelete(DeleteBehavior.Restrict); // No borrar mascotas en cascada
        });

        // ── Configuración de Pet ───────────────────────────────────────────
        modelBuilder.Entity<Pet>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Species).IsRequired().HasMaxLength(50);
            entity.Property(p => p.Weight).HasPrecision(5, 2); // Ej: 999.99 kg

            // Relación: una mascota tiene muchas citas
            entity.HasMany(p => p.Appointments)
                  .WithOne(a => a.Pet)
                  .HasForeignKey(a => a.PetId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Configuración de Veterinarian ──────────────────────────────────
        modelBuilder.Entity<Veterinarian>(entity =>
        {
            entity.HasKey(v => v.Id);
            entity.Property(v => v.Name).IsRequired().HasMaxLength(150);
            entity.Property(v => v.Document).IsRequired().HasMaxLength(20);

            // Relación: un veterinario tiene muchas citas
            entity.HasMany(v => v.Appointments)
                  .WithOne(a => a.Veterinarian)
                  .HasForeignKey(a => a.VeterinarianId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Configuración de Appointment ───────────────────────────────────
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.AppointmentDate).IsRequired();

            // Relación 1:1 con Treatment (opcional)
            entity.HasOne(a => a.Treatment)
                  .WithOne(t => t.Appointment)
                  .HasForeignKey<Treatment>(t => t.AppointmentId)
                  .OnDelete(DeleteBehavior.Cascade); // Si se borra la cita, se borra el tratamiento
        });

        // ── Configuración de Treatment ─────────────────────────────────────
        modelBuilder.Entity<Treatment>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Diagnosis).IsRequired().HasMaxLength(500);
            entity.Property(t => t.Observations).HasMaxLength(1000);
            entity.Property(t => t.Dose).HasMaxLength(100);
            entity.Property(t => t.Frequency).HasMaxLength(100);

            // Relación con Medicine
            entity.HasOne(t => t.Medicine)
                  .WithMany(m => m.Treatments)
                  .HasForeignKey(t => t.MedicineId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Configuración de Medicine ──────────────────────────────────────
        modelBuilder.Entity<Medicine>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Name).IsRequired().HasMaxLength(150);
            entity.Property(m => m.Description).HasMaxLength(500);
        });

        // ── Datos semilla para pruebas ─────────────────────────────────────
        SeedData(modelBuilder);
    }

    /// <summary>
    /// Datos iniciales (seed) para probar el sistema sin necesidad de
    /// insertar registros manualmente.
    /// </summary>
    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Propietarios de prueba
        modelBuilder.Entity<Owner>().HasData(
            new Owner { Id = 1, Name = "Carlos Ramírez", Document = "1234567890", Email = "carlos@email.com", Phone = "3001234567" },
            new Owner { Id = 2, Name = "María López",   Document = "0987654321", Email = "maria@email.com",  Phone = "3109876543" }
        );

        // Mascotas de prueba
        modelBuilder.Entity<Pet>().HasData(
            new Pet { Id = 1, Name = "Rex",   Species = "Perro", Race = Domain.Enums.PetRace.Labrador,     Age = 3, Weight = 25.5m, OwnerId = 1 },
            new Pet { Id = 2, Name = "Michi", Species = "Gato",  Race = Domain.Enums.PetRace.GatoSiames,   Age = 2, Weight = 4.2m,  OwnerId = 2 }
        );

        // Veterinarios de prueba
        modelBuilder.Entity<Veterinarian>().HasData(
            new Veterinarian { Id = 1, Name = "Dr. Andrés Torres",  Document = "1111111111", Speciality = Domain.Enums.Speciality.MedicinaGeneral, Schedule = Domain.Enums.ScheduleTime.Manana,   IsActive = true },
            new Veterinarian { Id = 2, Name = "Dra. Laura Herrera", Document = "2222222222", Speciality = Domain.Enums.Speciality.Cirugia,          Schedule = Domain.Enums.ScheduleTime.Tarde,    IsActive = true }
        );

        // Medicamentos de prueba
        modelBuilder.Entity<Medicine>().HasData(
            new Medicine { Id = 1, Name = "Amoxicilina 500mg", Description = "Antibiótico de amplio espectro",          IsAvailable = true, Stock = 50 },
            new Medicine { Id = 2, Name = "Ivermectina 1%",    Description = "Antiparasitario externo e interno",       IsAvailable = true, Stock = 30 },
            new Medicine { Id = 3, Name = "Meloxicam 1.5mg",   Description = "Antiinflamatorio y analgésico",           IsAvailable = true, Stock = 40 }
        );
    }
}
