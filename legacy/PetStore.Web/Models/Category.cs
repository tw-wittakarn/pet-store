using System;
using System.Collections.Generic;

namespace PetStore.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }

        // NOTE: lazy loading is not on, so this is usually null. Don't rely on it.
        public virtual List<Pet> Pets { get; set; }
    }
}
