using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Citas_App.Data
{
    // Hereda de IdentityDbContext: eso trae AspNetUsers, AspNetRoles, etc.
    // No definimos DbSet<Paciente/Medico/Cita> todavía — siguen en JSON.
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
    }
}