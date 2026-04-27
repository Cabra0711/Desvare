using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MiProyectoOnion.Application.DTOs;
using MiProyectoOnion.Application.Interfaces;
using MiProyectoOnion.Application.Services;
using MiProyectoOnion.Domain.Enums;
using MiProyectoOnion.Domain.Interfaces;
using MiProyectoOnion.Infrastructure.Email;
using MiProyectoOnion.Infrastructure.Persistence;
using MiProyectoOnion.Infrastructure.Repositories;

// ══════════════════════════════════════════════════════
// CONFIGURACIÓN DE INYECCIÓN DE DEPENDENCIAS
// ══════════════════════════════════════════════════════

/// <summary>
/// Punto de entrada del sistema de veterinaria.
/// Configura el contenedor de DI, aplica migraciones automáticamente
/// y muestra el menú principal de consola.
/// </summary>

var services = new ServiceCollection();

// Configurar EF Core con SQLite (el archivo veterinary.db se crea automáticamente)
services.AddDbContext<VeterinaryDbContext>(options =>
    options.UseSqlite("Data Source=veterinary.db"));

// Registrar repositorios (Infrastructure → Domain)
services.AddScoped<IOwnerRepository, OwnerRepository>();
services.AddScoped<IPetRepository, PetRepository>();
services.AddScoped<IVeterinarianRepository, VeterinarianRepository>();
services.AddScoped<IAppointmentRepository, AppointmentRepository>();
services.AddScoped<ITreatmentRepository, TreatmentRepository>();
services.AddScoped<IMedicineRepository, MedicineRepository>();

// Registrar servicios (Application → Domain)
services.AddScoped<IOwnerService, OwnerService>();
services.AddScoped<IPetService, PetService>();
services.AddScoped<IVeterinarianService, VeterinarianService>();
services.AddScoped<IAppointmentService, AppointmentService>();
services.AddScoped<ITreatmentService, TreatmentService>();
services.AddScoped<IMedicineService, MedicineService>();
services.AddScoped<IHistoryService, HistoryService>();
services.AddScoped<IReportService, ReportService>();

// Registrar servicio de email
services.AddScoped<IEmailService, EmailService>();

var serviceProvider = services.BuildServiceProvider();

// Aplicar migraciones y crear la base de datos automáticamente al iniciar
using (var scope = serviceProvider.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VeterinaryDbContext>();
    db.Database.EnsureCreated(); // Crea el schema si no existe (incluye datos semilla)
    Console.WriteLine("✅ Base de datos inicializada correctamente.\n");
}

// ══════════════════════════════════════════════════════
// MENÚ PRINCIPAL
// ══════════════════════════════════════════════════════

bool exit = false;

while (!exit)
{
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("╔══════════════════════════════════════════════╗");
    Console.WriteLine("║       SISTEMA DE GESTIÓN VETERINARIA         ║");
    Console.WriteLine("╠══════════════════════════════════════════════╣");
    Console.ResetColor();
    Console.WriteLine("║  1. Gestión de Propietarios                  ║");
    Console.WriteLine("║  2. Gestión de Mascotas                      ║");
    Console.WriteLine("║  3. Gestión de Veterinarios                  ║");
    Console.WriteLine("║  4. Gestión de Citas Médicas                 ║");
    Console.WriteLine("║  5. Gestión de Medicamentos                  ║");
    Console.WriteLine("║  6. Registrar Tratamiento                    ║");
    Console.WriteLine("║  7. Historial Clínico de Mascota             ║");
    Console.WriteLine("║  8. Reportes del Sistema                     ║");
    Console.WriteLine("║  0. Salir                                    ║");
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("╚══════════════════════════════════════════════╝");
    Console.ResetColor();
    Console.Write("\n  Elige una opción: ");

    var option = Console.ReadLine()?.Trim();

    using var scope = serviceProvider.CreateScope();
    var sp = scope.ServiceProvider;

    try
    {
        switch (option)
        {
            case "1": await MenuOwners(sp); break;
            case "2": await MenuPets(sp); break;
            case "3": await MenuVeterinarians(sp); break;
            case "4": await MenuAppointments(sp); break;
            case "5": await MenuMedicines(sp); break;
            case "6": await MenuTreatments(sp); break;
            case "7": await MenuHistory(sp); break;
            case "8": await MenuReports(sp); break;
            case "0": exit = true; Console.WriteLine("\n👋 ¡Hasta luego!"); break;
            default:  ShowError("Opción no válida."); break;
        }
    }
    catch (Exception ex)
    {
        ShowError($"Error: {ex.Message}");
    }

    if (!exit)
    {
        Console.WriteLine("\nPresiona cualquier tecla para continuar...");
        Console.ReadKey();
    }
}

