using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data.SQLite;

namespace Scrap
{
    class DataAccess
    {
        private static void CheckDB(string Name)
        {
            if (!File.Exists($"{Name}.db")) 
                {
                    SQLiteConnection.CreateFile($"{Name}.db");
                    Console.WriteLine($"Missing {Name}.db");
                    Console.WriteLine("Creating.....");
                    string cs = $@"URI=file:{Name}.db";
                    using var con = new SQLiteConnection(cs);
                    con.Open();
                    switch (Name) 
                    {
                       case "Enemy":
                           SQLiteCommand Enemy = new SQLiteCommand("CREATE TABLE Enemies (Name TEXT, Health INT, Armor INT, Attack INT, Speed INT, IsDead INT, Level INT )", con);
                            Enemy.ExecuteNonQuery();
                           break;
                       case "Hero":
                           SQLiteCommand Hero = new SQLiteCommand("CREATE TABLE Heroes (Name TEXT, Health INT, Armor INT, Attack INT, Speed INT, IsDead INT, Level INT, CurrentXP INT )", con);
                            Hero.ExecuteNonQuery();
                           break;
                       case "Item":
                            //TODO: Broken ID system when creating
                           SQLiteCommand Item = new SQLiteCommand("CREATE TABLE Items (ID INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT, Speed INT, Armor INT, Health INT, Attack INT, Durability INT, IsEquipped INT, Type INT )", con);
                            Item.ExecuteNonQuery();
                            break;
                       default:
                            throw new System.ArgumentException($"Invalid name of database {Name}");
                    }
                    con.Close();
                    
                }

        }
    }
}
