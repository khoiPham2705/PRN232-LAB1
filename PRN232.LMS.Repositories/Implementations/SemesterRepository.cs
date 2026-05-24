using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Models.Entities;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Implementations;

public class SemesterRepository : ISemesterRepository
{
    private readonly LmsDbContext _context;
    public SemesterRepository(LmsDbContext context) => _context = context;

    public async Task<PagedResult<Semester>> GetAllAsync(string? search, string? sort, int page, int size)
    {
        var query = _context.Semesters.Include(s => s.Courses).AsNoTracking();

        // Search
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(s => s.SemesterName.Contains(search));

        // Sort
        query = ApplySort(query, sort);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();
        return new PagedResult<Semester>(items, total);
    }

    private static IQueryable<Semester> ApplySort(IQueryable<Semester> q, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort)) return q.OrderBy(s => s.SemesterId);
        IOrderedQueryable<Semester>? ordered = null;
        foreach (var part in sort.Split(',').Select(p => p.Trim()))
        {
            var desc  = part.StartsWith('-');
            var field = (desc ? part[1..] : part).ToLower();
            if (ordered is null)
                ordered = (field, desc) switch
                {
                    ("semestername", false) => q.OrderBy(s => s.SemesterName),
                    ("semestername", true)  => q.OrderByDescending(s => s.SemesterName),
                    ("startdate",    false) => q.OrderBy(s => s.StartDate),
                    ("startdate",    true)  => q.OrderByDescending(s => s.StartDate),
                    ("enddate",      false) => q.OrderBy(s => s.EndDate),
                    ("enddate",      true)  => q.OrderByDescending(s => s.EndDate),
                    _                       => q.OrderBy(s => s.SemesterId)
                };
            else
                ordered = (field, desc) switch
                {
                    ("semestername", false) => ordered.ThenBy(s => s.SemesterName),
                    ("semestername", true)  => ordered.ThenByDescending(s => s.SemesterName),
                    ("startdate",    false) => ordered.ThenBy(s => s.StartDate),
                    ("startdate",    true)  => ordered.ThenByDescending(s => s.StartDate),
                    _                       => ordered.ThenBy(s => s.SemesterId)
                };
        }
        return ordered ?? q.OrderBy(s => s.SemesterId);
    }

    public async Task<Semester?> GetByIdAsync(int id)
        => await _context.Semesters
               .Include(s => s.Courses).ThenInclude(c => c.Enrollments)
               .AsNoTracking().FirstOrDefaultAsync(s => s.SemesterId == id);

    public async Task<Semester> CreateAsync(Semester semester)
    {
        _context.Semesters.Add(semester);
        await _context.SaveChangesAsync();
        return semester;
    }

    public async Task<Semester?> UpdateAsync(Semester semester)
    {
        var existing = await _context.Semesters.FindAsync(semester.SemesterId);
        if (existing is null) return null;
        existing.SemesterName = semester.SemesterName;
        existing.StartDate    = semester.StartDate;
        existing.EndDate      = semester.EndDate;
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.Semesters.FindAsync(id);
        if (entity is null) return false;
        _context.Semesters.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
        => await _context.Semesters.AnyAsync(s => s.SemesterId == id);
}
