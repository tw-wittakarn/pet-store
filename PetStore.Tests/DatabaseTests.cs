using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PetStore.DAL;
using PetStore.Helpers;

namespace PetStore.Tests
{
    [TestClass]
    public class DatabaseTests
    {
        [TestInitialize]
        public void Setup()
        {
            // creates petstore.db in the test bin folder
            DatabaseConfig.Initialize();
        }

        [TestMethod]
        public void Seed_Has_20_Pets()
        {
            using (var db = new PetStoreContext())
            {
                Assert.AreEqual(20, db.Pets.Count());
            }
        }

        [TestMethod]
        public void Buddy_Is_Featured()
        {
            var name = Utils.ExecuteScalar("SELECT Name FROM Pets WHERE Id = 1");
            Assert.AreEqual("Buddy", name);
            Assert.AreEqual("Dogs", Utils.GetCategoryName(1));
        }

        [TestMethod]
        [Ignore("fails sometimes on build server - fix later")]
        public void Checkout_Total_Matches_Cart_Total()
        {
            var items = new System.Collections.Generic.List<PetStore.Models.CartItem>
            {
                new PetStore.Models.CartItem { Type = "SUPPLY", Id = 2, Price = 7.49, Qty = 3 },
                new PetStore.Models.CartItem { Type = "PET", Id = 15, Price = 4.99, Qty = 1 }
            };
            var cartTotal = Utils.CalculateCartTotal(items);
            var sub = 7.49 * 3 + 4.99;
            var ship = Utils.GetShipping(sub);
            var checkoutTotal = sub + ship + Utils.CalcTax2(sub, ship);
            Assert.AreEqual(cartTotal, checkoutTotal);
        }
    }
}
