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
        static Role Organizer = new Role() { Name = "Organizer", DisplayName = "组织者" };
        static Role Worker = new Role() { Name = "Worker", DisplayName = "工作者" };
        static Role NormalUser = new Role() { Name = "NormalUser", DisplayName = "普通用户" };
        static Role Reader = new Role() { Name = "Reader", DisplayName = "浏览者" };
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
        }

        static void AddRoles(MooDB db)
        {
            db.Roles.AddObject(Organizer);
            db.Roles.AddObject(Worker);
            db.Roles.AddObject(NormalUser);
            db.Roles.AddObject(Reader);
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
                Role=Organizer,
                PreferredLanguage = "c++",
            };
            db.Users.AddObject(owner);
            db.SaveChanges();
        }
    }
}