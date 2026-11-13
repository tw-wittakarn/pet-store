using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using PetStore.Helpers;

namespace PetStore.Controllers
{
    public class CartController : Controller
    {
        // GET: /Cart  (the page itself is rendered by React now, see ClientApp/)
        public ActionResult Index()
        {
            var cart = Utils.GetCart();
            ViewBag.Cart = cart;
            ViewBag.SubTotal = Utils.CartSubTotal();
            ViewBag.Shipping = Utils.CalculateShipping(Utils.CartSubTotal());
            ViewBag.Tax = Utils.CalculateTax(cart);
            ViewBag.Total = Utils.CalculateCartTotal(cart);
            ViewBag.Title = "Your cart";
            return View();
        }

        // POST: /Cart/Add  (jQuery forms on catalog pages)
        [HttpPost]
        public ActionResult Add(string type, int id, int qty = 1, string returnUrl = null)
        {
            var error = Utils.AddToCart(type, id, qty);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { ok = error == null, message = error ?? "Added to cart!", count = Utils.CartCount() });
            }

            if (error != null) TempData["Error"] = error;
            else TempData["Message"] = "Added to cart!";

            if (!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer)) return Redirect(referer);
            return RedirectToAction("Index");
        }

        // GET: /Cart/Remove?type=PET&id=1
        public ActionResult Remove(string type, int id)
        {
            Utils.RemoveFromCart(type, id);
            TempData["Message"] = "Item removed";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Update(string type, int id, int qty)
        {
            Utils.UpdateCartQty(type, id, qty);
            return RedirectToAction("Index");
        }

        public ActionResult Clear()
        {
            Utils.ClearCart();
            return RedirectToAction("Index");
        }

        // used by the header badge (jQuery polls this)
        public ActionResult Count()
        {
            return Content(Utils.CartCount().ToString());
        }
    }
}
