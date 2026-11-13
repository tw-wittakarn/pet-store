using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using PetStore.DAL;
using PetStore.Helpers;
using PetStore.Models;

namespace PetStore.Controllers
{
    // copied from PetsController and switched to EF because raw sql was "too hard to maintain"
    public class SuppliesController : Controller
    {
        PetStoreContext _db = new PetStoreContext();

        public ActionResult Index(int? catId, string q, string sort, int page = 1)
        {
            // sqlite can't sort decimals server side so load everything
            var all = _db.Supplies.Where(s => s.Active == 1).ToList();

            if (catId != null && catId != 0)
            {
                all = all.Where(s => s.CatId == catId).ToList();
            }
            if (q != null && q != "")
            {
                all = all.Where(s => s.SupplyName.Contains(q) || (s.Brand != null && s.Brand.Contains(q))).ToList();
                Utils.AddRecentSearch(q);
            }

            if (sort == "price") all = all.OrderBy(s => s.Price).ToList();
            else if (sort == "price_desc") all = all.OrderByDescending(s => s.Price).ToList();
            else all = all.OrderBy(s => s.SupplyName).ToList();

            var pageSize = 8;
            var totalPages = (int)Math.Ceiling(all.Count / (double)pageSize);
            var items = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Categories = _db.Categories.ToList();
            ViewBag.CatId = catId;
            ViewBag.Q = q;
            ViewBag.Sort = sort;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Title = "Supplies";
            return View(items);
        }

        public ActionResult Details(int id)
        {
            var s = _db.Supplies.Find(id);
            if (s == null) return NotFound();
            ViewBag.CategoryName = Utils.GetCategoryName(s.CatId);
            ViewBag.Title = s.SupplyName;
            return View(s);
        }
    }
}