// ══════════════════════════════════════════════════════
// MENÚ: PROPIETARIOS
// ══════════════════════════════════════════════════════

async Task MenuOwners(IServiceProvider sp)
{
    var service = sp.GetRequiredService<IOwnerService>();

    ShowHeader("PROPIETARIOS");
    Console.WriteLine("  1. Listar todos");
    Console.WriteLine("  2. Registrar nuevo");
    Console.WriteLine("  3. Actualizar");
    Console.WriteLine("  4. Eliminar");
    Console.Write("\n  Opción: ");

    switch (Console.ReadLine()?.Trim())
    {
        case "1":
            var owners = await service.GetAllAsync();
            if (!owners.Any()) { ShowInfo("No hay propietarios registrados."); break; }
            Console.WriteLine($"\n{"ID",-5} {"Nombre",-25} {"Email",-30} {"Teléfono",-15} {"Mascotas",-10} {"Bloqueado"}");
            Console.WriteLine(new string('─', 95));
            foreach (var o in owners)
                Console.WriteLine($"{o.Id,-5} {o.Name,-25} {o.Email,-30} {o.Phone,-15} {o.TotalPets,-10} {(o.IsBlocked ? $"Sí (hasta {o.BlockedUntil:dd/MM/yyyy})" : "No")}");
            break;

        case "2":
            Console.Write("  Nombre:    "); var oName = Console.ReadLine()!;
            Console.Write("  Documento: "); var oDoc  = Console.ReadLine()!;
            Console.Write("  Email:     "); var oEmail = Console.ReadLine()!;
            Console.Write("  Teléfono:  "); var oPhone = Console.ReadLine()!;
            var created = await service.CreateAsync(new OwnerCreateDto { Name=oName, Document=oDoc, Email=oEmail, Phone=oPhone });
            ShowSuccess($"Propietario '{created.Name}' registrado con Id {created.Id}.");
            break;

        case "3":
            Console.Write("  Id del propietario a actualizar: ");
            if (!int.TryParse(Console.ReadLine(), out var updId)) { ShowError("Id inválido."); break; }
            Console.Write("  Nuevo nombre:    "); var uName  = Console.ReadLine()!;
            Console.Write("  Nuevo documento: "); var uDoc   = Console.ReadLine()!;
            Console.Write("  Nuevo email:     "); var uEmail = Console.ReadLine()!;
            Console.Write("  Nuevo teléfono:  "); var uPhone = Console.ReadLine()!;
            var updated = await service.UpdateAsync(updId, new OwnerCreateDto { Name=uName, Document=uDoc, Email=uEmail, Phone=uPhone });
            ShowSuccess($"Propietario '{updated.Name}' actualizado.");
            break;

        case "4":
            Console.Write("  Id del propietario a eliminar: ");
            if (!int.TryParse(Console.ReadLine(), out var delId)) { ShowError("Id inválido."); break; }
            await service.DeleteAsync(delId);
            ShowSuccess("Propietario eliminado.");
            break;

        default: ShowError("Opción no válida."); break;
    }
}

// ══════════════════════════════════════════════════════
// MENÚ: MASCOTAS
// ══════════════════════════════════════════════════════

