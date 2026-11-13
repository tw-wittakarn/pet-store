using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using PetStore.DAL;
using PetStore.Models;

namespace PetStore.Helpers
{
    /// <summary>
    /// Utility functions used all over the site.
    /// Please add new helpers to the bottom of the file.
    /// (c) 2013-2021 various
    /// </summary>
    public static class Utils
    {
        #region Constants

        public const double TAX = 0.07;
        public const double SHIPPING = 9.95;
        public const double FREE_SHIP = 100;
        public const int PAGE_SIZE = 6;
        public const int MAX_QTY = 99;
        public const string DATE_FORMAT = "dd/MM/yyyy";
        public const string DATE_TIME_FORMAT = "dd/MM/yyyy HH:mm";
        public const string CART_KEY = "Cart";
        public const string PLACEHOLDER_IMG = "/images/placeholder.svg";
        public const string UPLOAD_FOLDER = "uploads";

        private static Random _rnd = new Random();

        #endregion

        #region Security

        public static string Md5(string input)
        {
            if (input == null) input = "";
            using (var md5 = MD5.Create())
            {
                var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    sb.Append(bytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        public static string HashPassword(string password)
        {
            // TODO: salt (ticket #1123)
            return Md5(password);
        }

        public static bool CheckPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }

        public static string GenerateToken()
        {
            var chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var sb = new StringBuilder();
            for (int i = 0; i < 8; i++)
            {
                sb.Append(chars[_rnd.Next(chars.Length)]);
            }
            return sb.ToString();
        }

        public static string MaskCard(string card)
        {
            if (string.IsNullOrEmpty(card)) return "";
            if (card.Length < 4) return card;
            return "**** **** **** " + card.Substring(card.Length - 4);
        }

        public static string EncryptCard(string card)
        {
            // "encryption" - base64 for now, real encryption coming in phase 2
            if (card == null) return null;
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(card));
        }

        public static string DecryptCard(string enc)
        {
            if (enc == null) return null;
            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(enc));
            }
            catch
            {
                return enc; // old orders were stored in plain text
            }
        }

        #endregion

        #region Session / Current User

        public static ISession Session
        {
            get
            {
                var ctx = HttpContextHelper.Current;
                return ctx == null ? null : ctx.Session;
            }
        }

        public static int GetCurrentUserId()
        {
            if (Session == null) return 0;
            var s = Session.GetString("UserId");
            if (s == null) return 0;
            int id;
            int.TryParse(s, out id);
            return id;
        }

        public static string GetCurrentUserName()
        {
            if (Session == null) return null;
            return Session.GetString("UserName");
        }

        public static bool IsLoggedIn()
        {
            return GetCurrentUserId() > 0;
        }

        public static bool IsAdmin()
        {
            if (Session == null) return false;
            var a = Session.GetString("IsAdmin");
            return a == "1" || a == "True" || a == "true";
        }

        public static void SetLogin(User u)
        {
            Session.SetString("UserId", u.Id.ToString());
            Session.SetString("UserName", u.Username);
            Session.SetString("FullName", u.FullName ?? u.Username);
            Session.SetString("IsAdmin", u.IsAdmin.ToString());
            Session.SetString("Email", u.Email ?? "");
            Log("login " + u.Username);
        }

        public static void ClearLogin()
        {
            var name = GetCurrentUserName();
            Session.Remove("UserId");
            Session.Remove("UserName");
            Session.Remove("FullName");
            Session.Remove("IsAdmin");
            Session.Remove("Email");
            // NOTE: cart is kept on purpose so people don't lose it (or should we clear it?)
            Log("logout " + name);
        }

        #endregion

        #region Cart

        public static List<CartItem> GetCart()
        {
            if (Session == null) return new List<CartItem>();
            var json = Session.GetString(CART_KEY);
            if (string.IsNullOrEmpty(json)) return new List<CartItem>();
            try
            {
                return JsonConvert.DeserializeObject<List<CartItem>>(json) ?? new List<CartItem>();
            }
            catch
            {
                return new List<CartItem>();
            }
        }

