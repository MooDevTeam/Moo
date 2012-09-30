using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Moo.Core.DB;
using Moo.Core.Security;
namespace Moo.API.DataContracts
{
    public static class ExtMethods
    {
        public static FullProblem ToFullProblem(this Problem problem, MooDB db)
        {
            var myRecords = from r in db.Records
                            where r.Problem.ID == problem.ID && r.User.ID == Security.CurrentUser.ID
                                  && r.JudgeInfo != null && r.JudgeInfo.Score >= 0
                            select r;
            return new FullProblem()
            {
                ID = problem.ID,
                Name = problem.Name,
                Type = problem.Type,
                CreateTime = problem.CreateTime,
                CreatedBy = problem.CreatedBy.ID,
                MaximumScore = problem.MaximumScore,
                AverageScore = problem.SubmissionUser != 0 ? (double?)(problem.ScoreSum / (double)problem.SubmissionUser) : null,
                MyScore = myRecords.Any() ? (int?)myRecords.Max(r => r.JudgeInfo.Score) : null,
                SubmissionCount = problem.SubmissionCount,
                SubmissionUser = problem.SubmissionUser,
                LatestRevision = problem.LatestRevision == null ? null : (int?)problem.LatestRevision.ID,
            };
        }

        public static FullProblemRevision ToFullProblemRevision(this ProblemRevision revision)
        {
            return new FullProblemRevision()
            {
                ID = revision.ID,
                Content = revision.Content,
                CreateTime = revision.CreateTime,
                Reason = revision.Reason,
                CreatedBy = revision.CreatedBy.ID,
                Problem = revision.Problem.ID,
            };
        }

        public static BriefProblemRevision ToBriefProblemRevision(this ProblemRevision revision)
        {
            return new BriefProblemRevision()
            {
                ID = revision.ID,
                Reason = revision.Reason,
                CreateTime = revision.CreateTime,
                CreatedBy = revision.CreatedBy.ID,
                Problem = revision.Problem.ID
            };
        }

        public static BriefRecord ToBriefRecord(this Record record, MooDB db)
        {
            return new BriefRecord()
            {
                ID = record.ID,
                CreateTime = record.CreateTime,
                PublicCode = record.PublicCode,
                JudgeInfo = record.JudgeInfo == null ? null : (int?)record.JudgeInfo.ID,
                Language = record.Language,
                Problem = record.Problem.ID,
                User = record.User.ID
            };
        }

        public static FullRecord ToFullRecord(this Record record, MooDB db)
        {
            return new FullRecord()
            {
                ID = record.ID,
                CreateTime = record.CreateTime,
                PublicCode = record.PublicCode,
                Code = Access.Check(db, record, Function.ReadRecordCode) ? record.Code : null,
                JudgeInfo = record.JudgeInfo == null ? null : (int?)record.JudgeInfo.ID,
                Language = record.Language,
                Problem = record.Problem.ID,
                User = record.User.ID
            };
        }

        public static FullJudgeInfo ToFullJudgeInfo(this JudgeInfo info)
        {
            return new FullJudgeInfo()
            {
                ID = info.ID,
                Info = info.Info,
                Record = info.Record.ID,
                Score = info.Score
            };
        }

        public static FullTestCase ToFullTestCase(this TestCase testCase)
        {
            return new FullTestCase()
            {
                ID = testCase.ID,
                CreatedBy = testCase.CreatedBy.ID,
                Problem = testCase.Problem.ID
            };
        }

        public static FullTranditionalTestCase ToFullTranditionalTestCase(this TranditionalTestCase testCase)
        {
            return new FullTranditionalTestCase()
            {
                ID = testCase.ID,
                Answer = testCase.Answer,
                CreatedBy = testCase.CreatedBy.ID,
                Input = testCase.Input,
                MemoryLimit = testCase.MemoryLimit,
                Problem = testCase.Problem.ID,
                Score = testCase.Score,
                TimeLimit = testCase.TimeLimit,
            };
        }

        public static FullSpecialJudgedTestCase ToFullSpecialJudgedTestCase(this SpecialJudgedTestCase testCase)
        {
            return new FullSpecialJudgedTestCase()
            {
                ID = testCase.ID,
                Answer = testCase.Answer,
                CreatedBy = testCase.CreatedBy.ID,
                Input = testCase.Input,
                Judger = testCase.Judger.ID,
                MemoryLimit = testCase.MemoryLimit,
                TimeLimit = testCase.TimeLimit,
                Problem = testCase.Problem.ID
            };
        }

