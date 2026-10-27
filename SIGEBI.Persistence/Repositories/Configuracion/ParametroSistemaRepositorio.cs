using Microsoft.EntityFrameworkCore;
using SIGEBI.DAL.Models.Configuracion;
using SIGEBI.Persistence.Base;
using SIGEBI.Persistence.Context;
using SIGEBI.Persistence.Interfaces.Configuracion;

namespace SIGEBI.Persistence.Repositories.Configuracion;

public class ParametroSistemaRepositorio : RepositorioBase<ParametroSistema>, IParametroSistemaRepositorio
{
    public ParametroSistemaRepositorio(SigebiContext contexto) : base(contexto)
    {
    }

    // Para leer un valor configurable, por ejemplo MaxIntentosLogin o MinutosBloqueo
    public async Task<ParametroSistema?> ObtenerPorClaveAsync(string clave)
    {
        return await Entidad.FirstOrDefaultAsync(p => p.Clave == clave);
    }
}