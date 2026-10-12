-- ============================================================================
-- Anonymous Sharing Platform (WhisperSpace / SafeSpace)
-- Database Script for Microsoft SQL Server
-- Subject: B.Tech Semester 5 .NET Technologies Mini Project
-- Technology: Microsoft SQL Server, ADO.NET, ASP.NET Core MVC
-- ============================================================================

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'AnonymousSharingDB')
BEGIN
    CREATE DATABASE AnonymousSharingDB;
END
GO

USE AnonymousSharingDB;
GO

-- 1. Table: Categories
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Categories')
BEGIN
    CREATE TABLE Categories (
        CategoryId INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(300) NOT NULL,
        IconClass NVARCHAR(50) NOT NULL,
        BadgeColor NVARCHAR(30) NOT NULL
    );
END
GO

-- 2. Table: Posts
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Posts')
BEGIN
    CREATE TABLE Posts (
        PostId INT IDENTITY(1,1) PRIMARY KEY,
        Title NVARCHAR(200) NOT NULL,
        Content NVARCHAR(MAX) NOT NULL,
        CategoryId INT NOT NULL,
        MoodTag NVARCHAR(50) NOT NULL,
        AuthorAlias NVARCHAR(80) NOT NULL DEFAULT 'Anonymous Student',
        SupportCount INT NOT NULL DEFAULT 0,
        ReportCount INT NOT NULL DEFAULT 0,
        IsHidden BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_Posts_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId) ON DELETE CASCADE
    );
END
GO

-- 3. Table: Comments
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Comments')
BEGIN
    CREATE TABLE Comments (
        CommentId INT IDENTITY(1,1) PRIMARY KEY,
        PostId INT NOT NULL,
        AuthorAlias NVARCHAR(80) NOT NULL DEFAULT 'Supportive Peer',
        Content NVARCHAR(1500) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        IsHidden BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_Comments_Posts FOREIGN KEY (PostId) REFERENCES Posts(PostId) ON DELETE CASCADE
    );
END
GO

-- 4. Table: Reports
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Reports')
BEGIN
    CREATE TABLE Reports (
        ReportId INT IDENTITY(1,1) PRIMARY KEY,
        PostId INT NOT NULL,
        Reason NVARCHAR(100) NOT NULL,
        DetailedRemarks NVARCHAR(500) NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- 'Pending', 'Reviewed', 'Dismissed', 'ActionTaken'
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_Reports_Posts FOREIGN KEY (PostId) REFERENCES Posts(PostId) ON DELETE CASCADE
    );
END
GO

-- 5. Table: Admins
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Admins')
BEGIN
    CREATE TABLE Admins (
        AdminId INT IDENTITY(1,1) PRIMARY KEY,
        Username NVARCHAR(50) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(256) NOT NULL,
        FullName NVARCHAR(100) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
    );
END
GO

-- ============================================================================
-- SEED DATA
-- ============================================================================

-- Seed Categories
IF NOT EXISTS (SELECT 1 FROM Categories)
BEGIN
    INSERT INTO Categories (Name, Description, IconClass, BadgeColor) VALUES
    ('Academic & Exam Stress', 'Discuss semester exams, viva pressure, backlogs, and study anxieties.', 'academic', '#2563eb'),
    ('Career & Placements', 'Placement preparation, interview rejections, off-campus struggles, and future doubts.', 'career', '#059669'),
    ('Mental Health & Well-being', 'Feeling overwhelmed, loneliness, imposter syndrome, and emotional wellness.', 'mental', '#7c3aed'),
    ('Campus Life & Hostel', 'Hostel food, roommate conflicts, college events, and campus anecdotes.', 'campus', '#d97706'),
    ('Relationships & Peer Pressure', 'Friendship fallouts, family expectations, relationship advice, and peer comparisons.', 'social', '#e11d48'),
    ('Late Night Confessions', 'Unfiltered thoughts, hidden confessions, and late night reflections.', 'confession', '#4f46e5');
END
GO

-- Seed Admin (Default credentials: admin / Admin@123)
-- SHA256 of 'Admin@123': a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3
IF NOT EXISTS (SELECT 1 FROM Admins WHERE Username = 'admin')
BEGIN
    INSERT INTO Admins (Username, PasswordHash, FullName)
    VALUES ('admin', 'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', 'Platform Moderator');
END
GO

-- Seed Sample Posts
IF NOT EXISTS (SELECT 1 FROM Posts)
BEGIN
    INSERT INTO Posts (Title, Content, CategoryId, MoodTag, AuthorAlias, SupportCount, ReportCount, IsHidden, CreatedAt)
    VALUES 
    (
        'Struggling with Semester 5 syllabus and backlogs',
        'Between lab practical submissions, 6 subjects, and mini-project deadlines, I feel completely drained. Every time I open my books I feel anxious about not completing the syllabus. Does anyone have a realistic timetable or tips for balancing labs with theory?',
        1,
        'Overwhelmed',
        'Tired Phoenix',
        14,
        0,
        0,
        DATEADD(HOUR, -5, GETDATE())
    ),
    (
        'Got rejected in the final interview round today',
        'I prepared for three weeks for the on-campus technical drive. Cleared coding and technical rounds, but stumbled in the HR round. Seeing friends get offers makes me happy for them, but deeply insecure about my own future. Trying to stay optimistic.',
        2,
        'Heartbroken',
        'Silent Seeker',
        22,
        0,
        0,
        DATEADD(HOUR, -12, GETDATE())
    ),
    (
        'Imposter syndrome is hitting hard in team projects',
        'Everyone in my project group seems to know advanced tech stacks while I am still figuring out the basics. I hesitate to ask questions because I feel like I will be judged. Just venting here because I have nowhere else to say this.',
        3,
        'Anxious',
        'Gentle Breeze',
        18,
        0,
        0,
        DATEADD(DAY, -1, GETDATE())
    ),
    (
        'Hostel mess food is testing my will to live',
        'Week 8 of potato curry 4 days in a row. How do people survive 4 years of this? Sending thoughts and prayers to everyone eating in Block C tonight.',
        4,
        'Humorous',
        'Hungry Nomad',
        31,
        0,
        0,
        DATEADD(DAY, -2, GETDATE())
    );
END
GO

-- Seed Sample Comments
IF NOT EXISTS (SELECT 1 FROM Comments)
BEGIN
    INSERT INTO Comments (PostId, AuthorAlias, Content, CreatedAt)
    VALUES
    (1, 'Supportive Senior', 'Take a deep breath! Prioritize previous year question papers and focus on 3 high-weightage units first. You got this.', DATEADD(HOUR, -4, GETDATE())),
    (1, 'Calm Anchor', 'Do not try to finish everything in one night. Break it into 45-minute study sprints with 10-minute breaks.', DATEADD(HOUR, -2, GETDATE())),
    (2, 'Fellow Aspirant', 'Rejections are redirections. I faced 5 rejections before landing a great offer. Keep your chin up and analyze the gaps.', DATEADD(HOUR, -10, GETDATE())),
    (3, 'Kind Sparrow', 'Trust me, most people just google their way through it. You are not behind, everyone learns on the go.', DATEADD(HOUR, -20, GETDATE()));
END
GO
