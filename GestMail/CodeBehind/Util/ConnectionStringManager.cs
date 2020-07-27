namespace GestMail.CodeBehind.Util
{
   public class ConnectionStringManager
    {
        public static string Formatear(string ruta, string contraseña)
        {
            return @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + ruta + ";Jet OLEDB:Database Password=" + contraseña + ";";

        }
    }
}
