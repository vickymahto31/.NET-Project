# WhisperSpace - Anonymous Student Sharing Platform

> **B.Tech Semester 5 Mini Project — .NET Technologies**  
> **Tech Stack:** C# • ASP.NET Core MVC (.NET 8) • ADO.NET (`Microsoft.Data.SqlClient`) • Microsoft SQL Server • Vanilla CSS/HTML/JS  
> **Repository:** [https://github.com/vickymahto31/.NET-Project](https://github.com/vickymahto31/.NET-Project)

---

## 📌 Project Overview
**WhisperSpace** is an anonymous student sharing and peer-support web platform designed specifically for college campuses. It provides students with a safe, confidential, and judgment-free space to share academic anxieties, placement pressures, hostel experiences, and personal confessions while receiving constructive encouragement from fellow peers.

The application strictly adheres to the course curriculum constraints:
* **Architecture:** ASP.NET Core MVC (Model-View-Controller) with C#
* **Data Access Layer:** Pure **ADO.NET** (`SqlConnection`, `SqlCommand`, `SqlDataReader`, `SqlParameter`) — *No Entity Framework*
* **Database:** Microsoft SQL Server
* **Frontend:** Clean, responsive Vanilla CSS, HTML5, and JavaScript — *No React, Node.js, or external JS frameworks*

---

## 🌟 Key Features

### 1. Home & Discovery Page
* Hero banner with live community statistics (Total Thoughts, Peer Support Hearts, Supportive Replies).
* Category directory showcasing 6 distinct student-focused topics with custom color schemes and badges.
* Recent campus confessions feed with quick access to full discussions.
* Clear Anonymity & Data Protection pledge.

### 2. Anonymous Post Submission
* Post title and long-form rich content area with live character counter.
* Topic category selection and Mood/Emotion indicator (e.g. *Overwhelmed*, *Anxious*, *Seeking Advice*, *Heartbroken*, *Venting*).
* Dynamic pseudonym generator (e.g. *Quiet Owl*, *Curious Penguin*, *Brave Seeker*) allowing students to post under playful random aliases.
* Community safety pledge validation preventing submission without agreement.

### 3. Community Feed & Search
* Filter by Category (Academic Stress, Career & Placements, Mental Health, Hostel Life, etc.).
* Real-time keyword search across post titles and body content using parameterized SQL queries.
* Filter by Mood / Emotion.
* Sorting options: *Latest First*, *Most Supported (❤️)*, and *Most Discussed (💬)*.

### 4. Post Details & Supportive Comments
* Full post view with timestamp, category badge, and emotion tag.
* Peer Support reaction button with instant count increment.
* Threaded supportive anonymous comment replies.
* One-click "Report Post" flag dialog.

### 5. Content Reporting System
* Post flagging modal allowing users to report posts with specific categories:
  * Bullying or Harassment
  * Hate Speech or Abusive Language
  * Revealing Identity (Doxxing)
  * Self-Harm or Crisis Concern
  * Spam or Commercial Promotion
* Flagged posts are forwarded directly to the moderation queue.

### 6. Admin Moderation Console
* Secure session-based authentication with SHA-256 password hashing.
* **Demo Credentials for Viva / Evaluation:**
  * **Username:** `admin`
  * **Password:** `Admin@123`
* Key metrics dashboard: Total Posts, Comments, Pending Reports, Hidden Posts.
* Report resolution actions: *Mark Reviewed*, *Dismiss Report*, *Hide Post from Feed*, or *Permanent Delete*.

---

## 🗄️ Database Schema & ADO.NET Architecture

All database queries are executed using **pure ADO.NET** with parameterized commands to prevent SQL Injection attacks.

### Tables
1. **`Categories`**: `CategoryId`, `Name`, `Description`, `IconClass`, `BadgeColor`
2. **`Posts`**: `PostId`, `Title`, `Content`, `CategoryId`, `MoodTag`, `AuthorAlias`, `SupportCount`, `ReportCount`, `IsHidden`, `CreatedAt`
3. **`Comments`**: `CommentId`, `PostId`, `AuthorAlias`, `Content`, `CreatedAt`, `IsHidden`
4. **`Reports`**: `ReportId`, `PostId`, `Reason`, `DetailedRemarks`, `Status`, `CreatedAt`
5. **`Admins`**: `AdminId`, `Username`, `PasswordHash`, `FullName`, `CreatedAt`

> The complete SQL script with schema definitions, constraints, foreign keys, and seed records is located at:  
> [`Database/AnonymousSharingDB.sql`](Database/AnonymousSharingDB.sql)

---

## 🚀 Running the Project Locally

### Prerequisites
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* Microsoft SQL Server (LocalDB, SQL Express, or standard SQL Server instance)

### Setup Steps
1. **Clone the repository:**
   ```bash
   git clone https://github.com/vickymahto31/.NET-Project.git
   cd .NET-Project
   ```

2. **Configure Database Connection:**
   Open `appsettings.json` and verify the connection string:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AnonymousSharingDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
   }
   ```
   *(If SQL Server is not running on your machine, WhisperSpace automatically boots in resilient in-memory ADO.NET mode with seed data, ensuring hassle-free demo during viva).*

3. **(Optional) Run SQL Script in SQL Server Management Studio (SSMS):**
   Open and execute [`Database/AnonymousSharingDB.sql`](Database/AnonymousSharingDB.sql) to create the database and seed records.

4. **Run the Application:**
   ```bash
   dotnet build
   dotnet run
   ```

5. **Open in Browser:**
   Navigate to `http://localhost:5000` (or the URL displayed in the terminal).

---

## 🎓 College Viva & Evaluation Q&A

**Q1: Why was ADO.NET used instead of Entity Framework?**  
*Answer:* ADO.NET gives complete low-level control over SQL execution, optimizes performance by eliminating ORM overhead, and is a mandatory learning objective for B.Tech .NET Technologies curriculum to demonstrate direct database connectivity using `SqlConnection`, `SqlCommand`, `SqlDataReader`, and `SqlParameter`.

**Q2: How is SQL Injection prevented?**  
*Answer:* All database commands strictly use parameterized `SqlParameter` objects with explicitly defined `SqlDbType` data types. Dynamic string concatenation is completely avoided.

**Q3: How does the application preserve user anonymity?**  
*Answer:* The application does not store student login credentials, roll numbers, IP addresses, or browser fingerprints with posts. Each post is generated with an independent alias (e.g., "Quiet Owl").

**Q4: What happens if a harmful or threatening post is submitted?**  
*Answer:* Any user can click the "Report" button on a post. The post is flagged in the database and immediately appears in the Admin Moderation Dashboard (`/Admin`) where moderators can review, hide, or permanently delete the post.

---

## 👨‍💻 Author & Submission Details
* **Student Name:** Vicky Mahto
* **Course:** B.Tech Computer Science & Engineering
* **Semester:** Semester 5
* **Subject:** .NET Technologies (Mini Project)
