using System;
using System.Data;
using System.Windows;
using GestMail.CodeBehind.DataBase;

namespace GestMail.CodeBehind.Util
{
    public static class Log
    {

        private static Access _connectionAccess;
        public static string Process;
   
        public static void Insert(string type, string message)
        {
            _connectionAccess = new Access();

            var strSql = "insert into Log (Proceso,Tipo,Mensaje,FechaHora) " +
                         "values ('" + Process + "','" + type + "','" + message.Replace("'", "´") + "','" + DateTime.Now + "')";

            
            _connectionAccess.Command.CommandText = strSql;

            if (_connectionAccess.Connection.State == ConnectionState.Closed) 
            _connectionAccess.Connection.Open();


            try
            {
               
                _connectionAccess.Command.ExecuteNonQuery();
                _connectionAccess.Connection.Close();
            }

            catch (Exception e)
            {
                MessageBox.Show("Error en Log.Insert() " + e.Message, "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

          

        }

        public static void Delete()
        {
            _connectionAccess = new Access();
            _connectionAccess.ExecuteSqlNonQuery("delete from Log");
          
        }

        public static void Delete(string proceso)
        {
            _connectionAccess = new Access();
            _connectionAccess.ExecuteSqlNonQuery("delete from Log where Proceso like '%" + proceso + "%'");
          
        }

        public static DataTable Get()
        {
            _connectionAccess = new Access();
            var queryResult = _connectionAccess.ExecuteSqlAdapter("select Proceso,SWITCH(Tipo = 'I','Información',Tipo='E','Error') as Tipo,Mensaje,FechaHora from Log order by FechaHora Desc");
            return queryResult;
        }

        public static DataTable Get(string proceso)
        {
            _connectionAccess = new Access();
            var queryResult = _connectionAccess.ExecuteSqlAdapter("select Proceso,SWITCH(Tipo = 'I','Información',Tipo='E','Error') as Tipo,Mensaje,FechaHora from Log where Proceso like '%" + proceso + "%'  order by FechaHora Desc");
       
            return queryResult;
        
        }

        public static DataTable GetErrors()
        {
            _connectionAccess = new Access();
            var queryResult = _connectionAccess.ExecuteSqlAdapter("select Proceso,SWITCH(Tipo = 'I','Información',Tipo='E','Error') as Tipo,Mensaje,FechaHora from Log where Tipo = 'E'  order by FechaHora Desc");
           
            return queryResult;
        }

    }
}
