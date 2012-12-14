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
        static SqlConnection conn = new SqlConnection("Database=Indexer;" + ConfigurationManager.ConnectionStrings["IndexerDB"].ConnectionString);

        public static IEnumerable<int> search(string keyword, string type, int top)
        {
            List<int> ret = new List<int>();
            try
            {
                using (var cmd = new SqlCommand("SELECT [KEY] FROM FREETEXTTABLE("+type+",content,@keyword,@top) ORDER BY [RANK] DESC", conn))
                {
                    cmd.Parameters.Add(new SqlParameter("keyword", keyword));
                    cmd.Parameters.Add(new SqlParameter("top", top));
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                        ret.Add(reader.GetInt32(0));
                }
            }
            catch (Exception)
            {
                Console.Beep();
            }
            return ret;
        }
        static IEnumerable<string> split(string text)
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
            string[] keywords = split(keyword).ToArray();
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