async Task MenuPets(IServiceProvider sp)
{
    var service = sp.GetRequiredService<IPetService>();

    ShowHeader("MASCOTAS");
    Console.WriteLine("  1. Listar todas");
    Console.WriteLine("  2. Listar por propietario");
    Console.WriteLine("  3. Registrar nueva");
    Console.WriteLine("  4. Eliminar");
    Console.Write("\n  Opción: ");

    switch (Console.ReadLine()?.Trim())
    {
        case "1":
            var pets = await service.GetAllAsync();
            if (!pets.Any()) { ShowInfo("No hay mascotas registradas."); break; }
            Console.WriteLine($"\n{"ID",-5} {"Nombre",-15} {"Especie",-12} {"Raza",-20} {"Edad",-6} {"Peso",-8} {"Propietario"}");
            Console.WriteLine(new string('─', 85));
            foreach (var p in pets)
                Console.WriteLine($"{p.Id,-5} {p.Name,-15} {p.Species,-12} {p.Race,-20} {p.Age,-6} {p.Weight,-8} {p.OwnerName}");
            break;

        case "2":
            Console.Write("  Id del propietario: ");
            if (!int.TryParse(Console.ReadLine(), out var oid)) { ShowError("Id inválido."); break; }
            var ownerPets = await service.GetByOwnerAsync(oid);
            foreach (var p in ownerPets)
                Console.WriteLine($"  [{p.Id}] {p.Name} - {p.Species} ({p.Race}) - {p.Age} años - {p.Weight}kg");
            break;

        case "3":
            Console.Write("  Nombre:           "); var pName    = Console.ReadLine()!;
            Console.Write("  Especie:          "); var pSpecies = Console.ReadLine()!;
            Console.WriteLine("  Razas: " + string.Join(", ", Enum.GetNames<PetRace>()));
            Console.Write("  Raza (número):    "); 
            if (!Enum.TryParse<PetRace>(Console.ReadLine(), out var race)) { ShowError("Raza inválida."); break; }
            Console.Write("  Edad (años):      "); if (!int.TryParse(Console.ReadLine(), out var age)) { ShowError("Edad inválida."); break; }
            Console.Write("  Peso (kg):        "); if (!decimal.TryParse(Console.ReadLine(), out var weight)) { ShowError("Peso inválido."); break; }
            Console.Write("  Id del propietario: "); if (!int.TryParse(Console.ReadLine(), out var owId)) { ShowError("Id inválido."); break; }
            var newPet = await service.CreateAsync(new PetCreateDto { Name=pName, Species=pSpecies, Race=race, Age=age, Weight=weight, OwnerId=owId });
            ShowSuccess($"Mascota '{newPet.Name}' registrada con Id {newPet.Id}.");
            break;

        case "4":
            Console.Write("  Id de la mascota a eliminar: ");
            if (!int.TryParse(Console.ReadLine(), out var delPetId)) { ShowError("Id inválido."); break; }
            await service.DeleteAsync(delPetId);
            ShowSuccess("Mascota eliminada.");
            break;

        default: ShowError("Opción no válida."); break;
    }
}

// ══════════════════════════════════════════════════════
// MENÚ: VETERINARIOS
// ══════════════════════════════════════════════════════

async Task MenuVeterinarians(IServiceProvider sp)
{
    var service = sp.GetRequiredService<IVeterinarianService>();

    ShowHeader("VETERINARIOS");
    Console.WriteLine("  1. Listar todos");
    Console.WriteLine("  2. Registrar nuevo");
    Console.WriteLine("  3. Desactivar");
    Console.Write("\n  Opción: ");

    switch (Console.ReadLine()?.Trim())
    {
        case "1":
            var vets = await service.GetAllAsync();
            if (!vets.Any()) { ShowInfo("No hay veterinarios registrados."); break; }
            Console.WriteLine($"\n{"ID",-5} {"Nombre",-25} {"Documento",-15} {"Especialidad",-20} {"Horario",-12} {"Activo"}");
            Console.WriteLine(new string('─', 90));
            foreach (var v in vets)
                Console.WriteLine($"{v.Id,-5} {v.Name,-25} {v.Document,-15} {v.Speciality,-20} {v.Schedule,-12} {(v.IsActive ? "Sí" : "No")}");
            break;

        case "2":
            Console.Write("  Nombre:     "); var vName = Console.ReadLine()!;
            Console.Write("  Documento:  "); var vDoc  = Console.ReadLine()!;
            Console.WriteLine("  Especialidades: " + string.Join(", ", Enum.GetNames<Speciality>()));
            Console.Write("  Especialidad:  ");
            if (!Enum.TryParse<Speciality>(Console.ReadLine(), out var spec)) { ShowError("Especialidad inválida."); break; }
            Console.WriteLine("  Horarios: " + string.Join(", ", Enum.GetNames<ScheduleTime>()));
            Console.Write("  Horario:      ");
            if (!Enum.TryParse<ScheduleTime>(Console.ReadLine(), out var sched)) { ShowError("Horario inválido."); break; }
            var newVet = await service.CreateAsync(new VeterinarianCreateDto { Name=vName, Document=vDoc, Speciality=spec, Schedule=sched });
            ShowSuccess($"Veterinario '{newVet.Name}' registrado con Id {newVet.Id}.");
            break;

        case "3":
            Console.Write("  Id del veterinario a desactivar: ");
            if (!int.TryParse(Console.ReadLine(), out var deacId)) { ShowError("Id inválido."); break; }
            await service.DeactivateAsync(deacId);
            ShowSuccess("Veterinario desactivado.");
            break;

        default: ShowError("Opción no válida."); break;
    }
}

