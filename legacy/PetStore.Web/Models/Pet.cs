using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetStore.Models
{
    public class Pet
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }

        public string Breed { get; set; }

        // age is free text e.g. "3 months", "2 yrs", "1.5 years" - TODO normalise
        public string Age { get; set; }

        public double Price { get; set; }

        // description can contain HTML (marketing team wanted bold text)
        public string Description { get; set; }

        public string ImageUrl { get; set; }

        public bool IsSold { get; set; }

        public bool IsFeatured { get; set; }

        // stored as string because the old import tool sent "dd/MM/yyyy"
        public string CreatedDate { get; set; }

        [NotMapped]
        public string DisplayPrice { get { return "$" + Price.ToString("0.00"); } }

        [NotMapped]
        public bool IsNew
        {
            get
            {
                try
                {
                    var d = DateTime.ParseExact(CreatedDate, "dd/MM/yyyy", null);
                    return (DateTime.Now - d).TotalDays < 30;
                }
                catch { return false; }
            }
        }
    }
}
