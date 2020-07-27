using System;
using System.Data;
using System.Windows;
using System.Windows.Input;
using GestMail.CodeBehind;
using GestMail.CodeBehind.Models;

namespace GestMail.UI
{

    public partial class BillsManagerUi 
    {
        private readonly BillsManager _myBillsManager;

        public BillsManagerUi()
        {
            InitializeComponent();
            _myBillsManager = new BillsManager();
            ChkPending.IsChecked = true;
            TbSerie.Text = "F";
            TbYear.Text = DateTime.Now.Year.ToString();
        }

        private void ChkPending_Checked(object sender, RoutedEventArgs e)
        {
            ChkSent.IsChecked = false;
        }

        private void ChkSent_Checked(object sender, RoutedEventArgs e)
        {
            ChkPending.IsChecked = false;
        }

        private void TbYear_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key >= Key.D0 && e.Key <= Key.D9 || e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
                e.Handled = false;
            else
                e.Handled = true;
        }

        private void LvFacturas_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LvFacturas.SelectedItem == null) return;

            DataRowView  selectBill = (DataRowView)LvFacturas.SelectedItem;
            TbSerie.Text = selectBill[0].ToString();
            TbNumber.Text = selectBill[1].ToString();
            TbYear.Text = selectBill[2].ToString();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (TbSerie.Text == string.Empty || TbNumber.Text == string.Empty || TbYear.Text == string.Empty) return;

            Bills deleteBill = new Bills(TbSerie.Text, TbNumber.Text, TbYear.Text);

            if (deleteBill.GetNumber == null)
            {
                LblInfo.Content = "La factura " + TbSerie.Text + "/" + TbNumber.Text + "/" + TbYear.Text + " no existe";
            }
            else
            {
                if (deleteBill.GetSent == "Pendiente")
                {
                    _myBillsManager.Delete(deleteBill);
                    LblInfo.Content = "La factura " + deleteBill.GetSerie + "/" + deleteBill.GetNumber + "/" + deleteBill.GetYear + " se ha eliminado correctamente";
                }
                else LblInfo.Content = "No es posible eliminar facturas enviadas";
            }

            TbSerie.Text = "F";
            TbNumber.Text = "";
            TbYear.Text = DateTime.Now.Year.ToString();
            BtnSearch_Click(sender, null);
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            Customers customer = null;

            LvFacturas.ItemsSource = null;

            if (!string.IsNullOrEmpty(TbSerie.Text) && !string.IsNullOrEmpty(TbNumber.Text) &&
                !string.IsNullOrEmpty(TbYear.Text))
            {
                var factura = _myBillsManager.Search(TbSerie.Text.Trim(), TbNumber.Text.Trim(), TbYear.Text.Trim());
                LvFacturas.ItemsSource = factura.DefaultView;
            }

            else
            {
                DataTable bills = null;

                if (!string.IsNullOrEmpty(TbCustomerCode.Text))
                    customer = new Customers(Convert.ToInt32(TbCustomerCode.Text));

                if ((ChkSent.IsChecked != null && (ChkPending.IsChecked != null && (!ChkPending.IsChecked.Value && !ChkSent.IsChecked.Value))))
                    ChkPending.IsChecked = true;

                if (ChkPending.IsChecked != null && ChkPending.IsChecked.Value)
                {
                    bills = _myBillsManager.ConsultPending(customer);
                }

                if (ChkSent.IsChecked != null && ChkSent.IsChecked.Value)
                {
                    bills = _myBillsManager.ConsultSent(customer);
                }

                if (bills != null) LvFacturas.ItemsSource = bills.DefaultView;
            }
        }
    }
}
