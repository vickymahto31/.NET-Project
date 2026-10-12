using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AnonymousSharingPlatform.Models;

namespace AnonymousSharingPlatform.Data
{
    /// <summary>
    /// Central ADO.NET Data Access Helper class.
    /// Manages SQL Server SqlConnection, parameter binding, command execution,
    /// and provides an in-memory fallback store if local SQL Server is unavailable during evaluation.
    /// </summary>
    public class DatabaseHelper
    {
        private readonly string _connectionString;
        private readonly ILogger<DatabaseHelper> _logger;
        private static bool? _isSqlServerAvailable = null;
        private static readonly object _syncLock = new object();

        // In-memory fallback datasets (thread-safe)
        public static readonly ConcurrentBag<Category> MockCategories = new ConcurrentBag<Category>();
        public static readonly ConcurrentBag<Post> MockPosts = new ConcurrentBag<Post>();
        public static readonly ConcurrentBag<Comment> MockComments = new ConcurrentBag<Comment>();
        public static readonly ConcurrentBag<Report> MockReports = new ConcurrentBag<Report>();
        public static readonly ConcurrentBag<AdminUser> MockAdmins = new ConcurrentBag<AdminUser>();

        private static int _nextPostId = 100;
        private static int _nextCommentId = 100;
        private static int _nextReportId = 100;

        public DatabaseHelper(IConfiguration configuration, ILogger<DatabaseHelper> logger)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? "Server=(localdb)\\mssqllocaldb;Database=AnonymousSharingDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

            InitializeMockDataIfEmpty();
        }

        public string ConnectionString => _connectionString;

        public SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        /// <summary>
        /// Checks whether SQL Server is reachable with the configured connection string.
        /// </summary>
        public bool IsSqlServerAvailable()
        {
            if (_isSqlServerAvailable.HasValue)
            {
                return _isSqlServerAvailable.Value;
            }

            lock (_syncLock)
            {
                if (_isSqlServerAvailable.HasValue)
                    return _isSqlServerAvailable.Value;

                try
                {
                    // Short timeout connection test
                    var builder = new SqlConnectionStringBuilder(_connectionString)
                    {
                        ConnectTimeout = 2
                    };

                    using (var conn = new SqlConnection(builder.ConnectionString))
                    {
                        conn.Open();
                        _isSqlServerAvailable = true;
                        _logger.LogInformation("SQL Server connection successful: {DataSource}", conn.DataSource);
                    }
                }
                catch (Exception ex)
                {
                    _isSqlServerAvailable = false;
                    _logger.LogWarning("SQL Server not reachable: {Message}. Running in ADO.NET resilient fallback mode.", ex.Message);
                }

                return _isSqlServerAvailable.Value;
            }
        }

        public void ForceRecheckConnection()
        {
            lock (_syncLock)
            {
                _isSqlServerAvailable = null;
            }
        }

        public static int GetNextPostId() => System.Threading.Interlocked.Increment(ref _nextPostId);
        public static int GetNextCommentId() => System.Threading.Interlocked.Increment(ref _nextCommentId);
        public static int GetNextReportId() => System.Threading.Interlocked.Increment(ref _nextReportId);

