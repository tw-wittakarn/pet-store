using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetStore.DAL;
using PetStore.Helpers;
using PetStore.Models;

namespace PetStore.Controllers
{
    public class CheckoutController : Controller
    {
        private PetStoreContext db = new PetStoreContext();

        public ActionResult Index()
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return Redirect("/Account/Login?returnUrl=/Checkout");
            }

            var cart = Utils.GetCart();
            if (cart.Count == 0)
            {
                TempData["Error"] = "Your cart is empty";
                return RedirectToAction("Index", "Cart");
            }

            var userId = int.Parse(HttpContext.Session.GetString("UserId"));
            var user = db.Users.Find(userId);

            var m = new CheckoutViewModel();
            m.Items = cart;
            m.ShipName = user.FullName;
            m.ShipAddress = user.Address;
            m.ShipCity = user.City;
            m.ShipZip = user.Zip;
            m.SubTotal = cart.Sum(c => c.Price * c.Qty);
            m.Shipping = Utils.GetShipping(m.SubTotal);
            m.Tax = Utils.CalcTax2(m.SubTotal, m.Shipping);
            m.Total = m.SubTotal + m.Shipping + m.Tax;

            ViewBag.Title = "Checkout";
            return View(m);
        }

        [HttpPost]
        public ActionResult PlaceOrder(CheckoutViewModel m)
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return Redirect("/Account/Login?returnUrl=/Checkout");
            }
            var userId = int.Parse(HttpContext.Session.GetString("UserId"));

            var cart = Utils.GetCart();
            m.Items = cart;
            if (cart.Count == 0) return RedirectToAction("Index", "Cart");

            // validation
            var errors = new List<string>();
            if (string.IsNullOrEmpty(m.ShipName)) errors.Add("Name is required");
            if (string.IsNullOrEmpty(m.ShipAddress)) errors.Add("Address is required");
            if (string.IsNullOrEmpty(m.ShipCity)) errors.Add("City is required");
            if (!Utils.IsValidZip(m.ShipZip)) errors.Add("Postcode must be 4 digits");
            if (!Utils.IsValidCard(m.CardNumber)) errors.Add("Card number is invalid");
            if (!Utils.IsValidExpiry(m.CardExpiry)) errors.Add("Card expiry is invalid (MM/YY)");
            if (string.IsNullOrEmpty(m.CardCvv) || m.CardCvv.Length < 3) errors.Add("CVV is invalid");

            // totals
            double subTotal = 0;
            foreach (var item in cart)
            {
                subTotal = subTotal + item.Price * item.Qty;
            }
            double shipping = Utils.GetShipping(subTotal);
            double tax = Utils.CalcTax2(subTotal, shipping);
            double total = subTotal + shipping + tax;
            m.SubTotal = subTotal; m.Shipping = shipping; m.Tax = tax; m.Total = total;

            // stock check
            foreach (var item in cart)
            {
                if (item.Type.ToUpper() == "SUPPLY")
                {
                    var stock = Utils.GetSupplyStock(item.Id);
                    if (stock <= 0) errors.Add(item.Name + " is out of stock");
                }
            }

            if (errors.Count > 0)
            {
                ViewBag.Errors = errors;
                ViewBag.Title = "Checkout";
                return View("Index", m);
            }

            // take the money
            var approved = Utils.ChargeCard(m.CardNumber, total);
            if (!approved)
            {
                ViewBag.Errors = new List<string> { "Your card was declined" };
                ViewBag.Title = "Checkout";
                return View("Index", m);
            }

            var order = new Order
            {
                UserId = userId,
                OrderDate = Utils.FormatDateTime(DateTime.Now),
                SubTotal = subTotal,
                Shipping = shipping,
                Tax = tax,
                Total = total,
                Status = "Paid",
                ShipName = m.ShipName,
                ShipAddress = m.ShipAddress,
                ShipCity = m.ShipCity,
                ShipZip = m.ShipZip,
                CardNumber = m.CardNumber,
                CardExpiry = m.CardExpiry,
                Notes = m.Notes,
                Lines = new List<OrderLine>()
            };
            foreach (var item in cart)
            {
                order.Lines.Add(new OrderLine
                {
                    ItemType = item.Type.ToUpper(),
                    ItemId = item.Id,
                    ItemName = item.Name,
                    Qty = item.Qty,
                    UnitPrice = item.Price
                });
            }
            db.Orders.Add(order);
            db.SaveChanges();

            // update inventory
            foreach (var item in cart)
            {
                if (item.Type.ToUpper() == "PET")
                {
                    Utils.MarkPetSold(item.Id);
                }
                else
                {
                    var stock = Utils.GetSupplyStock(item.Id);
                    Utils.SetSupplyStock(item.Id, stock - item.Qty);
                }
            }

            Utils.SendOrderConfirmation(order, HttpContext.Session.GetString("Email"));
            Utils.Log("order placed " + order.Id + " total " + total);
            Utils.ClearCart();

            return RedirectToAction("Confirmation", new { id = order.Id });
        }

        public ActionResult Confirmation(int id)
        {
            var order = db.Orders.Include(o => o.Lines).FirstOrDefault(o => o.Id == id);
            if (order == null) return RedirectToAction("Index", "Home");
            ViewBag.OrderNumber = "PS-" + id.ToString("00000");
            ViewBag.Delivery = Utils.EstimatedDelivery();
            ViewBag.Title = "Thank you!";
            return View(order);
        }
    }
}