// ══════════════════════════════════════════════════════
// MENÚ: CITAS MÉDICAS
// ══════════════════════════════════════════════════════

async Task MenuAppointments(IServiceProvider sp)
{
    var service = sp.GetRequiredService<IAppointmentService>();

    ShowHeader("CITAS MÉDICAS");
    Console.WriteLine("  1. Listar todas");
    Console.WriteLine("  2. Agendar nueva cita");
    Console.WriteLine("  3. Marcar como Atendida");
    Console.WriteLine("  4. Marcar como No Asistió");
    Console.WriteLine("  5. Cancelar cita");
    Console.Write("\n  Opción: ");

    switch (Console.ReadLine()?.Trim())
    {
        case "1":
            var appts = await service.GetAllAsync();
            if (!appts.Any()) { ShowInfo("No hay citas registradas."); break; }
            Console.WriteLine($"\n{"ID",-5} {"Mascota",-15} {"Veterinario",-25} {"Fecha",-13} {"Inicio",-8} {"Fin",-8} {"Estado"}");
            Console.WriteLine(new string('─', 95));
            foreach (var a in appts)
                Console.WriteLine($"{a.Id,-5} {a.PetName,-15} {a.VeterinarianName,-25} {a.AppointmentDate:dd/MM/yyyy,-13} {a.StartTime:HH:mm,-8} {a.EndTime:HH:mm,-8} {a.Status}");
            break;

        case "2":
            Console.Write("  Id de la mascota:       "); if (!int.TryParse(Console.ReadLine(), out var petId)) { ShowError("Id inválido."); break; }
            Console.Write("  Id del veterinario:     "); if (!int.TryParse(Console.ReadLine(), out var vetId)) { ShowError("Id inválido."); break; }
            Console.Write("  Fecha (yyyy-MM-dd):     "); if (!DateTime.TryParse(Console.ReadLine(), out var date)) { ShowError("Fecha inválida."); break; }
            Console.Write("  Hora inicio (HH:mm):    "); if (!TimeOnly.TryParse(Console.ReadLine(), out var start)) { ShowError("Hora inválida."); break; }
            Console.Write("  Hora fin    (HH:mm):    "); if (!TimeOnly.TryParse(Console.ReadLine(), out var end)) { ShowError("Hora inválida."); break; }
            var newAppt = await service.CreateAsync(new AppointmentCreateDto { PetId=petId, VeterinarianId=vetId, AppointmentDate=date, StartTime=start, EndTime=end });
            ShowSuccess($"Cita #{newAppt.Id} agendada para '{newAppt.PetName}' el {newAppt.AppointmentDate:dd/MM/yyyy} a las {newAppt.StartTime:HH:mm}.");
            break;

        case "3":
            Console.Write("  Id de la cita: ");
            if (!int.TryParse(Console.ReadLine(), out var attendId)) { ShowError("Id inválido."); break; }
            await service.MarkAsAttendedAsync(attendId);
            ShowSuccess($"Cita #{attendId} marcada como Atendida.");
            break;

        case "4":
            Console.Write("  Id de la cita: ");
            if (!int.TryParse(Console.ReadLine(), out var noShowId)) { ShowError("Id inválido."); break; }
            await service.MarkAsNoShowAsync(noShowId);
            ShowSuccess($"Cita #{noShowId} marcada como No Asistió.");
            break;

        case "5":
            Console.Write("  Id de la cita a cancelar: ");
            if (!int.TryParse(Console.ReadLine(), out var cancelId)) { ShowError("Id inválido."); break; }
            await service.CancelAsync(cancelId);
            ShowSuccess($"Cita #{cancelId} cancelada.");
            break;

        default: ShowError("Opción no válida."); break;
    }
}

