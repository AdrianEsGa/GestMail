using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using GestMail.CodeBehind;
using GestMail.CodeBehind.Util;
using GestMail.CodeBehind.Util.Configuraciones;
using GestMail.UI;
using License = GestMail.CodeBehind.Util.License;

#region "Confidential"
//gestmail0@gmail.com gestmail2016
//Lic xXcCgG23TrEs@sdui654
//DataBase  g3stm@il
#endregion 

namespace GestMail
{

    public partial class MainWindow
    {
        public Global GlobalConfig;
        private ConfigValidator _configValidator;
        private BillsManager _myBillsManager;
        private ShipmentsManager _myShipmentsManager;
        private BackgroundWorker _syncBillsAgent;
        private BackgroundWorker _sendShipmentsAgent;
        private DispatcherTimer _shippingAgent;

        public bool DoingShipping;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                ContentHolder.Content = new GeneralInfoUi();
                LoadComponentsAndVariables();
                CheckIfLicenseIsActive();
                SyncUpBills();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }


        }

        private void LoadComponentsAndVariables()
        {
            Modulos.Items.Add("Inicio");
            Modulos.Items.Add("Clientes");
            Modulos.Items.Add("Facturas");
            Modulos.Items.Add("Envios");
            Modulos.Items.Add("Configuración y historial");

            GlobalConfig = new Global();
            GlobalConfig = Repositorio.Read();       

            DoingShipping = false;
            _shippingAgent = new DispatcherTimer();
            _shippingAgent.Tick += ShippingAgent_Tick;
            _shippingAgent.Interval = new TimeSpan(0, 0, 0, 1);
            _shippingAgent.Start();

        }

        private void CheckIfLicenseIsActive()
        {
            var license = new License();

            if (license.Check()) return;

            MessageBox.Show("Licencia expirada", "Licencia", MessageBoxButton.OK,
                MessageBoxImage.Information);
            var activateLicense = new LicenseUi();

            activateLicense.Show();
            Close();
        }

        private void ShippingAgent_Tick(object sender, EventArgs e)
        {
            GlobalConfig = Repositorio.Read();

            var horaActual = DateTime.Now.Hour.ToString().PadLeft(2, '0');
            var minutoActual = DateTime.Now.Minute.ToString().PadLeft(2, '0');

            if (horaActual + ":" + minutoActual != GlobalConfig.ShippingTime || DoingShipping) return;

            DoingShipping = true;
            _shippingAgent.Stop();

            //Sincronizar facturas
            SyncUpBills();

        }

        private void SyncUpBills()
        {
            _myBillsManager = new BillsManager();

            _syncBillsAgent = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            _syncBillsAgent.DoWork += SyncBillsAgentDoWork;
            _syncBillsAgent.RunWorkerCompleted += SyncBillsAgentWorkComplete;

            if (_syncBillsAgent.IsBusy != true)
            {
                _syncBillsAgent.RunWorkerAsync();
            }
        }

        private void SyncBillsAgentDoWork(object sender, DoWorkEventArgs e)
        {
            var syncBillsCount = 1;
            var pendingBillDirectory = new DirectoryInfo(GlobalConfig.PendingBillPath);
            var pendingBillFiles = pendingBillDirectory.GetFiles("*.pdf");

            Dispatcher.BeginInvoke(new Action(() =>
            {
                PgsBarSyncBills.Value = 0;
                PgsBarSyncBills.Maximum = pendingBillFiles.Length;
            }));


            foreach (var pendingBillFile in pendingBillFiles)
            {
                _myBillsManager.SyncUp(pendingBillFile);

                var count = syncBillsCount;

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    LblCountSyncBills.Content = count + "/" + pendingBillFiles.Length;
                    PgsBarSyncBills.Value = PgsBarSyncBills.Value + 1;
                }));

                syncBillsCount += 1;
            }
        }

        private void SyncBillsAgentWorkComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            //Al finalizar la Sync de facturas, si estamos en modo envío,realizamos los envios
            ContentHolder.Content = new GeneralInfoUi();

            if (DoingShipping)
                SendPendingShipments();
        }

        private void SendPendingShipments()
        {
            _myShipmentsManager = new ShipmentsManager();

            _sendShipmentsAgent = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            _sendShipmentsAgent.DoWork += SendShipmentsAgentDoWork;
            _sendShipmentsAgent.RunWorkerCompleted += SendShipmentsAgentWorkComplete;

            if (_sendShipmentsAgent.IsBusy != true)
            {
                _sendShipmentsAgent.RunWorkerAsync();
            }
        }

        private void SendShipmentsAgentDoWork(object sender, DoWorkEventArgs e)
        {
            var sendShipmentsCount = 1;

            //Comprobar la configuración
            if (!ValidateConfiguration()) return;

            var pendingShipments = _myShipmentsManager.Pending(null);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                PgsBarShipments.Value = 0;
                PgsBarShipments.Maximum = pendingShipments.Count;
            }));

            //Enviar

            //Limpiamos log
            Log.Delete();

            foreach (var shipmentsPendiente in _myShipmentsManager.Pending(null))
            {
                _myShipmentsManager.Send(shipmentsPendiente);

                var interval = Convert.ToInt32(GlobalConfig.ShippingInterval) * 1000;

                var count = sendShipmentsCount;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    LblCountSendShipments.Content = count + "/" + pendingShipments.Count;
                    PgsBarShipments.Value = PgsBarShipments.Value + 1;
                }));

                Thread.Sleep(interval);
                sendShipmentsCount += 1;
            }

        }

        private void SendShipmentsAgentWorkComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            //Fin de los Envios
            DoingShipping = false;

            GlobalConfig.LastShipmentWithErrors = _myShipmentsManager.LastShippingWithErrors() ? "yes" : "no";

            Repositorio.Save(GlobalConfig);

            _shippingAgent.Start();
            ContentHolder.Content = new GeneralInfoUi();
        }

        private bool ValidateConfiguration()
        {
            _configValidator = new ConfigValidator(GlobalConfig);
            var messages = _configValidator.Validate();

            if (messages == string.Empty)
                return true;

            MessageBox.Show("Errores de configuración", "Error config", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;

        }

        private void ListView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = (ListViewItem)sender;
            switch (item.Content.ToString())
            {
                case "Inicio":
                    ContentHolder.Content = new GeneralInfoUi();
                    break;

                case "Clientes":
                    ContentHolder.Content = new CustomersManagerUi();
                    break;

                case "Facturas":
                    ContentHolder.Content = new BillsManagerUi();
                    break;

                case "Envios":
                    ContentHolder.Content = new ShipmentManagerUi();
                    break;

                case "Configuración y historial":
                       ContentHolder.Content = new ConfigManagerUi();
                    break;
           
            }
        }

        private void BtnLicense_Click(object sender, RoutedEventArgs e)
        {
            var activateLicense = new LicenseUi();

            activateLicense.Show();
        }

        private void BtnSyncBills_MouseDown(object sender, MouseButtonEventArgs e)
        {
            LblCountSyncBills.Content = "0/0";
            LblCountSendShipments.Content = "0/0";
            PgsBarShipments.Value = 0;
            PgsBarSyncBills.Value = 0;

            if (_syncBillsAgent.IsBusy != true)
            {
                _syncBillsAgent.RunWorkerAsync();
            }
        }


    }
}
