using System.Windows;
using GestMail.CodeBehind.Util;

namespace GestMail.UI
{

    public partial class LicenseUi
    {
        private readonly License _license;
        public LicenseUi()
        {
           InitializeComponent();
           _license = new License();
            LoadControls();
        }

        private void BtnActivate_Click(object sender, RoutedEventArgs e)
        {
           
            if (_license.Activate(TbLicenseKey.Text.Trim()))
            {
                MessageBox.Show(Properties.Resources.PRODUCT_ACTIVATE, Properties.Resources.ACTIVE_PRODUCT, MessageBoxButton.OK, MessageBoxImage.Asterisk);
                var gestMail = new MainWindow();

                gestMail.Show();
                Close();

            }
            else
                MessageBox.Show(Properties.Resources.LICENSE_KEY_NO_VALID, Properties.Resources.ACTIVE_PRODUCT,
                    MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void LoadControls()
        {
            LblLicenseState.Content = _license.Check() ? Properties.Resources.ACTIVE_LICENSE : Properties.Resources.EXPIRED_LICESE;

        }
    }
}