// ══════════════════════════════════════════════════════
// MENÚ: MEDICAMENTOS
// ══════════════════════════════════════════════════════

async Task MenuMedicines(IServiceProvider sp)
{
    var service = sp.GetRequiredService<IMedicineService>();

    ShowHeader("MEDICAMENTOS");
    Console.WriteLine("  1. Listar todos");
    Console.WriteLine("  2. Listar disponibles");
    Console.WriteLine("  3. Registrar nuevo");
    Console.Write("\n  Opción: ");

    switch (Console.ReadLine()?.Trim())
    {
        case "1":
            var all = await service.GetAllAsync();
            PrintMedicines(all);
            break;

        case "2":
            var available = await service.GetAvailableAsync();
            PrintMedicines(available);
            break;

        case "3":
            Console.Write("  Nombre:      "); var mName = Console.ReadLine()!;
            Console.Write("  Descripción: "); var mDesc = Console.ReadLine()!;
            Console.Write("  Stock:       "); if (!int.TryParse(Console.ReadLine(), out var mStock)) { ShowError("Stock inválido."); break; }
            var med = await service.CreateAsync(new MedicineCreateDto { Name=mName, Description=mDesc, Stock=mStock });
            ShowSuccess($"Medicamento '{med.Name}' registrado con Id {med.Id}.");
            break;

        default: ShowError("Opción no válida."); break;
    }

    void PrintMedicines(IEnumerable<MedicineResponseDto> medicines)
    {
        if (!medicines.Any()) { ShowInfo("No hay medicamentos."); return; }
        Console.WriteLine($"\n{"ID",-5} {"Nombre",-25} {"Descripción",-40} {"Stock",-8} {"Disponible"}");
        Console.WriteLine(new string('─', 90));
        foreach (var m in medicines)
            Console.WriteLine($"{m.Id,-5} {m.Name,-25} {m.Description,-40} {m.Stock,-8} {(m.IsAvailable ? "Sí" : "No")}");
    }
}

// ══════════════════════════════════════════════════════
// MENÚ: TRATAMIENTOS
// ══════════════════════════════════════════════════════

async Task MenuTreatments(IServiceProvider sp)
{
    var service = sp.GetRequiredService<ITreatmentService>();

    ShowHeader("REGISTRAR TRATAMIENTO");
    Console.WriteLine("  (Solo en citas con estado: Atendida)\n");

    Console.Write("  Id de la cita atendida: ");
    if (!int.TryParse(Console.ReadLine(), out var apptId)) { ShowError("Id inválido."); return; }

    Console.Write("  Id del medicamento:    ");
    if (!int.TryParse(Console.ReadLine(), out var medId)) { ShowError("Id inválido."); return; }

    Console.Write("  Diagnóstico:          "); var diag = Console.ReadLine()!;
    Console.Write("  Observaciones:        "); var obs  = Console.ReadLine()!;
    Console.Write("  Dosis (ej: 5mg):      "); var dose = Console.ReadLine()!;
    Console.Write("  Frecuencia:           "); var freq = Console.ReadLine()!;

    var treatment = await service.CreateAsync(new TreatmentCreateDto
    {
        AppointmentId = apptId,
        MedicineId = medId,
        Diagnosis = diag,
        Observations = obs,
        Dose = dose,
        Frequency = freq
    });

    ShowSuccess($"Tratamiento registrado. Medicamento: {treatment.MedicineName}. Diagnóstico: {treatment.Diagnosis}.");
}

// ══════════════════════════════════════════════════════
// MENÚ: HISTORIAL CLÍNICO
// ══════════════════════════════════════════════════════

