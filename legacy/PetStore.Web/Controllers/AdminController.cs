using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PetStore.DAL;
using PetStore.Filters;
using PetStore.Helpers;
using PetStore.Models;

namespace PetStore.Controllers
{
    public class AdminController : Controller
    {
        private PetStoreContext db = new PetStoreContext();

        [AdminOnly]
        public ActionResult Index()
        {
            ViewBag.PetCount = Utils.CountRows("Pets");
            ViewBag.SoldCount = Convert.ToInt32(Utils.ExecuteScalar("SELECT COUNT(*) FROM Pets WHERE IsSold = 1"));
            ViewBag.OrderCount = Utils.CountRows("Orders");
            ViewBag.UserCount = Utils.CountRows("Users");
            ViewBag.Revenue = Utils.ToDouble(Utils.ExecuteScalar("SELECT SUM(Total) FROM Orders WHERE Status != 'Cancelled'"));
            ViewBag.TaxRate = Utils.GetSetting("TaxRate");
            ViewBag.RecentSearches = MvcApplication.RecentSearches;
            ViewBag.LastError = MvcApplication.LastError;
            ViewBag.Log = Utils.Query("SELECT * FROM AuditLog ORDER BY Id DESC LIMIT 15");
            ViewBag.Title = "Admin";
            return View();
        }

        // the React pet table lives here (Scripts/admin/petTable.jsx)
        [AdminOnly]
        public ActionResult Pets()
        {
            ViewBag.Title = "Manage pets";
            return View();
        }

        // json feed for the React table
        public ActionResult PetsJson()
        {
            var pets = db.Pets.Include(p => p.Category).ToList().Select(p => new
            {
                p.Id, p.Name, p.Breed, p.Age, p.Price, p.IsSold, p.IsFeatured, p.CreatedDate, p.ImageUrl, p.Description,
                Category = p.Category == null ? "?" : p.Category.Name
            });
            return Content(JsonConvert.SerializeObject(pets), "application/json");
        }

        [AdminOnly]
        [HttpPost]
        public ActionResult ToggleSold(int id)
        {
            var pet = db.Pets.Find(id);
            pet.IsSold = !pet.IsSold;
            db.SaveChanges();
            return Json(new { ok = true, isSold = pet.IsSold });
        }

        [AdminOnly]
        public ActionResult CreatePet()
        {
            ViewBag.Categories = db.Categories.ToList();
            ViewBag.Title = "Add pet";
            return View("EditPet", new Pet { CreatedDate = Utils.FormatDate(DateTime.Now) });
        }

        [AdminOnly]
        [HttpPost]
        public ActionResult CreatePet(Pet pet, IFormFile image)
        {
            var url = Utils.SaveUpload(image);
            if (url != null) pet.ImageUrl = url;
            pet.Description = Utils.SanitizeHtml(pet.Description);
            pet.CreatedDate = Utils.FormatDate(DateTime.Now);
            db.Pets.Add(pet);
            db.SaveChanges();
            Utils.Log("created pet " + pet.Name);
            TempData["Message"] = "Pet created";
            return RedirectToAction("Pets");
        }

        [AdminOnly]
        public ActionResult EditPet(int id)
        {
            ViewBag.Categories = db.Categories.ToList();
            ViewBag.Title = "Edit pet";
            return View(db.Pets.Find(id));
        }

        [AdminOnly]
        [HttpPost]
        public ActionResult EditPet(Pet pet, IFormFile image)
        {
            var existing = db.Pets.Find(pet.Id);
            existing.Name = pet.Name;
            existing.CategoryId = pet.CategoryId;
            existing.Breed = pet.Breed;
            existing.Age = pet.Age;
            existing.Price = pet.Price;
            existing.Description = Utils.SanitizeHtml(pet.Description);
            existing.IsFeatured = pet.IsFeatured;
            existing.IsSold = pet.IsSold;
            var url = Utils.SaveUpload(image);
            if (url != null) existing.ImageUrl = url;
            db.SaveChanges();
            Utils.Log("edited pet " + pet.Id);
            TempData["Message"] = "Saved";
            return RedirectToAction("Pets");
        }

        // GET: /Admin/DeletePet/5  (linked from the React table)
        public ActionResult DeletePet(int id)
        {
            var pet = db.Pets.Find(id);
            if (pet != null)
            {
                if (pet.ImageUrl != null && pet.ImageUrl.StartsWith("/uploads")) Utils.DeleteFile(pet.ImageUrl);
                db.Pets.Remove(pet);
                db.SaveChanges();
                Utils.Log("deleted pet " + id);
            }
            TempData["Message"] = "Pet deleted";
            return RedirectToAction("Pets");
        }

        [AdminOnly]
        public ActionResult Categories()
        {
            ViewBag.Title = "Categories";
            return View(db.Categories.OrderBy(c => c.SortOrder).ToList());
        }

        [AdminOnly]
        [HttpPost]
        public ActionResult SaveCategory(int id, string name, string description, int sortOrder)
        {
            if (id == 0)
            {
                Utils.ExecuteSql("INSERT INTO Categories (Name, Description, SortOrder) VALUES ('" + Utils.Esc(name) + "','" + Utils.Esc(description) + "'," + sortOrder + ")");
            }
            else
            {
                var c = db.Categories.Find(id);
                c.Name = name;
                c.Description = description;
                c.SortOrder = sortOrder;
                db.SaveChanges();
            }
            return RedirectToAction("Categories");
        }

        [AdminOnly]
        public ActionResult DeleteCategory(int id)
        {
            Utils.ExecuteSql("DELETE FROM Categories WHERE Id = " + id);
            return RedirectToAction("Categories");
        }

        [AdminOnly]
        public ActionResult Supplies()
        {
            ViewBag.Title = "Inventory";
            return View(db.Supplies.ToList().OrderBy(s => s.Stock).ToList());
        }

        [AdminOnly]
        [HttpPost]
        public ActionResult UpdateStock(int id, int stock, decimal price, int active)
        {
            var s = db.Supplies.Find(id);
            s.Stock = stock;
            s.Price = price;
            s.Active = active;
            db.SaveChanges();
            TempData["Message"] = s.SupplyName + " updated";
            return RedirectToAction("Supplies");
        }

        [AdminOnly]
        public ActionResult Orders(string status)
        {
            var orders = db.Orders.ToList();
            if (!string.IsNullOrEmpty(status)) orders = orders.Where(o => o.Status == status).ToList();
            var users = new Dictionary<int, string>();
            foreach (var o in orders)
            {
                if (!users.ContainsKey(o.UserId))
                {
                    var u = db.Users.Find(o.UserId);
                    users[o.UserId] = u == null ? "(deleted)" : u.Username;
                }
            }
            ViewBag.Users = users;
            ViewBag.Status = status;
            ViewBag.Title = "Orders";
            return View(orders.OrderByDescending(o => o.Id).ToList());
        }

        // GET: /Admin/UpdateOrderStatus?id=1&status=Shipped
        public ActionResult UpdateOrderStatus(int id, string status)
        {
            Utils.ExecuteSql("UPDATE Orders SET Status = '" + status + "' WHERE Id = " + id);
            Utils.Log("order " + id + " -> " + status);
            return RedirectToAction("Orders");
        }
    }
}
