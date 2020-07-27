using System;
using System.Data;
using GestMail.CodeBehind.DataBase;

namespace GestMail.CodeBehind.Util
{
    public class License
    {
        private readonly Access _connectionAccess;

        public License()
        {
            _connectionAccess = new Access();
            
        }

        public bool Check()
        {
           var heHasLicense = false;

           var sqlQuery = "select FechaLimite from Licencia";
           foreach (DataRow licenseRow  in _connectionAccess.ExecuteSqlAdapter(sqlQuery).Rows)
           {
               heHasLicense = DateTime.Now <= (DateTime)licenseRow["FechaLimite"];
           }

            if (!heHasLicense)
            {
                BlockLicense();
                return false;
            }


           sqlQuery = "select TieneLicencia from Licencia";
           foreach (DataRow licenseRow in _connectionAccess.ExecuteSqlAdapter(sqlQuery).Rows)
           {
               heHasLicense = (bool)licenseRow["TieneLicencia"];
           }


            return heHasLicense;
        }

        private void BlockLicense()
        {
            var sqlQuery = "update Licencia set TieneLicencia = 0";
            _connectionAccess.ExecuteSqlNonQuery(sqlQuery);
        }

        public bool Activate(string key)
        {
            var sqlQuery = "select 1 from Licencia where Clave = '" + key.Trim() + "'";

            var theAtivationIsSuccsesfull = _connectionAccess.ExecuteSqlAdapter(sqlQuery).Rows.Count != 0;

            if (!theAtivationIsSuccsesfull) return false;

            sqlQuery = "update Licencia set TieneLicencia = 1 , FechaLimite = FechaLicencia";
            _connectionAccess.ExecuteSqlNonQuery(sqlQuery);

            return true;
        }



    }
}
