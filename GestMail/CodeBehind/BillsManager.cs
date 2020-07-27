using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Linq;
using GestMail.CodeBehind.DataBase;
using GestMail.CodeBehind.Models;
using GestMail.CodeBehind.Util;


namespace GestMail.CodeBehind
{
    public class BillsManager
    {
        private readonly Access _connectionAccess;

        public BillsManager() 
        {
            _connectionAccess = new Access();
            Log.Process = "BillsManager";

        }

        public DataTable  Search(string serie, string number, string year)
        {

            var sqlQuery = "select f.Serie,f.Numero,f.Año,'(' + CStr(f.IdCliente)  + ') ' + c.Nombre as Nombre," +
                         "SWITCH(f.Enviada = True,'Enviada',f.Enviada=False,'Pendiente') as Estado,f.Fichero ,f.FechaImportacion " +
                         "from Facturas as f " +
                         "inner join Clientes as c on f.IdCliente = c.Id " +
                         "where f.Serie = '" + serie + "' and Numero = '" + number + "' and Año = '" + year + "'";

            return _connectionAccess.ExecuteSqlAdapter(sqlQuery);
            
        }

        public List<Bills> SearchPending(Customers customer) 
        {
            var strSql = "select f.Serie,f.Numero,f.Año,f.IdCliente,c.Nombre," +
                         "f.Enviada as Estado,f.Fichero,f.FechaImportacion " +
                         "from Facturas as f " +
                         "inner join Clientes as c on f.IdCliente = c.Id " +
                         "where Enviada = 0";
            
            if (customer!=null)
                strSql = strSql + " and IdCliente = " + customer.GetCode;

            return (from DataRow pendingBillRow in _connectionAccess.ExecuteSqlAdapter(strSql).Rows select new Bills(pendingBillRow["Serie"].ToString(), pendingBillRow["Numero"].ToString(), pendingBillRow["Año"].ToString(), (int)pendingBillRow["IdCliente"], (bool)pendingBillRow["Estado"], pendingBillRow["Fichero"].ToString(), (DateTime)pendingBillRow["FechaImportacion"])).ToList();
        }

        public DataTable  ConsultPending(Customers customer)
        {
            var sqlQuery = "select f.Serie,f.Numero,f.Año,'(' + CStr(f.IdCliente)  + ') ' + c.Nombre as Nombre," +
                         "SWITCH(f.Enviada = True,'Enviada',f.Enviada=False,'Pendiente') as Estado,f.Fichero,f.FechaImportacion " +
                         "from Facturas as f " +
                         "inner join Clientes as c on f.IdCliente = c.Id " +
                         "where Enviada = 0";

            if (customer != null)
                sqlQuery = sqlQuery + " and IdCliente = " + customer.GetCode;

            return _connectionAccess.ExecuteSqlAdapter(sqlQuery);
        }

        public DataTable ConsultSent(Customers customer)
        {
            var sqlQuery = "select f.Serie,f.Numero,f.Año,'(' + CStr(f.IdCliente)  + ') ' + c.Nombre as Nombre," +
                         "SWITCH(f.Enviada = True,'Enviada',f.Enviada=False,'Pendiente') as Estado,f.Fichero,f.FechaImportacion " +
                         "from Facturas as f " +
                         "inner join Clientes as c on f.IdCliente = c.Id " +
                         "where Enviada <> 0";

            if (customer != null)
                sqlQuery = sqlQuery + " and IdCliente = " + customer.GetCode;

            return _connectionAccess.ExecuteSqlAdapter(sqlQuery);
         }