        public static void SaveCart(List<CartItem> cart)
        {
            Session.SetString(CART_KEY, JsonConvert.SerializeObject(cart));
        }

        public static void ClearCart()
        {
            Session.Remove(CART_KEY);
        }

        public static string AddToCart(string type, int id, int qty)
        {
            if (qty <= 0) qty = 1;
            if (qty > MAX_QTY) qty = MAX_QTY;

            var cart = GetCart();
            type = type.ToUpper();

            if (type == "PET")
            {
                var dt = GetDataTable("SELECT * FROM Pets WHERE Id = " + id);
                if (dt.Rows.Count == 0) return "Pet not found";
                var row = dt.Rows[0];
                if (Convert.ToInt32(row["IsSold"]) == 1) return "Sorry, " + row["Name"] + " has already found a home";

                if (cart.Any(c => c.Type == "PET" && c.Id == id))
                    return "This pet is already in your cart";

                cart.Add(new CartItem
                {
                    Type = "PET",
                    Id = id,
                    Name = row["Name"].ToString(),
                    Price = Convert.ToDouble(row["Price"]),
                    Qty = 1, // can only buy one of each pet
                    Image = row["ImageUrl"].ToString()
                });
            }
            else if (type == "SUPPLY")
            {
                var dt = GetDataTable("SELECT * FROM Supplies WHERE SupplyId = " + id);
                if (dt.Rows.Count == 0) return "Item not found";
                var row = dt.Rows[0];
                var stock = Convert.ToInt32(row["Stock"]);

                var existing = cart.FirstOrDefault(c => c.Type == "SUPPLY" && c.Id == id);
                var newQty = qty + (existing != null ? existing.Qty : 0);
                if (newQty > stock) return "Only " + stock + " left in stock";

                if (existing != null)
                {
                    existing.Qty = newQty;
                }
                else
                {
                    cart.Add(new CartItem
                    {
                        Type = "SUPPLY",
                        Id = id,
                        Name = row["SupplyName"].ToString(),
                        Price = Convert.ToDouble(row["Price"]),
                        Qty = qty,
                        Image = row["ImagePath"].ToString()
                    });
                }
            }
            else
            {
                return "Unknown item type";
            }

            SaveCart(cart);
            return null; // null = success
        }

        public static void RemoveFromCart(string type, int id)
        {
            var cart = GetCart();
            cart.RemoveAll(c => c.Type.ToUpper() == type.ToUpper() && c.Id == id);
            SaveCart(cart);
        }

        public static void UpdateCartQty(string type, int id, int qty)
        {
            var cart = GetCart();
            foreach (var c in cart)
            {
                if (c.Type == type && c.Id == id)
                {
                    if (c.Type == "PET") c.Qty = 1;
                    else c.Qty = qty;
                }
            }
            cart.RemoveAll(c => c.Qty <= 0);
            SaveCart(cart);
        }

        // number of items (sum of quantities) - shown in the header
        public static int CartCount()
        {
            return GetCart().Sum(c => c.Qty);
        }

        // number of lines in the cart
        public static int CartLineCount()
        {
            return GetCart().Count;
        }

        public static double CartSubTotal()
        {
            double total = 0;
            foreach (var item in GetCart())
            {
                total += item.Price * item.Qty;
            }
            return total;
        }

        #endregion

        #region Money

        public static string FormatPrice(double price)
        {
            return "$" + price.ToString("0.00");
        }

        // same as FormatPrice but with thousands separator (used on admin pages)
        public static string FormatMoney(double amount)
        {
            return amount.ToString("C", new CultureInfo("en-US"));
        }

        // used by supplies (decimal)
        public static string ToCurrency(decimal amount)
        {
            return string.Format("${0:N2}", amount);
        }

