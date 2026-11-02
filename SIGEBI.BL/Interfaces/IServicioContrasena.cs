namespace SIGEBI.BL.Interfaces;

public interface IServicioContrasena
{
    // Convierte la contraseña en un texto cifrado que no se puede revertir
    string Cifrar(string contrasena);

    // Comprueba si una contraseña corresponde al texto cifrado guardado
    bool Verificar(string contrasena, string contrasenaCifrada);
}