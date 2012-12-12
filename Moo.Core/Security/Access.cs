using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moo.Core.DB;
namespace Moo.Core.Security
{
    public class Access
    {
        SiteUser me;
        MooDB db;

        Dictionary<Function, List<Func<Problem, bool?>>> ProblemRules
        {
            get
            {
                return new Dictionary<Function, List<Func<Problem, bool?>>>
                {
                    {Function.CreateProblem,new List<Func<Problem,bool?>>(){
                        p=>me.Role>=SiteRole.NormalUser?(bool?)true:null,
                    }},
                    {Function.ReadProblem,new List<Func<Problem,bool?>>(){
                        p=>me.Role>=SiteRole.Reader
                    }},
                    {Function.ModifyProblem,new List<Func<Problem,bool?>>(){
                        p=>me.Role<=SiteRole.Reader?(bool?)false:null,
                        p=>me.Role>=SiteRole.Worker?(bool?)true:null,
                        p=>p.CreatedBy.ID==me.ID?(bool?)true:null,
                    }},
                    {Function.DeleteProblem,new List<Func<Problem,bool?>>(){
                        p=>me.Role<=SiteRole.Reader?(bool?)false:null,
                        p=>me.Role>=SiteRole.Worker?(bool?)true:null,
                        p=>p.CreatedBy.ID==me.ID?(bool?)true:null,
                    }}, 
                };
            }
        }

        Dictionary<Function, List<Func<ProblemRevision, bool?>>> ProblemRevisionRules
        {
            get
            {
                return new Dictionary<Function, List<Func<ProblemRevision, bool?>>>()
                {
                    {Function.CreateProblemRevision,new List<Func<ProblemRevision,bool?>>(){
                        r=>me.Role>=SiteRole.Worker?(bool?)true:null,
                        r=>r.Problem.Locked?(bool?)false:null,
                        r=>me.Role>=SiteRole.NormalUser?(bool?)true:null,
                    }},
                    {Function.ReadProblemRevision,new List<Func<ProblemRevision,bool?>>(){
                        r=>me.Role>=SiteRole.Worker?(bool?)true:null,
                        r=>r.Problem.Hidden?(bool?)false:null,
                        r=>me.Role>=SiteRole.Reader?(bool?)true:null,
                    }},
                    {Function.DeleteProblemRevision,new List<Func<ProblemRevision,bool?>>(){
                        r=>me.Role<=SiteRole.Reader?(bool?)false:null,
                        r=>me.Role>=SiteRole.Worker?(bool?)true:null,
                        r=>r.CreatedBy.ID==me.ID?(bool?)true:null
                    }},
                };
            }
        }

        Dictionary<Function, List<Func<User, bool?>>> UserRules
        {
            get
            {
                return new Dictionary<Function, List<Func<User, bool?>>>()
                {
                    {Function.CreateUser,new List<Func<User,bool?>>(){
                        u=>me.Role>=SiteRole.Worker?(bool?)true:null
                    }},
                    {Function.ReadUser,new List<Func<User,bool?>>(){
                        u=>me.Role>=SiteRole.Reader?(bool?)true:null
                    }},
                    {Function.ModifyUser,new List<Func<User,bool?>>(){
                        u=>me.Role<=SiteRole.Reader?(bool?)false:null,
                        u=>me.Role>=SiteRole.Worker && me.Role>(SiteRole)Enum.Parse(typeof(SiteRole),u.Role.Name)?(bool?)true:null,
                        u=>u.ID==me.ID?(bool?)true:null
                    }},
                    {Function.ModifyUserRole,new List<Func<User,bool?>>(){
                        u=>me.Role>=SiteRole.Organizer
                    }},
                };
            }
        }

