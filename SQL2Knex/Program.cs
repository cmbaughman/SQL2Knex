using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace SQL2Knex
{
    class Program
    {
        public static string conStr = ConfigurationManager.ConnectionStrings["conStr"].ConnectionString;
        public static SqlConnection scon;
        public const char LF = '\n';

        static void Main(string[] args)
        {
            List<string> tabs = ListTables();
            foreach (String t in tabs)
            {
                Console.WriteLine(t);
                WriteFile(t, GetKnex(t));
            }
            Console.WriteLine("done!");
          
        }

        static void WriteFile(string tableName, string tex)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(ConfigurationManager.AppSettings["BaseDir"].ToString() + "\\" + tableName + ".js"))
            {
                Console.WriteLine("Writing " + tableName + ".js");
                file.Write(tex);
            }
        }

        static String GetKnex(string tableName)
        {
            List<string> cols = new List<string>();
            StringBuilder sb = new StringBuilder();

            if (!String.IsNullOrEmpty(tableName))
            {
                string sql = "SELECT * FROM " + tableName;
                using (SqlCommand cmd = new SqlCommand(sql))
                {
                    scon = new SqlConnection(conStr);
                    scon.Open();
                    cmd.Connection = scon;
                    var reader = cmd.ExecuteReader();
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        cols.Add(reader.GetName(i));
                    }

                    if (reader.HasRows) {
                        sb.Append(GetStartFile(tableName));
                        while (reader.Read()) {
                            sb.Append("    knex('" + tableName + "').insert({");
                            for (var i = 0; i < reader.FieldCount; i++)
                            {
                                string name = reader.GetName(i);
                                if (i == (reader.FieldCount - 1))
                                {
                                    sb.Append(name + ": '" + reader[name].ToString() + "'" + LF);
                                }
                                else
                                {
                                    sb.Append(name + ": '" + reader[name].ToString() + "', ");
                                }
                            }
                            sb.Append("}),");
                        }
                        sb.Append(GetEndFile());
                    }
                }
            }
            return sb.ToString();
        }

        public static String GetStartFile(string tableName)
        {
            StringBuilder sb = new StringBuilder("exports.seed = function(knex, Promise) {" + LF +
                "  return Promise.join(" + LF + 
                "    knex('" + tableName + "').del()," + LF
            );
            return sb.ToString();
        }

        public static String GetEndFile()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("    })" + LF +
                "  );" + LF +
                "};" + LF);
            return sb.ToString();
        }

        public static List<string> ListTables()
        {
            List<string> tables = new List<string>();
            using (scon = new SqlConnection(conStr))
            {
                scon.Open();
                DataTable dt = scon.GetSchema("Tables");
                foreach (DataRow dr in dt.Rows)
                {
                    string tablename = (string)dr[2];
                    tables.Add(tablename);
                }
            }
            
            return tables;
        }
    }
}
