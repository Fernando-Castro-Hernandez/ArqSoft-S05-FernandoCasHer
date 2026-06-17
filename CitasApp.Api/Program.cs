using Citas_App.Application.Services;
using Citas_App.Interfaces;
using Citas_App.Repositories;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

// CORS: permite que una página web (de cualquier origen) consuma la API.
// Solo para desarrollo — en producción se restringe a orígenes concretos.
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});


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
app.UseCors("PermitirTodo");   // <-- debe ir ANTES de UseAuthorization y MapControllers
app.UseAuthorization();
app.MapControllers();
app.Run();