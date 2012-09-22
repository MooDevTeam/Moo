using System;
using System.Collections.Generic;
using System.Linq;
namespace Moo.Core.DB
{
    /// <summary>
    /// 安装数据库
    /// </summary>
    public static class DatabaseInstaller
    {
        static Role RoleManager = new Role() { Name = "RoleManager", DisplayName = "角色管理者" };
        static Role ACLManager = new Role() { Name = "ACLManager", DisplayName = "ACL管理者" };
        static Role SiteManager = new Role() { Name = "SiteManager", DisplayName = "网站管理者" };
        static Role Contributor = new Role() { Name = "Contributor", DisplayName = "贡献者" };
        static Role Reader = new Role() { Name = "Reader", DisplayName = "浏览者" };
        static Role Tester = new Role() { Name = "Tester", DisplayName = "测评机" };

        static Function CreateHomepage = new Function() { Name = "homepage.create", DisplayName = "创建首页" };
        static Function ReadHomepage = new Function() { Name = "homepage.read", DisplayName = "读取首页" };
        static Function DeleteHomepage = new Function() { Name = "homepage.delete", DisplayName = "删除首页" };

        static Function CreateProblem = new Function() { Name = "problem.create", DisplayName = "创建题目" };
        static Function ReadProblem = new Function() { Name = "problem.read", DisplayName = "读取题目" };
        static Function ModifyProblem = new Function() { Name = "problem.modify", DisplayName = "修改题目" };
        static Function DeleteProblem = new Function() { Name = "problem.delete", DisplayName = "删除题目" };

        static Function CreateProblemRevision = new Function() { Name = "problem.revision.create", DisplayName = "创建题目版本" };
        static Function ReadProblemRevision = new Function() { Name = "problem.revision.read", DisplayName = "读取题目版本" };
        static Function DeleteProblemRevision = new Function() { Name = "problem.revision.delete", DisplayName = "删除题目版本" };

        static Function ReadTestCase = new Function() { Name = "testcase.read", DisplayName = "读取测试数据" };
        static Function CreateTestCase = new Function() { Name = "testcase.create", DisplayName = "创建测试数据" };
        static Function ModifyTestCase = new Function() { Name = "testcase.modify", DisplayName = "修改测试数据" };
        static Function DeleteTestCase = new Function() { Name = "testcase.delete", DisplayName = "删除测试数据" };

        static Function ReadSolution = new Function() { Name = "solution.read", DisplayName = "读取题解" };
        static Function UpdateSolution = new Function() { Name = "solution.update", DisplayName = "更新题解" };
        static Function DeleteSolution = new Function() { Name = "solution.delete", DisplayName = "删除题解" };

        static Function CreatePost = new Function() { Name = "post.create", DisplayName = "创建帖子" };
        static Function ReadPost = new Function() { Name = "post.read", DisplayName = "读取帖子" };
        static Function ModifyPost = new Function() { Name = "post.modify", DisplayName = "修改帖子" };
        static Function DeletePost = new Function() { Name = "post.delete", DisplayName = "删除帖子" };

        static Function CreatePostItem = new Function() { Name = "post.item.create", DisplayName = "创建帖子楼层" };
        static Function ReadPostItem = new Function() { Name = "post.item.read", DisplayName = "读取帖子楼层" };
        static Function ModifyPostItem = new Function() { Name = "post.item.modify", DisplayName = "修改帖子楼层" };
        static Function DeletePostItem = new Function() { Name = "post.item.delete", DisplayName = "删除帖子楼层" };

        static Function CreateRecord = new Function() { Name = "record.create", DisplayName = "创建记录" };
        static Function ReadRecord = new Function() { Name = "record.read", DisplayName = "读取记录" };
        static Function ReadRecordCode = new Function() { Name = "record.code.read", DisplayName = "读取记录代码" };
        static Function ModifyRecord = new Function() { Name = "record.modify", DisplayName = "修改记录" };
        static Function DeleteRecord = new Function() { Name = "record.delete", DisplayName = "删除记录" };

        static Function CreateRecordJudgeInfo = new Function() { Name = "record.judgeinfo.create", DisplayName = "创建测评信息" };
        static Function ReadRecordJudgeInfo = new Function() { Name = "record.judgeinfo.read", DisplayName = "读取测评信息" };
        static Function DeleteRecordJudgeInfo = new Function() { Name = "record.judgeinfo.delete", DisplayName = "删除测评信息" };

        static Function CreateUser = new Function() { Name = "user.create", DisplayName = "创建用户" };
        static Function ReadUser = new Function() { Name = "user.read", DisplayName = "读取用户" };
        static Function ModifyUser = new Function() { Name = "user.modify", DisplayName = "修改用户" };
        static Function ModifyUserRole = new Function() { Name = "user.role.modify", DisplayName = "修改用户角色" };

        static Function CreateMail = new Function() { Name = "mail.create", DisplayName = "创建邮件" };
        static Function ReadMail = new Function() { Name = "mail.read", DisplayName = "读取邮件" };
        static Function DeleteMail = new Function() { Name = "mail.delete", DisplayName = "删除邮件" };