        Dictionary<Function, List<Func<Record, bool?>>> RecordRules
        {
            get
            {
                return new Dictionary<Function, List<Func<Record, bool?>>>()
                {
                    {Function.CreateRecord,new List<Func<Record,bool?>>(){
                        r=>me.Role>=SiteRole.Worker?(bool?)true:null,
                        r=>r.Problem.RecordLocked?(bool?)false:null,
                        r=>me.Role>=SiteRole.NormalUser?(bool?)true:null
                    }},
                    {Function.ReadRecord,new List<Func<Record,bool?>>(){
                        r=>me.Role>=SiteRole.Reader?(bool?)true:null
                    }},
                    {Function.ReadRecordCode,new List<Func<Record,bool?>>(){
                        r=>me.Role>=SiteRole.Worker?(bool?)true:null,
                        r=>r.PublicCode?(bool?)true:null,
                        r=>r.User.ID==me.ID?(bool?)true:null
                    }},
                    {Function.ModifyRecord,new List<Func<Record,bool?>>(){
                        r=>me.Role<=SiteRole.Reader?(bool?)false:null,
                        r=>me.Role>=SiteRole.Worker?(bool?)true:null,
                        r=>r.Problem.RecordLocked?(bool?)false:null,
                        r=>r.User.ID==me.ID?(bool?)true:null
                    }},
                    {Function.DeleteRecord,new List<Func<Record,bool?>>(){
                        r=>me.Role<=SiteRole.Reader?(bool?)false:null,
                        r=>me.Role>=SiteRole.Worker?(bool?)true:null,
                        r=>r.User.ID==me.ID?(bool?)true:null
                    }},
                };
            }
        }

        Dictionary<Function, List<Func<JudgeInfo, bool?>>> JudgeInfoRules
        {
            get
            {
                return new Dictionary<Function, List<Func<JudgeInfo, bool?>>>
                {
                    {Function.ReadJudgeInfo,new List<Func<JudgeInfo,bool?>>{
                        j=>me.Role>=SiteRole.Worker?(bool?)true:null,
                        j=>j.Record.Problem.JudgeInfoHidden?(bool?)false:null,
                        j=>me.Role>=SiteRole.Reader?(bool?)true:false
                    }},
                    {Function.DeleteJudgeInfo,new List<Func<JudgeInfo,bool?>>{
                        j=>me.Role>=SiteRole.NormalUser?(bool?)true:null
                    }}
                };
            }
        }

        Dictionary<Function, List<Func<TestCase, bool?>>> TestCaseRules
        {
            get
            {
                return new Dictionary<Function, List<Func<TestCase, bool?>>>
                {
                    {Function.CreateTestCase,new List<Func<TestCase,bool?>>{
                        t=>me.Role>=SiteRole.Worker?(bool?)true:null,
                        t=>t.Problem.TestCaseLocked?(bool?)false:null,
                        t=>me.Role>=SiteRole.NormalUser?(bool?)true:null
                    }},
                    {Function.ReadTestCase,new List<Func<TestCase,bool?>>{
                        t=>me.Role>=SiteRole.Worker?(bool?)true:null,
                        t=>t.Problem.TestCaseHidden?(bool?)false:null,
                        t=>me.Role>=SiteRole.Reader?(bool?)true:null
                    }},
                    {Function.ModifyTestCase,new List<Func<TestCase,bool?>>{
                        t=>me.Role<=SiteRole.Reader?(bool?)false:null,
                        t=>me.Role>=SiteRole.Worker?(bool?)true:null,
                        t=>t.Problem.TestCaseLocked?(bool?)false:null,
                        t=>t.CreatedBy.ID==me.ID?(bool?)true:null,
                    }},
                    {Function.DeleteTestCase,new List<Func<TestCase,bool?>>{
                        t=>me.Role<=SiteRole.Reader?(bool?)false:null,
                        t=>me.Role>=SiteRole.Worker?(bool?)true:null,
                        t=>t.Problem.TestCaseLocked?(bool?)false:null,
                        t=>t.CreatedBy.ID==me.ID?(bool?)true:null,
                    }},
                };
            }
        }

