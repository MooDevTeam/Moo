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
    public class FullIndexDaemon : Daemon
    {
        public static FullIndexDaemon Instance = new FullIndexDaemon();
        static SqlConnection conn;
        IndexInterface indexInterface;
        int sleepTime = 10000;
        Dictionary<string,DateTime> timeStamp;
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
                "    EXECUTE sp_fulltext_catalog 'ft_Indexer','create','Indexer'\r\n", conn
                ))
            {
                cmd.ExecuteNonQuery();
            }
        }
        void init()
        {
            indexInterface = new IndexInterface();
            timeStamp = new Dictionary<string, DateTime>();
            foreach (string type in indexInterface.Types)
            {
                timeStamp.Add(type, DateTime.Now);
                using (var cmd = new SqlCommand(
                "IF NOT EXISTS(SELECT * FROM sysobjects WHERE [id] =object_id(@TableName)and OBJECTPROPERTY(id, N'IsUserTable') = 1)\r\n" +
                "BEGIN\r\n" +
                "CREATE TABLE [" + type + "]\r\n" +
                "(\r\n" +
                "    [ID] INT,\r\n" +
                "    [title] nvarchar(50),\r\n" +
                "    [content] nvarchar(max),\r\n" +
                "    [time] datetime\r\n" +
                "    CONSTRAINT PK_"+type+" PRIMARY KEY([ID])\r\n" +
                ")\r\n" +
                "CREATE FULLTEXT INDEX ON [" + type + "]([content]) KEY INDEX PK_"+type+" ON ft_Indexer WITH(CHANGE_TRACKING = AUTO)\r\n" +
                "END\r\n" +
                "IF NOT EXISTS(SELECT * FROM sysobjects WHERE ID=object_id(@SuffixTableName) and OBJECTPROPERTY(id, N'IsUserTable') = 1)\r\n" +
                "BEGIN\r\n" +
                "    CREATE  TABLE [Suffix" + type + "]\r\n" +
                "    (\r\n" +
                "        [ID] INT FOREIGN KEY([ID]) REFERENCES ["+type+"]([ID]) ON DELETE CASCADE,\r\n" +
                "        [content] nvarchar(50)\r\n" +
                "    )\r\n" +
                "    CREATE NONCLUSTERED INDEX IDX_CONTENT ON [Suffix" + type + "] ([content])\r\n" +
                "    CREATE NONCLUSTERED INDEX IDX_ID ON [Suffix" + type + "] ([ID])\r\n" +
                "END\r\n", conn
                ))
                {
                    cmd.Parameters.AddWithValue("TableName", type);
                    cmd.Parameters.AddWithValue("SuffixTableName", "Suffix" + type);
                    cmd.ExecuteNonQuery();
                }
                int flg;
                using (var cmd = new SqlCommand("select COUNT(*) from dbo.sysobjects where id = object_id(N'fn_search_" + type + "') and xtype='TF'", conn))
                {
                    flg = (int)cmd.ExecuteScalar();
                }
                if (flg == 0)
                {
                    using (var cmd = new SqlCommand("" +
                            "CREATE FUNCTION fn_search_" + type + "(@key nvarchar(50),@top int,@fulltext int = 1)\r\n" +
                            "RETURNS @ReturnTable TABLE([ID] INT,[title] nvarchar(50),[content] varchar(max))\r\n" +
                            "AS\r\n" +
                            "BEGIN\r\n" +
                            "    SELECT @key=REPLACE(@key,'%',' ')\r\n" +
                            "    SELECT @key=REPLACE(@key,'_',' ')\r\n" +
                            "    SELECT @key=REPLACE(@key,'[',' ')\r\n" +
                            "    SELECT @key=REPLACE(@key,']',' ')\r\n" +
                            "    SELECT @key=REPLACE(@key,'^',' ')\r\n" +
                            "    DECLARE @T1 Table([ID] INT,[RANK] INT)\r\n" +
                            "    DECLARE @T2 Table([ID] INT,[RANK] INT)\r\n" +
                            "    IF @fulltext <> 0\r\n" +
                            "       INSERT INTO @T1\r\n" +
                            "       SELECT  DISTINCT [ID],[RANK]=1000000  FROM [Suffix" + type + "] WHERE [content] like @key+'%' UNION ALL\r\n" +
                            "       SELECT * FROM FREETEXTTABLE([" + type + "],[content],@key)\r\n" +
                            "    ELSE\r\n" +
                            "       INSERT INTO @T1\r\n" +
                            "       SELECT  DISTINCT [ID],[RANK]=1000000  FROM [Suffix" + type + "] WHERE [content] like @key+'%'" +
                            "    INSERT INTO @T2\r\n" +
                            "    SELECT TOP(@top) [ID],SUM([RANK]) FROM @T1 GROUP BY [ID] ORDER BY SUM([RANK]) DESC\r\n" +
                            "    INSERT INTO @ReturnTable\r\n" +
                            "    SELECT [@T2].[ID],[title],[content] FROM\r\n" +
                            "    @T2 LEFT JOIN ["+type+"] ON [@T2].[ID]=["+type+"].[ID]\r\n" +
                            "    RETURN\r\n" +
                            "END\r\n", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
        void doIndex()
        {
            bool reNew = true;
            foreach (string type in indexInterface.Types)
            {
                IndexItem item;
                if (null != (item = indexInterface.Next(type)))
                {
                    reNew = false;
                    using (var cmd = new SqlCommand(
                        "IF NOT EXISTS(SELECT * FROM [" + type + "] WHERE [ID]=@ID)\r\n" +
                        "   INSERT INTO [" + type + "]([ID],[title],[content],[time]) VALUES(@ID,@title,@content,@time)\r\n" +
                        "ELSE\r\n" +
                        //"IF "+
                        "   UPDATE [" + type + "] SET [content]=@content,[title]=@title,[time]=@time WHERE [ID]=@ID\r\n"
                        , conn))
                    {
                        cmd.Parameters.AddWithValue("ID", item.ID);
                        cmd.Parameters.AddWithValue("content", item.Content);
                        cmd.Parameters.AddWithValue("title", item.Title);
                        cmd.Parameters.AddWithValue("time", timeStamp[type]);
                        cmd.ExecuteNonQuery();
                    }
                    using (var cmd = new SqlCommand("DELETE FROM [Suffix" + type + "] WHERE ID=@ID", conn))
                    {
                        cmd.Parameters.AddWithValue("ID", item.ID);
                        cmd.ExecuteNonQuery();
                    }
                    item.Keywords.Add(item.Title);
                    foreach (string keyword in item.Keywords)
                    {
                        for (int i = 0; i < keyword.Length; i++)
                        {
                            using (var cmd = new SqlCommand("INSERT INTO [Suffix" + type + "]([ID],[content]) VALUES(@ID,@keyword)", conn))
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
            {
                foreach (string type in indexInterface.Types)
                {
                    using (var cmd = new SqlCommand("DELETE FROM [" + type + "] WHERE [time]<>@time", conn))
                    {
                        cmd.Parameters.AddWithValue("time", timeStamp[type]);
                        cmd.ExecuteNonQuery();
                    }
                }
                indexInterface = null;
                sleepTime = 10000;
            }
        }
        protected override int Run()
        {
            if (null == indexInterface)
            {
                init();
            }
            doIndex();
            return sleepTime;
        }
    }
}
