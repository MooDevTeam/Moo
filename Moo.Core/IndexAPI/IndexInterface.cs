using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moo.Core.DB;
namespace Moo.Core.IndexAPI
{
    public class IndexInterface
    {
        /// <summary>
        /// 获取供索引的类型
        /// </summary>
        public IEnumerable<string> Types
        {
            get
            {
                return new List<string>
                {
                    "Problem","Article","User","Contest","Tag"
                };
            }
        }

        /// <summary>
        /// 返回下一项实体
        /// </summary>
        /// <param name="type">实体类型，出自Types</param>
        /// <returns>下一项实体，null代表本类型实体结束</returns>
        /// <exception cref="ArgumentException">类型不存在</exception>
        public IndexItem Next(string type)
        {
            switch (type)
            {
                case "Problem":
                    return NextProblem();
                case "User":
                    return NextUser();
                case "Article":
                    return NextArticle();
                case "Contest":
                    return NextContest();
                case "Tag":
                    return NextTag();
                default:
                    throw new ArgumentException("类型不存在", "type");
            }
        }

        int passedProblemNumber;
        IndexItem NextProblem()
        {
            using (MooDB db = new MooDB())
            {
                Problem problem = (from p in db.Problems
                                   where !p.Hidden
                                   orderby p.ID
                                   select p).Skip(passedProblemNumber++).FirstOrDefault();
                if (problem == null) return null;

                return new IndexItem
                {
                    ID = problem.ID,
                    Content = problem.LatestRevision == null ? null : problem.LatestRevision.Content,
                    Keywords = problem.Tag.Select(t => t.Name).ToList(),
                    Title = problem.Name
                };
            }
        }

        int passedUserNumber;
        IndexItem NextUser()
        {
            using (MooDB db = new MooDB())
            {
                User user = (from u in db.Users
                             orderby u.ID
                             select u).Skip(passedUserNumber++).FirstOrDefault();
                if (user == null) return null;
                return new IndexItem
                {
                    ID = user.ID,
                    Content = user.BriefDescription + user.Description,
                    Keywords = new List<string>(),
                    Title = user.Name,
                };
            }
        }

        int passedArticleNumber;
        IndexItem NextArticle()
        {
            using (MooDB db = new MooDB())
            {
                Article article = (from a in db.Articles
                                   orderby a.ID
                                   select a).Skip(passedArticleNumber++).FirstOrDefault();
                if (article == null) return null;
                return new IndexItem
                {
                    ID = article.ID,
                    Content = article.LatestRevision.Content,
                    Keywords = article.Tag.Select(t => t.Name).ToList(),
                    Title = article.Name
                };
            }
        }

        int passedContestNumber;
        IndexItem NextContest()
        {
            using (MooDB db = new MooDB())
            {
                Contest contest = (from c in db.Contests
                                   orderby c.ID
                                   select c).Skip(passedContestNumber++).FirstOrDefault();
                if (contest == null) return null;
                return new IndexItem
                {
                    ID = contest.ID,
                    Content = contest.Description,
                    Keywords = new List<string>(),
                    Title = contest.Name
                };
            }
        }

        int passedTagNumber;
        IndexItem NextTag()
        {
            using (MooDB db = new MooDB())
            {
                Tag tag = (from t in db.Tags
                           orderby t.ID
                           select t).Skip(passedTagNumber++).FirstOrDefault();
                if (tag == null) return null;

                return new IndexItem
                {
                    ID = tag.ID,
                    Keywords = new List<string>(),
                    Title = tag.Name,
                    Content = ""
                };
            }
        }
    }
}
