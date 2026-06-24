using Citas_App.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace Citas_App.Repositories
{
    public static class RepositoryFactory
    {
        public static IPacienteRepository CrearPacienteRepository(string entorno, IWebHostEnvironment env)
        {
            return entorno switch
            {
                "Production" => new MemoriaPacienteRepository(),
                _ => new JsonPacienteRepository(env)
            };
        }

        public static IMedicoRepository CrearMedicoRepository(string entorno, IWebHostEnvironment env)
        {
            return entorno switch
            {
                "Production" => new JsonMedicoRepository(env),
                _ => new JsonMedicoRepository(env)
            };
        }

        public static ICitaRepository CrearCitaRepository(string entorno, IWebHostEnvironment env)
        {
            return entorno switch
            {
                "Production" => new JsonCitaRepository(env),
                _ => new JsonCitaRepository(env)
            };
        }
    }
}