        public static string ToCurrency(object amount)
        {
            if (amount == null || amount == DBNull.Value) return "$0.00";
            return "$" + Convert.ToDouble(amount).ToString("0.00");
        }

        public static double RoundMoney(double amount)
        {
            return Math.Round(amount, 2);
        }

        // tax on a single line, rounded per line (accountants asked for this in 2016)
        public static double CalculateLineTax(CartItem item)
        {
            return Math.Round(item.Price * item.Qty * TAX, 2);
        }

        // cart page tax - sum of per-line rounded tax
        public static double CalculateTax(List<CartItem> items)
        {
            double tax = 0;
            foreach (var i in items)
            {
                tax += CalculateLineTax(i);
            }
            return tax;
        }

        // checkout tax - on the whole order including shipping (as per 2019 rule change)
        public static double CalcTax2(double subtotal, double shipping)
        {
            return (subtotal + shipping) * MvcApplication.TaxRate;
        }

        public static double CalculateShipping(double subtotal)
        {
            if (subtotal > FREE_SHIP) return 0;
            if (subtotal == 0) return 0;
            return SHIPPING;
        }

        // DEPRECATED - use CalculateShipping. (still used by checkout)
        public static double GetShipping(double subtotal)
        {
            if (subtotal >= MvcApplication.FreeShippingThreshold) return 0;
            return MvcApplication.ShippingFee;
        }

        public static double CalculateCartTotal(List<CartItem> items)
        {
            var sub = items.Sum(i => i.Price * i.Qty);
            var ship = CalculateShipping(sub);
            var tax = CalculateTax(items);
            return sub + ship + tax;
        }

        public static double ApplyDiscount(double price, string categoryName)
        {
            // Big dog promo - 10% off dogs over $1000 (displayed on site only - checkout TODO)
            if (categoryName == "Dogs" && price > 1000)
            {
                return price * 0.9;
            }
            return price;
        }

        public static double ParseMoney(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return 0;
            s = s.Replace("$", "").Replace(",", "").Trim();
            double d;
            if (double.TryParse(s, out d)) return d;
            return 0;
        }

        #endregion

        #region Database (raw)

        public static SqliteConnection GetConnection()
        {
            var conn = new SqliteConnection(MvcApplication.ConnectionString);
            conn.Open();
            return conn;
        }

        public static int ExecuteSql(string sql)
        {
            using (var conn = GetConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                return cmd.ExecuteNonQuery();
            }
        }

        public static object ExecuteScalar(string sql)
        {
            using (var conn = GetConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                return cmd.ExecuteScalar();
            }
        }