        Dictionary<Function, List<Func<Post, bool?>>> PostRules
        {
            get
            {
                return new Dictionary<Function, List<Func<Post, bool?>>>
                {
                    {Function.CreatePost,new List<Func<Post,bool?>>{
                        p=>me.Role>=SiteRole.Worker?(bool?)true:null,
                        p=>p.Problem!=null && p.Problem.PostLocked?(bool?)false:null,
                        p=>me.Role>=SiteRole.NormalUser?(bool?)true:null
                    }},
                    {Function.ReadPost,new List<Func<Post,bool?>>{
                        p=>me.Role>=SiteRole.Reader?(bool?)true:null
                    }},
                    {Function.ModifyPost,new List<Func<Post,bool?>>{
                        p=>me.Role>=SiteRole.Worker?(bool?)true:null,
                    }},
                    {Function.DeletePost,new List<Func<Post,bool?>>{
                        p=>me.Role>=SiteRole.Worker?(bool?)true:null,
                    }},
                };
            }
        }

        Dictionary<Function, List<Func<PostItem, bool?>>> PostItemRules
        {
            get
            {
                return new Dictionary<Function, List<Func<PostItem, bool?>>>
                {
                    {Function.CreatePostItem,new List<Func<PostItem,bool?>>{
                        i=>me.Role>=SiteRole.Worker?(bool?)true:null,
                        i=>i.Post.Locked?(bool?)false:null,
                        i=>i.Post.Problem!=null && i.Post.Problem.PostLocked?(bool?)false:null,
                        i=>me.Role>=SiteRole.NormalUser?(bool?)true:null
                    }},
                    {Function.ReadPostItem,new List<Func<PostItem,bool?>>{
                        i=>me.Role>=SiteRole.Reader?(bool?)true:null
                    }},
                    {Function.ModifyPostItem,new List<Func<PostItem,bool?>>{
                        i=>me.Role<=SiteRole.Reader?(bool?)false:null,
                        i=>me.Role>=SiteRole.Worker?(bool?)true:null,
                        i=>i.Post.Locked?(bool?)false:null,
                        i=>i.Post.Problem!=null && i.Post.Problem.PostLocked?(bool?)false:null,
                        i=>i.CreatedBy.ID==me.ID?(bool?)true:null
                    }},
                    {Function.DeletePostItem,new List<Func<PostItem,bool?>>{
                        i=>me.Role<=SiteRole.Reader?(bool?)false:null,
                        i=>me.Role>=SiteRole.Worker?(bool?)true:null,
                        i=>i.Post.Locked?(bool?)false:null,
                        i=>i.Post.Problem!=null && i.Post.Problem.PostLocked?(bool?)false:null,
                        i=>i.CreatedBy.ID==me.ID?(bool?)true:null
                    }},
                };
            }
        }

        Dictionary<Function, List<Func<Article, bool?>>> ArticleRules
        {
            get
            {
                return new Dictionary<Function, List<Func<Article, bool?>>>
                {
                    {Function.CreateArticle,new List<Func<Article,bool?>>{
                        a=>me.Role>=SiteRole.Worker?(bool?)true:null,
                        a=>a.Problem!=null && a.Problem.ArticleLocked?(bool?)false:null,
                        a=>me.Role>=SiteRole.NormalUser?(bool?)true:null
                    }},
                    {Function.ReadArticle,new List<Func<Article,bool?>>{
                        a=>me.Role>=SiteRole.Reader?(bool?)true:null
                    }},
                    {Function.ModifyArticle,new List<Func<Article,bool?>>{
                        a=>me.Role<=SiteRole.Reader?(bool?)false:null,
                        a=>me.Role>=SiteRole.Worker?(bool?)true:null,
                        a=>a.Problem!=null && a.Problem.ArticleLocked?(bool?)false:null,
                        a=>a.CreatedBy.ID==me.ID?(bool?)true:null
                    }},
                    {Function.DeleteArticle,new List<Func<Article,bool?>>{
                        a=>me.Role<=SiteRole.Reader?(bool?)false:null,
                        a=>me.Role>=SiteRole.Worker?(bool?)true:null,
                        a=>a.Problem!=null && a.Problem.ArticleLocked?(bool?)false:null,
                        a=>a.CreatedBy.ID==me.ID?(bool?)true:null
                    }},
                };
            }
        }