        static Function ReadContest = new Function() { Name = "contest.read", DisplayName = "读取比赛" };
        static Function CreateContest = new Function() { Name = "contest.create", DisplayName = "创建比赛" };
        static Function ModifyContest = new Function() { Name = "contest.modify", DisplayName = "修改比赛" };
        static Function AttendContest = new Function() { Name = "contest.attend", DisplayName = "参加比赛" };
        static Function DeleteContest = new Function() { Name = "contest.delete", DisplayName = "删除比赛" };

        static Function CreateFile = new Function() { Name = "file.create", DisplayName = "创建文件" };
        static Function ReadFile = new Function() { Name = "file.read", DisplayName = "读取文件" };
        static Function ModifyFile = new Function() { Name = "file.modify", DisplayName = "修改文件" };
        static Function DeleteFile = new Function() { Name = "file.delete", DisplayName = "删除文件" };

        static Function ReadACL = new Function() { Name = "acl.read", DisplayName = "读取访问控制表" };
        static Function CreateACE = new Function() { Name = "ace.create", DisplayName = "创建访问控制项" };
        static Function ModifyACE = new Function() { Name = "ace.modify", DisplayName = "修改访问控制项" };
        static Function DeleteACE = new Function() { Name = "ace.delete", DisplayName = "删除访问控制项" };

        static User owner;

        public static void Install()
        {
            using (MooDB db = new MooDB())
            {
                Install(db);
            }
        }

        public static void Install(MooDB db)
        {
            db.CreateDatabase();
            FixDatabase(db);
            AddRequiredData(db);
        }

        static void FixDatabase(MooDB db)
        {
            db.ExecuteStoreCommand("CREATE UNIQUE INDEX IX_Users_Name ON [dbo].[Users] ([Name])");

            db.ExecuteStoreCommand("CREATE INDEX IX_ACL_Object ON [dbo].[ACL] ([Object])");
            db.ExecuteStoreCommand("CREATE INDEX IX_ACL_Subject ON [dbo].[ACL] ([Subject])");

            db.ExecuteStoreCommand("CREATE UNIQUE INDEX IX_Functions_Name ON [dbo].[Functions] ([Name])");

            db.ExecuteStoreCommand("ALTER TABLE [dbo].[TestCases_TranditionalTestCase] DROP CONSTRAINT [FK_TranditionalTestCase_inherits_TestCase];");
            db.ExecuteStoreCommand("ALTER TABLE [dbo].[TestCases_TranditionalTestCase] ADD CONSTRAINT [FK_TranditionalTestCase_inherits_TestCase] FOREIGN KEY ([ID]) REFERENCES [dbo].[TestCases]([ID]) ON DELETE CASCADE;");

            db.ExecuteStoreCommand("ALTER TABLE [dbo].[TestCases_SpecialJudgedTestCase] DROP CONSTRAINT [FK_SpecialJudgedTestCase_inherits_TestCase];");
            db.ExecuteStoreCommand("ALTER TABLE [dbo].[TestCases_SpecialJudgedTestCase] ADD CONSTRAINT [FK_SpecialJudgedTestCase_inherits_TestCase] FOREIGN KEY ([ID]) REFERENCES [dbo].[TestCases]([ID]) ON DELETE CASCADE;");

            db.ExecuteStoreCommand("ALTER TABLE [dbo].[TestCases_InteractiveTestCase] DROP CONSTRAINT [FK_InteractiveTestCase_inherits_TestCase];");
            db.ExecuteStoreCommand("ALTER TABLE [dbo].[TestCases_InteractiveTestCase] ADD CONSTRAINT [FK_InteractiveTestCase_inherits_TestCase] FOREIGN KEY ([ID]) REFERENCES [dbo].[TestCases]([ID]) ON DELETE CASCADE;");

            db.ExecuteStoreCommand("ALTER TABLE [dbo].[TestCases_AnswerOnlyTestCase] DROP CONSTRAINT [FK_AnswerOnlyTestCase_inherits_TestCase];");
            db.ExecuteStoreCommand("ALTER TABLE [dbo].[TestCases_AnswerOnlyTestCase] ADD CONSTRAINT [FK_AnswerOnlyTestCase_inherits_TestCase] FOREIGN KEY ([ID]) REFERENCES [dbo].[TestCases]([ID]) ON DELETE CASCADE;");
        }

        static void AddRequiredData(MooDB db)
        {
            AddRoles(db);
            AddOwner(db);
            AddHomepage(db);
        }

