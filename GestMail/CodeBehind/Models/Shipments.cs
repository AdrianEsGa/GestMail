using System;
using System.Collections.Generic;


namespace GestMail.CodeBehind.Models
{
    public class Shipments
    {
        private readonly Customers _customer;
        private readonly IEnumerable<Bills> _bills;
        private readonly DateTime _date;

        public Shipments(Customers customer, IEnumerable<Bills> bills, DateTime date) 
        {
            _customer = customer;
            _bills = bills;
            _date = date;
        }

        public Customers GetCustomer { get { return _customer; } }
        public IEnumerable<Bills> GetBills { get { return _bills; } }
        public DateTime GetDate {
            get { return _date; }
        }
    }
}
