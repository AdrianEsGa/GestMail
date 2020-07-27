using System;
using System.IO;
using GestMail.CodeBehind.DataBase;
using GestMail.CodeBehind.Util.Configuraciones;

namespace GestMail.CodeBehind.Util
{
    public class ConfigValidator
    {
        private readonly Global _confiGlobal;
        public ConfigValidator(Global confiGlobal)
        {
            _confiGlobal = confiGlobal;
        }

        public string Validate()
        {
            var message = string.Empty;

            if (!ValidateDataBasePath()) message = "Error de conexión con la base de datos" + Environment.NewLine;
            if (!ValidatePendingBillPath()) message += "La ruta de facturas pendientes no existe" + Environment.NewLine;
            if (!ValidateSentBillPath()) message += "La ruta de facturas enviadas no existe" + Environment.NewLine;
            if (!ValidateShippingTime()) message += "La hora de envío no es correcta" + Environment.NewLine;
            if (!ValidateShippingInterval()) message += "Los segundos de intervalo de envio no son correctos" + Environment.NewLine;
           // if (!ValidateEmail()) message += "Datos de email incorrectos. No se ha enviado email de prueba" + Environment.NewLine;

            return message;
        }

        private bool ValidateEmail()
        {
            var shipmentsTest = new Mail(_confiGlobal.Email, _confiGlobal.EmailPassWord, _confiGlobal.EmailSever, _confiGlobal.EmailSeverPort);
            return shipmentsTest.SendMail(_confiGlobal.Email, Properties.Resources.EMAIL_TEST_SUBJECT);
        }

        private bool ValidateShippingInterval()
        {
          int shippingInterval; 
          var shippingIntervalIsNumeric = int.TryParse(_confiGlobal.ShippingInterval, out shippingInterval);
          return shippingIntervalIsNumeric;
        }

        private bool ValidateShippingTime()
        {
            DateTime time;
            var shippingTimeIsTime = DateTime.TryParse(_confiGlobal.ShippingTime, out time);
            return shippingTimeIsTime;
        }

        private bool ValidateSentBillPath()
        {
          return Directory.Exists(_confiGlobal.SentBillPath );
        }

        private bool ValidatePendingBillPath()
        {
           return Directory.Exists(_confiGlobal.PendingBillPath);
        }

        private bool ValidateDataBasePath()
        {
           var databaseConnection = new Access(_confiGlobal.DataBasePath);
           return databaseConnection.Test();
        }
    }
}
