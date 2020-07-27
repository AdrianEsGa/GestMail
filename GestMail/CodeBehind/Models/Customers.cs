using System.Collections.Generic;
using System.Data.OleDb;
using GestMail.CodeBehind.DataBase;

namespace GestMail.CodeBehind.Models
{
    public class Customers
    {

        private readonly int _code;
        private string _name;
        private List<string> _getEmails;
 
        public Customers(int code)
        {
            _code = code;
            LoadData();
        }

        public Customers(int code, string name)
        {
            _code = code;
            _name = name;
        }

        public Customers(int code, string getName, string email1, string email2, string email3)
        {
            _code = code;
            _name = getName;
            _getEmails = new List<string>() {email1, email2, email3};
        }

        private void LoadData()
        {
            var dbConnection = new Access();

            dbConnection.Command = new OleDbCommand("SELECT Nombre,Email1,Email2,Email3 FROM Clientes WHERE Id = @Id", dbConnection.Connection);
            dbConnection.Parameter = new OleDbParameter("@Id", OleDbType.Integer);
            dbConnection.Parameter = dbConnection.Command.CreateParameter();
            dbConnection.Parameter.Value = _code;
            dbConnection.Command.Parameters.Add(dbConnection.Parameter);

            dbConnection.Connection.Open();

            dbConnection.DataReader = dbConnection.Command.ExecuteReader();

            if (dbConnection.DataReader != null && dbConnection.DataReader.Read())
            {
                _name = dbConnection.DataReader["Nombre"].ToString();
                _getEmails = new List<string>()
                {
                    dbConnection.DataReader["Email1"].ToString(),
                    dbConnection.DataReader["Email2"].ToString(),
                    dbConnection.DataReader["Email3"].ToString()
                };
            }

            if (dbConnection.DataReader != null)
            {
                dbConnection.DataReader.Close();
                dbConnection.DataReader.Dispose();
            }

            dbConnection.Connection.Close();
            dbConnection.Connection.Dispose();

        }


        public int GetCode { get { return _code; } }
        public string GetName { get { return _name; } }

        public List<string> GetEmails
        {
            get { return _getEmails; }
        }

        public string GetCodeAndName { get { return "(" + _code + ") " + _name; } }
    }
}
