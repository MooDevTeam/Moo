
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 12/13/2012 22:06:36
-- Generated from EDMX file: D:\VSProject\Moo\Moo.Core\DB\MooDB.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [MooTesting];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_RecordProblem]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Records] DROP CONSTRAINT [FK_RecordProblem];
GO
IF OBJECT_ID(N'[dbo].[FK_TestDataProblem]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TestCases] DROP CONSTRAINT [FK_TestDataProblem];
GO
IF OBJECT_ID(N'[dbo].[FK_ProblemProblemRevision]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ProblemRevisions] DROP CONSTRAINT [FK_ProblemProblemRevision];
GO
IF OBJECT_ID(N'[dbo].[FK_UserProblemRevision]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ProblemRevisions] DROP CONSTRAINT [FK_UserProblemRevision];
GO
IF OBJECT_ID(N'[dbo].[FK_UserRole]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Users] DROP CONSTRAINT [FK_UserRole];
GO
IF OBJECT_ID(N'[dbo].[FK_UserCreatePostItem]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PostItems] DROP CONSTRAINT [FK_UserCreatePostItem];
GO
IF OBJECT_ID(N'[dbo].[FK_PostItemPost]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PostItems] DROP CONSTRAINT [FK_PostItemPost];
GO
IF OBJECT_ID(N'[dbo].[FK_PostProblem]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Posts] DROP CONSTRAINT [FK_PostProblem];
GO
IF OBJECT_ID(N'[dbo].[FK_RecordJudgeInfo]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[JudgeInfos] DROP CONSTRAINT [FK_RecordJudgeInfo];
GO
IF OBJECT_ID(N'[dbo].[FK_LastestRevisionOfProblem]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ProblemRevisions] DROP CONSTRAINT [FK_LastestRevisionOfProblem];
GO
IF OBJECT_ID(N'[dbo].[FK_UserAttendContest_User]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserAttendContest] DROP CONSTRAINT [FK_UserAttendContest_User];
GO
IF OBJECT_ID(N'[dbo].[FK_UserAttendContest_Contest]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserAttendContest] DROP CONSTRAINT [FK_UserAttendContest_Contest];
GO
IF OBJECT_ID(N'[dbo].[FK_UserRecord]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Records] DROP CONSTRAINT [FK_UserRecord];
GO
IF OBJECT_ID(N'[dbo].[FK_ContestProblem_Contest]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ContestProblem] DROP CONSTRAINT [FK_ContestProblem_Contest];
GO
IF OBJECT_ID(N'[dbo].[FK_ContestProblem_Problem]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ContestProblem] DROP CONSTRAINT [FK_ContestProblem_Problem];
GO
IF OBJECT_ID(N'[dbo].[FK_SpecialJudgedTestCaseUploadedFile]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TestCases_SpecialJudgedTestCase] DROP CONSTRAINT [FK_SpecialJudgedTestCaseUploadedFile];
GO
IF OBJECT_ID(N'[dbo].[FK_InteractiveTestCaseInvokerFile]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TestCases_InteractiveTestCase] DROP CONSTRAINT [FK_InteractiveTestCaseInvokerFile];
GO
IF OBJECT_ID(N'[dbo].[FK_AnswerOnlyTestCaseUploadedFile]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TestCases_AnswerOnlyTestCase] DROP CONSTRAINT [FK_AnswerOnlyTestCaseUploadedFile];
GO
IF OBJECT_ID(N'[dbo].[FK_UserTestCase]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TestCases] DROP CONSTRAINT [FK_UserTestCase];
GO
IF OBJECT_ID(N'[dbo].[FK_UploadedFileUser]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UploadedFiles] DROP CONSTRAINT [FK_UploadedFileUser];
GO
IF OBJECT_ID(N'[dbo].[FK_UserCreateProblem]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Problems] DROP CONSTRAINT [FK_UserCreateProblem];
GO
IF OBJECT_ID(N'[dbo].[FK_UserCreateArticle]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Articles] DROP CONSTRAINT [FK_UserCreateArticle];
GO
IF OBJECT_ID(N'[dbo].[FK_UserCreateArticleRevision]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ArticleRevisions] DROP CONSTRAINT [FK_UserCreateArticleRevision];
GO
IF OBJECT_ID(N'[dbo].[FK_ArticleArticleRevision]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ArticleRevisions] DROP CONSTRAINT [FK_ArticleArticleRevision];
GO
IF OBJECT_ID(N'[dbo].[FK_ArticleLatestRevision]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ArticleRevisions] DROP CONSTRAINT [FK_ArticleLatestRevision];
GO
IF OBJECT_ID(N'[dbo].[FK_ArticleProblem]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Articles] DROP CONSTRAINT [FK_ArticleProblem];
GO
IF OBJECT_ID(N'[dbo].[FK_ArticleTag_Article]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ArticleTag] DROP CONSTRAINT [FK_ArticleTag_Article];
GO
IF OBJECT_ID(N'[dbo].[FK_ArticleTag_Tag]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ArticleTag] DROP CONSTRAINT [FK_ArticleTag_Tag];
GO
IF OBJECT_ID(N'[dbo].[FK_ProblemTag_Problem]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ProblemTag] DROP CONSTRAINT [FK_ProblemTag_Problem];
GO
IF OBJECT_ID(N'[dbo].[FK_ProblemTag_Tag]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ProblemTag] DROP CONSTRAINT [FK_ProblemTag_Tag];
GO
IF OBJECT_ID(N'[dbo].[FK_MessageFrom]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Messages] DROP CONSTRAINT [FK_MessageFrom];
GO
IF OBJECT_ID(N'[dbo].[FK_MessageTo]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Messages] DROP CONSTRAINT [FK_MessageTo];
GO
IF OBJECT_ID(N'[dbo].[FK_SpecialJudgedTestCase_inherits_TestCase]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TestCases_SpecialJudgedTestCase] DROP CONSTRAINT [FK_SpecialJudgedTestCase_inherits_TestCase];
GO
IF OBJECT_ID(N'[dbo].[FK_InteractiveTestCase_inherits_TestCase]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TestCases_InteractiveTestCase] DROP CONSTRAINT [FK_InteractiveTestCase_inherits_TestCase];
GO
IF OBJECT_ID(N'[dbo].[FK_AnswerOnlyTestCase_inherits_TestCase]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TestCases_AnswerOnlyTestCase] DROP CONSTRAINT [FK_AnswerOnlyTestCase_inherits_TestCase];
GO
IF OBJECT_ID(N'[dbo].[FK_TraditionalTestCase_inherits_TestCase]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TestCases_TraditionalTestCase] DROP CONSTRAINT [FK_TraditionalTestCase_inherits_TestCase];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Users]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Users];
GO
IF OBJECT_ID(N'[dbo].[Roles]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Roles];
GO
IF OBJECT_ID(N'[dbo].[Problems]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Problems];
GO
IF OBJECT_ID(N'[dbo].[Records]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Records];
GO
IF OBJECT_ID(N'[dbo].[TestCases]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TestCases];
GO
IF OBJECT_ID(N'[dbo].[ProblemRevisions]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ProblemRevisions];
GO
IF OBJECT_ID(N'[dbo].[Posts]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Posts];
GO
IF OBJECT_ID(N'[dbo].[PostItems]', 'U') IS NOT NULL
    DROP TABLE [dbo].[PostItems];
GO
IF OBJECT_ID(N'[dbo].[JudgeInfos]', 'U') IS NOT NULL
    DROP TABLE [dbo].[JudgeInfos];
GO
IF OBJECT_ID(N'[dbo].[Contests]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Contests];
GO
IF OBJECT_ID(N'[dbo].[UploadedFiles]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UploadedFiles];
GO
IF OBJECT_ID(N'[dbo].[Articles]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Articles];
GO
IF OBJECT_ID(N'[dbo].[ArticleRevisions]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ArticleRevisions];
GO
IF OBJECT_ID(N'[dbo].[Tags]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Tags];
GO
IF OBJECT_ID(N'[dbo].[Messages]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Messages];
GO
IF OBJECT_ID(N'[dbo].[TestCases_SpecialJudgedTestCase]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TestCases_SpecialJudgedTestCase];
GO
IF OBJECT_ID(N'[dbo].[TestCases_InteractiveTestCase]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TestCases_InteractiveTestCase];
GO
IF OBJECT_ID(N'[dbo].[TestCases_AnswerOnlyTestCase]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TestCases_AnswerOnlyTestCase];
GO
IF OBJECT_ID(N'[dbo].[TestCases_TraditionalTestCase]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TestCases_TraditionalTestCase];
GO
IF OBJECT_ID(N'[dbo].[UserAttendContest]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserAttendContest];
GO
IF OBJECT_ID(N'[dbo].[ContestProblem]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ContestProblem];
GO
IF OBJECT_ID(N'[dbo].[ArticleTag]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ArticleTag];
GO
IF OBJECT_ID(N'[dbo].[ProblemTag]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ProblemTag];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Users'
CREATE TABLE [dbo].[Users] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(20)  NOT NULL,
    [Password] char(64)  NOT NULL,
    [BriefDescription] nvarchar(40)  NOT NULL,
    [Email] nvarchar(max)  NOT NULL,
    [Description] nvarchar(max)  NOT NULL,
    [Score] int  NOT NULL,
    [CreateTime] datetime  NOT NULL,
    [Role_ID] int  NOT NULL
);
GO

-- Creating table 'Roles'
CREATE TABLE [dbo].[Roles] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Name] varchar(12)  NOT NULL
);
GO

-- Creating table 'Problems'
CREATE TABLE [dbo].[Problems] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(40)  NOT NULL,
    [Type] varchar(20)  NOT NULL,
    [SubmissionTimes] int  NOT NULL,
    [ScoreSum] bigint  NOT NULL,
    [SubmissionUser] int  NOT NULL,
    [MaximumScore] int  NULL,
    [CreateTime] datetime  NOT NULL,
    [Hidden] bit  NOT NULL,
    [Locked] bit  NOT NULL,
    [RecordLocked] bit  NOT NULL,
    [PostLocked] bit  NOT NULL,
    [ArticleLocked] bit  NOT NULL,
    [TestCaseLocked] bit  NOT NULL,
    [EnableTesting] bit  NOT NULL,
    [TestCaseHidden] bit  NOT NULL,
    [JudgeInfoHidden] bit  NOT NULL,
    [CreatedBy_ID] int  NOT NULL
);
GO

-- Creating table 'Records'
CREATE TABLE [dbo].[Records] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Code] nvarchar(max)  NOT NULL,
    [CreateTime] datetime  NOT NULL,
    [Language] varchar(12)  NOT NULL,
    [PublicCode] bit  NOT NULL,
    [Problem_ID] int  NOT NULL,
    [User_ID] int  NOT NULL
);
GO

-- Creating table 'TestCases'
CREATE TABLE [dbo].[TestCases] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [CreateTime] datetime  NOT NULL,
    [Problem_ID] int  NOT NULL,
    [CreatedBy_ID] int  NOT NULL
);
GO

-- Creating table 'ProblemRevisions'
CREATE TABLE [dbo].[ProblemRevisions] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Content] nvarchar(max)  NOT NULL,
    [Reason] nvarchar(40)  NOT NULL,
    [CreateTime] datetime  NOT NULL,
    [Problem_ID] int  NOT NULL,
    [CreatedBy_ID] int  NOT NULL,
    [LastestRevisionOfProblem_ProblemRevision_ID] int  NULL
);
GO

-- Creating table 'Posts'
CREATE TABLE [dbo].[Posts] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(40)  NOT NULL,
    [OnTop] bit  NOT NULL,
    [ReplyTime] datetime  NOT NULL,
    [Locked] bit  NOT NULL,
    [Problem_ID] int  NULL
);
GO

-- Creating table 'PostItems'
CREATE TABLE [dbo].[PostItems] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Content] nvarchar(max)  NOT NULL,
    [CreateTime] datetime  NOT NULL,
    [CreatedBy_ID] int  NOT NULL,
    [Post_ID] int  NOT NULL
);
GO

-- Creating table 'JudgeInfos'
CREATE TABLE [dbo].[JudgeInfos] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Score] int  NOT NULL,
    [Info] nvarchar(max)  NOT NULL,
    [Record_ID] int  NOT NULL
);
GO

-- Creating table 'Contests'
CREATE TABLE [dbo].[Contests] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [StartTime] datetime  NOT NULL,
    [EndTime] datetime  NOT NULL,
    [Name] nvarchar(40)  NOT NULL,
    [LockProblemOnStart] bit  NOT NULL,
    [LockTestCaseOnStart] bit  NOT NULL,
    [LockPostOnStart] bit  NOT NULL,
    [HideTestCaseOnStart] bit  NOT NULL,
    [EnableTestingOnStart] bit  NOT NULL,
    [Description] nvarchar(max)  NOT NULL,
    [Status] varchar(12)  NOT NULL,
    [HideProblemOnStart] bit  NOT NULL,
    [LockRecordOnStart] bit  NOT NULL,
    [LockProblemOnEnd] bit  NOT NULL,
    [LockTestCaseOnEnd] bit  NOT NULL,
    [LockPostOnEnd] bit  NOT NULL,
    [LockRecordOnEnd] bit  NOT NULL,
    [EnableTestingOnEnd] bit  NOT NULL,
    [HideProblemOnEnd] bit  NOT NULL,
    [HideTestCaseOnEnd] bit  NOT NULL,
    [LockArticleOnStart] bit  NOT NULL,
    [LockArticleOnEnd] bit  NOT NULL,
    [HideJudgeInfoOnStart] bit  NOT NULL,
    [HideJudgeInfoOnEnd] bit  NOT NULL,
    [ViewResultAnyTime] bit  NOT NULL
);
GO

-- Creating table 'UploadedFiles'
CREATE TABLE [dbo].[UploadedFiles] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(40)  NOT NULL,
    [FileName] nvarchar(max)  NOT NULL,
    [Description] nvarchar(max)  NOT NULL,
    [CreatedBy_ID] int  NOT NULL
);
GO

-- Creating table 'Articles'
CREATE TABLE [dbo].[Articles] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(40)  NOT NULL,
    [CreateTime] datetime  NOT NULL,
    [CreatedBy_ID] int  NOT NULL,
    [Problem_ID] int  NULL
);
GO

-- Creating table 'ArticleRevisions'
CREATE TABLE [dbo].[ArticleRevisions] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Content] nvarchar(max)  NOT NULL,
    [CreateTime] datetime  NOT NULL,
    [Reason] nvarchar(40)  NOT NULL,
    [CreatedBy_ID] int  NOT NULL,
    [Article_ID] int  NOT NULL,
    [ArticleLatestRevision_ArticleRevision_ID] int  NULL
);
GO

-- Creating table 'Tags'
CREATE TABLE [dbo].[Tags] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(40)  NOT NULL
);
GO

-- Creating table 'Messages'
CREATE TABLE [dbo].[Messages] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [CreateTime] datetime  NOT NULL,
    [Content] nvarchar(max)  NOT NULL,
    [DeletedByFrom] bit  NOT NULL,
    [DeletedByTo] bit  NOT NULL,
    [HasRead] bit  NOT NULL,
    [From_ID] int  NOT NULL,
    [To_ID] int  NULL
);
GO

-- Creating table 'TestCases_SpecialJudgedTestCase'
CREATE TABLE [dbo].[TestCases_SpecialJudgedTestCase] (
    [Input] varbinary(max)  NOT NULL,
    [Answer] varbinary(max)  NOT NULL,
    [TimeLimit] int  NOT NULL,
    [MemoryLimit] int  NOT NULL,
    [ID] int  NOT NULL,
    [Judger_ID] int  NOT NULL
);
GO

-- Creating table 'TestCases_InteractiveTestCase'
CREATE TABLE [dbo].[TestCases_InteractiveTestCase] (
    [TestData] varbinary(max)  NOT NULL,
    [TimeLimit] int  NOT NULL,
    [MemoryLimit] int  NOT NULL,
    [ID] int  NOT NULL,
    [Invoker_ID] int  NOT NULL
);
GO

-- Creating table 'TestCases_AnswerOnlyTestCase'
CREATE TABLE [dbo].[TestCases_AnswerOnlyTestCase] (
    [TestData] varbinary(max)  NOT NULL,
    [ID] int  NOT NULL,
    [Judger_ID] int  NOT NULL
);
GO

-- Creating table 'TestCases_TraditionalTestCase'
CREATE TABLE [dbo].[TestCases_TraditionalTestCase] (
    [Input] varbinary(max)  NOT NULL,
    [Answer] varbinary(max)  NOT NULL,
    [TimeLimit] int  NOT NULL,
    [MemoryLimit] int  NOT NULL,
    [Score] int  NOT NULL,
    [ID] int  NOT NULL
);
GO

-- Creating table 'UserAttendContest'
CREATE TABLE [dbo].[UserAttendContest] (
    [User_ID] int  NOT NULL,
    [Contest_ID] int  NOT NULL
);
GO

-- Creating table 'ContestProblem'
CREATE TABLE [dbo].[ContestProblem] (
    [Contest_ID] int  NOT NULL,
    [Problem_ID] int  NOT NULL
);
GO

-- Creating table 'ArticleTag'
CREATE TABLE [dbo].[ArticleTag] (
    [Article_ID] int  NOT NULL,
    [Tag_ID] int  NOT NULL
);
GO

-- Creating table 'ProblemTag'
CREATE TABLE [dbo].[ProblemTag] (
    [Problem_ID] int  NOT NULL,
    [Tag_ID] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [ID] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [PK_Users]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'Roles'
ALTER TABLE [dbo].[Roles]
ADD CONSTRAINT [PK_Roles]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'Problems'
ALTER TABLE [dbo].[Problems]
ADD CONSTRAINT [PK_Problems]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'Records'
ALTER TABLE [dbo].[Records]
ADD CONSTRAINT [PK_Records]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'TestCases'
ALTER TABLE [dbo].[TestCases]
ADD CONSTRAINT [PK_TestCases]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'ProblemRevisions'
ALTER TABLE [dbo].[ProblemRevisions]
ADD CONSTRAINT [PK_ProblemRevisions]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'Posts'
ALTER TABLE [dbo].[Posts]
ADD CONSTRAINT [PK_Posts]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'PostItems'
ALTER TABLE [dbo].[PostItems]
ADD CONSTRAINT [PK_PostItems]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'JudgeInfos'
ALTER TABLE [dbo].[JudgeInfos]
ADD CONSTRAINT [PK_JudgeInfos]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'Contests'
ALTER TABLE [dbo].[Contests]
ADD CONSTRAINT [PK_Contests]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'UploadedFiles'
ALTER TABLE [dbo].[UploadedFiles]
ADD CONSTRAINT [PK_UploadedFiles]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'Articles'
ALTER TABLE [dbo].[Articles]
ADD CONSTRAINT [PK_Articles]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'ArticleRevisions'
ALTER TABLE [dbo].[ArticleRevisions]
ADD CONSTRAINT [PK_ArticleRevisions]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'Tags'
ALTER TABLE [dbo].[Tags]
ADD CONSTRAINT [PK_Tags]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'Messages'
ALTER TABLE [dbo].[Messages]
ADD CONSTRAINT [PK_Messages]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'TestCases_SpecialJudgedTestCase'
ALTER TABLE [dbo].[TestCases_SpecialJudgedTestCase]
ADD CONSTRAINT [PK_TestCases_SpecialJudgedTestCase]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'TestCases_InteractiveTestCase'
ALTER TABLE [dbo].[TestCases_InteractiveTestCase]
ADD CONSTRAINT [PK_TestCases_InteractiveTestCase]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'TestCases_AnswerOnlyTestCase'
ALTER TABLE [dbo].[TestCases_AnswerOnlyTestCase]
ADD CONSTRAINT [PK_TestCases_AnswerOnlyTestCase]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'TestCases_TraditionalTestCase'
ALTER TABLE [dbo].[TestCases_TraditionalTestCase]
ADD CONSTRAINT [PK_TestCases_TraditionalTestCase]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [User_ID], [Contest_ID] in table 'UserAttendContest'
ALTER TABLE [dbo].[UserAttendContest]
ADD CONSTRAINT [PK_UserAttendContest]
    PRIMARY KEY NONCLUSTERED ([User_ID], [Contest_ID] ASC);
GO

-- Creating primary key on [Contest_ID], [Problem_ID] in table 'ContestProblem'
ALTER TABLE [dbo].[ContestProblem]
ADD CONSTRAINT [PK_ContestProblem]
    PRIMARY KEY NONCLUSTERED ([Contest_ID], [Problem_ID] ASC);
GO

-- Creating primary key on [Article_ID], [Tag_ID] in table 'ArticleTag'
ALTER TABLE [dbo].[ArticleTag]
ADD CONSTRAINT [PK_ArticleTag]
    PRIMARY KEY NONCLUSTERED ([Article_ID], [Tag_ID] ASC);
GO

-- Creating primary key on [Problem_ID], [Tag_ID] in table 'ProblemTag'
ALTER TABLE [dbo].[ProblemTag]
ADD CONSTRAINT [PK_ProblemTag]
    PRIMARY KEY NONCLUSTERED ([Problem_ID], [Tag_ID] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [Problem_ID] in table 'Records'
ALTER TABLE [dbo].[Records]
ADD CONSTRAINT [FK_RecordProblem]
    FOREIGN KEY ([Problem_ID])
    REFERENCES [dbo].[Problems]
        ([ID])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_RecordProblem'
CREATE INDEX [IX_FK_RecordProblem]
ON [dbo].[Records]
    ([Problem_ID]);
GO

-- Creating foreign key on [Problem_ID] in table 'TestCases'
ALTER TABLE [dbo].[TestCases]
ADD CONSTRAINT [FK_TestDataProblem]
    FOREIGN KEY ([Problem_ID])
    REFERENCES [dbo].[Problems]
        ([ID])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_TestDataProblem'
CREATE INDEX [IX_FK_TestDataProblem]
ON [dbo].[TestCases]
    ([Problem_ID]);
GO

-- Creating foreign key on [Problem_ID] in table 'ProblemRevisions'
ALTER TABLE [dbo].[ProblemRevisions]
ADD CONSTRAINT [FK_ProblemProblemRevision]
    FOREIGN KEY ([Problem_ID])
    REFERENCES [dbo].[Problems]
        ([ID])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ProblemProblemRevision'
CREATE INDEX [IX_FK_ProblemProblemRevision]
ON [dbo].[ProblemRevisions]
    ([Problem_ID]);
GO

-- Creating foreign key on [CreatedBy_ID] in table 'ProblemRevisions'
ALTER TABLE [dbo].[ProblemRevisions]
ADD CONSTRAINT [FK_UserProblemRevision]
    FOREIGN KEY ([CreatedBy_ID])
    REFERENCES [dbo].[Users]
        ([ID])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_UserProblemRevision'
CREATE INDEX [IX_FK_UserProblemRevision]
ON [dbo].[ProblemRevisions]
    ([CreatedBy_ID]);
GO

-- Creating foreign key on [Role_ID] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [FK_UserRole]
    FOREIGN KEY ([Role_ID])
    REFERENCES [dbo].[Roles]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_UserRole'
CREATE INDEX [IX_FK_UserRole]
ON [dbo].[Users]
    ([Role_ID]);
GO

-- Creating foreign key on [CreatedBy_ID] in table 'PostItems'
ALTER TABLE [dbo].[PostItems]
ADD CONSTRAINT [FK_UserCreatePostItem]
    FOREIGN KEY ([CreatedBy_ID])
    REFERENCES [dbo].[Users]
        ([ID])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_UserCreatePostItem'
CREATE INDEX [IX_FK_UserCreatePostItem]
ON [dbo].[PostItems]
    ([CreatedBy_ID]);
GO

-- Creating foreign key on [Post_ID] in table 'PostItems'
ALTER TABLE [dbo].[PostItems]
ADD CONSTRAINT [FK_PostItemPost]
    FOREIGN KEY ([Post_ID])
    REFERENCES [dbo].[Posts]
        ([ID])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_PostItemPost'
CREATE INDEX [IX_FK_PostItemPost]
ON [dbo].[PostItems]
    ([Post_ID]);
GO

-- Creating foreign key on [Problem_ID] in table 'Posts'
ALTER TABLE [dbo].[Posts]
ADD CONSTRAINT [FK_PostProblem]
    FOREIGN KEY ([Problem_ID])
    REFERENCES [dbo].[Problems]
        ([ID])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_PostProblem'
CREATE INDEX [IX_FK_PostProblem]
ON [dbo].[Posts]
    ([Problem_ID]);
GO

-- Creating foreign key on [Record_ID] in table 'JudgeInfos'
ALTER TABLE [dbo].[JudgeInfos]
ADD CONSTRAINT [FK_RecordJudgeInfo]
    FOREIGN KEY ([Record_ID])
    REFERENCES [dbo].[Records]
        ([ID])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_RecordJudgeInfo'
CREATE INDEX [IX_FK_RecordJudgeInfo]
ON [dbo].[JudgeInfos]
    ([Record_ID]);
GO

-- Creating foreign key on [LastestRevisionOfProblem_ProblemRevision_ID] in table 'ProblemRevisions'
ALTER TABLE [dbo].[ProblemRevisions]
ADD CONSTRAINT [FK_LastestRevisionOfProblem]
    FOREIGN KEY ([LastestRevisionOfProblem_ProblemRevision_ID])
    REFERENCES [dbo].[Problems]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_LastestRevisionOfProblem'
CREATE INDEX [IX_FK_LastestRevisionOfProblem]
ON [dbo].[ProblemRevisions]
    ([LastestRevisionOfProblem_ProblemRevision_ID]);
GO

-- Creating foreign key on [User_ID] in table 'UserAttendContest'
ALTER TABLE [dbo].[UserAttendContest]
ADD CONSTRAINT [FK_UserAttendContest_User]
    FOREIGN KEY ([User_ID])
    REFERENCES [dbo].[Users]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Contest_ID] in table 'UserAttendContest'
ALTER TABLE [dbo].[UserAttendContest]
ADD CONSTRAINT [FK_UserAttendContest_Contest]
    FOREIGN KEY ([Contest_ID])
    REFERENCES [dbo].[Contests]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_UserAttendContest_Contest'
CREATE INDEX [IX_FK_UserAttendContest_Contest]
ON [dbo].[UserAttendContest]
    ([Contest_ID]);
GO

-- Creating foreign key on [User_ID] in table 'Records'
ALTER TABLE [dbo].[Records]
ADD CONSTRAINT [FK_UserRecord]
    FOREIGN KEY ([User_ID])
    REFERENCES [dbo].[Users]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_UserRecord'
CREATE INDEX [IX_FK_UserRecord]
ON [dbo].[Records]
    ([User_ID]);
GO

-- Creating foreign key on [Contest_ID] in table 'ContestProblem'
ALTER TABLE [dbo].[ContestProblem]
ADD CONSTRAINT [FK_ContestProblem_Contest]
    FOREIGN KEY ([Contest_ID])
    REFERENCES [dbo].[Contests]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Problem_ID] in table 'ContestProblem'
ALTER TABLE [dbo].[ContestProblem]
ADD CONSTRAINT [FK_ContestProblem_Problem]
    FOREIGN KEY ([Problem_ID])
    REFERENCES [dbo].[Problems]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ContestProblem_Problem'
CREATE INDEX [IX_FK_ContestProblem_Problem]
ON [dbo].[ContestProblem]
    ([Problem_ID]);
GO

-- Creating foreign key on [Judger_ID] in table 'TestCases_SpecialJudgedTestCase'
ALTER TABLE [dbo].[TestCases_SpecialJudgedTestCase]
ADD CONSTRAINT [FK_SpecialJudgedTestCaseUploadedFile]
    FOREIGN KEY ([Judger_ID])
    REFERENCES [dbo].[UploadedFiles]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_SpecialJudgedTestCaseUploadedFile'
CREATE INDEX [IX_FK_SpecialJudgedTestCaseUploadedFile]
ON [dbo].[TestCases_SpecialJudgedTestCase]
    ([Judger_ID]);
GO

-- Creating foreign key on [Invoker_ID] in table 'TestCases_InteractiveTestCase'
ALTER TABLE [dbo].[TestCases_InteractiveTestCase]
ADD CONSTRAINT [FK_InteractiveTestCaseInvokerFile]
    FOREIGN KEY ([Invoker_ID])
    REFERENCES [dbo].[UploadedFiles]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_InteractiveTestCaseInvokerFile'
CREATE INDEX [IX_FK_InteractiveTestCaseInvokerFile]
ON [dbo].[TestCases_InteractiveTestCase]
    ([Invoker_ID]);
GO

-- Creating foreign key on [Judger_ID] in table 'TestCases_AnswerOnlyTestCase'
ALTER TABLE [dbo].[TestCases_AnswerOnlyTestCase]
ADD CONSTRAINT [FK_AnswerOnlyTestCaseUploadedFile]
    FOREIGN KEY ([Judger_ID])
    REFERENCES [dbo].[UploadedFiles]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_AnswerOnlyTestCaseUploadedFile'
CREATE INDEX [IX_FK_AnswerOnlyTestCaseUploadedFile]
ON [dbo].[TestCases_AnswerOnlyTestCase]
    ([Judger_ID]);
GO

-- Creating foreign key on [CreatedBy_ID] in table 'TestCases'
ALTER TABLE [dbo].[TestCases]
ADD CONSTRAINT [FK_UserTestCase]
    FOREIGN KEY ([CreatedBy_ID])
    REFERENCES [dbo].[Users]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_UserTestCase'
CREATE INDEX [IX_FK_UserTestCase]
ON [dbo].[TestCases]
    ([CreatedBy_ID]);
GO

-- Creating foreign key on [CreatedBy_ID] in table 'UploadedFiles'
ALTER TABLE [dbo].[UploadedFiles]
ADD CONSTRAINT [FK_UploadedFileUser]
    FOREIGN KEY ([CreatedBy_ID])
    REFERENCES [dbo].[Users]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_UploadedFileUser'
CREATE INDEX [IX_FK_UploadedFileUser]
ON [dbo].[UploadedFiles]
    ([CreatedBy_ID]);
GO

-- Creating foreign key on [CreatedBy_ID] in table 'Problems'
ALTER TABLE [dbo].[Problems]
ADD CONSTRAINT [FK_UserCreateProblem]
    FOREIGN KEY ([CreatedBy_ID])
    REFERENCES [dbo].[Users]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_UserCreateProblem'
CREATE INDEX [IX_FK_UserCreateProblem]
ON [dbo].[Problems]
    ([CreatedBy_ID]);
GO

-- Creating foreign key on [CreatedBy_ID] in table 'Articles'
ALTER TABLE [dbo].[Articles]
ADD CONSTRAINT [FK_UserCreateArticle]
    FOREIGN KEY ([CreatedBy_ID])
    REFERENCES [dbo].[Users]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_UserCreateArticle'
CREATE INDEX [IX_FK_UserCreateArticle]
ON [dbo].[Articles]
    ([CreatedBy_ID]);
GO

-- Creating foreign key on [CreatedBy_ID] in table 'ArticleRevisions'
ALTER TABLE [dbo].[ArticleRevisions]
ADD CONSTRAINT [FK_UserCreateArticleRevision]
    FOREIGN KEY ([CreatedBy_ID])
    REFERENCES [dbo].[Users]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_UserCreateArticleRevision'
CREATE INDEX [IX_FK_UserCreateArticleRevision]
ON [dbo].[ArticleRevisions]
    ([CreatedBy_ID]);
GO

-- Creating foreign key on [Article_ID] in table 'ArticleRevisions'
ALTER TABLE [dbo].[ArticleRevisions]
ADD CONSTRAINT [FK_ArticleArticleRevision]
    FOREIGN KEY ([Article_ID])
    REFERENCES [dbo].[Articles]
        ([ID])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ArticleArticleRevision'
CREATE INDEX [IX_FK_ArticleArticleRevision]
ON [dbo].[ArticleRevisions]
    ([Article_ID]);
GO

-- Creating foreign key on [ArticleLatestRevision_ArticleRevision_ID] in table 'ArticleRevisions'
ALTER TABLE [dbo].[ArticleRevisions]
ADD CONSTRAINT [FK_ArticleLatestRevision]
    FOREIGN KEY ([ArticleLatestRevision_ArticleRevision_ID])
    REFERENCES [dbo].[Articles]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ArticleLatestRevision'
CREATE INDEX [IX_FK_ArticleLatestRevision]
ON [dbo].[ArticleRevisions]
    ([ArticleLatestRevision_ArticleRevision_ID]);
GO

-- Creating foreign key on [Problem_ID] in table 'Articles'
ALTER TABLE [dbo].[Articles]
ADD CONSTRAINT [FK_ArticleProblem]
    FOREIGN KEY ([Problem_ID])
    REFERENCES [dbo].[Problems]
        ([ID])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ArticleProblem'
CREATE INDEX [IX_FK_ArticleProblem]
ON [dbo].[Articles]
    ([Problem_ID]);
GO

-- Creating foreign key on [Article_ID] in table 'ArticleTag'
ALTER TABLE [dbo].[ArticleTag]
ADD CONSTRAINT [FK_ArticleTag_Article]
    FOREIGN KEY ([Article_ID])
    REFERENCES [dbo].[Articles]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Tag_ID] in table 'ArticleTag'
ALTER TABLE [dbo].[ArticleTag]
ADD CONSTRAINT [FK_ArticleTag_Tag]
    FOREIGN KEY ([Tag_ID])
    REFERENCES [dbo].[Tags]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ArticleTag_Tag'
CREATE INDEX [IX_FK_ArticleTag_Tag]
ON [dbo].[ArticleTag]
    ([Tag_ID]);
GO

-- Creating foreign key on [Problem_ID] in table 'ProblemTag'
ALTER TABLE [dbo].[ProblemTag]
ADD CONSTRAINT [FK_ProblemTag_Problem]
    FOREIGN KEY ([Problem_ID])
    REFERENCES [dbo].[Problems]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Tag_ID] in table 'ProblemTag'
ALTER TABLE [dbo].[ProblemTag]
ADD CONSTRAINT [FK_ProblemTag_Tag]
    FOREIGN KEY ([Tag_ID])
    REFERENCES [dbo].[Tags]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ProblemTag_Tag'
CREATE INDEX [IX_FK_ProblemTag_Tag]
ON [dbo].[ProblemTag]
    ([Tag_ID]);
GO

-- Creating foreign key on [From_ID] in table 'Messages'
ALTER TABLE [dbo].[Messages]
ADD CONSTRAINT [FK_MessageFrom]
    FOREIGN KEY ([From_ID])
    REFERENCES [dbo].[Users]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_MessageFrom'
CREATE INDEX [IX_FK_MessageFrom]
ON [dbo].[Messages]
    ([From_ID]);
GO

-- Creating foreign key on [To_ID] in table 'Messages'
ALTER TABLE [dbo].[Messages]
ADD CONSTRAINT [FK_MessageTo]
    FOREIGN KEY ([To_ID])
    REFERENCES [dbo].[Users]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_MessageTo'
CREATE INDEX [IX_FK_MessageTo]
ON [dbo].[Messages]
    ([To_ID]);
GO

-- Creating foreign key on [ID] in table 'TestCases_SpecialJudgedTestCase'
ALTER TABLE [dbo].[TestCases_SpecialJudgedTestCase]
ADD CONSTRAINT [FK_SpecialJudgedTestCase_inherits_TestCase]
    FOREIGN KEY ([ID])
    REFERENCES [dbo].[TestCases]
        ([ID])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [ID] in table 'TestCases_InteractiveTestCase'
ALTER TABLE [dbo].[TestCases_InteractiveTestCase]
ADD CONSTRAINT [FK_InteractiveTestCase_inherits_TestCase]
    FOREIGN KEY ([ID])
    REFERENCES [dbo].[TestCases]
        ([ID])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [ID] in table 'TestCases_AnswerOnlyTestCase'
ALTER TABLE [dbo].[TestCases_AnswerOnlyTestCase]
ADD CONSTRAINT [FK_AnswerOnlyTestCase_inherits_TestCase]
    FOREIGN KEY ([ID])
    REFERENCES [dbo].[TestCases]
        ([ID])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [ID] in table 'TestCases_TraditionalTestCase'
ALTER TABLE [dbo].[TestCases_TraditionalTestCase]
ADD CONSTRAINT [FK_TraditionalTestCase_inherits_TestCase]
    FOREIGN KEY ([ID])
    REFERENCES [dbo].[TestCases]
        ([ID])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------