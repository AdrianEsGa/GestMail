using System;
using System.Data;
using System.Windows;
using System.Windows.Input;
using GestMail.CodeBehind;
using GestMail.CodeBehind.Models;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace GestMail.UI
{
    public partial class ShipmentManagerUi 
    {
        private readonly ShipmentsManager _myShipmentsManager;
     
        public ShipmentManagerUi()
        {
            InitializeComponent();
            _myShipmentsManager = new ShipmentsManager();
            DtPickDateSince.SelectedDate = DateTime.Now;
            DtPickDateUntil.SelectedDate = DateTime.Now;
        }

        private void ChkPending_Checked(object sender, RoutedEventArgs e)
        {
            ChkSent.IsChecked = false;
        }

        private void ChkSent_Checked(object sender, RoutedEventArgs e)
        {
            ChkPending.IsChecked = false;
        }

        private void LvShipping_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (LvShipping.Items.Count == 0) return;

            LvShippingBills.ItemsSource = null;
            var selectShippments = LvShipping.SelectedItems;

            if (ChkPending.IsChecked == true)
            {
                foreach (DataRowView  select in selectShippments)
                {
                    LvShippingBills.ItemsSource =
                        _myShipmentsManager.GetPendingShippingBills(Convert.ToInt32(select["IdCliente"])).DefaultView;
                }
            }

            if (ChkSent.IsChecked == true)
            {
                foreach (DataRowView select in selectShippments)
                {
                    LvShippingBills.ItemsSource =
                        _myShipmentsManager.GetSentShippingBills(Convert.ToInt32(select["IdEnvio"])).DefaultView;
                }
            }
        }

        private void TbCustomerCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key >= Key.D0 && e.Key <= Key.D9 || e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
                e.Handled = false;
            else
                e.Handled = true;
        }

        private void BtnSearch_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Customers customer = null;
            LvShipping.ItemsSource = null;
            LvShippingBills.ItemsSource = null;

            if (!string.IsNullOrEmpty(TbCustomerCode.Text))
                customer = new Customers(Convert.ToInt32(TbCustomerCode.Text));

            if (ChkPending.IsChecked == false && ChkSent.IsChecked == false)
                ChkPending.IsChecked = true;

            if (ChkPending.IsChecked == true)
            {
                LvShipping.ItemsSource = _myShipmentsManager.ConsultPending(customer).DefaultView;
            }

            if (ChkSent.IsChecked == true)
            {
                LvShipping.ItemsSource =
                    _myShipmentsManager.ConsultSent(customer, DtPickDateSince.SelectedDate, DtPickDateUntil.SelectedDate)
                        .DefaultView;
            }
        }

 
    }
}