        Dictionary<Function, List<Func<ArticleRevision, bool?>>> ArticleRevisionRules
        {
            get
            {
                return new Dictionary<Function, List<Func<ArticleRevision, bool?>>>
                {
                    {Function.CreateArticleRevision,new List<Func<ArticleRevision,bool?>>{
                        r=>me.Role>=SiteRole.Worker?(bool?)true:null,
                        r=>r.Article.Problem!=null && r.Article.Problem.ArticleLocked?(bool?)false:null,
                        r=>me.Role>=SiteRole.NormalUser?(bool?)true:null
                    }},
                    {Function.ReadArticleRevision,new List<Func<ArticleRevision,bool?>>{
                        r=>me.Role>=SiteRole.Reader?(bool?)true:null
                    }},
                    {Function.DeleteArticleRevision,new List<Func<ArticleRevision,bool?>>{
                        r=>me.Role<=SiteRole.Reader?(bool?)false:null,
                        r=>me.Role>=SiteRole.Worker?(bool?)true:null,
                        r=>r.Article.Problem!=null && r.Article.Problem.ArticleLocked?(bool?)false:null,
                        r=>r.CreatedBy.ID==me.ID?(bool?)true:null
                    }},
                };
            }
        }

        Dictionary<Function, List<Func<Contest, bool?>>> ContestRules
        {
            get
            {
                return new Dictionary<Function, List<Func<Contest, bool?>>>
                {
                    {Function.AttendContest,new List<Func<Contest,bool?>>{
                        c=>me.Role>=SiteRole.NormalUser?(bool?)true:null
                    }},
                    {Function.ReadContestResult,new List<Func<Contest,bool?>>{
                        c=>me.Role>=SiteRole.Worker?(bool?)true:null,
                        c=>!c.ViewResultAnyTime && DateTime.Now<c.EndTime?(bool?)false:null,
                        c=>me.Role>=SiteRole.Reader?(bool?)true:null
                    }},
                    {Function.CreateContest,new List<Func<Contest,bool?>>{
                        c=>me.Role>=SiteRole.Worker?(bool?)true:null
                    }},
                    {Function.DeleteContest,new List<Func<Contest,bool?>>{
                        c=>me.Role>=SiteRole.Worker?(bool?)true:null
                    }},
                    {Function.ModifyContest,new List<Func<Contest,bool?>>{
                        c=>me.Role>=SiteRole.Worker?(bool?)true:null
                    }},
                    {Function.ReadContest,new List<Func<Contest,bool?>>{
                        c=>me.Role>=SiteRole.Reader?(bool?)true:null
                    }}
                };
            }
        }

        Dictionary<Function, List<Func<Message, bool?>>> MessageRules
        {
            get
            {
                return new Dictionary<Function, List<Func<Message, bool?>>>
                {
                    {Function.CreateMessage,new List<Func<Message,bool?>>{
                        m=>me.Role>=SiteRole.NormalUser?(bool?)true:null
                    }},
                    {Function.DeletePublicMessage,new List<Func<Message,bool?>>{
                        m=>me.Role>=SiteRole.Worker?(bool?)true:null
                    }},
                    {Function.DeletePrivateMessage,new List<Func<Message,bool?>>{
                        m=>me.Role>=SiteRole.NormalUser?(bool?)true:null
                    }}
                };
            }
        }

        Dictionary<Function, List<Func<UploadedFile, bool?>>> FileRules
        {
            get
            {
                return new Dictionary<Function, List<Func<UploadedFile, bool?>>>
                {
                    {Function.ReadFile,new List<Func<UploadedFile,bool?>>{
                        f=>me.Role>=SiteRole.Reader?(bool?)true:null
                    }},
                    {Function.CreateFile,new List<Func<UploadedFile,bool?>>{
                        f=>me.Role>=SiteRole.NormalUser?(bool?)true:null
                    }},
                    {Function.ModifyFile,new List<Func<UploadedFile,bool?>>{
                        f=>me.Role<=SiteRole.Reader?(bool?)false:null,
                        f=>me.Role>=SiteRole.Worker?(bool?)true:null,
                        f=>me.ID==f.CreatedBy.ID?(bool?)true:null
                    }},
                    {Function.DeleteFile,new List<Func<UploadedFile,bool?>>{
                        f=>me.Role<=SiteRole.Reader?(bool?)false:null,
                        f=>me.Role>=SiteRole.Worker?(bool?)true:null,
                        f=>me.ID==f.CreatedBy.ID?(bool?)true:null
                    }}
                };
            }
        }

