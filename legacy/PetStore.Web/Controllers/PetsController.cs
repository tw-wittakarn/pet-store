using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetStore.DAL;
using PetStore.Helpers;
using PetStore.Models;

namespace PetStore.Controllers
{
    public class PetsController : Controller
    {
        private PetStoreContext db = new PetStoreContext();

        // GET: /Pets?categoryId=1&search=bud&minPrice=10&maxPrice=500&sort=price&page=1
        public ActionResult Index(int? categoryId, string search, double? minPrice, double? maxPrice, string sort, int page = 1)
        {
            string where = " WHERE 1=1";
            if (categoryId.HasValue && categoryId > 0)
            {
                where += " AND CategoryId = " + categoryId;
            }
            if (!string.IsNullOrEmpty(search))
            {
                where += " AND (Name LIKE '%" + search + "%' OR Breed LIKE '%" + search + "%')";
                Utils.AddRecentSearch(search);
            }
            if (minPrice.HasValue) where += " AND Price >= " + minPrice;
            if (maxPrice.HasValue) where += " AND Price <= " + maxPrice;

            string orderBy;
            switch (sort)
            {
                case "price": orderBy = " ORDER BY Price"; break;
                case "price_desc": orderBy = " ORDER BY Price DESC"; break;
                case "name": orderBy = " ORDER BY Name"; break;
                default: orderBy = " ORDER BY IsSold, Id"; break;
            }

            var total = Convert.ToInt32(Utils.ExecuteScalar("SELECT COUNT(*) FROM Pets" + where));
            var totalPages = total / Utils.PAGE_SIZE;
            if (totalPages == 0) totalPages = 1;
            if (page < 1) page = 1;

            var sql = "SELECT * FROM Pets" + where + orderBy + " LIMIT " + Utils.PAGE_SIZE + " OFFSET " + ((page - 1) * Utils.PAGE_SIZE);
            var dt = Utils.GetDataTable(sql);

            var pets = new List<Pet>();
            foreach (DataRow r in dt.Rows)
            {
                pets.Add(Utils.RowToPet(r));
            }

            var model = new PetListModel
            {
                Pets = pets,
                Categories = db.Categories.OrderBy(c => c.SortOrder).ToList(),
                Page = page,
                TotalPages = totalPages,
                Search = search,
                CategoryId = categoryId,
                Sort = sort,
                MinPrice = minPrice,
                MaxPrice = maxPrice
            };

            ViewBag.Title = categoryId.HasValue ? Utils.GetCategoryName(categoryId.Value) : "All Pets";
            ViewBag.ResultCount = total;
            return View(model);
        }

        // GET: /Pets/Details/5   (also /pet.aspx?id=5)
        public ActionResult Details(int? id)
        {
            if (id == null && Request.Query.ContainsKey("id"))
            {
                id = int.Parse(Request.Query["id"]);
            }
            if (id == null) return RedirectToAction("Index");

            var pet = db.Pets.Include(p => p.Category).FirstOrDefault(p => p.Id == id);
            if (pet == null)
            {
                TempData["Error"] = "Pet not found";
                return RedirectToAction("Index");
            }

            // related pets - same category
            var related = db.Pets.ToList().Where(p => p.CategoryId == pet.CategoryId && p.Id != pet.Id && !p.IsSold).Take(3).ToList();

            ViewBag.Related = related;
            ViewBag.DiscountedPrice = Utils.ApplyDiscount(pet.Price, pet.Category.Name);
            ViewBag.Title = pet.Name;
            return View(pet);
        }
    }
}
