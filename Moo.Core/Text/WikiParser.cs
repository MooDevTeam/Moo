using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.Text;
using Moo.Core.DB;
namespace Moo.Core.Text
{
    /// <summary>
    /// Wiki文本解析
    /// </summary>
    public static class WikiParser
    {
        public static string Parse(string text)
        {
            using (MooDB db = new MooDB())
            {
                return Parse(db, text);
            }
        }

        public static string Parse(MooDB db,string text)
        {
            SortedSet<User> beAt;
            text = ParseAt(db, text, out beAt);
           // return new WikiPlex.WikiEngine().Render(text);
            return null;
        }

        class UserComparer : IComparer<User>
        {
            public int Compare(User a, User b)
            {
                return a.ID.CompareTo(b.ID);
            }
        }

        public static string ParseAt(MooDB db, string text,out SortedSet<User> userBeAt)
        {
            userBeAt = new SortedSet<User>(new UserComparer());
            StringBuilder sb = new StringBuilder(text);
            for (int i = 0; i < sb.Length; i++)
            {
                if (sb[i] == '@' && (i == 0 || char.IsWhiteSpace(sb[i - 1])) && i < sb.Length - 1 && !char.IsWhiteSpace(sb[i + 1]))
                {
                    int end = i + 1;
                    StringBuilder userNameSb = new StringBuilder();
                    while (end < sb.Length && !char.IsWhiteSpace(sb[end]))
                    {
                        userNameSb.Append(sb[end]);
                        end++;
                    }
                    string userName = userNameSb.ToString();
                    User user = (from u in db.Users
                                 where u.Name == userName
                                 select u).SingleOrDefault<User>();
                    if (user != null)
                    {
                        string link = "[url:@" + user.Name + "|../User/?id=" + user.ID + "]";
                        sb.Remove(i, end - i);
                        sb.Insert(i, link);
                        i += link.Length;
                        if (!userBeAt.Contains(user))
                        {
                            userBeAt.Add(user);
                        }
                    }
                }
            }
            return sb.ToString();
        }
    }
}