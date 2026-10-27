using Microsoft.EntityFrameworkCore;
using SIGEBI.DAL.Models.Configuracion;
using SIGEBI.Persistence.Base;
using SIGEBI.Persistence.Context;
using SIGEBI.Persistence.Interfaces.Configuracion;

namespace SIGEBI.Persistence.Repositories.Configuracion;

public class PoliticaPrestamoRepositorio : RepositorioBase<PoliticaPrestamo>, IPoliticaPrestamoRepositorio
{
    public PoliticaPrestamoRepositorio(SigebiContext contexto) : base(contexto)
    {
    }

    // Máximo de préstamos y días de plazo que corresponden a estudiantes o docentes
    public async Task<PoliticaPrestamo?> ObtenerPorTipoUsuarioAsync(string tipoUsuario)
    {
        return await Entidad.FirstOrDefaultAsync(p => p.TipoUsuario == tipoUsuario && p.Estatus);
    }
}