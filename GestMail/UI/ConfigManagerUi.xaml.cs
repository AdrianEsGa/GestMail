using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using GestMail.CodeBehind.Util;
using GestMail.CodeBehind.Util.Configuraciones;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;


namespace GestMail.UI
{

    public partial class ConfigManagerUi 
    {
        private Global _config;

        public ConfigManagerUi()
        {
            InitializeComponent();           
            LoadConfiguration();
        }

        private void BtnViewHistorial_Click(object sender, RoutedEventArgs e)
        {
            LvHistorial.ItemsSource = null;


            if (ChkErrorsHistorial.IsChecked != null && ChkErrorsHistorial.IsChecked.Value)
            {
                LvHistorial.ItemsSource = Log.GetErrors().DefaultView;
                return;
            }

            LvHistorial.ItemsSource = Log.Get().DefaultView;


        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TbDataBase.Text.Trim()) || string.IsNullOrEmpty(TbPathPendingBills.Text.Trim()) ||
                string.IsNullOrEmpty(TbEmail.Text.Trim()) || string.IsNullOrEmpty(TbPassWord.Password.Trim()) ||
                string.IsNullOrEmpty(TbPathSentBills.Text.Trim()) || string.IsNullOrEmpty(TbShippingTime.Text.Trim())
                || string.IsNullOrEmpty(TbIntervalos.Text.Trim()) || string.IsNullOrEmpty(TbEmailServer.Text.Trim()) || 
                string.IsNullOrEmpty(TbEmailServerPort.Text.Trim()))
            {
                MessageBox.Show(Properties.Resources.MUST_WRITE_ANY_CONFIG, Properties.Resources.INCOMPLETE_CONFIG , MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var config = new Global
            {
                DataBasePath = TbDataBase.Text,
                PendingBillPath = TbPathPendingBills.Text,
                SentBillPath = TbPathSentBills.Text,
                Email = TbEmail.Text,
                EmailPassWord = TbPassWord.Password,
                EmailSever = TbEmailServer.Text,
                EmailSeverPort = int.Parse(TbEmailServerPort.Text),
                ShippingTime = TbShippingTime.Text,
                ShippingInterval = TbIntervalos.Text,
                LastShipmentWithErrors = _config.LastShipmentWithErrors 
            };

            var configValidate = new ConfigValidator(config);

            var messages = configValidate.Validate();

            if (messages != string.Empty)
            {
                MessageBox.Show(messages, Properties.Resources.CONFIG_ERRORS, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            Repositorio.Save(config);
            MessageBox.Show(Properties.Resources.CORRECT_SAVE, Properties.Resources.SAVE_CONFIG, MessageBoxButton.OK, MessageBoxImage.Asterisk);

        }

        private void LoadConfiguration()
        {
            _config = Repositorio.Read();

            TbDataBase.Text = _config.DataBasePath;
            TbPathPendingBills.Text = _config.PendingBillPath;
            TbPathSentBills.Text = _config.SentBillPath;
            TbEmail.Text = _config.Email;
            TbPassWord.Password = _config.EmailPassWord;
            TbEmailServer.Text = _config.EmailSever;
            TbEmailServerPort.Text = _config.EmailSeverPort.ToString();
            TbShippingTime.Text = _config.ShippingTime;
            TbIntervalos.Text = _config.ShippingInterval;
        }

        private void BtnTestEmail_Click(object sender, RoutedEventArgs e)
        {
            var shipmentsTest = new Mail(_config.Email,_config.EmailPassWord,_config.EmailSever, _config.EmailSeverPort);
            shipmentsTest.SendMail(_config.Email, Properties.Resources.EMAIL_TEST_SUBJECT);
        }

        private void TbIntervalos_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key >= Key.D0 && e.Key <= Key.D9 || e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
                e.Handled = false;
            else
                e.Handled = true;
        }

        private void BtnReportErrors_Click(object sender, RoutedEventArgs e)
        {
            var strErrors = Log.GetErrors().Rows.Cast<DataRow>().Aggregate(string.Empty, (current, logErrorsDataRow) => current + (logErrorsDataRow["Proceso"] + "-" + logErrorsDataRow["Mensaje"] + "-" + logErrorsDataRow["FechaHora"] + Environment.NewLine));

            if (strErrors == string.Empty)
            {
                MessageBox.Show("No existen errores que reportar","Reportar errores", MessageBoxButton.OK, MessageBoxImage.Information );
                return;
            }

            var shipmentsTest = new Mail("gestmail0@gmail.com", "gestmail2016", "smtp.gmail.com", 587);
            shipmentsTest.SendMail("gestmail0@gmail.com", strErrors);

            MessageBox.Show(Properties.Resources.CORRECT_ERRORS_REPORT , Properties.Resources.ERRORS_REPORT , MessageBoxButton.OK, MessageBoxImage.Asterisk);


        }

        private void BtnSelectDataBase_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var fichero = new OpenFileDialog();
            fichero.ShowDialog();
            TbDataBase.Text = fichero.FileName;
        }

        private void BtnSelectPathPendingBills_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var directorio = new FolderBrowserDialog();
            directorio.ShowDialog();
            TbPathPendingBills.Text = directorio.SelectedPath;
        }

        private void BtnSelectPathShippingBills_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var directorio = new FolderBrowserDialog();
            directorio.ShowDialog();
            TbPathSentBills.Text = directorio.SelectedPath;
        }



    }
}
