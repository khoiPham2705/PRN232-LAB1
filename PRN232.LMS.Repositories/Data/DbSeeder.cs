using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Data;

/// <summary>Seeds the database with required lab data. Runs only if tables are empty.</summary>
public static class DbSeeder
{
    public static async Task SeedAsync(LmsDbContext context)
    {
        // Apply any pending migrations first
        await context.Database.MigrateAsync();

        // Seed users if empty
        if (!await context.Users.AnyAsync())
        {
            var adminUser = new User
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Role = "Admin"
            };
            var studentUser = new User
            {
                Username = "se231231",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Role = "Student"
            };
            context.Users.AddRange(adminUser, studentUser);
            await context.SaveChangesAsync();
        }

        if (await context.Semesters.AnyAsync()) return; // already seeded

        var rng = new Random(42);

        // ── Semesters ────────────────────────────────────────────────────────────
        var semesters = new List<Semester>
        {
            new() { SemesterName = "Spring 2023", StartDate = new DateTime(2023, 1, 10), EndDate = new DateTime(2023, 5, 20) },
            new() { SemesterName = "Summer 2023", StartDate = new DateTime(2023, 6, 1),  EndDate = new DateTime(2023, 8, 31) },
            new() { SemesterName = "Fall 2023",   StartDate = new DateTime(2023, 9, 5),  EndDate = new DateTime(2024, 1, 15) },
            new() { SemesterName = "Spring 2024", StartDate = new DateTime(2024, 1, 22), EndDate = new DateTime(2024, 5, 25) },
            new() { SemesterName = "Summer 2024", StartDate = new DateTime(2024, 6, 3),  EndDate = new DateTime(2024, 8, 30) },
        };
        context.Semesters.AddRange(semesters);
        await context.SaveChangesAsync();

        // ── Subjects ─────────────────────────────────────────────────────────────
        var subjects = new List<Subject>
        {
            new() { SubjectCode = "PRN211", SubjectName = "Basic Cross-Platform Application Programming",  Credit = 3 },
            new() { SubjectCode = "PRN221", SubjectName = "Advanced Cross-Platform Application Programming", Credit = 3 },
            new() { SubjectCode = "PRN231", SubjectName = "Building Cross-Platform Back-End Application", Credit = 3 },
            new() { SubjectCode = "PRN232", SubjectName = "Building RESTful APIs",                          Credit = 3 },
            new() { SubjectCode = "SWP391", SubjectName = "Application Development Project",               Credit = 5 },
            new() { SubjectCode = "MAD101", SubjectName = "Discrete Mathematics",                          Credit = 3 },
            new() { SubjectCode = "MAE101", SubjectName = "Mathematics for Engineering",                   Credit = 3 },
            new() { SubjectCode = "SSL101", SubjectName = "Academic English 1",                            Credit = 3 },
            new() { SubjectCode = "OSG202", SubjectName = "Operating Systems",                             Credit = 3 },
            new() { SubjectCode = "NWC203", SubjectName = "Computer Networking",                           Credit = 3 },
        };
        context.Subjects.AddRange(subjects);
        await context.SaveChangesAsync();

        // ── Courses (20 total, 4 per semester) ───────────────────────────────────
        var courseNames = new[]
        {
            "Introduction to C#", "Advanced C# Programming", "ASP.NET Core Basics", "RESTful API Design",
            "Database Design", "Algorithms & Data Structures", "Web Development Fundamentals", "Mobile App Development",
            "Software Engineering", "Project Management", "Network Security", "Cloud Computing",
            "DevOps Practices", "UI/UX Design", "Machine Learning Basics", "Data Analytics",
            "Microservices Architecture", "Docker & Kubernetes", "Agile Development", "Business Analysis"
        };

        var courses = new List<Course>();
        for (int i = 0; i < 20; i++)
        {
            courses.Add(new Course
            {
                CourseName = courseNames[i],
                SemesterId = semesters[i / 4].SemesterId
            });
        }
        context.Courses.AddRange(courses);
        await context.SaveChangesAsync();

        // ── Students (50 total) ──────────────────────────────────────────────────
        var firstNames = new[] { "An", "Binh", "Cuong", "Dung", "Em", "Phuong", "Giang", "Hoa", "Lan", "Mai",
                                  "Nam", "Oanh", "Phuc", "Quyen", "Rong", "Son", "Tuan", "Uyen", "Van", "Xuan",
                                  "Yen", "Anh", "Bach", "Chau", "Dat", "Gia", "Ha", "Hung", "Khanh", "Linh" };
        var lastNames  = new[] { "Nguyen", "Tran", "Le", "Pham", "Hoang", "Phan", "Vu", "Dang", "Bui", "Do" };

        var students = new List<Student>();
        for (int i = 1; i <= 50; i++)
        {
            var fn = firstNames[(i - 1) % firstNames.Length];
            var ln = lastNames[(i - 1) % lastNames.Length];
            students.Add(new Student
            {
                FullName    = $"{ln} {fn} {i:D2}",
                Email       = $"student{i:D2}@fpt.edu.vn",
                DateOfBirth = new DateTime(2000 + rng.Next(0, 6), rng.Next(1, 13), rng.Next(1, 28))
            });
        }
        context.Students.AddRange(students);
        await context.SaveChangesAsync();

        // ── Enrollments (500 total, unique student-course pairs) ─────────────────
        var statuses = new[] { "Active", "Completed", "Dropped", "Pending" };
        var usedPairs = new HashSet<(int s, int c)>();
        var enrollments = new List<Enrollment>();

        while (enrollments.Count < 500)
        {
            var student = students[rng.Next(students.Count)];
            var course  = courses[rng.Next(courses.Count)];
            var key = (student.StudentId, course.CourseId);

            if (usedPairs.Contains(key)) continue;
            usedPairs.Add(key);

            // Enroll date within the semester window
            var semester = semesters.First(sem => sem.SemesterId == course.SemesterId);
            var range    = (int)(semester.EndDate - semester.StartDate).TotalDays;
            var enrollDate = semester.StartDate.AddDays(rng.Next(0, Math.Max(range, 1)));

            enrollments.Add(new Enrollment
            {
                StudentId  = student.StudentId,
                CourseId   = course.CourseId,
                EnrollDate = enrollDate,
                Status     = statuses[rng.Next(statuses.Length)]
            });
        }

        context.Enrollments.AddRange(enrollments);
        await context.SaveChangesAsync();
    }
}
