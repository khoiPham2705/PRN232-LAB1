using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Implementations;

public class CourseRepository : ICourseRepository
{
    private readonly LmsDbContext _context;
    public CourseRepository(LmsDbContext context) => _context = context;

    public async Task<PagedResult<Course>> GetAllAsync(
        string? search, string? sort, int page, int size,
        int? semesterId = null, bool expandSemester = false)
    {
        var query = _context.Courses
            .Include(c => c.Semester)
            .Include(c => c.Enrollments)
            .AsNoTracking();

        // Filter by semester
        if (semesterId.HasValue)
            query = query.Where(c => c.SemesterId == semesterId.Value);

        // Search
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.CourseName.Contains(search) ||
                                     c.Semester.SemesterName.Contains(search));
        // Sort
        query = ApplySort(query, sort);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();
        return new PagedResult<Course>(items, total);
    }

    private static IQueryable<Course> ApplySort(IQueryable<Course> q, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort)) return q.OrderBy(c => c.CourseId);
        IOrderedQueryable<Course>? ordered = null;
        foreach (var part in sort.Split(',').Select(p => p.Trim()))
        {
            var desc  = part.StartsWith('-');
            var field = (desc ? part[1..] : part).ToLower();
            if (ordered is null)
                ordered = (field, desc) switch
                {
                    ("coursename", false) => q.OrderBy(c => c.CourseName),
                    ("coursename", true)  => q.OrderByDescending(c => c.CourseName),
                    ("semesterid", false) => q.OrderBy(c => c.SemesterId),
                    ("semesterid", true)  => q.OrderByDescending(c => c.SemesterId),
                    _                     => q.OrderBy(c => c.CourseId)
                };
            else
                ordered = (field, desc) switch
                {
                    ("coursename", false) => ordered.ThenBy(c => c.CourseName),
                    ("coursename", true)  => ordered.ThenByDescending(c => c.CourseName),
                    _                     => ordered.ThenBy(c => c.CourseId)
                };
        }
        return ordered ?? q.OrderBy(c => c.CourseId);
    }

    public async Task<Course?> GetByIdAsync(int id)
        => await _context.Courses
               .Include(c => c.Semester)
               .Include(c => c.Enrollments).ThenInclude(e => e.Student)
               .AsNoTracking().FirstOrDefaultAsync(c => c.CourseId == id);

    public async Task<Course> CreateAsync(Course course)
    {
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();
        return course;
    }

    public async Task<Course?> UpdateAsync(Course course)
    {
        var existing = await _context.Courses.FindAsync(course.CourseId);
        if (existing is null) return null;
        existing.CourseName = course.CourseName;
        existing.SemesterId = course.SemesterId;
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.Courses.FindAsync(id);
        if (entity is null) return false;
        _context.Courses.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
        => await _context.Courses.AnyAsync(c => c.CourseId == id);
}