        public static FullInteractiveTestCase ToFullInteractiveTestCase(this InteractiveTestCase testCase)
        {
            return new FullInteractiveTestCase()
            {
                ID = testCase.ID,
                CreatedBy = testCase.CreatedBy.ID,
                Invoker = testCase.Invoker.ID,
                MemoryLimit = testCase.MemoryLimit,
                Problem = testCase.Problem.ID,
                TestData = testCase.TestData,
                TimeLimit = testCase.TimeLimit,
            };
        }

        public static FullAnswerOnlyTestCase ToFullAnswerOnlyTestCase(this AnswerOnlyTestCase testCase)
        {
            return new FullAnswerOnlyTestCase()
            {
                CreatedBy = testCase.CreatedBy.ID,
                ID = testCase.ID,
                Judger = testCase.Judger.ID,
                Problem = testCase.Problem.ID,
                TestData = testCase.TestData,
            };
        }

        public static FullUser ToFullUser(this User user)
        {
            return new FullUser()
            {
                BriefDescription = user.BriefDescription,
                Description = user.Description,
                Email = user.Email,
                ID = user.ID,
                Name = user.Name,
                PreferredLanguage = user.PreferredLanguage,
                Role = user.Role.ID,
                Score = user.Score
            };
        }

        public static BriefUser ToBriefUser(this User user)
        {
            return new BriefUser()
            {
                ID = user.ID,
                Name = user.Name
            };
        }

        public static FullPost ToFullPost(this Post post)
        {
            return new FullPost()
            {
                ID = post.ID,
                Name = post.Name,
                OnTop = post.OnTop,
                Problem = post.Problem == null ? null : (int?)post.Problem.ID,
                ReplyTime = post.ReplyTime
            };
        }

        public static BriefPostItem ToBriefPostItem(this PostItem postItem)
        {
            return new BriefPostItem()
            {
                ID = postItem.ID,
                CreatedBy = postItem.CreatedBy.ID,
                Post = postItem.Post.ID,
                CreateTime = postItem.CreateTime
            };
        }

        public static FullPostItem ToFullPostItem(this PostItem postItem)
        {
            return new FullPostItem()
            {
                ID = postItem.ID,
                Content = postItem.Content,
                CreateTime = postItem.CreateTime,
                CreatedBy = postItem.CreatedBy.ID,
                Post = postItem.Post.ID,
            };
        }

        public static FullArticle ToFullArticle(this Article article)
        {
            return new FullArticle()
            {
                ID = article.ID,
                CreatedBy = article.CreatedBy.ID,
                CreateTime = article.CreateTime,
                LatestRevision = article.LatestRevision == null ? null : (int?)article.LatestRevision.ID,
                Name = article.Name,
                Category = article.Category.ID,
                Problem = article.Problem.ID
            };
        }

        public static BriefArticleRevision ToBriefArticleRevision(this ArticleRevision articleRevision)
        {
            return new BriefArticleRevision()
            {
                Article = articleRevision.Article.ID,
                CreatedBy = articleRevision.CreatedBy.ID,
                CreateTime = articleRevision.CreateTime,
                ID = articleRevision.ID
            };
        }

        public static FullArticleRevision ToFullArticleRevision(this ArticleRevision articleRevision)
        {
            return new FullArticleRevision()
            {
                Article = articleRevision.Article.ID,
                Content = articleRevision.Content,
                CreatedBy = articleRevision.CreatedBy.ID,
                CreateTime = articleRevision.CreateTime,
                ID = articleRevision.ID
            };
        }

        public static FullCategory ToFullCategory(this Category category)
        {
            return new FullCategory()
            {
                ID = category.ID,
                Name = category.Name
            };
        }

        public static BriefMail ToBriefMail(this Mail mail)
        {
            return new BriefMail()
            {
                CreateTime = mail.CreateTime,
                From = mail.From.ID,
                ID = mail.ID,
                IsRead = mail.IsRead,
                Name = mail.Name,
                To = mail.To.ID
            };
        }

        public static FullMail ToFullMail(this Mail mail)
        {
            return new FullMail()
            {
                CreateTime = mail.CreateTime,
                From = mail.From.ID,
                Content = mail.Content,
                ID = mail.ID,
                IsRead = mail.IsRead,
                Name = mail.Name,
                To = mail.To.ID
            };
        }
    }
}