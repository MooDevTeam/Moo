﻿using System;
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
                var indexInterface = new IndexInterface();
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
                        "EXECUTE sp_fulltext_table @TableName,'create','ft_Indexer','PK_ID'\r\n" +
                        "EXECUTE sp_fulltext_column @TableName,'content','add'\r\n" +
                        "EXECUTE sp_fulltext_table @TableName,'activate'\r\n" +
                        "EXECUTE sp_fulltext_table @TableName,'start_full'\r\n" +
                        "END\r\n", conn
                        ))
                    {
                        cmd.Parameters.Add(new SqlParameter("TableName", type));
                        cmd.ExecuteNonQuery();
                    }
                    IndexItem item;
                    while (null != (item = indexInterface.Next(type)))
                    {
                        using (var cmd = new SqlCommand(
                            "IF NOT EXISTS(SELECT * FROM " + type + " WHERE [ID]=@ID)\r\n" +
                            "   INSERT INTO " + type + "([ID],[content]) VALUES(@ID,@content)\r\n" +
                            "ELSE\r\n" +
                            "   UPDATE " + type + " SET [content]=@content WHERE [ID]=@ID\r\n"
                            , conn))
                        {
                            cmd.Parameters.Add(new SqlParameter("ID", item.ID));
                            cmd.Parameters.Add(new SqlParameter("content", item.Content));
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception )
            {
                Console.Beep();
            }
            return 10000;
        }
    }
}
