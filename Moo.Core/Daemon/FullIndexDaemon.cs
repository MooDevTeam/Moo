using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Configuration;
namespace Moo.Core.Daemon
{
    class FullIndexDaemon : Daemon
    {
        public static FullIndexDaemon Instance = new FullIndexDaemon();
        SqlConnection conn;
        SqlCommand cmd;
        FullIndexDaemon()
        {
            conn = new SqlConnection(ConfigurationManager.ConnectionStrings["IndexerDB"].ConnectionString);
            conn.Open();
            cmd = new SqlCommand("SELECT * FROM sys.databases WHERE [Name]='Indexer'", conn);
            using (var reader = cmd.ExecuteReader())
            {
                if (!reader.Read())
                {
                    SqlCommand newDatabase = new SqlCommand("", conn);
                }
            }
        }
        protected override int Run()
        {
            Console.Beep();
            return 10000;
        }
    }
}