        static void AddRoles(MooDB db)
        {
            db.Roles.AddObject(RoleManager);
            db.Roles.AddObject(ACLManager);
            db.Roles.AddObject(SiteManager);
            db.Roles.AddObject(Contributor);
            db.Roles.AddObject(Reader);
            db.SaveChanges();

            Guid roleManager = RoleManager.ID, aclManager = ACLManager.ID, siteManager = SiteManager.ID, contributor = Contributor.ID, reader = Reader.ID;
            new List<ACE>
            {
                //RoleManager
                new ACE(){Subject=roleManager,Function=ModifyUserRole,Allowed=true},

                //aclManager
                new ACE(){Subject=aclManager,Function=CreateACE,Allowed=true},
                new ACE(){Subject=aclManager,Function=DeleteACE,Allowed=true},

                //siteManager
                new ACE(){Subject=siteManager,Function=CreateHomepage,Allowed=true},
                new ACE(){Subject=siteManager,Function=DeleteHomepage,Allowed=true},
                new ACE(){Subject=siteManager,Function=ModifyProblem,Allowed=true},
                new ACE(){Subject=siteManager,Function=DeleteProblem,Allowed=true},
                new ACE(){Subject=siteManager,Function=DeleteProblemRevision,Allowed=true},
                new ACE(){Subject=siteManager,Function=ModifyTestCase,Allowed=true},
                new ACE(){Subject=siteManager,Function=DeleteTestCase,Allowed=true},
                new ACE(){Subject=siteManager,Function=DeleteSolution,Allowed=true},
                new ACE(){Subject=siteManager,Function=ModifyPost,Allowed=true},
                new ACE(){Subject=siteManager,Function=DeletePost,Allowed=true},
                new ACE(){Subject=siteManager,Function=DeletePostItem,Allowed=true},
                new ACE(){Subject=siteManager,Function=ModifyPostItem,Allowed=true},
                new ACE(){Subject=siteManager,Function=ReadRecordCode,Allowed=true},
                new ACE(){Subject=siteManager,Function=DeleteRecord,Allowed=true},
                new ACE(){Subject=siteManager,Function=ModifyUser,Allowed=true},
                new ACE(){Subject=siteManager,Function=CreateContest,Allowed=true},
                new ACE(){Subject=siteManager,Function=ModifyContest,Allowed=true},
                new ACE(){Subject=siteManager,Function=DeleteProblem,Allowed=true},
                new ACE(){Subject=siteManager,Function=ModifyFile,Allowed=true},
                new ACE(){Subject=siteManager,Function=DeleteFile,Allowed=true},

                //Contributor
                new ACE(){Subject=contributor,Function=CreateProblemRevision,Allowed=true},
                new ACE(){Subject=contributor,Function=CreateProblem,Allowed=true},
                new ACE(){Subject=contributor,Function=CreateTestCase,Allowed=true},
                new ACE(){Subject=contributor,Function=UpdateSolution,Allowed=true},
                new ACE(){Subject=contributor,Function=CreatePostItem,Allowed=true},
                new ACE(){Subject=contributor,Function=CreateRecord,Allowed=true},
                new ACE(){Subject=contributor,Function=DeleteRecordJudgeInfo,Allowed=true},
                new ACE(){Subject=contributor,Function=CreateMail,Allowed=true},
                new ACE(){Subject=contributor,Function=AttendContest,Allowed=true},
                new ACE(){Subject=contributor,Function=CreateFile,Allowed=true},

                //Reader
                new ACE(){Subject=reader,Function=ReadHomepage,Allowed=true},
                new ACE(){Subject=reader,Function=ReadProblem,Allowed=true},
                new ACE(){Subject=reader,Function=ReadProblemRevision,Allowed=true},
                new ACE(){Subject=reader,Function=ReadTestCase,Allowed=true},
                new ACE(){Subject=reader,Function=ReadSolution,Allowed=true},
                new ACE(){Subject=reader,Function=ReadPost,Allowed=true},
                new ACE(){Subject=reader,Function=ReadPostItem,Allowed=true},
                new ACE(){Subject=reader,Function=ReadRecord,Allowed=true},
                new ACE(){Subject=reader,Function=ReadRecordJudgeInfo,Allowed=true},
                new ACE(){Subject=reader,Function=ReadUser,Allowed=true},
                new ACE(){Subject=reader,Function=ReadContest,Allowed=true},
                new ACE(){Subject=reader,Function=ReadFile,Allowed=true},
                new ACE(){Subject=reader,Function=ReadACL,Allowed=true},
            }.ForEach(ace => db.ACL.AddObject(ace));
            db.SaveChanges();
        }

        static void AddOwner(MooDB db)
        {
            owner = new User()
            {
                Name = "MooOwner",
                Password = Moo.Core.Utility.Converter.ToSHA256Hash("s3cret"),
                BriefDescription = "这个账户为Moo的拥有者准备",
                Description = "",
                Email = "",
                Score = 0,
                PreferredLanguage = "c++",
            };
            owner.Role.Add(ACLManager);
            owner.Role.Add(RoleManager);
            owner.Role.Add(SiteManager);
            owner.Role.Add(Contributor);
            owner.Role.Add(Reader);
            db.Users.AddObject(owner);
            db.SaveChanges();
            db.ACL.AddObject(new ACE() { Subject = owner.ID, Object = owner.ID, Function = ModifyUser });
            db.SaveChanges();
        }

        static void AddHomepage(MooDB db)
        {
            db.HomepageRevisions.AddObject(new HomepageRevision()
            {
                Title = "Moo的主页",
                Content = "请点击上方的更新，以便使这里显示您需要的内容。",
                CreatedBy = owner,
                Reason = "安装Moo"
            });
            db.SaveChanges();
        }
    }
}