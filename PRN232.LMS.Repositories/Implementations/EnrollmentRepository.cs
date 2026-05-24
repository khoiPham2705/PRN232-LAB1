using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Models.Entities;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Implementations;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly LmsDbContext _context;
    public EnrollmentRepository(LmsDbContext context) => _context = context;

    public async Task<PagedResult<Enrollment>> GetAllAsync(
        string? search, string? sort, int page, int size,
        int? studentId = null, int? courseId = null,
        bool expandStudent = false, bool expandCourse = false)
    {
        // Always include enough for basic response fields
        var query = _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course).ThenInclude(c => c.Semester)
            .AsNoTracking();

        // Filters
        if (studentId.HasValue) query = query.Where(e => e.StudentId == studentId.Value);
        if (courseId.HasValue)  query = query.Where(e => e.CourseId  == courseId.Value);

        // Search: status or student name
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(e => e.Status.Contains(search) ||
                                     e.Student.FullName.Contains(search) ||
                                     e.Course.CourseName.Contains(search));
        // Sort
        query = ApplySort(query, sort);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();
        return new PagedResult<Enrollment>(items, total);
    }

    private static IQueryable<Enrollment> ApplySort(IQueryable<Enrollment> q, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort)) return q.OrderBy(e => e.EnrollmentId);
        IOrderedQueryable<Enrollment>? ordered = null;
        foreach (var part in sort.Split(',').Select(p => p.Trim()))
        {
            var desc  = part.StartsWith('-');
            var field = (desc ? part[1..] : part).ToLower();
            if (ordered is null)
                ordered = (field, desc) switch
                {
                    ("enrolldate", false) => q.OrderBy(e => e.EnrollDate),
                    ("enrolldate", true)  => q.OrderByDescending(e => e.EnrollDate),
                    ("status",     false) => q.OrderBy(e => e.Status),
                    ("status",     true)  => q.OrderByDescending(e => e.Status),
                    ("studentid",  false) => q.OrderBy(e => e.StudentId),
                    ("studentid",  true)  => q.OrderByDescending(e => e.StudentId),
                    ("courseid",   false) => q.OrderBy(e => e.CourseId),
                    ("courseid",   true)  => q.OrderByDescending(e => e.CourseId),
                    _                     => q.OrderBy(e => e.EnrollmentId)
                };
            else
                ordered = (field, desc) switch
                {
                    ("enrolldate", false) => ordered.ThenBy(e => e.EnrollDate),
                    ("enrolldate", true)  => ordered.ThenByDescending(e => e.EnrollDate),
                    ("status",     false) => ordered.ThenBy(e => e.Status),
                    ("status",     true)  => ordered.ThenByDescending(e => e.Status),
                    _                     => ordered.ThenBy(e => e.EnrollmentId)
                };
        }
        return ordered ?? q.OrderBy(e => e.EnrollmentId);
    }

    public async Task<Enrollment?> GetByIdAsync(int id)
        => await _context.Enrollments
               .Include(e => e.Student)
               .Include(e => e.Course).ThenInclude(c => c.Semester)
               .AsNoTracking().FirstOrDefaultAsync(e => e.EnrollmentId == id);

    public async Task<Enrollment> CreateAsync(Enrollment enrollment)
    {
        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();
        return enrollment;
    }

    public async Task<Enrollment?> UpdateAsync(Enrollment enrollment)
    {
        var existing = await _context.Enrollments.FindAsync(enrollment.EnrollmentId);
        if (existing is null) return null;
        existing.StudentId  = enrollment.StudentId;
        existing.CourseId   = enrollment.CourseId;
        existing.EnrollDate = enrollment.EnrollDate;
        existing.Status     = enrollment.Status;
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.Enrollments.FindAsync(id);
        if (entity is null) return false;
        _context.Enrollments.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
        => await _context.Enrollments.AnyAsync(e => e.EnrollmentId == id);
}
