using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Configuration;
namespace Moo.Core.IndexAPI
{
    public class Search
    {
        public static Search Instance = new Search();
        class MatchResult
        {
            public int pos;
            public int len;
        }
        public class SearchResult
        {
            public class ContentSegment
            {
                public bool Match;
                public string Text;
            }
            public int ID;
            public List<ContentSegment> Content;
            public string Title;
        }
        SqlConnection conn = new SqlConnection("Database=Indexer;" + ConfigurationManager.ConnectionStrings["IndexerDB"].ConnectionString);

        Search()
        {
            conn.Open();
        }
        public IEnumerable<SearchResult> search(string keyword, string type, int top)
        {
            keyword = keyword.Replace('%', ' ');
            List<string> keywords = split(keyword);
            List<SearchResult> ret = new List<SearchResult>();
            try
            {
                using (var cmd = new SqlCommand(""+
                    "DECLARE @T1 Table([ID] INT,[RANK] INT)\r\n"+
                    "DECLARE @T2 Table([ID] INT,[RANK] INT)\r\n"+
                    "INSERT INTO @T1\r\n"+
                    "SELECT  DISTINCT [ID],[RANK]=1000000  FROM [Suffix"+type+"] WHERE [content] like @key+'%' UNION ALL\r\n"+
                    "SELECT * FROM FREETEXTTABLE(["+type+"],[content],@key)\r\n"+
                    "INSERT INTO @T2\r\n"+
                    "SELECT TOP(@top) [ID],SUM([RANK]) FROM @T1 GROUP BY [ID] ORDER BY SUM([RANK]) DESC\r\n"+
                    "SELECT [@T2].[ID],[title],[content] FROM \r\n"+
                    "@T2 LEFT JOIN [Problem] ON [@T2].[ID]=[Problem].[ID]\r\n", conn))
                {
                    cmd.Parameters.AddWithValue("key", keyword);
                    cmd.Parameters.AddWithValue("top", top);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            ret.Add(new SearchResult()
                                {
                                    ID = reader.GetInt32(0),
                                    Title = reader.GetString(1),
                                    Content = getMatchRange(reader.GetString(2), keywords)
                                });
                    }
                }
            }
            catch (Exception)
            {
                Console.Beep();
            }
            return ret;
        }
        public List<string> split(string text)
        {
            text = text.Replace('"', ' ');
            List<string> ret = new List<string>();
            using (var cmd = new SqlCommand("SELECT [display_term] FROM sys.dm_fts_parser(@text,2052,0,0) WHERE [special_term]='Exact Match'", conn))
            {
                cmd.Parameters.AddWithValue("text", "\"" + text + "\"");
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ret.Add(reader.GetString(0));
                    }
                }
            }
            return ret;
        }
        public List<SearchResult.ContentSegment> getMatchRange(string content, List<string> keywords)
        {
            List<SearchResult.ContentSegment> ret = new List<SearchResult.ContentSegment>();
            List<MatchResult> match = new List<MatchResult>();
            int last = 0;
            for (int i = 0; i < content.Length; i++)
            {
                foreach (string key in keywords)
                {
                    if (i+key.Length<content.Length && key == content.Substring(i, key.Length))
                    {
                        if (last < i)
                        {
                            ret.Add(new SearchResult.ContentSegment() { Match = false, Text = content.Substring(last, i - last) });
                            last = i + key.Length;
                        }
                        ret.Add(new SearchResult.ContentSegment() { Match = true, Text = key });
                        i += key.Length - 1;
                        break;
                    }
                }
            }
            if (last < content.Length - 1)
                ret.Add(new SearchResult.ContentSegment() { Match = false, Text = content.Substring(last) });
            return ret;
        }
    }
}
