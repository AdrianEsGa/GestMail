using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using GestMail.CodeBehind.DataBase;
using GestMail.CodeBehind.Models;
using GestMail.CodeBehind.Util;
using GestMail.CodeBehind.Util.Configuraciones;


namespace GestMail.CodeBehind
{
    public class ShipmentsManager
    {
        private readonly Access _connectionAccess;
        private readonly Global _globalConfig;
        public ShipmentsManager()
        {
             _globalConfig = Repositorio.Read();
            _connectionAccess = new Access();
            Log.Process = "ShipmentsManager";
        }


        public List<Shipments> Pending(Customers customer)
        {
         
            var shipments = new List<Shipments>();

            var sqlQuery = "select Serie,Numero,Año,IdCliente,Fichero from Facturas where Enviada = 0";

            if (customer != null)
                sqlQuery = sqlQuery + " and IdCliente = " + customer.GetCode;


            var pendingBills = (from DataRow fPendientes in _connectionAccess.ExecuteSqlAdapter(sqlQuery).Rows select new Bills(fPendientes["Serie"].ToString(), fPendientes["Numero"].ToString(), fPendientes["Año"].ToString())).ToList();

            var customersWithPendingBills = pendingBills.GroupBy(factura => factura.GetCustomer.GetCode)
                   .Select(grp => grp.First().GetCustomer.GetCode).ToList();

            foreach (var customerWithPendingBills in customersWithPendingBills)
            {
                var copyCustomerWithPendingBills = customerWithPendingBills;
                var pendingBillsByCustomer = pendingBills.Where(cus => cus.GetCustomer.GetCode == copyCustomerWithPendingBills);

                var shipment = new Shipments(new Customers(customerWithPendingBills), pendingBillsByCustomer, DateTime.Now);
                shipments.Add(shipment);
            }

            return shipments;
        }

        public DataTable ConsultSent(Customers customer, DateTime? sinceDate, DateTime? untilDate)
        {
            var sqlQuery = "SELECT E.Id as IdEnvio,E.IdCliente,E.Fecha,'(' + CStr(E.IdCliente) + ') ' + C.Nombre as Nombre FROM Envios as E INNER JOIN Clientes as C on C.Id = E.IdCliente WHERE 1 = 1";
            if (customer != null)
                sqlQuery = sqlQuery + " and IdCliente = " + customer.GetCode;

            if (sinceDate != null && untilDate != null)
                sqlQuery = sqlQuery + " and Fecha >= #" + sinceDate.Value.Date.AddHours(00).ToString("yy-MM-dd") + "# and Fecha < #" + untilDate.Value.Date.AddDays(1).ToString("yy-MM-dd") + "#";

            return _connectionAccess.ExecuteSqlAdapter(sqlQuery);
        }

        public DataTable ConsultPending(Customers customer)
        {
            var sqlQuery = "select F.IdCliente,'(' + CStr(F.IdCliente) + ') ' + C.Nombre as Nombre from Facturas as F INNER JOIN Clientes as C on C.Id = F.IdCliente where Enviada = 0 Group by F.IdCliente,'(' + CStr(F.IdCliente) + ') ' + C.Nombre";
            if (customer != null)
                sqlQuery = sqlQuery + " and IdCliente = " + customer.GetCode;

            return _connectionAccess.ExecuteSqlAdapter(sqlQuery);
        }

        public DataTable GetSentShippingBills(int idShipments)
        {
        
            var sqlQuery = "SELECT F.Serie,F.Numero,F.Año,'Enviado' as Estado,F.Fichero " +
                           " FROM EnviosFacturas as EF " +
                           "  INNER JOIN Facturas as F on EF.Serie = F.Serie and EF.Numero = F.Numero and EF.Año = F.Año " +
                           "WHERE IdEnvio = " + idShipments;

            return _connectionAccess.ExecuteSqlAdapter(sqlQuery);
 
        }


        public DataTable GetPendingShippingBills(int customerCode)
        {
            var sqlQuery = "SELECT Serie,Numero,Año,'Pendiente' as Estado,Fichero FROM Facturas WHERE Enviada = 0 and IdCliente = " + customerCode;

            return _connectionAccess.ExecuteSqlAdapter(sqlQuery);        
        }


        public void Send(Shipments shipment)
        {
            var myMailManager = new Mail(_globalConfig.Email, _globalConfig.EmailPassWord, _globalConfig.EmailSever, _globalConfig.EmailSeverPort);  
 
            var billNames = new List<string>();
            billNames.AddRange(shipment.GetBills.Select(factura => factura.GetFile));

            if (myMailManager.SendMail(shipment.GetCustomer.GetEmails, Properties.Resources.EMAIL_CUSTOMER_SUBJECT, billNames))
            {
                Log.Insert("I",
                    "El envio del cliente " + shipment.GetCustomer.GetCodeAndName + " se ha realizado correctamente");

                MoveBillFile(shipment);
                SaveShipment(shipment);
            }
            else
                Log.Insert("E",
                    "El envio del cliente " + shipment.GetCustomer.GetCodeAndName + " no se ha realizado");

        }

        private void MoveBillFile(Shipments shipment)
        {
                foreach (var bill in shipment.GetBills)
                {
                   File.Move(_globalConfig.PendingBillPath + "\\" + bill.GetFile,
                        _globalConfig.SentBillPath + "\\" + bill.GetFile);
                }
        }

        private void SaveShipment(Shipments shipment)
        {
           var newId = GetNewShipmentId();

            try
            {

                var sqlQuery = "insert into Envios (Id,IdCliente,Fecha) " +
                               "values (" + newId + "," + shipment.GetCustomer.GetCode + ",'" + DateTime.Now + "')";


                _connectionAccess.ExecuteSqlNonQuery(sqlQuery);


                foreach (var bill in shipment.GetBills)
                {
                    sqlQuery = "insert into EnviosFacturas (IdEnvio,Serie,Numero,Año) " +
                               "values (" + newId + ",'" + bill.GetSerie + "','" + bill.GetNumber + "','" + bill.GetYear + "')";


                    _connectionAccess.ExecuteSqlNonQuery(sqlQuery);


                    sqlQuery = "update Facturas " +
                               "set Enviada = 1 " +
                               "where Serie = '" + bill.GetSerie + "' and Numero = '" + bill.GetNumber + "' and Año = '" + bill.GetYear + "'";


                    _connectionAccess.ExecuteSqlNonQuery(sqlQuery);

                   
                }


            }

            catch (Exception e)
            {
                try
                {
                    Log.Insert("E",
                     "Envio con id " + shipment.GetCustomer + " actualizado en base de datos - " + e.Message);
                }
                catch
                {
                    // Do nothing here; transaction is not active.
                }             

            }

        }

        private int GetNewShipmentId()
        {
           return _connectionAccess.ExecuteSqlEscalar("select switch(count(1) = 0,0,count(1)<>0,max(Id)) + 1 FROM Envios");
        }


        public bool LastShippingWithErrors()
        {
            const string strSql = "select Id from Log where Tipo = 'E'";

            return _connectionAccess.ExecuteSqlAdapter(strSql).Rows.Count != 0;
        }
    }
}
