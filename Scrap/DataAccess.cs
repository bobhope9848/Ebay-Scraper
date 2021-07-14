using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data.SQLite;

namespace Scrap
{
    class DataAccess
    {
        /// <summary>
        /// Checks if database exists and if not create with specified columns
        /// </summary>
        /// <param name="Name"></param>
        private static void CheckDB(string Name)
        {
            if (!File.Exists($"{Name}.db")) 
                {
                    //Init database file
                    SQLiteConnection.CreateFile($"{Name}.db");
                    Console.WriteLine($"Missing {Name}.db");
                    Console.WriteLine("Creating.....");
                    using var con = new SQLiteConnection($@"URI=file:{Name}.db");
                    //Open database for writing
                    con.Open();
                    //Prepare to add columns to database
                    SQLiteCommand Result = new SQLiteCommand("CREATE TABLE Result (SaleType TEXT, Title TEXT, Price DECIMAL, Url TEXT UNIQUE, Time TIMESTAMP DEFAULT CURRENT_TIMESTAMP )", con);
                    //Run prepared SQL command
                    Result.ExecuteNonQuery();
                    //Close database
                    con.Close();
                    
                }

        }
        /// <summary>
        /// Takes in structured list of results and pushes to SQLite db
        /// </summary>
        /// <param name="name"></param>
        /// <param name="listed"></param>
        public static void SaveResults(string name, List<Listed> listed)
        {
            //Checks if database exists
            CheckDB(name);
            using var con = new SQLiteConnection($@"URI=file:{name}.db");
            con.Open();
            using var send = new SQLiteCommand(con);
            int result = 0;
            //TODO: Send values by range instead of one by one
            //Recursively adds each item to database
            foreach (var item in listed)
            {
                //Inserts values into db but only if unique (unique based on url)
                send.CommandText = $"INSERT OR IGNORE INTO Result(saletype, title, price, url) VALUES(@saletype, @title, @price, @url)";
                send.Parameters.AddWithValue("@saletype", $"{item.SaleType}");
                send.Parameters.AddWithValue("@title", $"{item.Title}");
                send.Parameters.AddWithValue("@price", $"{item.Price}");
                send.Parameters.AddWithValue("@url", $"{item.Url}");
                send.Prepare();
                result += send.ExecuteNonQuery();
            }
            con.Close();
            Console.WriteLine("Rows Added : {0}", result);
        }
    }
}
