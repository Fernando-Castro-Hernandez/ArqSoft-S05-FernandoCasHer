using Citas_App.Application.Services;
using Citas_App.Interfaces;
using Citas_App.Repositories;
using Citas_App.Interfaces;
using Citas_App.Repositories;
using Citas_App.Application.Services;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();


// Repositorios
builder.Services.AddScoped<IPacienteRepository, JsonPacienteRepository>();
builder.Services.AddScoped<IMedicoRepository, JsonMedicoRepository>();
builder.Services.AddScoped<ICitaRepository, JsonCitaRepository>();


// Servicios de aplicación
builder.Services.AddScoped<PacienteService>();
builder.Services.AddScoped<MedicoService>();
builder.Services.AddScoped<CitaService>();
var app = builder.Build();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();