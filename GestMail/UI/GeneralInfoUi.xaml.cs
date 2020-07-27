using System;
using System.ComponentModel;
using System.Windows.Media;
using GestMail.CodeBehind;
using GestMail.CodeBehind.Util.Configuraciones;


namespace GestMail.UI
{

    public partial class GeneralInfoUi 
    {
        private readonly CustomersManager _myCustomersManager;
        private readonly BillsManager _myBillsManager;
        private readonly Global _globalConfig;
        private TimeSpan _time;


        public GeneralInfoUi()
        {
            InitializeComponent();
            _globalConfig = Repositorio.Read();

            _myCustomersManager = new CustomersManager();
            _myBillsManager = new BillsManager();

            var calculeStatisticsAgent = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            calculeStatisticsAgent.DoWork += CalculeStatisticsAgentDoWork;
            calculeStatisticsAgent.RunWorkerCompleted += CalculeStatisticsAgentRunWorkerCompleted;

            if (calculeStatisticsAgent.IsBusy != true)
            {
                calculeStatisticsAgent.RunWorkerAsync();
            }

            CalculateRestantTimeToInitShippments();
           
        }

        private void CalculeStatisticsAgentRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
  
        }

        private void CalculeStatisticsAgentDoWork(object sender, DoWorkEventArgs e)
        {



            Dispatcher.BeginInvoke(new Action(() =>
            {
                LblCreatedCustomersNumber.Content = _myCustomersManager.HowMuchCustomerCreated();
                LblPendingBillsNumber.Content = _myBillsManager.HowMuchBillsCreated(true);
                LblSentBillsNumber.Content = _myBillsManager.HowMuchBillsCreated(false);
                LblShippingTimeNumber.Content = _globalConfig.ShippingTime;

                if (_globalConfig.LastShipmentWithErrors == "yes")
                {
                    LblStatusLastShippingValue.Content = "No OK";
                    LblStatusLastShippingValue.Foreground = Brushes.Red;
                }

                else
                {
                    LblStatusLastShippingValue.Content = "OK";
                    LblStatusLastShippingValue.Foreground = Brushes.Green;
                }
            }));




        }

        private void CalculateRestantTimeToInitShippments()
        {

            var nowDate = DateTime.Now;
            var shipingDate = Convert.ToDateTime( _globalConfig.ShippingTime);

            _time = TimeSpan.Parse(nowDate > shipingDate ? (shipingDate.AddDays(1) - nowDate).ToString() : (shipingDate - nowDate).ToString());

            var timer = new System.Windows.Forms.Timer   {Interval = 1000};

            timer.Tick += (a, b) =>
            {
                _time = _time.Subtract(new TimeSpan(0, 0, 1));
                LblRestantTime.Content = _time.ToString().Substring(0, 8);          
            };

            timer.Start();
        }

    }
}