        public static DataTable GetDataTable(string sql)
        {
            var dt = new DataTable();
            using (var conn = GetConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                using (var reader = cmd.ExecuteReader())
                {
                    // DataTable.Load chokes on sqlite schema sometimes, do it by hand
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        dt.Columns.Add(reader.GetName(i), typeof(object));
                    }
                    while (reader.Read())
                    {
                        var row = dt.NewRow();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row[i] = reader.GetValue(i);
                        }
                        dt.Rows.Add(row);
                    }
                }
            }
            return dt;
        }

        public static string Esc(string s)
        {
            // escape quotes for sql
            if (s == null) return "";
            return s.Replace("'", "''");
        }

        public static string GetSetting(string key)
        {
            try
            {
                var v = ExecuteScalar("SELECT [Value] FROM Settings WHERE [Key] = '" + key + "'");
                return v == null ? null : v.ToString();
            }
            catch
            {
                return null;
            }
        }

        public static void SetSetting(string key, string value)
        {
            ExecuteSql("INSERT OR REPLACE INTO Settings ([Key],[Value]) VALUES ('" + key + "','" + value + "')");
        }

        public static void Log(string message)
        {
            try
            {
                var who = GetCurrentUserName() ?? "anonymous";
                ExecuteSql("INSERT INTO AuditLog (Message, Who, At) VALUES ('" + Esc(message) + "','" + Esc(who) + "','" + DateTime.Now.ToString() + "')");
            }
            catch
            {
                // logging failed, try the file
                try
                {
                    File.AppendAllText("petstore.log", DateTime.Now + " " + message + Environment.NewLine);
                }
                catch { }
            }
        }

        public static string GetCategoryName(int id)
        {
            if (MvcApplication.CategoryCache == null)
            {
                MvcApplication.CategoryCache = new Dictionary<int, string>();
                var dt = GetDataTable("SELECT Id, Name FROM Categories");
                foreach (DataRow r in dt.Rows)
                {
                    MvcApplication.CategoryCache[Convert.ToInt32(r["Id"])] = r["Name"].ToString();
                }
            }
            if (MvcApplication.CategoryCache.ContainsKey(id)) return MvcApplication.CategoryCache[id];
            return "Unknown";
        }

        public static int GetSupplyStock(int supplyId)
        {
            var o = ExecuteScalar("SELECT Stock FROM Supplies WHERE SupplyId = " + supplyId);
            return o == null ? 0 : Convert.ToInt32(o);
        }

        public static void SetSupplyStock(int supplyId, int stock)
        {
            ExecuteSql("UPDATE Supplies SET Stock = " + stock + " WHERE SupplyId = " + supplyId);
        }

        public static bool IsPetSold(int petId)
        {
            var o = ExecuteScalar("SELECT IsSold FROM Pets WHERE Id = " + petId);
            return o != null && Convert.ToInt32(o) == 1;
        }

        public static void MarkPetSold(int petId)
        {
            ExecuteSql("UPDATE Pets SET IsSold = 1 WHERE Id = " + petId);
        }

        public static int CountRows(string table)
        {
            return Convert.ToInt32(ExecuteScalar("SELECT COUNT(*) FROM " + table));
        }

        public static List<Dictionary<string, object>> Query(string sql)
        {
            var list = new List<Dictionary<string, object>>();
            var dt = GetDataTable(sql);
            foreach (DataRow r in dt.Rows)
            {
                var d = new Dictionary<string, object>();
                foreach (DataColumn c in dt.Columns)
                {
                    d[c.ColumnName] = r[c];
                }
                list.Add(d);
            }
            return list;
        }

        public static Pet RowToPet(DataRow r)
        {
            return new Pet
            {
                Id = Convert.ToInt32(r["Id"]),
                Name = r["Name"].ToString(),
                CategoryId = Convert.ToInt32(r["CategoryId"]),
                Breed = r["Breed"] == DBNull.Value ? "" : r["Breed"].ToString(),
                Age = r["Age"] == DBNull.Value ? "" : r["Age"].ToString(),
                Price = Convert.ToDouble(r["Price"]),
                Description = r["Description"] == DBNull.Value ? "" : r["Description"].ToString(),
                ImageUrl = r["ImageUrl"] == DBNull.Value ? null : r["ImageUrl"].ToString(),
                IsSold = Convert.ToInt32(r["IsSold"]) == 1,
                IsFeatured = Convert.ToInt32(r["IsFeatured"]) == 1,
                CreatedDate = r["CreatedDate"] == DBNull.Value ? null : r["CreatedDate"].ToString()
            };
        }

        #endregion

        #region HTML builders

        public static string BuildPetCardHtml(Pet p)
        {
            var sb = new StringBuilder();
            sb.Append("<div class=\"col-sm-6 col-md-4\">");
            sb.Append("<div class=\"thumbnail pet-card\">");
            sb.Append("<a href=\"/Pets/Details/" + p.Id + "\"><img src=\"" + GetImageOrPlaceholder(p.ImageUrl) + "\" alt=\"" + p.Name + "\" style=\"height:160px\" /></a>");
            sb.Append("<div class=\"caption\">");
            sb.Append("<h4>" + p.Name);
            if (p.IsNew) sb.Append(" <span class=\"label label-success\">NEW</span>");
            if (p.IsSold) sb.Append(" <span class=\"label label-default\">SOLD</span>");
            sb.Append("</h4>");
            sb.Append("<p class=\"text-muted\">" + p.Breed + " &middot; " + p.Age + "</p>");
            sb.Append("<p><strong>" + FormatPrice(p.Price) + "</strong></p>");
            if (!p.IsSold)
            {
                sb.Append("<form method=\"post\" action=\"/Cart/Add\" class=\"add-to-cart-form\">");
                sb.Append("<input type=\"hidden\" name=\"type\" value=\"PET\" />");
                sb.Append("<input type=\"hidden\" name=\"id\" value=\"" + p.Id + "\" />");
                sb.Append("<button type=\"submit\" class=\"btn btn-primary btn-sm\"><span class=\"glyphicon glyphicon-shopping-cart\"></span> Add to cart</button>");
                sb.Append("</form>");
            }
            sb.Append("</div></div></div>");
            return sb.ToString();
        }

        public static string BuildSupplyCardHtml(Supply s)
        {
            var sb = new StringBuilder();
            sb.Append("<div class=\"col-sm-6 col-md-3\">");
            sb.Append("<div class=\"thumbnail\">");
            sb.Append("<img src=\"" + GetImageOrPlaceholder(s.ImagePath) + "\" style=\"height:120px\" />");
            sb.Append("<div class=\"caption\">");
            sb.Append("<h5>" + s.SupplyName + "</h5>");
            sb.Append("<p>" + ToCurrency(s.Price) + "</p>");
            if (s.Stock <= 0)
                sb.Append("<span class=\"label label-danger\">Out of stock</span>");
            else if (s.Stock < 5)
                sb.Append("<span class=\"label label-warning\">Only " + s.Stock + " left!</span>");
            sb.Append("</div></div></div>");
            return sb.ToString();
        }

        public static string BuildPager(int page, int totalPages, string baseUrl)
        {
            if (totalPages <= 1) return "";
            var sep = baseUrl.Contains("?") ? "&" : "?";
            var sb = new StringBuilder("<nav><ul class=\"pagination\">");
            for (int i = 1; i <= totalPages; i++)
            {
                if (i == page)
                    sb.Append("<li class=\"active\"><span>" + i + "</span></li>");
                else
                    sb.Append("<li><a href=\"" + baseUrl + sep + "page=" + i + "\">" + i + "</a></li>");
            }
            sb.Append("</ul></nav>");
            return sb.ToString();
        }

        public static string BuildCategoryDropdown(string name, int? selected)
        {
            var sb = new StringBuilder("<select name=\"" + name + "\" class=\"form-control\">");
            sb.Append("<option value=\"\">-- All --</option>");
            var dt = GetDataTable("SELECT Id, Name FROM Categories ORDER BY SortOrder");
            foreach (DataRow r in dt.Rows)
            {
                var id = Convert.ToInt32(r["Id"]);
                sb.Append("<option value=\"" + id + "\"" + (selected == id ? " selected" : "") + ">" + r["Name"] + "</option>");
            }
            sb.Append("</select>");
            return sb.ToString();
        }

        public static string BuildStars(int rating)
        {
            var s = "";
            for (int i = 0; i < 5; i++)
            {
                s += i < rating ? "<span class=\"glyphicon glyphicon-star\"></span>" : "<span class=\"glyphicon glyphicon-star-empty\"></span>";
            }
            return s;
        }

        public static string BuildBreadcrumb(params string[] parts)
        {
            var sb = new StringBuilder("<ol class=\"breadcrumb\"><li><a href=\"/\">Home</a></li>");
            for (int i = 0; i < parts.Length; i++)
            {
                if (i == parts.Length - 1) sb.Append("<li class=\"active\">" + parts[i] + "</li>");
                else sb.Append("<li>" + parts[i] + "</li>");
            }
            sb.Append("</ol>");
            return sb.ToString();
        }

        public static string StatusLabel(string status)
        {
            switch ((status ?? "").ToLower())
            {
                case "new": return "<span class=\"label label-info\">New</span>";
                case "paid": return "<span class=\"label label-primary\">Paid</span>";
                case "shipped": return "<span class=\"label label-success\">Shipped</span>";
                case "cancelled": return "<span class=\"label label-default\">Cancelled</span>";
                default: return "<span class=\"label label-warning\">" + status + "</span>";
            }
        }

        public static string Truncate(string s, int len)
        {
            if (s == null) return "";
            if (s.Length <= len) return s;
            return s.Substring(0, len) + "...";
        }

        public static string StripHtml(string s)
        {
            if (s == null) return "";
            return Regex.Replace(s, "<.*?>", "");
        }

        // removes script tags so people can't hack us
        public static string SanitizeHtml(string html)
        {
            if (html == null) return null;
            return html.Replace("<script>", "").Replace("</script>", "");
        }

        #endregion

        #region Validation

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return false;
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[a-z]{2,4}$");
        }

        public static bool IsValidZip(string zip)
        {
            // australian postcodes are 4 digits
            if (zip == null) return false;
            return Regex.IsMatch(zip, @"^\d{4}$");
        }

        public static bool IsValidCard(string card)
        {
            if (card == null) return false;
            card = card.Replace(" ", "").Replace("-", "");
            if (card.Length < 13 || card.Length > 19) return false;
            foreach (var ch in card)
            {
                if (!char.IsDigit(ch)) return false;
            }
            return true;
        }

        // expiry in MM/yy
        public static bool IsValidExpiry(string expiry)
        {
            if (string.IsNullOrEmpty(expiry) || !expiry.Contains("/")) return false;
            var parts = expiry.Split('/');
            int m, y;
            if (!int.TryParse(parts[0], out m) || !int.TryParse(parts[1], out y)) return false;
            if (m < 1 || m > 12) return false;
            y += 2000;
            if (y < DateTime.Now.Year) return false;
            if (y == DateTime.Now.Year && m < DateTime.Now.Month) return false;
            return true;
        }

        public static bool IsValidUsername(string u)
        {
            if (string.IsNullOrEmpty(u)) return false;
            return u.Length >= 3 && u.Length <= 20;
        }

        public static bool IsStrongPassword(string p)
        {
            return p != null && p.Length >= 4;
        }

        // simulated payment gateway: cards starting with 4 are approved, 4000 0000 0000 0002 is declined
        public static bool ChargeCard(string card, double amount)
        {
            card = (card ?? "").Replace(" ", "");
            if (card == "4000000000000002") return false;
            System.Threading.Thread.Sleep(500); // simulate network
            return card.StartsWith("4") || card.StartsWith("5");
        }

        #endregion

        #region Dates

        public static string FormatDate(DateTime d)
        {
            return d.ToString(DATE_FORMAT);
        }

        public static string FormatDateTime(DateTime d)
        {
            return d.ToString(DATE_TIME_FORMAT);
        }

        public static string Now()
        {
            return DateTime.Now.ToString(DATE_TIME_FORMAT);
        }

        public static DateTime? ParseDate(string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            DateTime d;
            if (DateTime.TryParseExact(s, DATE_FORMAT, null, DateTimeStyles.None, out d)) return d;
            if (DateTime.TryParseExact(s, DATE_TIME_FORMAT, null, DateTimeStyles.None, out d)) return d;
            if (DateTime.TryParse(s, out d)) return d;
            return null;
        }

        public static string TimeAgo(DateTime d)
        {
            var span = DateTime.Now - d;
            if (span.TotalMinutes < 1) return "just now";
            if (span.TotalHours < 1) return (int)span.TotalMinutes + " minutes ago";
            if (span.TotalDays < 1) return (int)span.TotalHours + " hours ago";
            if (span.TotalDays < 30) return (int)span.TotalDays + " days ago";
            return FormatDate(d);
        }

        public static bool IsWeekend()
        {
            return DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday;
        }

        public static string EstimatedDelivery()
        {
            // 3 business days (ish)
            var d = DateTime.Now.AddDays(IsWeekend() ? 5 : 3);
            return d.ToString("dddd dd MMM");
        }

        public static int AgeInMonths(string age)
        {
            // "3 months", "2 yrs", "1.5 years", "1 yr"
            if (string.IsNullOrEmpty(age)) return 0;
            var m = Regex.Match(age, @"([\d\.]+)\s*(\w+)");
            if (!m.Success) return 0;
            double n;
            double.TryParse(m.Groups[1].Value, out n);
            var unit = m.Groups[2].Value.ToLower();
            if (unit.StartsWith("y")) return (int)(n * 12);
            if (unit.StartsWith("m")) return (int)n;
            if (unit.StartsWith("w")) return (int)(n / 4);
            return 0;
        }

        #endregion

        #region Files

        public static string SaveUpload(IFormFile file)
        {
            if (file == null || file.Length == 0) return null;
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", UPLOAD_FOLDER);
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            // keep original file name so the marketing team can find their pictures
            var fileName = file.FileName;
            var path = Path.Combine(folder, fileName);
            using (var fs = new FileStream(path, FileMode.Create))
            {
                file.CopyTo(fs);
            }
            return "/" + UPLOAD_FOLDER + "/" + fileName;
        }

        public static string GetImageOrPlaceholder(string url)
        {
            if (string.IsNullOrEmpty(url)) return PLACEHOLDER_IMG;
            return url;
        }

        public static void DeleteFile(string relativeUrl)
        {
            if (string.IsNullOrEmpty(relativeUrl)) return;
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativeUrl.TrimStart('/'));
            if (File.Exists(path)) File.Delete(path);
        }

        #endregion

        #region Strings

        public static string Slugify(string s)
        {
            if (s == null) return "";
            s = s.ToLower().Trim();
            s = Regex.Replace(s, @"[^a-z0-9\s-]", "");
            s = Regex.Replace(s, @"\s+", "-");
            return s;
        }

        public static string TitleCase(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
        }

        public static string Left(string s, int n)
        {
            if (s == null) return null;
            return s.Length <= n ? s : s.Substring(0, n);
        }

        public static string Right(string s, int n)
        {
            if (s == null) return null;
            return s.Length <= n ? s : s.Substring(s.Length - n);
        }

        public static int ToInt(object o)
        {
            if (o == null || o == DBNull.Value) return 0;
            int i;
            if (int.TryParse(o.ToString(), out i)) return i;
            return 0;
        }

        public static int ToInt(string s, int def)
        {
            int i;
            if (int.TryParse(s, out i)) return i;
            return def;
        }

        public static double ToDouble(object o)
        {
            if (o == null || o == DBNull.Value) return 0;
            double d;
            if (double.TryParse(o.ToString(), out d)) return d;
            return 0;
        }

        public static string SafeString(object o)
        {
            if (o == null || o == DBNull.Value) return "";
            return o.ToString();
        }

        public static string Pluralize(int n, string word)
        {
            return n + " " + (n == 1 ? word : word + "s");
        }

        public static bool IsNullOrEmpty(string s)
        {
            return s == null || s == "" || s.Trim() == "";
        }

        #endregion

        #region Misc

        public static string GenerateOrderNumber(int orderId)
        {
            MvcApplication.OrdersPlacedSinceStartup++;
            return "PS-" + DateTime.Now.ToString("yyMMdd") + "-" + orderId.ToString("00000");
        }

        public static void SendEmail(string to, string subject, string body)
        {
            // TODO: hook up SMTP (password is in appsettings)
            try
            {
                File.AppendAllText("emails.log", "TO: " + to + "\nSUBJECT: " + subject + "\n" + body + "\n-----\n");
            }
            catch { }
        }

        public static void SendOrderConfirmation(Order o, string email)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Thanks for your order " + o.ShipName + "!");
            sb.AppendLine("Order: " + GenerateOrderNumber(o.Id));
            sb.AppendLine("Total: " + FormatPrice(o.Total));
            sb.AppendLine("Card: " + o.CardNumber);
            sb.AppendLine("Estimated delivery: " + EstimatedDelivery());
            SendEmail(email, "Your Paws & Claws order", sb.ToString());
        }

        public static void AddRecentSearch(string term)
        {
            if (string.IsNullOrWhiteSpace(term)) return;
            MvcApplication.RecentSearches.Insert(0, term);
            if (MvcApplication.RecentSearches.Count > 10)
                MvcApplication.RecentSearches.RemoveAt(MvcApplication.RecentSearches.Count - 1);
        }

        public static string GetGreeting()
        {
            var h = DateTime.Now.Hour;
            if (h < 12) return "Good morning";
            if (h < 18) return "Good afternoon";
            return "Good evening";
        }

        public static bool IsMaintenanceMode()
        {
            return GetSetting("MaintenanceMode") == "true";
        }

        public static string ToJson(object o)
        {
            return JsonConvert.SerializeObject(o);
        }

        // used by layout to pass config to javascript
        public static string ClientConfigJson()
        {
            var cfg = new Hashtable();
            cfg["taxRate"] = TAX;
            cfg["shipping"] = SHIPPING;
            cfg["freeShip"] = FREE_SHIP;
            cfg["userId"] = GetCurrentUserId();
            cfg["userName"] = GetCurrentUserName();
            cfg["isAdmin"] = IsAdmin();
            cfg["email"] = Session == null ? null : Session.GetString("Email");
            cfg["siteName"] = MvcApplication.SiteName;
            return JsonConvert.SerializeObject(cfg);
        }

        #endregion

        #region OLD CODE - DO NOT DELETE

        // public static bool PayPalCharge(string token, double amount)
        // {
        //     var req = (HttpWebRequest)WebRequest.Create("https://api-3t.paypal.com/nvp");
        //     req.Method = "POST";
        //     var data = "USER=petstore_api1&PWD=QWERTY123&SIGNATURE=abc&METHOD=DoExpressCheckoutPayment&TOKEN=" + token + "&AMT=" + amount;
        //     using (var sw = new StreamWriter(req.GetRequestStream())) { sw.Write(data); }
        //     var resp = new StreamReader(req.GetResponse().GetResponseStream()).ReadToEnd();
        //     return resp.Contains("ACK=Success");
        // }

        // public static string GetCartFromCookie()
        // {
        //     var c = HttpContext.Current.Request.Cookies["cart"];
        //     if (c == null) return "";
        //     return HttpUtility.UrlDecode(c.Value);
        // }

        // public static void MigrateOldCart()
        // {
        //     // 2014: carts used to be stored in a cookie as "PET:1|SUPPLY:3x2"
        //     var raw = GetCartFromCookie();
        //     foreach (var part in raw.Split('|'))
        //     {
        //         var bits = part.Split(':');
        //         if (bits[0] == "PET") AddToCart("PET", int.Parse(bits[1]), 1);
        //         else
        //         {
        //             var q = bits[1].Split('x');
        //             AddToCart("SUPPLY", int.Parse(q[0]), q.Length > 1 ? int.Parse(q[1]) : 1);
        //         }
        //     }
        // }

        // public static double CalculateTaxOld(double amount)
        // {
        //     // GST changed in 2000 - leaving in case it changes back
        //     return amount * 0.1;
        // }

        // public static string GetWeatherWidget()
        // {
        //     return new WebClient().DownloadString("http://weather.example.com/widget?city=melbourne");
        // }

        // public static void Christmas()
        // {
        //     if (DateTime.Now.Month == 12) HttpContext.Current.Items["Theme"] = "xmas";
        // }

        #endregion
    }
}
