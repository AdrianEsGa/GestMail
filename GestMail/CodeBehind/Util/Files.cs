using System.IO;

namespace GestMail.CodeBehind.Util
{
    public class Files
    {
        public static void Delete(string fileName)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }

        public static bool Exists(string fileName)
        {
            return File.Exists(fileName);
        }
    }
}
