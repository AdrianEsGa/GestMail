using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using GestMail.CodeBehind.Util.Configuraciones;

namespace GestMail.CodeBehind.Util
{
    public class Mail
    {
        //https://www.google.com/settings/security/lesssecureapps
        private readonly MailAddress _mailFrom;
        private readonly SmtpClient _smtpClient;
        private readonly string _rutaFacturas;

        public Mail(string emailFrom, string emailPassword, string emailServer, int emailSeverPort)
        {
            var config = Repositorio.Read();
            Log.Process = "Mail";

            _rutaFacturas = config.PendingBillPath;
            _mailFrom = new MailAddress(emailFrom);

            _smtpClient = new SmtpClient();
            var netCredentials = new NetworkCredential
            {
                UserName = emailFrom,
                Password = emailPassword
            };

            _smtpClient.Host = emailServer;

            //switch (emailSeverType)
            //{
            //    case "Gmail":
            //        _smtpClient.Host = "smtp.gmail.com";
            //        break;
            //    case "Hotmail":
            //        _smtpClient.Host = "smtp.live.com";
            //        break;
            //    case "MensaTui":
            //        _smtpClient.Host = "mail.mensatui.com";
            //        break;
            //}

            _smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            _smtpClient.UseDefaultCredentials = false;
            _smtpClient.Credentials = netCredentials;
            _smtpClient.Port = emailSeverPort;
            _smtpClient.EnableSsl = true;
            _smtpClient.Timeout = 100000;
        }

        public bool SendMail(List<string> strMailTo, string subject, List<string> attachments)
        {
            var mailMsg = new MailMessage {From = _mailFrom};

            foreach (var mailTo in strMailTo.Where(mailTo => !String.IsNullOrEmpty(mailTo)))
            {
                mailMsg.To.Add(new MailAddress(mailTo));
            }

            foreach (var attachment in attachments)
            {
                if (File.Exists(_rutaFacturas + "\\" + attachment))

                    mailMsg.Attachments.Add(new Attachment(_rutaFacturas + "\\" + attachment));

                else
                {
                    Log.Insert( "E",
                        "La factura " + _rutaFacturas + "\\" + attachment + " no existe. Se ha cancelado el envio");
                     return false;
                }             
            }
                 
            mailMsg.Subject = subject;
            mailMsg.IsBodyHtml = true;

            var sr = File.OpenText(@"C:\GestMail\Firma.html");

            mailMsg.Body = sr.ReadToEnd();

            ServicePointManager.ServerCertificateValidationCallback = AcceptAllCertifications;
 
            try
            {
                _smtpClient.Send(mailMsg);
                mailMsg.Dispose();
                return true;
            }
            catch (Exception e)
            {
                Log.Insert("E", "Se ha producido un error al relizar el envio " + e.Message);
                mailMsg.Dispose();
                return false;
            }
        }

        public bool SendMail(string strMailTo, string subject)
        {
            try
            {
                var mailMsg = new MailMessage { From = _mailFrom };
                mailMsg.To.Add(new MailAddress(strMailTo));
                mailMsg.Subject = subject;
                mailMsg.IsBodyHtml = true;

                var sr = File.OpenText(@"C:\GestMail\Firma.html");

                mailMsg.Body = sr.ReadToEnd();

                ServicePointManager.ServerCertificateValidationCallback = AcceptAllCertifications;

                _smtpClient.Send(mailMsg);
                return true;
            }
            catch (Exception e)
            {
                Log.Insert("E", "Se ha producido un error al relizar el envio " + e.Message);
                return false;
            }
        }

        public static bool AcceptAllCertifications(object sender, X509Certificate certification, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return sslPolicyErrors == SslPolicyErrors.None;
        }
    }
}
