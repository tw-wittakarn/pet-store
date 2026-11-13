using System;
using System.Collections.Generic;

namespace PetStore.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }
        public bool ShowRequestId { get { return !string.IsNullOrEmpty(RequestId); } }
    }

    public class CheckoutViewModel
    {
        public string ShipName { get; set; }
        public string ShipAddress { get; set; }
        public string ShipCity { get; set; }
        public string ShipZip { get; set; }
        public string CardNumber { get; set; }
        public string CardExpiry { get; set; }
        public string CardCvv { get; set; }
        public string Notes { get; set; }
        public List<CartItem> Items { get; set; }
        public double SubTotal { get; set; }
        public double Shipping { get; set; }
        public double Tax { get; set; }
        public double Total { get; set; }
    }

    // used by admin pages AND by the pets index page (don't change!!)
    public class PetListModel
    {
        public List<Pet> Pets { get; set; }
        public List<Category> Categories { get; set; }
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public string Search { get; set; }
        public int? CategoryId { get; set; }
        public string Sort { get; set; }
        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
    }
}
