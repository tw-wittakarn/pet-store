using System;

namespace PetStore.Models
{
    [Serializable]
    public class CartItem
    {
        public string Type { get; set; } // "PET" / "SUPPLY" / sometimes "pet" from the api
        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public int Qty { get; set; }
        public string Image { get; set; }

        public double LineTotal { get { return Price * Qty; } }
    }
}
