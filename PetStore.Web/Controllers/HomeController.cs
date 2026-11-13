using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using PetStore.DAL;
using PetStore.Helpers;
using PetStore.Models;

namespace PetStore.Controllers
{
    public class HomeController : Controller
    {
        private PetStoreContext db = new PetStoreContext();

        public ActionResult Index()
        {
            if (Utils.IsMaintenanceMode())
            {
                return Content("<h1>We'll be right back!</h1>", "text/html");
            }

            // featured pets (raw sql is faster than EF)
            var dt = Utils.GetDataTable("SELECT * FROM Pets WHERE IsFeatured = 1 AND IsSold = 0 ORDER BY Id DESC LIMIT 6");
            var featured = new List<Pet>();
            foreach (DataRow r in dt.Rows)
            {
                featured.Add(Utils.RowToPet(r));
            }

            var cats = db.Categories.OrderBy(c => c.SortOrder).ToList();
            var counts = new Dictionary<int, int>();
            foreach (var c in cats)
            {
                counts[c.Id] = db.Pets.Count(p => p.CategoryId == c.Id && !p.IsSold);
            }

            var lowStock = db.Supplies.Where(s => s.Active == 1 && s.Stock > 0).ToList().Where(s => s.Stock < 5).ToList();

            ViewBag.Featured = featured;
            ViewBag.Categories = cats;
            ViewBag.Counts = counts;
            ViewBag.LowStock = lowStock;
            ViewBag.Greeting = Utils.GetGreeting();
            ViewBag.Title = "Home";
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Title = "About us";
            ViewBag.Since = 2009;
            ViewBag.PetCount = Utils.CountRows("Pets");
            ViewBag.Uptime = (DateTime.Now - MvcApplication.StartedAt).ToString();
            return View();
        }

        public ActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
