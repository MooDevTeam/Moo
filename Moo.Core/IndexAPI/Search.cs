using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Configuration;
using System.Threading;

namespace Moo.Core.IndexAPI
{
    public class Search : IDisposable
    {
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

        public Search()
        {
            conn.Open();
        }
        public void Dispose()
        {
            conn.Dispose();
        }
        public IEnumerable<SearchResult> DoSearch(string keyword, string type, int top)
        {
            List<string> keywords = split(keyword);
            List<SearchResult> ret = new List<SearchResult>();
            using (var cmd = new SqlCommand("" +
                "SELECT * FROM [fn_search_" + type + "](@key,@top,@fulltext)", conn))
            {
                cmd.Parameters.AddWithValue("key", keyword);
                cmd.Parameters.AddWithValue("top", top);
                cmd.Parameters.AddWithValue("fulltext", keywords.Count);
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
            return ret;
        }
        List<string> split(string text)
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
        List<SearchResult.ContentSegment> getMatchRange(string content, List<string> keywords)
        {
            List<SearchResult.ContentSegment> ret = new List<SearchResult.ContentSegment>();
            List<MatchResult> match = new List<MatchResult>();
            int last = 0;
            for (int i = 0; i < content.Length; i++)
            {
                foreach (string key in keywords)
                {
                    if (i + key.Length < content.Length && key == content.Substring(i, key.Length))
                    {
                        if (last < i)
                        {
                            ret.Add(new SearchResult.ContentSegment() { Match = false, Text = content.Substring(last, i - last) });
                        }
                        ret.Add(new SearchResult.ContentSegment() { Match = true, Text = key });
                        i += key.Length - 1;
                        last = i + key.Length;
                        break;
                    }
                }
            }
            if (last < content.Length - 1)
                ret.Add(new SearchResult.ContentSegment() { Match = false, Text = content.Substring(last) });
            return ret;
        }
        public Int32 IndexStatistics(string type)
        {
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM [" + type + "] ", conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return reader.GetInt32(0);
                    else
                        throw new Exception(type + " Get Count Failed.");
                }
            }

        }
    }
}
