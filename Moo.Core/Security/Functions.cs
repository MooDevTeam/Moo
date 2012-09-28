using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Moo.Core.Security
{
    public enum Function
    {
        //Homepages
        CreateHomepage,
        ReadHomepage,
        DeleteHomepage,

        //Problems
        CreateProblem,
        ReadProblem,
        ModifyProblem,
        DeleteProblem,

        //ProblemRevisions
        CreateProblemRevision,
        ReadProblemRevision,
        DeleteProblemRevision,

        //TestCases
        ReadTestCase,
        CreateTestCase,
        ModifyTestCase,
        DeleteTestCase,

        //Posts
        CreatePost,
        ReadPost,
        ModifyPost,
        DeletePost,

        //PostItems
        CreatePostItem,
        ReadPostItem,
        ModifyPostItem,
        DeletePostItem,

        //Records
        CreateRecord,
        ReadRecord,
        ReadRecordCode,
        ModifyRecord,
        DeleteRecord,

        //JudgeInfos
        CreateJudgeInfo,
        ReadJudgeInfo,
        DeleteJudgeInfo,

        //Users
        CreateUser,
        ReadUser,
        ModifyUser,
        ModifyUserRole,

        //Mails
        CreateMail,
        ReadMail,
        DeleteMail,

        //Contests
        ReadContest,
        CreateContest,
        ModifyContest,
        AttendContest,
        DeleteContest,

        //Files
        CreateFile,
        ReadFile,
        ModifyFile,
        DeleteFile,
    }
}
