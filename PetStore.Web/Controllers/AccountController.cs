using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PetStore.DAL;
using PetStore.Filters;
using PetStore.Helpers;
using PetStore.Models;

namespace PetStore.Controllers
{
    public class AccountController : Controller
    {
        private PetStoreContext db = new PetStoreContext();

        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Title = "Login";
            return View();
        }

        [HttpPost]
        public ActionResult Login(string username, string password, string returnUrl)
        {
            var hash = Utils.HashPassword(password);
            var dt = Utils.GetDataTable("SELECT * FROM Users WHERE Username = '" + username + "' AND PasswordHash = '" + hash + "'");

            if (dt.Rows.Count == 0)
            {
                ViewBag.Error = "Invalid username or password";
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.Title = "Login";
                return View();
            }

            var r = dt.Rows[0];
            var user = new User
            {
                Id = Convert.ToInt32(r["Id"]),
                Username = r["Username"].ToString(),
                Email = r["Email"].ToString(),
                FullName = r["FullName"].ToString(),
                IsAdmin = Convert.ToInt32(r["IsAdmin"])
            };
            Utils.SetLogin(user);

            if (!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
            if (user.IsAdmin == 1) return Redirect("/Admin");
            return Redirect("/");
        }

        public ActionResult Logout()
        {
            Utils.ClearLogin();
            return Redirect("/");
        }

        public ActionResult Register()
        {
            ViewBag.Title = "Register";
            return View(new User());
        }

        [HttpPost]
        public ActionResult Register(User user, string password, string confirmPassword)
        {
            ViewBag.Title = "Register";
            if (!Utils.IsValidUsername(user.Username)) ViewBag.Error = "Username must be 3-20 characters";
            else if (!Utils.IsValidEmail(user.Email)) ViewBag.Error = "Email is invalid";
            else if (!Utils.IsStrongPassword(password)) ViewBag.Error = "Password too short";
            else if (password != confirmPassword) ViewBag.Error = "Passwords do not match";
            else if (db.Users.Any(u => u.Username == user.Username)) ViewBag.Error = "Username already taken";

            if (ViewBag.Error != null) return View(user);

            user.PasswordHash = Utils.HashPassword(password);
            user.CreatedAt = DateTime.Now;
            db.Users.Add(user);
            db.SaveChanges();

            Utils.SetLogin(user);
            Utils.SendEmail(user.Email, "Welcome!", "Hi " + user.FullName + ", your password is " + password);
            TempData["Message"] = "Welcome to the family, " + user.Username + "!";
            return Redirect("/");
        }

        [MustLogin]
        public ActionResult MyOrders()
        {
            var userId = Utils.GetCurrentUserId();
            var orders = db.Orders.Where(o => o.UserId == userId).OrderByDescending(o => o.OrderDate).ToList();
            foreach (var o in orders)
            {
                o.Lines = db.OrderLines.Where(l => l.OrderId == o.Id).ToList();
            }
            ViewBag.Title = "My orders";
            return View(orders);
        }
    }
}
