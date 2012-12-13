using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moo.Core.DB;
namespace Moo.Core.IndexAPI
{
    public class IndexAPI
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
                    "Problem"
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
                default:
                    throw new ArgumentException("类型不存在", "type");
            }
        }

        int passedProblemNumber;
        IndexItem NextProblem()
        {
            using (MooDB db = new MooDB())
            {
                Problem problem = db.Problems.OrderBy(p => p.ID).Skip(passedProblemNumber++).FirstOrDefault();
                if (problem == null) return null;

                List<string> keywords = problem.Tag.Select(t => t.Name).ToList();
                keywords.Add(problem.Name);

                return new IndexItem
                {
                    ID = problem.ID,
                    Content = problem.LatestRevision == null ? null : problem.LatestRevision.Content,
                    Keywords = keywords
                };
            }
        }
    }
}
