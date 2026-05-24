using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Models.Entities;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Implementations;

public class StudentRepository : IStudentRepository
{
    private readonly LmsDbContext _context;
    public StudentRepository(LmsDbContext context) => _context = context;

    public async Task<PagedResult<Student>> GetAllAsync(
        string? search, string? sort, int page, int size, bool expandEnrollments = false)
    {
        IQueryable<Student> query = _context.Students.AsNoTracking();

        // Conditionally include enrollments (expand)
        query = expandEnrollments
            ? query.Include(s => s.Enrollments).ThenInclude(e => e.Course).ThenInclude(c => c.Semester)
            : query.Include(s => s.Enrollments);   // minimal – just for count

        // Search: fullName or email
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(s => s.FullName.Contains(search) ||
                                     s.Email.Contains(search));
        // Sort
        query = ApplySort(query, sort);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();
        return new PagedResult<Student>(items, total);
    }

    private static IQueryable<Student> ApplySort(IQueryable<Student> q, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort)) return q.OrderBy(s => s.StudentId);
        IOrderedQueryable<Student>? ordered = null;
        foreach (var part in sort.Split(',').Select(p => p.Trim()))
        {
            var desc  = part.StartsWith('-');
            var field = (desc ? part[1..] : part).ToLower();
            if (ordered is null)
                ordered = (field, desc) switch
                {
                    ("fullname",    false) => q.OrderBy(s => s.FullName),
                    ("fullname",    true)  => q.OrderByDescending(s => s.FullName),
                    ("email",       false) => q.OrderBy(s => s.Email),
                    ("email",       true)  => q.OrderByDescending(s => s.Email),
                    ("dateofbirth", false) => q.OrderBy(s => s.DateOfBirth),
                    ("dateofbirth", true)  => q.OrderByDescending(s => s.DateOfBirth),
                    _                      => q.OrderBy(s => s.StudentId)
                };
            else
                ordered = (field, desc) switch
                {
                    ("fullname",    false) => ordered.ThenBy(s => s.FullName),
                    ("fullname",    true)  => ordered.ThenByDescending(s => s.FullName),
                    ("email",       false) => ordered.ThenBy(s => s.Email),
                    ("email",       true)  => ordered.ThenByDescending(s => s.Email),
                    ("dateofbirth", false) => ordered.ThenBy(s => s.DateOfBirth),
                    ("dateofbirth", true)  => ordered.ThenByDescending(s => s.DateOfBirth),
                    _                      => ordered.ThenBy(s => s.StudentId)
                };
        }
        return ordered ?? q.OrderBy(s => s.StudentId);
    }

    public async Task<Student?> GetByIdAsync(int id)
        => await _context.Students
               .Include(s => s.Enrollments).ThenInclude(e => e.Course).ThenInclude(c => c.Semester)
               .AsNoTracking().FirstOrDefaultAsync(s => s.StudentId == id);

    public async Task<Student> CreateAsync(Student student)
    {
        _context.Students.Add(student);
        await _context.SaveChangesAsync();
        return student;
    }

    public async Task<Student?> UpdateAsync(Student student)
    {
        var existing = await _context.Students.FindAsync(student.StudentId);
        if (existing is null) return null;
        existing.FullName    = student.FullName;
        existing.Email       = student.Email;
        existing.DateOfBirth = student.DateOfBirth;
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.Students.FindAsync(id);
        if (entity is null) return false;
        _context.Students.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
        => await _context.Students.AnyAsync(s => s.StudentId == id);
}
