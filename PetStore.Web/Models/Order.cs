using System;
using System.Collections.Generic;

namespace PetStore.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string OrderDate { get; set; } // formatted with Utils.FormatDate
        public double SubTotal { get; set; }
        public double Shipping { get; set; }
        public double Tax { get; set; }
        public double Total { get; set; }
        public string Status { get; set; } // "New", "Paid", "Shipped", "Cancelled", "cancelled"
        public string ShipName { get; set; }
        public string ShipAddress { get; set; }
        public string ShipCity { get; set; }
        public string ShipZip { get; set; }
        public string CardNumber { get; set; }
        public string CardExpiry { get; set; }
        public string Notes { get; set; }

        public virtual List<OrderLine> Lines { get; set; }
    }

    public class OrderLine
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string ItemType { get; set; } // "PET" or "SUPPLY"
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int Qty { get; set; }
        public double UnitPrice { get; set; }
    }
}
