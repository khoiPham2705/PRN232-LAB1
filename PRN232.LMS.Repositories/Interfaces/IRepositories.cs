using PRN232.LMS.Models.Entities;

namespace PRN232.LMS.Repositories.Interfaces;

// Repository return type for paged queries (not a Request/Response model – lives in Repositories)
public record PagedResult<T>(IEnumerable<T> Items, int TotalCount);

public interface ISemesterRepository
{
    Task<PagedResult<Semester>> GetAllAsync(string? search, string? sort, int page, int size);
    Task<Semester?> GetByIdAsync(int id);
    Task<Semester> CreateAsync(Semester semester);
    Task<Semester?> UpdateAsync(Semester semester);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

public interface ICourseRepository
{
    Task<PagedResult<Course>> GetAllAsync(string? search, string? sort, int page, int size, int? semesterId = null, bool expandSemester = false);
    Task<Course?> GetByIdAsync(int id);
    Task<Course> CreateAsync(Course course);
    Task<Course?> UpdateAsync(Course course);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

public interface ISubjectRepository
{
    Task<PagedResult<Subject>> GetAllAsync(string? search, string? sort, int page, int size);
    Task<Subject?> GetByIdAsync(int id);
    Task<Subject> CreateAsync(Subject subject);
    Task<Subject?> UpdateAsync(Subject subject);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

public interface IStudentRepository
{
    Task<PagedResult<Student>> GetAllAsync(string? search, string? sort, int page, int size, bool expandEnrollments = false);
    Task<Student?> GetByIdAsync(int id);
    Task<Student> CreateAsync(Student student);
    Task<Student?> UpdateAsync(Student student);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

public interface IEnrollmentRepository
{
    Task<PagedResult<Enrollment>> GetAllAsync(string? search, string? sort, int page, int size,
        int? studentId = null, int? courseId = null, bool expandStudent = false, bool expandCourse = false);
    Task<Enrollment?> GetByIdAsync(int id);
    Task<Enrollment> CreateAsync(Enrollment enrollment);
    Task<Enrollment?> UpdateAsync(Enrollment enrollment);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
