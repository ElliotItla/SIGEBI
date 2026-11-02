using System.Security.Cryptography;
using SIGEBI.BL.Interfaces;

namespace SIGEBI.BL.Services;

public class ServicioContrasena : IServicioContrasena
{
    private const int Iteraciones = 120000;
    private const int LargoSal = 16;
    private const int LargoHash = 32;

    // Formato guardado: PBKDF2$iteraciones$sal$hash
    public string Cifrar(string contrasena)
    {
        var sal = RandomNumberGenerator.GetBytes(LargoSal);
        var hash = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, Iteraciones, HashAlgorithmName.SHA256, LargoHash);

        return $"PBKDF2${Iteraciones}${Convert.ToBase64String(sal)}${Convert.ToBase64String(hash)}";
    }

    public bool Verificar(string contrasena, string contrasenaCifrada)
    {
        var partes = contrasenaCifrada.Split('$');

        if (partes.Length != 4 || partes[0] != "PBKDF2" || !int.TryParse(partes[1], out var iteraciones))
        {
            return false;
        }

        try
        {
            var sal = Convert.FromBase64String(partes[2]);
            var esperado = Convert.FromBase64String(partes[3]);
            var calculado = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, iteraciones, HashAlgorithmName.SHA256, esperado.Length);

            return CryptographicOperations.FixedTimeEquals(calculado, esperado);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}