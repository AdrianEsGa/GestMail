using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using GestMail.CodeBehind;
using GestMail.CodeBehind.Models;


namespace GestMail.UI
{

    public partial class CustomersManagerUi
    {
        private readonly CustomersManager _myCustomersManager;

        public CustomersManagerUi()
        {
            InitializeComponent();
            _myCustomersManager = new CustomersManager();
        }

        private void ControlsClear()
        {
            TbCode.Text = string.Empty;
            TbName.Text = string.Empty;
            TbEmail1.Text = string.Empty;
            TbEmail2.Text = string.Empty;
            TbEmail3.Text = string.Empty;
            LvCustomers.Items.Clear();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TbCode.Text)) return;
          
            if (!EmailsValidate())
            {
                LblInfo.Content = Properties.Resources.NO_VALID_EMAIL_FORMAT;
                return;
            }

            var customer = new Customers(Convert.ToInt32(TbCode.Text), TbName.Text, TbEmail1.Text, TbEmail2.Text, TbEmail3.Text);

            if (_myCustomersManager.Exists(customer))
                _myCustomersManager.Update(customer);
            else
                _myCustomersManager.Create(customer);

            LblInfo.Content = Properties.Resources.CORRECT_SAVE;

            ControlsClear();
        }

        private bool EmailsValidate()
        {
            var isMail1 = true; 
            var isMail2 = true; 
            var isMail3 = true;

            if(!string.IsNullOrEmpty(TbEmail1.Text))
                isMail1 = Regex.IsMatch(TbEmail1.Text, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);

            if (!string.IsNullOrEmpty(TbEmail2.Text))
                isMail2 = Regex.IsMatch(TbEmail2.Text, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);

            if (!string.IsNullOrEmpty(TbEmail3.Text))
                isMail3 = Regex.IsMatch(TbEmail3.Text, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);


            return isMail3 && isMail2 && isMail1;
        }

        private void LvCustomers_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(LvCustomers.SelectedItem == null) return;

            var selectCliente = (Customers) LvCustomers.SelectedItem;
            TbCode.Text = selectCliente.GetCode.ToString();
            TbName.Text = selectCliente.GetName;
            TbEmail1.Text = selectCliente.GetEmails[0];
            TbEmail2.Text = selectCliente.GetEmails[1];
            TbEmail3.Text = selectCliente.GetEmails[2];

        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            ControlsClear();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TbCode.Text)) return;

            var cliente = new Customers(Convert.ToInt32(TbCode.Text), TbName.Text, "","","");

            if (!_myCustomersManager.ItHasBills(cliente))
            {
                _myCustomersManager.Delete(cliente);
                LblInfo.Content = Properties.Resources.CORRECT_DELETE;
            }
            else LblInfo.Content = Properties.Resources.NO_POSIBLE_DELETE_CUSTOMER;

            ControlsClear();

        }

        private void TbId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key >= Key.D0 && e.Key <= Key.D9 || e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
                e.Handled = false;
            else
                e.Handled = true;
        }

        private void BtnSearch_MouseDown(object sender, MouseButtonEventArgs e)
        {
            LvCustomers.Items.Clear();
            List<Customers> customers;

            LblInfo.Content = string.Empty;

            if (string.IsNullOrEmpty(TbCode.Text) && string.IsNullOrEmpty(TbName.Text))
                customers = _myCustomersManager.SearchAll();
            else customers = _myCustomersManager.Search(TbCode.Text, TbName.Text);

            foreach (var customer in customers)
            {
                LvCustomers.Items.Add(customer);
            }
        }

    }
}
