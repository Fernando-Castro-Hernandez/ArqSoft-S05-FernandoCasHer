using Citas_App.Interfaces;
using Citas_App.Repositories;
using CitasApp.Infrastructure.Repositories;
using Citas_App.Services;
using Citas_App.Observers;
using Citas_App.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ── Carpeta de datos (para los CSV de Médico y Cita) ─────────────────────────
var dataFolder = Path.Combine(builder.Environment.WebRootPath, "data");
Directory.CreateDirectory(dataFolder);

var csvMedicos = Path.Combine(dataFolder, "medicos.csv");
var csvCitas = Path.Combine(dataFolder, "citas.csv");

// ── Médico y Cita: adapters CSV (práctica anterior) ──────────────────────────
builder.Services.AddScoped<IMedicoRepository>(_ => new CsvMedicoRepository(csvMedicos));
builder.Services.AddScoped<ICitaRepository>(_ => new CsvCitaRepository(csvCitas));

// ── Paciente: Factory + Decorator (práctica #26) ─────────────────────────────
// El Factory decide qué repositorio crear según el entorno.
// El Decorator lo envuelve para agregar logging sin tocar el repo original.
builder.Services.AddScoped<IPacienteRepository>(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    var repo = RepositoryFactory.CrearPacienteRepository(env.EnvironmentName, env); // ← Factory decide cuál
    return new LoggingPacienteRepository(repo);                                     // ← Decorator lo envuelve
});

// ── Observer ─────────────────────────────────────────────────────────────────
// Cada observer se registra bajo la MISMA interfaz. Al pedir IEnumerable<ICitaObserver>,
// la inyección de dependencias entrega los dos.
builder.Services.AddScoped<ICitaObserver, SmsObserver>();
builder.Services.AddScoped<ICitaObserver, EmailObserver>();

// ── Base de datos (PostgreSQL) + Identity Core ───────────────────────────────
// DbContext apunta a PostgreSQL usando la cadena de appsettings.json.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Identity: usuarios + roles, respaldados por AppDbContext (las tablas AspNet*).
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();


// CitaService (Domain) recibe el repo de citas + todos los observers
builder.Services.AddScoped<CitaService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllers();   // habilita las rutas de atributo del controller API

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();