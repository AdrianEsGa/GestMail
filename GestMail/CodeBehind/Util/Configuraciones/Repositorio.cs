using System;
using System.IO;
using System.Xml;

namespace GestMail.CodeBehind.Util.Configuraciones
{
    public class Repositorio
    {
        private const string FileName = "C:\\GestMail\\Config.xml";

        public static Global Read()
        {
            var config = new Global();

            if (File.Exists(FileName))
            {
                var reader = XmlReader.Create(FileName);

                while (reader.Read())
                {

                    switch (reader.Name)
                    {
                        case "DataBasePath":
                            config.DataBasePath = ValorTagXml(reader);
                            reader.Read();
                            break;
                        case "PendingBillPath":
                            config.PendingBillPath = ValorTagXml(reader);
                            reader.Read();
                            break;
                        case "ShippingBillPath":
                            config.SentBillPath = ValorTagXml(reader);
                            reader.Read();
                            break;
                        case "Email":
                            config.Email = ValorTagXml(reader);
                            reader.Read();
                            break;
                        case "EmailPassWord":
                            config.EmailPassWord = ValorTagXml(reader);
                            reader.Read();
                            break;
                        case "EmailSever":
                            config.EmailSever = ValorTagXml(reader);
                            reader.Read();
                            break;
                        case "EmailSeverPort":
                            config.EmailSeverPort = int.Parse(ValorTagXml(reader));
                            reader.Read();
                            break;
                        case "ShippingTime":
                            config.ShippingTime = ValorTagXml(reader);
                            reader.Read();
                            break;
                        case "ShippingInterval":
                            config.ShippingInterval = ValorTagXml(reader);
                            reader.Read();
                            break;
                        case "LastShipmentWithErrors":
                            config.LastShipmentWithErrors = ValorTagXml(reader);
                            reader.Read();
                            break; 
                    }

                }
                reader.Close();
            }
            return config;
        }

        public static void Save(Global config)
        {
            Files.Delete(FileName);

            var settings = new XmlWriterSettings { Indent = true };
            var writer = XmlWriter.Create(FileName, settings);

            writer.WriteStartDocument();
            writer.WriteComment("Configuration base, no modify.");
            writer.WriteStartElement("Configuration");


            //Configuracion
            writer.WriteElementString("DataBasePath", config.DataBasePath);
            writer.WriteElementString("PendingBillPath", config.PendingBillPath);
            writer.WriteElementString("ShippingBillPath", config.SentBillPath);
            writer.WriteElementString("Email", config.Email);
            writer.WriteElementString("EmailPassWord", config.EmailPassWord);
            writer.WriteElementString("EmailSever", config.EmailSever);
            writer.WriteElementString("EmailSeverPort", config.EmailSeverPort.ToString());
            writer.WriteElementString("ShippingTime", config.ShippingTime);
            writer.WriteElementString("ShippingInterval", config.ShippingInterval);
            writer.WriteElementString("LastShipmentWithErrors", config.LastShipmentWithErrors);

            writer.WriteEndElement();

            writer.WriteEndDocument();
            writer.Flush();
            writer.Close();
        }

        private static string ValorTagXml(XmlReader reader)
        {
            while (reader.NodeType != XmlNodeType.EndElement)
            {
                reader.Read();
                if (reader.NodeType == XmlNodeType.Text)
                {
                    return reader.Value;
                }
            }
            return "";
        }
    }
}
