using System;

namespace PetStore.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public int IsAdmin { get; set; } // 1 = admin
        public DateTime CreatedAt { get; set; }
    }
}