        public List<Bills> All(Customers customer)
        {
            var sqlQuery = "select f.Serie,f.Numero,f.Año,f.IdCliente,c.Nombre," +
                         "f.Enviada as Estado,f.Fichero , f.FechaImportacion " +
                         "from Facturas as f " +
                         "inner join Clientes as c on f.IdCliente = c.Id ";
                  
            if (customer != null)
                sqlQuery = sqlQuery + "where IdCliente = " + customer.GetCode;

            return (from DataRow allBillsRow in _connectionAccess.ExecuteSqlAdapter(sqlQuery).Rows select new Bills(allBillsRow["Serie"].ToString(), allBillsRow["Numero"].ToString(), allBillsRow["Año"].ToString(), (int)allBillsRow["IdCliente"], (bool)allBillsRow["Estado"], allBillsRow["Fichero"].ToString(), (DateTime)allBillsRow["FechaImportacion"])).ToList();
        }

        public void SyncUp(FileInfo pendingBillFile) 
        {

              var pendingBillFileNameParts =  Path.GetFileNameWithoutExtension(pendingBillFile.ToString()).Split('_');
              var customerCode =  pendingBillFileNameParts[1].Substring(pendingBillFileNameParts[1].IndexOf('(') + 1,pendingBillFileNameParts[1].IndexOf(')')- 1);
              var customerName = pendingBillFileNameParts[1].Substring(pendingBillFileNameParts[1].IndexOf(')') + 1, pendingBillFileNameParts[1].Length - pendingBillFileNameParts[1].IndexOf(')') -1);

              var bill = new Bills(pendingBillFileNameParts[2], pendingBillFileNameParts[4], pendingBillFileNameParts[3], Convert.ToInt32( customerCode), customerName, false, pendingBillFile.ToString(),DateTime.Now);
           
              Create(bill);

        }

        public void Delete (Bills bill)
        {
            var sqlQuery = "delete from Facturas where Serie = '" + bill.GetSerie + "' and Numero = '" + bill.GetNumber + "' and Año = '"+ bill.GetYear + "'";

            _connectionAccess.ExecuteSqlNonQuery(sqlQuery);
        }

        private void Create(Bills bill) 
        {
            if (Exists(bill)) return;

            if (!CustomersExists(bill))
            {
                var notExistCustomer = new Customers(bill.GetCustomer.GetCode, bill.GetCustomer.GetName, "", "", "");
                var myCustomerManager = new CustomersManager();

                myCustomerManager.Create(notExistCustomer);

                Log.Insert("I", "Se han dado de alta el cliente (" + bill.GetCustomer.GetCode + ") " + bill.GetCustomer.GetName + " no existente. Recuerde configurar sus emails");

            }

            var sqlQuery = "insert into Facturas (Serie,Numero,Año,IdCliente,Enviada,Fichero,FechaImportacion)" +
                         "values ('" + bill.GetSerie + "','" + bill.GetNumber + "','" + bill.GetYear + "'," + bill.GetCustomer.GetCode + ",0,'" + bill.GetFile + "','" + DateTime.Now + "')";

            _connectionAccess.ExecuteSqlNonQuery(sqlQuery);

            Log.Insert("I", "Se ha cargado la factura " + bill.GetSerie + "/" + bill.GetNumber + "/" + bill.GetYear);
        }

        private bool Exists(Bills bill)
        {
            var sqlQuery = "select 1 from Facturas where Serie = '" + bill.GetSerie + "' and Numero = '" + bill.GetNumber + "' and Año = '" + bill.GetYear + "'";

            return _connectionAccess.ExecuteSqlAdapter(sqlQuery).Rows.Count != 0;
        }

        private bool CustomersExists(Bills bill)
        {
            var sqlQuery = "select 1 from Clientes where Id = " + bill.GetCustomer.GetCode;

            return _connectionAccess.ExecuteSqlAdapter(sqlQuery).Rows.Count != 0;
        }

        public int HowMuchBillsCreated(bool pending)
        {
            var strSql = pending ? "select count(1) from Facturas where Enviada = 0" : "select count(1) from Facturas where Enviada <> 0";
            return _connectionAccess.ExecuteSqlEscalar(strSql);
        }


    }
}
