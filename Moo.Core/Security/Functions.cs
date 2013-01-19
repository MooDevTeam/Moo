using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Moo.Core.Security
{
    public enum Function
    {
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
        ReadJudgeInfo,
        DeleteJudgeInfo,

        //Users
        CreateUser,
        ReadUser,
        ModifyUser,
        ModifyUserRole,

        //Contests
        ReadContest,
        ReadContestResult,
        CreateContest,
        ModifyContest,
        AttendContest,
        DeleteContest,

        //Files
        CreateFile,
        ReadFile,
        ModifyFile,
        DeleteFile,

        //Articles
        CreateArticle,
        ReadArticle,
        ModifyArticle,
        DeleteArticle,

        //ArticleRevisions
        CreateArticleRevision,
        ReadArticleRevision,
        DeleteArticleRevision,

        //Message
        CreateMessage,
        DeletePublicMessage,
        DeletePrivateMessage,

        //Tag
        CreateTag,
        ModifyTag,
        DeleteTag,

        //Management
        GarbageCollect,
        ReadIndexStatistics
    }
}
