using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Moo.Core.Utility;
using Moo.Core.Security;
namespace Moo.Core.DB
{
    public class MooTestData
    {
        public static void AddTestData()
        {
            using (MooDB db = new MooDB())
            {
                AddTestData(db);
            }
        }
        public static void AddTestData(MooDB db)
        {
            SiteRoles siteRoles = new SiteRoles(db);

            //Users
            User MrPhone = new User()
            {
                Name = "onetwogoo",
                Password = Converter.ToSHA256Hash("123456"),
                BriefDescription = "我觉得我写这么多应该够两行了",
                Description = "我是--屌丝--我骄傲，我为国家省钞票!",
                Email = "onetwogoo@live.com",
                Role = siteRoles.Organizer,
                Score = 256,
                PreferredLanguage = "c++"
            };
            db.Users.AddObject(MrPhone);

            User ShaBi = new User()
            {
                Name = "ShaBi",
                Password = Converter.ToSHA256Hash("ShaBi"),
                BriefDescription = "我觉得我写这么多应该够两行了",
                Description = "Moo*真*他妈的好！",
                Email = "sunjiayu_2006@126.com",
                Role = siteRoles.Worker,
                Score = 128,
                PreferredLanguage = "c++"
            };
            db.Users.AddObject(ShaBi);

            User Baby = new User()
            {
                Name = "Baby",
                Password = Converter.ToSHA256Hash("Baby"),
                BriefDescription = "我啥都不懂",
                Description = "真不懂",
                Email = "helloyuhao@gmail.com",
                Role = siteRoles.NormalUser,
                Score = 1000,
                PreferredLanguage = "pascal"
            };
            db.Users.AddObject(Baby);

            db.Users.AddObject(new User()
            {
                Name = "BeiJu",
                Password = Converter.ToSHA256Hash("BeiJu"),
                BriefDescription = "冤枉啊!啥都没干就被封了！",
                Description = "太他妈冤枉了！",
                Email = "",
                Role = siteRoles.Reader,
                Score = 0,
                PreferredLanguage = "c"
            });

            //Problems
            Problem APlusB = new Problem()
            {
                Name = "A+B问题",
                Type = "Tranditional",
                SubmissionCount = 10,
                ScoreSum = 100,
                SubmissionUser = 1,
                MaximumScore = 30,
                CreateTime = DateTime.Now,
                CreatedBy = MrPhone
            };

            Problem CPlusD = new Problem()
            {
                Name = "C+D问题",
                Type = "Tranditional",
                SubmissionCount = 20,
                ScoreSum = 5,
                SubmissionUser = 2,
                MaximumScore = 120,
                CreateTime = DateTime.Now.AddSeconds(1),
                CreatedBy = ShaBi
            };
            db.Problems.AddObject(CPlusD);

            Problem EPlusF = new Problem()
            {
                Name = "E+F问题",
                Type = "Tranditional",
                SubmissionCount = 40,
                ScoreSum = 300,
                SubmissionUser = 4,
                MaximumScore = 110,
                CreateTime = DateTime.Now.AddSeconds(2),
                CreatedBy = Baby
            };
            db.Problems.AddObject(EPlusF);

            Problem Cat = new Problem()
            {
                Name = "Cat",
                Type = "SpecialJudged",
                CreateTime = DateTime.Now.AddSeconds(3),
                CreatedBy = MrPhone
            };
            db.Problems.AddObject(Cat);

            Problem EasyAPlusB = new Problem()
            {
                Name = "Easy A+B",
                Type = "Interactive",
                CreateTime = DateTime.Now.AddSeconds(4),
                CreatedBy = MrPhone
            };
            db.Problems.AddObject(EasyAPlusB);

            Problem AnswerAPlusB = new Problem()
            {
                Name = "Answer A+B",
                Type = "AnswerOnly",
                CreateTime = DateTime.Now.AddSeconds(5),
                CreatedBy = MrPhone
            };

            //File
            UploadedFile file = new UploadedFile()
            {
                Name = "SPJ for Cat",
                Description = "给Cat的SPJ",
                Path = "D:\\Cat.exe",
                CreatedBy = MrPhone
            };
            db.UploadedFiles.AddObject(file);

            //Test Cases
            db.TestCases.AddObject(new TranditionalTestCase()
            {
                Problem = CPlusD,
                Input = Encoding.ASCII.GetBytes("qwertyuioplkjhgfdsazxcvbnm"),
                Answer = Encoding.ASCII.GetBytes("mnbvcxzasdfghjklpoiuytrewq"),
                TimeLimit = 1000,
                MemoryLimit = 1024 * 1024 * 6,
                Score = 12,
                CreatedBy = MrPhone
            });
            db.SaveChanges();

            db.TestCases.AddObject(new TranditionalTestCase()
            {
                Problem = APlusB,
                Input = Encoding.ASCII.GetBytes("1 2"),
                Answer = Encoding.ASCII.GetBytes("3"),
                TimeLimit = 1000,
                MemoryLimit = 60 * 1024 * 1024,
                Score = 50,
                CreatedBy = MrPhone
            });

            db.TestCases.AddObject(new TranditionalTestCase()
            {
                Problem = APlusB,
                Input = Encoding.ASCII.GetBytes("100 345"),
                Answer = Encoding.ASCII.GetBytes("445"),
                TimeLimit = 1000,
                MemoryLimit = 60 * 1024 * 1024,
                Score = 50,
                CreatedBy = MrPhone
            });

            db.TestCases.AddObject(new SpecialJudgedTestCase()
            {
                Problem = Cat,
                Input = Encoding.ASCII.GetBytes("1 2"),
                Answer = Encoding.ASCII.GetBytes("Miao~"),
                TimeLimit = 1000,
                MemoryLimit = 60 * 1024 * 1024,
                Judger = file,
                CreatedBy = MrPhone
            });

            file = new UploadedFile()
            {
                Name = "Invoker for EasyA+B",
                Description = "EasyA+B的调用程序",
                Path = "D:\\EasyA+B.o",
                CreatedBy = MrPhone
            };

            db.TestCases.AddObject(new InteractiveTestCase()
            {
                Problem = EasyAPlusB,
                TestData = Encoding.ASCII.GetBytes("1123 3212"),
                TimeLimit = 1000,
                MemoryLimit = 60 * 1024 * 1024,
                Invoker = file,
                CreatedBy = MrPhone
            });

            db.TestCases.AddObject(new InteractiveTestCase()
            {
                Problem = EasyAPlusB,
                TestData = Encoding.ASCII.GetBytes("1 3"),
                TimeLimit = 1000,
                MemoryLimit = 60 * 1024 * 1024,
                Invoker = file,
                CreatedBy = MrPhone
            });

            file = new UploadedFile()
            {
                Name = "Judger Of Answer A+B",
                Description = "*测评程序*啊",
                Path = "D:\\AnswerA+B.exe",
                CreatedBy = MrPhone
            };

            AnswerOnlyTestCase answerOnlyTestCase1 = new AnswerOnlyTestCase()
            {
                Problem = AnswerAPlusB,
                TestData = Encoding.ASCII.GetBytes("23 345"),
                Judger = file,
                CreatedBy = MrPhone
            };
            db.TestCases.AddObject(answerOnlyTestCase1);

            AnswerOnlyTestCase answerOnlyTestCase2 = new AnswerOnlyTestCase()
            {
                Problem = AnswerAPlusB,
                TestData = Encoding.ASCII.GetBytes("453 123"),
                Judger = file,
                CreatedBy = MrPhone
            };
            db.TestCases.AddObject(answerOnlyTestCase2);

            //Problem Revision
            db.ProblemRevisions.AddObject(new ProblemRevision()
             {
                 Problem = APlusB,
                 Content = "输入A,B。输出A+B。啊！输错了！",
                 Reason = "蛋疼",
                 CreatedBy = ShaBi,
                 CreateTime = DateTime.Now
             });

            db.ProblemRevisions.AddObject(new ProblemRevision()
            {
                Problem = APlusB,
                Content = "输入A,B。输出它们的和。",
                Reason = "蛋疼",
                CreatedBy = MrPhone,
                CreateTime = DateTime.Now
            });

            db.ProblemRevisions.AddObject(new ProblemRevision()
            {
                Problem = APlusB,
                Content = "输入俩蛋，输出它们的和。",
                Reason = "蛋疼",
                CreatedBy = ShaBi,
                CreateTime = DateTime.Now
            });

            APlusB.LatestRevision = new ProblemRevision()
            {
                Problem = APlusB,
                Content = "输入两个Int32，输出它们的和。",
                Reason = "蛋疼",
                CreatedBy = MrPhone,
                CreateTime = DateTime.Now
            };

            CPlusD.LatestRevision = new ProblemRevision()
            {
                Problem = CPlusD,
                Content = "输入C,D。*注意是Int64*输出它们的和。",
                Reason = "蛋疼",
                CreatedBy = ShaBi,
                CreateTime = DateTime.Now
            };

            EPlusF.LatestRevision = new ProblemRevision()
            {
                Problem = EPlusF,
                Content = "输入E,F。输出它们的和。",
                Reason = "蛋疼",
                CreatedBy = ShaBi,
                CreateTime = DateTime.Now
            };

            Cat.LatestRevision = new ProblemRevision()
            {
                Problem = Cat,
                Content = "模拟Cat",
                Reason = "擦！",
                CreatedBy = MrPhone,
                CreateTime = DateTime.Now
            };

            EasyAPlusB.LatestRevision = new ProblemRevision()
            {
                Problem = EasyAPlusB,
                Content = "仅需编写一个int APlusB(int,int);即可。",
                Reason = "This is Interactive",
                CreatedBy = ShaBi,
                CreateTime = DateTime.Now
            };

            AnswerAPlusB.LatestRevision = new ProblemRevision()
            {
                Problem = AnswerAPlusB,
                Content = "提交答案吧！",
                Reason = "None",
                CreatedBy = MrPhone,
                CreateTime = DateTime.Now
            };

            //Post
            Post post = new Post()
            {
                Name = "讨论一下出题人心理",
                Problem = APlusB,
                ReplyTime = DateTime.Now,
                OnTop = true,
            };
            db.Posts.AddObject(post);

            db.PostItems.AddObject(new PostItem()
            {
                Post = post,
                CreateTime = DateTime.Now,
                Content = "出--这么水--的题，找死啊",
                CreatedBy = ShaBi,
            });
            db.SaveChanges();

            db.PostItems.AddObject(new PostItem()
            {
                Post = post,
                CreateTime = DateTime.Now,
                Content = "靠！作为出题人我压力很大啊!",
                CreatedBy = MrPhone
            });

            post = new Post()
            {
                Name = "认真研究一下此题变形",
                Problem = APlusB,
                ReplyTime = DateTime.Now,
                OnTop = false
            };
            db.Posts.AddObject(post);

            db.PostItems.AddObject(new PostItem()
            {
                Post = post,
                CreateTime = DateTime.Now,
                Content = "A+B能有什么变形呢？",
                CreatedBy = MrPhone
            });
            db.SaveChanges();

            db.PostItems.AddObject(new PostItem()
            {
                Post = post,
                CreateTime = DateTime.Now,
                Content = "靠！没人回答，我寂寞了~",
                CreatedBy = MrPhone
            });

            post = new Post()
            {
                Name = "讨论一下Moo好不好",
                ReplyTime = DateTime.Now,
                OnTop = false
            };
            db.Posts.AddObject(post);

            db.PostItems.AddObject(new PostItem()
            {
                Post = post,
                CreateTime = DateTime.Now,
                Content = "Moo很好啊。*注意此贴没有对应题目且被锁*",
                CreatedBy = MrPhone
            });

            //Mail
            db.Mails.AddObject(new Mail()
            {
                Name = "咱把Moo黑了如何？",
                Content = "嘿！onetwogoo!把Moo--黑--了吧！",
                CreateTime = DateTime.Now,
                IsRead = true,
                From = ShaBi,
                To = MrPhone
            });
            db.SaveChanges();

            db.Mails.AddObject(new Mail()
            {
                Name = "找死啊！",
                Content = "我会去黑自己网站吗？",
                CreateTime = DateTime.Now,
                IsRead = true,
                From = MrPhone,
                To = ShaBi
            });
            db.SaveChanges();

            db.Mails.AddObject(new Mail()
            {
                Name = "没准嘞！",
                Content = "敢说你没这想法",
                CreateTime = DateTime.Now,
                IsRead = true,
                From = ShaBi,
                To = MrPhone
            });
            db.SaveChanges();

            db.Mails.AddObject(new Mail()
            {
                Name = "傻逼",
                Content = "不解释！",
                CreateTime = DateTime.Now,
                IsRead = false,
                From = MrPhone,
                To = ShaBi
            });
            db.SaveChanges();

            db.Mails.AddObject(new Mail()
            {
                Name = "把我弄成组织者如何啊",
                Content = "???",
                CreateTime = DateTime.Now,
                IsRead = false,
                From = Baby,
                To = MrPhone
            });
            db.SaveChanges();

            db.Mails.AddObject(new Mail()
            {
                Name = "美死你",
                Content = "怎么可能",
                CreateTime = DateTime.Now,
                IsRead = true,
                From = MrPhone,
                To = Baby
            });
            db.SaveChanges();

            //Contest
            Contest contest = new Contest()
            {
                StartTime = DateTime.Now.AddMinutes(1),
                EndTime = DateTime.Now.AddMinutes(2),
                Status = "Before",
                Name = "Moo水题大赛",
                Description = "全是--水--题啊！",
                LockProblemOnStart = true,
                LockPostOnStart = true,
                LockTestCaseOnStart = true,
                EnableTestingOnStart = false,
                HideTestCaseOnStart = true,
                LockRecordOnStart = false,
                HideProblemOnStart = false,
                EnableTestingOnEnd = true,
                LockPostOnEnd = false,
                LockProblemOnEnd = false,
                LockRecordOnEnd = false,
                LockTestCaseOnEnd = false,
                HideProblemOnEnd = false,
                HideTestCaseOnEnd = false,
                LockArticleOnEnd = false,
                LockArticleOnStart = true,
            };
            //contest.Problem.Add(APlusB);
            contest.Problem.Add(CPlusD);
            contest.User.Add(ShaBi);

            //Record
            JudgeInfo info = new JudgeInfo()
            {
                Info = "Test",
                Score = 10
            };

            Record record = new Record()
            {
                Code = "",
                Language = "c++",
                CreateTime = DateTime.Now.AddMinutes(1.5),
                User = ShaBi,
                Problem = APlusB,
            };
            record.JudgeInfo = info;
            info.Record = record;
            db.Records.AddObject(record);

            info = new JudgeInfo()
            {
                Info = "test",
                Score = 30
            };

            record = new Record()
            {
                Code = "",
                Language = "c++",
                CreateTime = DateTime.Now.AddMinutes(1.5),
                User = ShaBi,
                Problem = APlusB,
            };
            record.JudgeInfo = info;
            info.Record = record;
            db.Records.AddObject(record);

            info = new JudgeInfo()
            {
                Info = "<color:green>All Cases Passed</color> <color:red>But I'm not happy, so you got 60.</color>",
                Score = 60
            };

            record = new Record()
            {
                Code = "#include <iostream>\n"
                    + "int main(int argc,char* argv[]) {\n"
                    + "\tint c,d;\n"
                    + "\tstd::cin>>c>>d;\n"
                    + "\tstd::cout<<c+d<<std::endl;\n"
                    + "}",
                Language = "c++",
                CreateTime = DateTime.Now.AddMinutes(1.5),
                User = ShaBi,
                Problem = CPlusD,
            };
            record.JudgeInfo = info;
            info.Record = record;
            db.Records.AddObject(record);

            db.Records.AddObject(new Record()
            {
                Code = "int APlusB(int x,int y){\n\treturn 4;\n}",
                CreateTime = DateTime.Now,
                Language = "c++",
                Problem = EasyAPlusB,
                User = MrPhone,
            });

            db.Records.AddObject(new Record()
            {
                Code = "<Moo:Answer testCase='" + answerOnlyTestCase1.ID + "'>368</Moo:Answer>\n"
                + "<Moo:Answer testCase='" + answerOnlyTestCase2.ID + "'>496</Moo:Answer>",
                CreateTime = DateTime.Now,
                Language = "plaintext",
                Problem = AnswerAPlusB,
                User = MrPhone,
            });

            db.SaveChanges();
        }
    }
}