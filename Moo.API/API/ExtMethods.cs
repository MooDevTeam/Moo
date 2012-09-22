using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Moo.Core.DB;
using Moo.Core.Security;
namespace Moo.API.API
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
                MaximumScore = problem.MaximumScore,
                AverageScore = problem.SubmissionUser != 0 ? (double?)(problem.ScoreSum / (double)problem.SubmissionUser) : null,
                MyScore = myRecords.Any() ? (int?)myRecords.Max(r => r.JudgeInfo.Score) : null,
                SubmissionCount = problem.SubmissionCount,
                SubmissionUser = problem.SubmissionUser,
                LatestRevision = problem.LatestRevision == null ? null : (Guid?)problem.LatestRevision.ID,
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
                PublicCode = (from a in db.ACL
                              where a.Allowed && a.Object == record.ID
                                  && a.Subject == new SiteRoles(db).Reader.ID && a.Function.ID == Security.GetFunctionID("record.code.read")
                              select a).Any(),
                JudgeInfo = record.JudgeInfo == null ? null : (Guid?)record.JudgeInfo.ID,
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
                PublicCode = (from a in db.ACL
                              where a.Allowed && a.Object == record.ID
                                  && a.Subject == new SiteRoles(db).Reader.ID && a.Function.ID == Security.GetFunctionID("record.code.read")
                              select a).Any(),
                Code = Security.CheckPermission(db, record.ID, "Record", "record.code.read") ? record.Code : null,
                JudgeInfo = record.JudgeInfo == null ? null : (Guid?)record.JudgeInfo.ID,
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
    }
}