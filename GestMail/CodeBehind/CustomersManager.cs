using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using GestMail.CodeBehind.DataBase;
using GestMail.CodeBehind.Models;

namespace GestMail.CodeBehind
{
   public class CustomersManager
    {
       private readonly Access _dbConnection;

       public CustomersManager()
        {
            _dbConnection = new Access();
        }

       public List<Customers> Search(string code, string name)
        {
           var sqlQuery = "select Id,Nombre,Email1,Email2,Email3 " +
                                  "from clientes " +
                                  "where ";

            if (!string.IsNullOrEmpty(code))
                sqlQuery = sqlQuery + "Id = " + code;

            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(code))
                sqlQuery = sqlQuery + " or ";

            if (!string.IsNullOrEmpty(name))
                sqlQuery = sqlQuery + "Nombre like '%" + name + "%'";

            return (from DataRow customerRow in _dbConnection.ExecuteSqlAdapter(sqlQuery).Rows select new Customers(Convert.ToInt32(customerRow["Id"]), customerRow["Nombre"].ToString(), customerRow["Email1"].ToString(), customerRow["Email2"].ToString(), customerRow["Email3"].ToString())).ToList();
        }

       public List<Customers> SearchAll()
       {
           const string strSql = "select Id,Nombre,Email1,Email2,Email3 " +
                                 "from clientes ";

           return (from DataRow customerRow in _dbConnection.ExecuteSqlAdapter(strSql).Rows select new Customers(Convert.ToInt32(customerRow["Id"]), customerRow["Nombre"].ToString(), customerRow["Email1"].ToString(), customerRow["Email2"].ToString(), customerRow["Email3"].ToString())).ToList();
       } 

       public Customers SearchByCode(string code)
       {
           Customers customer = null;
           var strSql = "select Id,Nombre,Email1,Email2,Email3 " +
                                 "from clientes " +
                                 "where Id = " + code;

           foreach (DataRow rowCliente in _dbConnection.ExecuteSqlAdapter(strSql).Rows)
           {

               customer = new Customers(Convert.ToInt32(rowCliente["Id"]));
           }

           return customer;
       }

       public void Create(Customers customer)
        {
            var sqlQuery = "insert into Clientes (Id,Nombre,Email1,Email2,Email3)" +
                         "values ('" + Convert.ToInt32(customer.GetCode) + "','" + customer.GetName + "','" + customer.GetEmails[0] + "','" + customer.GetEmails[1] + "','" + customer.GetEmails[2] + "')";

            _dbConnection.ExecuteSqlNonQuery(sqlQuery);
        
        }
 
       public void Delete(Customers customer)
        {
            var sqlQuery = "delete from Clientes where Id = " + customer.GetCode;

            _dbConnection.ExecuteSqlNonQuery(sqlQuery);
        }

       public void Update(Customers customer)
        {
            var sqlQuery = "update Clientes " +
                         "set Nombre = '" + customer.GetName + "', Email1 = '" + customer.GetEmails[0] + "', Email2 = '" + customer.GetEmails[1] + "', Email3 = '" + customer.GetEmails[2] + "' " +
                         "where Id = " + customer.GetCode;

            _dbConnection.ExecuteSqlNonQuery(sqlQuery);
        }

       public bool ItHasBills(Customers customer)
       {
           var sqlQuery = "select 1 from Facturas where IdCliente = " + customer.GetCode;

           return _dbConnection.ExecuteSqlAdapter(sqlQuery).Rows.Count != 0;
       }

       public bool Exists(Customers customer)
       {
           var strSql = "select 1 from Clientes where Id = " + customer.GetCode;

           return _dbConnection.ExecuteSqlAdapter(strSql).Rows.Count != 0;
       }

       public int HowMuchCustomerCreated()
       {
           var strSql = "select count(1) from Clientes";

           return _dbConnection.ExecuteSqlEscalar(strSql);
       }

    }
}