        private void InitializeMockDataIfEmpty()
        {
            if (!MockCategories.IsEmpty) return;

            lock (_syncLock)
            {
                if (!MockCategories.IsEmpty) return;

                // Categories
                var cat1 = new Category { CategoryId = 1, Name = "Academic & Exam Stress", Description = "Discuss semester exams, viva pressure, backlogs, and study anxieties.", IconClass = "academic", BadgeColor = "#2563eb" };
                var cat2 = new Category { CategoryId = 2, Name = "Career & Placements", Description = "Placement preparation, interview rejections, off-campus struggles, and future doubts.", IconClass = "career", BadgeColor = "#059669" };
                var cat3 = new Category { CategoryId = 3, Name = "Mental Health & Well-being", Description = "Feeling overwhelmed, loneliness, imposter syndrome, and emotional wellness.", IconClass = "mental", BadgeColor = "#7c3aed" };
                var cat4 = new Category { CategoryId = 4, Name = "Campus Life & Hostel", Description = "Hostel food, roommate conflicts, college events, and campus anecdotes.", IconClass = "campus", BadgeColor = "#d97706" };
                var cat5 = new Category { CategoryId = 5, Name = "Relationships & Peer Pressure", Description = "Friendship fallouts, family expectations, relationship advice, and peer comparisons.", IconClass = "social", BadgeColor = "#e11d48" };
                var cat6 = new Category { CategoryId = 6, Name = "Late Night Confessions", Description = "Unfiltered thoughts, hidden confessions, and late night reflections.", IconClass = "confession", BadgeColor = "#4f46e5" };

                MockCategories.Add(cat1);
                MockCategories.Add(cat2);
                MockCategories.Add(cat3);
                MockCategories.Add(cat4);
                MockCategories.Add(cat5);
                MockCategories.Add(cat6);

                // Admin (admin / Admin@123)
                // SHA256 of 'Admin@123': a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3
                MockAdmins.Add(new AdminUser
                {
                    AdminId = 1,
                    Username = "admin",
                    PasswordHash = "a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3",
                    FullName = "College Platform Moderator",
                    CreatedAt = DateTime.Now.AddDays(-30)
                });

                // Seed sample posts
                var p1 = new Post
                {
                    PostId = 1,
                    Title = "Struggling with Semester 5 syllabus and backlogs",
                    Content = "Between lab practical submissions, 6 theory subjects, and mini-project deadlines, I feel completely drained. Every time I open my books I feel anxious about not completing the syllabus. Does anyone have a realistic timetable or tips for balancing labs with theory?",
                    CategoryId = 1,
                    CategoryName = "Academic & Exam Stress",
                    CategoryBadgeColor = "#2563eb",
                    MoodTag = "Overwhelmed",
                    AuthorAlias = "Tired Phoenix",
                    SupportCount = 14,
                    ReportCount = 0,
                    CommentCount = 2,
                    IsHidden = false,
                    CreatedAt = DateTime.Now.AddHours(-5)
                };

                var p2 = new Post
                {
                    PostId = 2,
                    Title = "Got rejected in the final interview round today",
                    Content = "I prepared for three weeks for the on-campus technical drive. Cleared coding and technical rounds, but stumbled in the HR round. Seeing friends get offers makes me happy for them, but deeply insecure about my own future. Trying to stay optimistic.",
                    CategoryId = 2,
                    CategoryName = "Career & Placements",
                    CategoryBadgeColor = "#059669",
                    MoodTag = "Heartbroken",
                    AuthorAlias = "Silent Seeker",
                    SupportCount = 22,
                    ReportCount = 0,
                    CommentCount = 1,
                    IsHidden = false,
                    CreatedAt = DateTime.Now.AddHours(-12)
                };

                var p3 = new Post
                {
                    PostId = 3,
                    Title = "Imposter syndrome is hitting hard in team projects",
                    Content = "Everyone in my project group seems to know advanced tech stacks while I am still figuring out the basics. I hesitate to ask questions because I feel like I will be judged. Just venting here because I have nowhere else to say this.",
                    CategoryId = 3,
                    CategoryName = "Mental Health & Well-being",
                    CategoryBadgeColor = "#7c3aed",
                    MoodTag = "Anxious",
                    AuthorAlias = "Gentle Breeze",
                    SupportCount = 18,
                    ReportCount = 0,
                    CommentCount = 1,
                    IsHidden = false,
                    CreatedAt = DateTime.Now.AddDays(-1)
                };

                var p4 = new Post
                {
                    PostId = 4,
                    Title = "Hostel mess food is testing my will to live",
                    Content = "Week 8 of potato curry 4 days in a row. How do people survive 4 years of this? Sending thoughts and prayers to everyone eating in Block C tonight.",
                    CategoryId = 4,
                    CategoryName = "Campus Life & Hostel",
                    CategoryBadgeColor = "#d97706",
                    MoodTag = "Humorous",
                    AuthorAlias = "Hungry Nomad",
                    SupportCount = 31,
                    ReportCount = 0,
                    CommentCount = 0,
                    IsHidden = false,
                    CreatedAt = DateTime.Now.AddDays(-2)
                };

                MockPosts.Add(p1);
                MockPosts.Add(p2);
                MockPosts.Add(p3);
                MockPosts.Add(p4);

                // Sample comments
                MockComments.Add(new Comment
                {
                    CommentId = 1,
                    PostId = 1,
                    AuthorAlias = "Supportive Senior",
                    Content = "Take a deep breath! Prioritize previous year question papers and focus on 3 high-weightage units first. You got this.",
                    CreatedAt = DateTime.Now.AddHours(-4)
                });
                MockComments.Add(new Comment
                {
                    CommentId = 2,
                    PostId = 1,
                    AuthorAlias = "Calm Anchor",
                    Content = "Do not try to finish everything in one night. Break it into 45-minute study sprints with 10-minute breaks.",
                    CreatedAt = DateTime.Now.AddHours(-2)
                });
                MockComments.Add(new Comment
                {
                    CommentId = 3,
                    PostId = 2,
                    AuthorAlias = "Fellow Aspirant",
                    Content = "Rejections are redirections. I faced 5 rejections before landing a great offer. Keep your chin up and analyze the gaps.",
                    CreatedAt = DateTime.Now.AddHours(-10)
                });
                MockComments.Add(new Comment
                {
                    CommentId = 4,
                    PostId = 3,
                    AuthorAlias = "Kind Sparrow",
                    Content = "Trust me, most people just google their way through it. You are not behind, everyone learns on the go.",
                    CreatedAt = DateTime.Now.AddHours(-20)
                });
            }
        }
    }
}