        Dictionary<Function, List<Func<Tag, bool?>>> TagRules
        {
            get
            {
                return new Dictionary<Function, List<Func<Tag, bool?>>>
                {
                    {Function.CreateTag,new List<Func<Tag,bool?>>{
                        t=>me.Role>=SiteRole.Worker?(bool?)true:null
                    }},
                    {Function.ModifyTag,new List<Func<Tag,bool?>>{
                        t=>me.Role>=SiteRole.Worker?(bool?)true:null
                    }},
                    {Function.DeleteTag,new List<Func<Tag,bool?>>{
                        t=>me.Role>=SiteRole.Worker?(bool?)true:null
                    }}
                };
            }
        }

        Dictionary<Function, List<Func<bool?>>> OtherRules
        {
            get
            {
                return new Dictionary<Function, List<Func<bool?>>>
                {
                    {Function.GarbageCollect,new List<Func<bool?>>{
                        ()=>me.Role>=SiteRole.Worker?(bool?)true:null
                    }},
                };
            }
        }

        public static bool Check(MooDB dbContext, object @object, Function function)
        {
            return new Access()
            {
                me = Security.CurrentUser,
                db = dbContext
            }.Check(@object, function);
        }

        public static void Required(MooDB dbContext, object @object, Function function)
        {
            if (!Check(dbContext, @object, function))
            {
                throw new UnauthorizedAccessException(function.ToString());
            }
        }

        public bool Check(object @object, Function function)
        {
            if (@object is Problem)
            {
                return CheckRules(@object as Problem, ProblemRules, function);
            }
            else if (@object is ProblemRevision)
            {
                return CheckRules(@object as ProblemRevision, ProblemRevisionRules, function);
            }
            else if (@object is User)
            {
                return CheckRules(@object as User, UserRules, function);
            }
            else if (@object is Record)
            {
                return CheckRules(@object as Record, RecordRules, function);
            }
            else if (@object is JudgeInfo)
            {
                return CheckRules(@object as JudgeInfo, JudgeInfoRules, function);
            }
            else if (@object is TestCase)
            {
                return CheckRules(@object as TestCase, TestCaseRules, function);
            }
            else if (@object is Post)
            {
                return CheckRules(@object as Post, PostRules, function);
            }
            else if (@object is PostItem)
            {
                return CheckRules(@object as PostItem, PostItemRules, function);
            }
            else if (@object is Article)
            {
                return CheckRules(@object as Article, ArticleRules, function);
            }
            else if (@object is ArticleRevision)
            {
                return CheckRules(@object as ArticleRevision, ArticleRevisionRules, function);
            }
            else if (@object is Contest)
            {
                return CheckRules(@object as Contest, ContestRules, function);
            }
            else if (@object is Message)
            {
                return CheckRules(@object as Message, MessageRules, function);
            }
            else if (@object is UploadedFile)
            {
                return CheckRules(@object as UploadedFile, FileRules, function);
            }
            else if (@object is Tag)
            {
                return CheckRules(@object as Tag, TagRules, function);
            }
            else if (@object == null)
            {
                if (!OtherRules.ContainsKey(function))
                {
                    throw new NotImplementedException("糟糕！未知的权限项！");
                }
                foreach (var func in OtherRules[function])
                {
                    bool? result = func();
                    if (result != null) return (bool)result;
                }
                return false;
            }
            else
                throw new NotImplementedException("糟糕！权限模块不完整！");
        }

        bool CheckRules<T>(T @object, Dictionary<Function, List<Func<T, bool?>>> rules, Function function)
        {
            if (!rules.ContainsKey(function)) throw new NotImplementedException("糟糕！未知的权限项！");
            foreach (var func in rules[function])
            {
                bool? result = func(@object);
                if (result != null) return (bool)result;
            }
            return false;
        }
    }
}
