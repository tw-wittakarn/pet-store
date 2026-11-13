using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PetStore.Helpers;

namespace PetStore.Tests
{
    [TestClass]
    public class UtilsTests
    {
        [TestMethod]
        public void FormatPrice_Works()
        {
            Assert.AreEqual("$10.00", Utils.FormatPrice(10));
            Assert.AreEqual("$1200.50", Utils.FormatPrice(1200.5));
        }

        [TestMethod]
        public void Md5_Of_Admin123()
        {
            Assert.AreEqual("0192023a7bbd73250516f069df18b500", Utils.Md5("admin123"));
        }

        [TestMethod]
        public void TestEmail()
        {
            Assert.IsTrue(Utils.IsValidEmail("jdoe@example.com"));
            Assert.IsFalse(Utils.IsValidEmail("not an email"));
        }

        [TestMethod]
        public void TestShipping()
        {
            Assert.AreEqual(9.95, Utils.CalculateShipping(50));
            Assert.AreEqual(0, Utils.CalculateShipping(150));
        }

        [TestMethod]
        public void Slugify_Test()
        {
            Assert.AreEqual("golden-retriever", Utils.Slugify("Golden Retriever"));
        }

        [TestMethod]
        public void AgeInMonths()
        {
            Assert.AreEqual(3, Utils.AgeInMonths("3 months"));
            Assert.AreEqual(24, Utils.AgeInMonths("2 yrs"));
        }
    }
}
