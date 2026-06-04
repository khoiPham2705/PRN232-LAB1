using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Implementations;

public class SubjectRepository : ISubjectRepository
{
    private readonly LmsDbContext _context;
    public SubjectRepository(LmsDbContext context) => _context = context;

    public async Task<PagedResult<Subject>> GetAllAsync(string? search, string? sort, int page, int size)
    {
        var query = _context.Subjects.AsNoTracking();

        // Search
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(s => s.SubjectCode.Contains(search) ||
                                     s.SubjectName.Contains(search));
        // Sort
        query = ApplySort(query, sort);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();
        return new PagedResult<Subject>(items, total);
    }

    private static IQueryable<Subject> ApplySort(IQueryable<Subject> q, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort)) return q.OrderBy(s => s.SubjectId);
        IOrderedQueryable<Subject>? ordered = null;
        foreach (var part in sort.Split(',').Select(p => p.Trim()))
        {
            var desc  = part.StartsWith('-');
            var field = (desc ? part[1..] : part).ToLower();
            if (ordered is null)
                ordered = (field, desc) switch
                {
                    ("subjectcode", false) => q.OrderBy(s => s.SubjectCode),
                    ("subjectcode", true)  => q.OrderByDescending(s => s.SubjectCode),
                    ("subjectname", false) => q.OrderBy(s => s.SubjectName),
                    ("subjectname", true)  => q.OrderByDescending(s => s.SubjectName),
                    ("credit",      false) => q.OrderBy(s => s.Credit),
                    ("credit",      true)  => q.OrderByDescending(s => s.Credit),
                    _                      => q.OrderBy(s => s.SubjectId)
                };
            else
                ordered = (field, desc) switch
                {
                    ("subjectcode", false) => ordered.ThenBy(s => s.SubjectCode),
                    ("subjectcode", true)  => ordered.ThenByDescending(s => s.SubjectCode),
                    ("subjectname", false) => ordered.ThenBy(s => s.SubjectName),
                    ("subjectname", true)  => ordered.ThenByDescending(s => s.SubjectName),
                    _                      => ordered.ThenBy(s => s.SubjectId)
                };
        }
        return ordered ?? q.OrderBy(s => s.SubjectId);
    }

    public async Task<Subject?> GetByIdAsync(int id)
        => await _context.Subjects.AsNoTracking().FirstOrDefaultAsync(s => s.SubjectId == id);

    public async Task<Subject> CreateAsync(Subject subject)
    {
        _context.Subjects.Add(subject);
        await _context.SaveChangesAsync();
        return subject;
    }

    public async Task<Subject?> UpdateAsync(Subject subject)
    {
        var existing = await _context.Subjects.FindAsync(subject.SubjectId);
        if (existing is null) return null;
        existing.SubjectCode = subject.SubjectCode;
        existing.SubjectName = subject.SubjectName;
        existing.Credit      = subject.Credit;
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.Subjects.FindAsync(id);
        if (entity is null) return false;
        _context.Subjects.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
        => await _context.Subjects.AnyAsync(s => s.SubjectId == id);
}
