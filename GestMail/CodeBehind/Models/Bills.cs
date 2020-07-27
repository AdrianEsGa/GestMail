using System;
using System.Data.OleDb;
using GestMail.CodeBehind.DataBase;

namespace GestMail.CodeBehind.Models
{
    public class Bills
    {
        private string _serie;
        private string _number;
        private string _year;
        private Customers _customer;
        private string _file;
        private bool _sent;
        private DateTime _importDate;


        public Bills(string serie, string number, string year, int customerId, bool sent, string file, DateTime importDate)
        {
            _serie = serie;
            _number = number;
            _year = year;
            _customer = new Customers(customerId);
            _file = file;
            _sent = sent;
            _importDate = importDate;
        }

        public Bills(string serie, string number, string year, int customerId, string customerName, bool sent, string file, DateTime importDate)
        {
            _serie = serie;
            _number = number;
            _year = year;
            _customer = new Customers(customerId, customerName);
            _file = file;
            _sent = sent;
            _importDate = importDate;
        }

        public Bills(string serie, string number, string year)
        {
            _serie = serie;
            _number = number;
            _year = year;
            LoadData();
        }

        private void LoadData()
        {
           var dbConnection = new Access();

            dbConnection.Command = new OleDbCommand("SELECT IdCliente,Enviada,Fichero,FechaImportacion " +
                                                    "FROM Facturas " +
                                                    "WHERE Serie = @Serie and Numero = @Number and Año = @Year", dbConnection.Connection);


           dbConnection.Parameter = new OleDbParameter("@Serie", OleDbType.VarChar);
           dbConnection.Parameter = dbConnection.Command.CreateParameter();
           dbConnection.Parameter.Value = _serie;
           dbConnection.Command.Parameters.Add(dbConnection.Parameter);

           dbConnection.Parameter = new OleDbParameter("@Number", OleDbType.VarChar);
           dbConnection.Parameter = dbConnection.Command.CreateParameter();
           dbConnection.Parameter.Value = _number;
           dbConnection.Command.Parameters.Add(dbConnection.Parameter);

           dbConnection.Parameter = new OleDbParameter("@Year", OleDbType.VarChar);
           dbConnection.Parameter = dbConnection.Command.CreateParameter();
           dbConnection.Parameter.Value = _year;
           dbConnection.Command.Parameters.Add(dbConnection.Parameter);

           dbConnection.Connection.Open();

           dbConnection.DataReader = dbConnection.Command.ExecuteReader();

            if (dbConnection.DataReader != null && dbConnection.DataReader.Read())
            {
                _customer = new Customers((int)dbConnection.DataReader["IdCliente"]);
                _sent = (bool)dbConnection.DataReader["Enviada"];
                _file = dbConnection.DataReader["Fichero"].ToString();
                _importDate = (DateTime)dbConnection.DataReader["FechaImportacion"];
            }
            else
            {
                _serie = null;
                _number = null;
                _year = null;
            }

            if (dbConnection.DataReader != null)
            {
                dbConnection.DataReader.Dispose();
            }
            dbConnection.Command.Dispose();
            dbConnection.Connection.Dispose();


        }


        public string GetSerie { get { return _serie; } }
        public string GetNumber { get { return _number; } }
        public string GetYear { get { return _year; } }
     

        public Customers GetCustomer
        {
            get { return _customer; }
        }

        public string GetFile
        {
            get { return _file; }
        }

        public string GetSent 
        {
            get { return _sent ? "Enviada" : "Pendiente"; }
        }

        public DateTime GetImportDate { get { return _importDate; } }
    }
}
