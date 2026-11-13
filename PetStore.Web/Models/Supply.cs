using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetStore.Models
{
    // Supplies were added in 2017 by a different team, copied from Pet and changed a bit.
    [Table("Supplies")]
    public class Supply
    {
        [Key]
        public int SupplyId { get; set; }

        public string SupplyName { get; set; }

        // category id (same Categories table as pets, "CatId" because of the old Access db)
        public int CatId { get; set; }

        public decimal Price { get; set; }

        public int Stock { get; set; }

        public string Desc { get; set; }

        public string ImagePath { get; set; }

        public string Brand { get; set; }

        public int Active { get; set; } // 1 = active, 0 = hidden, 2 = discontinued(?)

        [NotMapped]
        public string DisplayPrice { get { return string.Format("${0:N2}", Price); } }
    }
}
