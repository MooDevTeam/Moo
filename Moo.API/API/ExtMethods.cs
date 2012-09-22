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
                MaximumScore = problem.MaximumScore,
                AverageScore = problem.SubmissionUser != 0 ? (double?)(problem.ScoreSum / (double)problem.SubmissionUser) : null,
                MyScore = myRecords.Any() ? (int?)myRecords.Max(r => r.JudgeInfo.Score) : null,
                SubmissionCount = problem.SubmissionCount,
                SubmissionUser = problem.SubmissionUser,
                LatestRevision = problem.LatestRevision == null ? null : (Guid?)problem.LatestRevision.ID,
                LatestSolution = problem.LatestSolution == null ? null : (Guid?)problem.LatestSolution.ID
            };
        }

        public static FullProblemRevision ToFullProblemRevision(this ProblemRevision revision)
        {
            return new FullProblemRevision()
            {
                ID=revision.ID,
                Content=revision.Content,
                Reason=revision.Reason,
                CreatedBy=revision.CreatedBy.ID,
                Problem=revision.Problem.ID,
            };
        }

        public static BriefProblemRevision ToBriefProblemRevision(this ProblemRevision revision)
        {
            return new BriefProblemRevision()
            {
                ID = revision.ID,
                Reason = revision.Reason,
                CreatedBy = revision.CreatedBy.ID,
                Problem = revision.Problem.ID
            };
        }
    }
}