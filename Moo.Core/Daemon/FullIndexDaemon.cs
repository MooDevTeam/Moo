using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Configuration;
using Moo.Core.IndexAPI;
namespace Moo.Core.Daemon
{
    class FullIndexDaemon : Daemon
    {
        public static FullIndexDaemon Instance = new FullIndexDaemon();
        static SqlConnection conn;
        IndexInterface indexInterface;
        FullIndexDaemon()
        {
            using (var tconn = new SqlConnection(ConfigurationManager.ConnectionStrings["IndexerDB"].ConnectionString))
            {
                tconn.Open();
                using (var cmd = new SqlCommand(
                    "IF NOT EXISTS(SELECT * FROM sys.databases WHERE [name]='Indexer')\r\n" +
                    "BEGIN\r\n" +
                    "    CREATE DATABASE Indexer\r\n" +
                    "END\r\n", tconn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            conn = new SqlConnection("Database=Indexer;" + ConfigurationManager.ConnectionStrings["IndexerDB"].ConnectionString);
            conn.Open();
            using (var cmd = new SqlCommand(
                "EXECUTE sp_fulltext_database 'enable'\r\n" +
                "IF NOT EXISTS(SELECT * FROM sys.fulltext_catalogs WHERE [name]='ft_Indexer')\r\n" +
                "    EXECUTE sp_fulltext_catalog 'ft_Indexer','create','Indexer'\r\n",conn
                ))
            {
                cmd.ExecuteNonQuery();
            }
        }
        protected override int Run()
        {
            try
            {
                if(null==indexInterface)
                    indexInterface= new IndexInterface();
                bool reNew = true;
                foreach (string type in indexInterface.Types)
                {
                    using (var cmd = new SqlCommand(
                        "IF NOT EXISTS(SELECT * FROM sysobjects WHERE [id] =object_id(@TableName)and OBJECTPROPERTY(id, N'IsUserTable') = 1)\r\n" +
                        "BEGIN\r\n" +
                        "CREATE TABLE " + type + "\r\n" +
                        "(\r\n" +
                        "    [ID] INT,\r\n" +
                        "    [content] nvarchar(max)\r\n" +
                        "    CONSTRAINT PK_ID PRIMARY KEY([ID])\r\n" +
                        ")\r\n" +
                        "CREATE FULLTEXT INDEX ON "+type+"([content]) KEY INDEX PK_ID ON ft_Indexer WITH(CHANGE_TRACKING = AUTO)\r\n"+
                        "END\r\n"+
                        "IF NOT EXISTS(SELECT * FROM sysobjects WHERE ID=object_id(@SuffixTableName) and OBJECTPROPERTY(id, N'IsUserTable') = 1)\r\n"+
                        "BEGIN\r\n"+
	                    "    CREATE  TABLE Suffix"+type+"\r\n"+
	                    "    (\r\n"+
		                "        [ID] INT,\r\n"+
		                "        [content] nvarchar(50)\r\n"+
	                    "    )\r\n"+
	                    "    CREATE NONCLUSTERED INDEX IDX_CONTENT ON Suffix"+type+" ([content])\r\n"+
                        "    CREATE NONCLUSTERED INDEX IDX_ID ON Suffix"+type+" ([ID])\r\n"+
                        "END\r\n", conn
                        ))
                    {
                        cmd.Parameters.AddWithValue("TableName", type);
                        cmd.Parameters.AddWithValue("SuffixTableName", "Suffix" + type);
                        cmd.ExecuteNonQuery();
                    }
                    IndexItem item;
                    if (null != (item = indexInterface.Next(type)))
                    {
                        reNew = false;
                        using (var cmd = new SqlCommand(
                            "IF NOT EXISTS(SELECT * FROM " + type + " WHERE [ID]=@ID)\r\n" +
                            "   INSERT INTO " + type + "([ID],[content]) VALUES(@ID,@content)\r\n" +
                            "ELSE\r\n" +
                            "   UPDATE " + type + " SET [content]=@content WHERE [ID]=@ID\r\n"
                            , conn))
                        {
                            cmd.Parameters.AddWithValue("ID", item.ID);
                            cmd.Parameters.AddWithValue("content", item.Content);
                            cmd.ExecuteNonQuery();
                        }
                        foreach (string keyword in item.Keywords)
                        {
                            using (var cmd = new SqlCommand("DELETE FROM Suffix" + type + " WHERE ID=@ID",conn))
                            {
                                cmd.Parameters.AddWithValue("ID", item.ID);
                                cmd.ExecuteNonQuery();
                            }
                            for (int i = 0; i < keyword.Length; i++)
                            {
                                using (var cmd = new SqlCommand("INSERT INTO Suffix"+type+"([ID],[content]) VALUES(@ID,@keyword)", conn))
                                {
                                    cmd.Parameters.AddWithValue("ID", item.ID);
                                    cmd.Parameters.AddWithValue("keyword", keyword.Substring(i));
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
                if (reNew)
                    indexInterface = new IndexInterface();
            }
            catch (Exception )
            {
                Console.Beep();
            }
            return 10000;
        }
    }
}
