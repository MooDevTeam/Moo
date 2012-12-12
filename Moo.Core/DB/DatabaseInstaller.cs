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
        static Role Organizer = new Role() { Name = "Organizer" };
        static Role Worker = new Role() { Name = "Worker" };
        static Role NormalUser = new Role() { Name = "NormalUser" };
        static Role Reader = new Role() { Name = "Reader" };

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

            db.ExecuteStoreCommand("ALTER TABLE [dbo].[TestCases_TraditionalTestCase] DROP CONSTRAINT [FK_TraditionalTestCase_inherits_TestCase];");
            db.ExecuteStoreCommand("ALTER TABLE [dbo].[TestCases_TraditionalTestCase] ADD CONSTRAINT [FK_TraditionalTestCase_inherits_TestCase] FOREIGN KEY ([ID]) REFERENCES [dbo].[TestCases]([ID]) ON DELETE CASCADE;");

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
        }

        static void AddRoles(MooDB db)
        {
            db.Roles.AddObject(Organizer);
            db.Roles.AddObject(Worker);
            db.Roles.AddObject(NormalUser);
            db.Roles.AddObject(Reader);
            db.SaveChanges();
        }
    }
}