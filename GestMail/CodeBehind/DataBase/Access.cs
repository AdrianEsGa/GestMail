using System;
using System.Data;
using System.Data.OleDb;
using System.Windows;
using GestMail.CodeBehind.Util;
using GestMail.CodeBehind.Util.Configuraciones;

namespace GestMail.CodeBehind.DataBase
{
    public class Access
    {
        public OleDbConnection Connection;
        public OleDbDataReader DataReader;
        public OleDbCommand Command;
        public OleDbParameter Parameter;
        public OleDbTransaction Transaction;
        public readonly string Path;
        public readonly string Password;

        public Access()
        {
            var config = Repositorio.Read();
            Path = config.DataBasePath;
            Password = "g3stm@il";
            Connection = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Path + ";Jet OLEDB:Database Password= " + Password + ";");
            Command = new OleDbCommand {Connection = Connection};
            Log.Process = "Access";
        }

        public Access(string path)
        {
            Password = "g3stm@il";
            Connection = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + ";Jet OLEDB:Database Password= " + Password + ";");
            Command = new OleDbCommand { Connection = Connection };
            Log.Process = "Access";
        }


        public bool Test()
        {
            if (Connection.State == ConnectionState.Open) return true;

            try
            {
                Connection.Open();
            }
            catch (Exception)
            {
                // ignored
            }

            if (Connection.State != ConnectionState.Open)
                return false;

            Connection.Close();

            return true;
        }

        public void Open()
        {
            if (Connection.State == ConnectionState.Open) return;

            try
            {
                Connection.Open();
            }
            catch (Exception e)
            {
                MessageBox.Show("Sin acceso a la base de datos - " + e.Message , "Error base de datos");
            }
        }

        public void Close()
        {
            if (Connection.State == ConnectionState.Closed) return;

            try
            {
                Connection.Close();
            }
            catch (Exception e)
            {
                Log.Insert("E", e.Message);
            }
        }

        public DataTable ExecuteSqlAdapter(string strSql)
        {
            var tabla = new DataTable();
            var dAdapter = new OleDbDataAdapter(strSql, Connection);

            Open();

            try
            {              
                dAdapter.Fill(tabla);
            }
            catch (Exception e)
            {
                Log.Insert("E", e.Message);
            }

            Close();

            return tabla;

        }

        public void ExecuteSqlNonQuery(string strSql)
        {

            Command.CommandText = strSql;
            
            Open();

            try
            {
                Command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Log.Insert("E", e.Message);
            }

            Close();          

        }


        public int ExecuteSqlEscalar(string strSql)
        {

            Command.CommandText = strSql;
            var escalar = 0;

            Open();

            try
            {
                escalar = (int)Command.ExecuteScalar();
            }
            catch (Exception e)
            {
                Log.Insert("E", e.Message);
            }

            Close();

            return escalar;

        }

    }
}
