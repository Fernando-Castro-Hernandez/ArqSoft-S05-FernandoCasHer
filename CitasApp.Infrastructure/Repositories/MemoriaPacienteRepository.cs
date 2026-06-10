using System;
using System.Collections.Generic;
using System.Text;
using Citas_App.Interfaces;
using Citas_App.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;

namespace Citas_App.Repositories
{
    internal class MemoriaPacienteRepository : IPacienteRepository
    {
        public Paciente? ObtenerPorId(int id)
        {
            throw new NotImplementedException();
        }

        public List<Paciente> ObtenerTodos()
        {
            throw new NotImplementedException();
        }
    }
}
