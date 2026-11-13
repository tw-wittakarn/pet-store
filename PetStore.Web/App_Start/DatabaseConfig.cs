using System;
using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite;
using PetStore.DAL;

namespace PetStore
{
    public static class DatabaseConfig
    {
        public static bool Seeded;

        // static ctor runs the first time anybody touches DatabaseConfig
        static DatabaseConfig()
        {
            try
            {
                using (var db = new PetStoreContext())
                {
                    db.Database.EnsureCreated();

                    if (db.Pets.Count() == 0)
                    {
                        var path = Path.Combine(AppContext.BaseDirectory, "App_Data", "seed.sql");
                        if (!File.Exists(path)) path = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "seed.sql");
                        var sql = File.ReadAllText(path);

                        using (var conn = new SqliteConnection(MvcApplication.ConnectionString))
                        {
                            conn.Open();
                            foreach (var stmt in sql.Split(';'))
                            {
                                var s = stmt.Trim();
                                // strip comment lines
                                var lines = s.Split('\n').Where(l => !l.Trim().StartsWith("--"));
                                s = string.Join("\n", lines).Trim();
                                if (s.Length == 0) continue;
                                var cmd = conn.CreateCommand();
                                cmd.CommandText = s;
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    Seeded = true;
                }
            }
            catch (Exception ex)
            {
                // if seeding fails just carry on, site will show empty
                Console.WriteLine("DB INIT FAILED: " + ex.Message);
            }
        }

        public static void Initialize()
        {
            // intentionally empty - calling this triggers the static constructor
        }
    }
}
