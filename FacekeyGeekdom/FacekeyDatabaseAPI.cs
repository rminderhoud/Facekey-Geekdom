using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.IO;
using System.Text;

namespace FacekeyGeekdom
{
    public class FacekeyDatabaseAPI
    {
        private SQLiteConnection db; 
        private string sql;
        private SQLiteCommand query;
        private SQLiteDataReader reader;

        public bool LoadDatabase(string database_name)
        {
            if (!File.Exists(database_name))
                return false;

            db = new SQLiteConnection("Data Source="+database_name+";Version=3;");
            db.Open();
            return true;
        }

        public bool CreateDatabase(string database_name)
        {
            SQLiteConnection.CreateFile("facekeygeekdom.sqlite");
            return true;
        }

        public bool Connect(string database_name)
        {
            db = new SQLiteConnection("Data Source=" + database_name + ";Version=3;");
            db.Open();
            return true;
        }

        public void ExecuteNonQuery(string sql)
        {
            query = new SQLiteCommand(sql, db);
            query.ExecuteNonQuery();
        }

        public SQLiteDataReader ExecuteReader(string sql)
        {
            query = new SQLiteCommand(sql, db);
            reader = query.ExecuteReader();

            return reader;
        }

        public int GetGeekdomID(int facekey_id)
        {
            sql = "SELECT geekdom_id FROM ids WHERE facekey_id=" + facekey_id.ToString();
            query = new SQLiteCommand(sql, db);
            reader = query.ExecuteReader();

            if (reader.Read())
                return Int32.Parse("" + reader["geekdom_id"]);
            else
                return -1;
        }
    }
}