async Task MenuHistory(IServiceProvider sp)
{
    var service = sp.GetRequiredService<IHistoryService>();

    ShowHeader("HISTORIAL CLÍNICO");
    Console.Write("  Id de la mascota: ");
    if (!int.TryParse(Console.ReadLine(), out var petId)) { ShowError("Id inválido."); return; }

    var history = await service.GetPetHistoryAsync(petId);
    if (history is null) { ShowError($"No se encontró la mascota con Id {petId}."); return; }

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\n  🐾 {history.PetName} | {history.Species} - {history.Race} | {history.Age} años | {history.Weight}kg");
    Console.WriteLine($"  👤 Propietario: {history.OwnerName}");
    Console.ResetColor();
    Console.WriteLine($"\n  Total de citas: {history.Appointments.Count}");
    Console.WriteLine(new string('─', 80));

    if (!history.Appointments.Any())
    {
        ShowInfo("Esta mascota no tiene citas registradas.");
        return;
    }

    foreach (var a in history.Appointments)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n  📅 Cita #{a.AppointmentId} - {a.Date:dd/MM/yyyy} | Estado: {a.Status}");
        Console.ResetColor();
        Console.WriteLine($"     Veterinario: {a.VeterinarianName}");

        if (!string.IsNullOrEmpty(a.Diagnosis))
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"     Diagnóstico: {a.Diagnosis}");
            Console.WriteLine($"     Observaciones: {a.TreatmentObservations}");
            Console.WriteLine($"     Medicamento: {a.MedicineName}");
            Console.ResetColor();
        }
    }
}

// ══════════════════════════════════════════════════════
// MENÚ: REPORTES
// ══════════════════════════════════════════════════════

async Task MenuReports(IServiceProvider sp)
{
    var service = sp.GetRequiredService<IReportService>();

    ShowHeader("REPORTES DEL SISTEMA");

    // Reporte 1: Veterinario con más citas
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("  1️⃣  VETERINARIO CON MÁS CITAS ATENDIDAS");
    Console.ResetColor();
    var topVet = await service.GetTopVeterinarianAsync();
    if (topVet is null) Console.WriteLine("     Sin datos.");
    else Console.WriteLine($"     {topVet.Name} ({topVet.Speciality}): {topVet.TotalAppointments} citas");

    // Reporte 2: Mascotas más atendidas
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("\n  2️⃣  MASCOTAS MÁS ATENDIDAS (Top 5)");
    Console.ResetColor();
    var pets = await service.GetMostAttendedPetsAsync();
    if (!pets.Any()) Console.WriteLine("     Sin datos.");
    foreach (var p in pets)
        Console.WriteLine($"     {p.PetName} (dueño: {p.OwnerName}): {p.TotalAttended} atenciones");

    // Reporte 3: Medicamentos más usados
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("\n  3️⃣  MEDICAMENTOS MÁS UTILIZADOS (Top 5)");
    Console.ResetColor();
    var meds = await service.GetTopMedicinesAsync();
    if (!meds.Any()) Console.WriteLine("     Sin datos.");
    foreach (var m in meds)
        Console.WriteLine($"     {m.MedicineName}: {m.TimesUsed} usos");

    // Reporte 4: Tasa de inasistencia
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("\n  4️⃣  TASA DE INASISTENCIA");
    Console.ResetColor();
    var rate = await service.GetAbsenceRateAsync();
    Console.WriteLine($"     Total citas: {rate.TotalAppointments}");
    Console.WriteLine($"     Inasistencias: {rate.NoShowCount}");
    Console.ForegroundColor = rate.AbsenceRatePercent > 30 ? ConsoleColor.Red : ConsoleColor.Green;
    Console.WriteLine($"     Tasa: {rate.AbsenceRatePercent}%");
    Console.ResetColor();
}

// ══════════════════════════════════════════════════════
// UTILIDADES DE CONSOLA
// ══════════════════════════════════════════════════════

/// <summary>Muestra un encabezado de sección con color.</summary>
void ShowHeader(string title)
{
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"\n═══════════════════════════════════════");
    Console.WriteLine($"   {title}");
    Console.WriteLine($"═══════════════════════════════════════\n");
    Console.ResetColor();
}

/// <summary>Muestra un mensaje de éxito en verde.</summary>
void ShowSuccess(string msg)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\n  ✅ {msg}");
    Console.ResetColor();
}

/// <summary>Muestra un mensaje de error en rojo.</summary>
void ShowError(string msg)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n  ❌ {msg}");
    Console.ResetColor();
}

/// <summary>Muestra un mensaje informativo en amarillo.</summary>
void ShowInfo(string msg)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"\n  ℹ️  {msg}");
    Console.ResetColor();
}
