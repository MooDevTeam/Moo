using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;

using Moo.Core.Logic;
using Moo.Core.DB;
using Moo.Core.Security;
namespace Moo.API.API
{
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class JsonAPI
    {
        #region Test
        [OperationContract]
        public string Echo(string text)
        {
            return text;
        }
        [OperationContract]
        public string Debug()
        {
            return null;
        }
        #endregion

        #region Misc
        [OperationContract]
        public string Wiki2HTML(string wiki)
        {
            return wiki;
        }
        #endregion

        #region Security
        [OperationContract]
        public string Login(string userName, string password)
        {
            return Security.Login(userName, password);
        }

        [OperationContract]
        public BriefUser GetCurrentUser(string token)
        {
            Security.Authenticate(token);
            SiteUser currentUser = Security.CurrentUser;
            return new BriefUser()
            {
                ID = currentUser.ID,
                Name = currentUser.Name
            };
        }

        [OperationContract]
        public void Logout(string token)
        {
            Security.Authenticate(token);
            Security.Logout();
        }
        #endregion

        #region Problems
        [OperationContract]
        public int CreateProblem(string token, FullProblem problem)
        {
            Security.Authenticate(token);
            Problem newProblem = new Problem()
            {
                Name = problem.Name,
                Type = problem.Type,
            };
            Problems.Create(newProblem);
            return newProblem.ID;
        }

        public List<BriefProblem> ListProblem(string token)
        {
            Security.Authenticate(token);
            return (from p in Problems.List()
                    select new BriefProblem()
                    {
                        ID = p.ID,
                        Name = p.Name
                    }).ToList<BriefProblem>();
        }

        [OperationContract]
        public FullProblem GetProblem(string token, int id)
        {
            Security.Authenticate(token);
            Problem problem = Problems.Get(id);
            return new FullProblem()
            {
                ID = problem.ID,
                Name = problem.Name,
                Type = problem.Type,
                Lock = problem.Lock,
                LockPost = problem.LockPost,
                LockRecord = problem.LockRecord,
                LockSolution = problem.LockSolution,
                LockTestCase = problem.LockTestCase,
                AllowTesting = problem.AllowTesting,
                Hidden = problem.Hidden,
                TestCaseHidden = problem.TestCaseHidden,
                MaximumScore = problem.MaximumScore,
                ScoreSum = problem.ScoreSum,
                SubmissionCount = problem.SubmissionCount,
                SubmissionUser = problem.SubmissionUser,
                LatestRevision = problem.LatestRevision == null ? null : (int?)problem.LatestRevision.ID,
                LatestSolution = problem.LatestSolution == null ? null : (int?)problem.LatestSolution.ID
            };
        }

        [OperationContract]
        public void DeleteProblem(string token, int id)
        {
            Security.Authenticate(token);
            Problems.Delete(id);
        }

        [OperationContract]
        public void UpdateProblem(string token, FullProblem problem)
        {
            Security.Authenticate(token);
            Problems.Update(new Problem()
            {
                ID = problem.ID,
                Name = problem.Name,
                AllowTesting = problem.AllowTesting,
                Hidden = problem.Hidden,
                Lock = problem.Lock,
                LockPost = problem.LockPost,
                LockRecord = problem.LockRecord,
                LockSolution = problem.LockSolution,
                LockTestCase = problem.LockTestCase,
                TestCaseHidden = problem.TestCaseHidden,
                Type = problem.Type
            });
        }

        [OperationContract]
        public FullProblemRevision GetProblemRevision(string token, int id)
        {
            Security.Authenticate(token);
            ProblemRevision revision = Problems.GetRevision(id);
            return new FullProblemRevision()
            {
                ID = revision.ID,
                Content = revision.Content,
                Reason = revision.Reason,
                CreatedBy = revision.CreatedBy.ID,
                Problem = revision.Problem.ID
            };
        }

        [OperationContract]
        public int CreateProblemRevision(string token, FullProblemRevision revision)
        {
            Security.Authenticate(token);
            ProblemRevision problemRevision = new ProblemRevision()
            {
                Content = revision.Content,
                Problem = new Problem() { ID = revision.Problem },
                Reason = revision.Reason,
            };
            Problems.CreateRevision(problemRevision);
            return problemRevision.ID;
        }

        [OperationContract]
        public void DeleteProblemRevision(string token, int id)
        {
            Security.Authenticate(token);
            Problems.DeleteRevision(id);
        }

        #endregion
    }
}
