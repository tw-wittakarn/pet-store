using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PetStore.DAL;
using PetStore.Helpers;
using PetStore.Models;

namespace PetStore.Controllers
{
    public class CartRequest
    {
        public string type { get; set; }
        public int id { get; set; }
        public int qty { get; set; }
    }

    // JSON api for the React cart (2020). Not using Utils cart functions because they redirect/return strings.
    [Route("api/cart")]
    public class CartApiController : Controller
    {
        private PetStoreContext db = new PetStoreContext();

        private List<CartItem> Load()
        {
            var json = HttpContext.Session.GetString("Cart");
            if (json == null) return new List<CartItem>();
            return JsonConvert.DeserializeObject<List<CartItem>>(json);
        }

        private void Save(List<CartItem> items)
        {
            HttpContext.Session.SetString("Cart", JsonConvert.SerializeObject(items));
        }

        private ContentResult CartJson(List<CartItem> items)
        {
            double sub = 0;
            foreach (var i in items) sub += i.Price * i.Qty;
            var shipping = Utils.GetShipping(sub);
            if (items.Count == 0) shipping = 0;
            var tax = sub * Utils.TAX;
            var result = new
            {
                items = items,
                subTotal = sub,
                shipping = shipping,
                tax = tax,
                total = sub + shipping + tax,
                count = items.Count
            };
            return Content(JsonConvert.SerializeObject(result), "application/json");
        }

        [HttpGet("")]
        public ActionResult Get()
        {
            return CartJson(Load());
        }

        [HttpPost("")]
        public ActionResult Add([FromBody] CartRequest req)
        {
            var items = Load();
            if (req.type == "PET" || req.type == "pet")
            {
                var pet = db.Pets.Find(req.id);
                if (pet == null) return NotFound();
                if (!items.Any(i => i.Type == req.type && i.Id == req.id))
                    items.Add(new CartItem { Type = req.type, Id = pet.Id, Name = pet.Name, Price = pet.Price, Qty = 1, Image = pet.ImageUrl });
            }
            else
            {
                var s = db.Supplies.Find(req.id);
                if (s == null) return NotFound();
                var existing = items.FirstOrDefault(i => i.Type == req.type && i.Id == req.id);
                if (existing != null) existing.Qty += req.qty;
                else items.Add(new CartItem { Type = req.type, Id = s.SupplyId, Name = s.SupplyName, Price = (double)s.Price, Qty = req.qty, Image = s.ImagePath });
            }
            Save(items);
            return CartJson(items);
        }

        [HttpPut("")]
        public ActionResult Update([FromBody] CartRequest req)
        {
            var items = Load();
            var item = items.FirstOrDefault(i => i.Type == req.type && i.Id == req.id);
            if (item != null)
            {
                item.Qty = req.qty;
                if (item.Qty <= 0) items.Remove(item);
            }
            Save(items);
            return CartJson(items);
        }

        [HttpDelete("")]
        public ActionResult Remove(string type, int id)
        {
            var items = Load();
            items = items.Where(i => !(i.Type == type && i.Id == id)).ToList();
            Save(items);
            return CartJson(items);
        }
    }
}
