using System;
using System.Collections.Generic;

namespace PetStore
{
    // Ported from Global.asax.cs when we "upgraded" to Core. Kept the name so people can find things.
    public static class MvcApplication
    {
        public static string SiteName = "Paws & Claws Pet Store";
        public static string ConnectionString = "Data Source=petstore.db";

        public static double TaxRate = 0.07;
        public static double ShippingFee = 9.95;
        public static double FreeShippingThreshold = 100;

        // global mutable state (shared across all users / threads)
        public static List<string> RecentSearches = new List<string>();
        public static Dictionary<int, string> CategoryCache = null;
        public static int OrdersPlacedSinceStartup = 0;
        public static DateTime StartedAt;
        public static string LastError;

        public static void Application_Start()
        {
            StartedAt = DateTime.Now;
            DatabaseConfig.Initialize();
        }

        public static void Application_Error(Exception ex)
        {
            LastError = ex.ToString();
            try { PetStore.Helpers.Utils.Log("ERROR: " + ex.Message); } catch { }
        }
    }
}
