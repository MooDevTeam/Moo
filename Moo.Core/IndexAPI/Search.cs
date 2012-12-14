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
    class Search
    {
        public class MatchRange
        {
            public int pos, len;
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
        }
        static SqlConnection conn = new SqlConnection("Database=Indexer;" + ConfigurationManager.ConnectionStrings["IndexerDB"].ConnectionString);

        public static IEnumerable<SearchResult> search(string keyword, string type, int top)
        {
            List<SearchResult> ret = new List<SearchResult>();
            try
            {
                keyword = keyword.Replace('%', ' ');
                using (var cmd = new SqlCommand(""+
                    "DECLARE @Table Table([ID] INT,[RANK] INT)\r\n"+
                    "INSERT INTO @Table\r\n"+
                    "SELECT TOP @top DISTINCT [ID],[RANK]=1000000 FROM [Suffix"+type+"] WHERE [content] like @key+'%' UNION ALL \r\n"+
                    "SELECT * FROM FREETEXTTABLE(["+type+"],[content],@key,@top) \r\n"+
                    "SELECT TOP @top [ID],SUM(RANK) FROM @Table GROUP BY [ID] ORDER BY SUM([RANK]) DESC\r\n", conn))
                {
                    cmd.Parameters.AddWithValue("keyword", keyword);
                    cmd.Parameters.AddWithValue("top", top);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                        ret.Add(new SearchResult() { ID = reader.GetInt32(0), Content = new List<SearchResult.ContentSegment>() });
                }
            }
            catch (Exception)
            {
                Console.Beep();
            }
            return ret;
        }
        public static IEnumerable<string> split(string text)
        {
            List<string> ret = new List<string>();
            using (var cmd = new SqlCommand("SELECT [display_term] FROM sys.dm_fts_parser(@text,2052,0,0) WHERE [special_term]='Exact Match'", conn))
            {
                cmd.Parameters.AddWithValue("text", text);
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
        public static IEnumerable<MatchRange> getMatchRange(string content, string keyword)
        {
            List<MatchRange> ret = new List<MatchRange>();
            var keywords = split(keyword);
            foreach (string key in keywords)
            {
                int position=0;
                while ((position=content.IndexOf(key, position)) != -1)
                {
                    ret.Add(new MatchRange() { pos = position, len = key.Length });
                    position++;
                    if (position >= key.Length)
                        break;
                }
            }
            return ret;
        }
    }
}
