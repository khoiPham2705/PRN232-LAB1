using PRN232.LMS.Models.BusinessModels;
using PRN232.LMS.Models.Entities;
using PRN232.LMS.Models.RequestModels;
using PRN232.LMS.Models.ResponseModels;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Helpers;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.Services.Implementations;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _repo;
    public StudentService(IStudentRepository repo) => _repo = repo;

    private static StudentBM ToBusinessModel(Student e) => new()
    {
        StudentId       = e.StudentId,
        FullName        = e.FullName,
        Email           = e.Email,
        DateOfBirth     = e.DateOfBirth,
        EnrollmentCount = e.Enrollments?.Count ?? 0
    };

    private static StudentResponse ToResponse(StudentBM bm) => new()
    {
        StudentId       = bm.StudentId,
        FullName        = bm.FullName,
        Email           = bm.Email,
        DateOfBirth     = bm.DateOfBirth,
        EnrollmentCount = bm.EnrollmentCount
    };

    public async Task<PagedApiResponse> GetAllAsync(QueryParams q)
    {
        bool expandEnrollments = q.HasExpand("enrollments");
        var result     = await _repo.GetAllAsync(q.Search, q.Sort, q.Page, q.Size, expandEnrollments);
        var dtos       = result.Items.Select(e =>
        {
            var r = ToResponse(ToBusinessModel(e));
            // Populate nested enrollments only when expand=enrollments
            if (expandEnrollments)
                r.Enrollments = e.Enrollments?.Select(en => new EnrollmentSummaryResponse
                {
                    EnrollmentId = en.EnrollmentId,
                    CourseId     = en.CourseId,
                    CourseName   = en.Course?.CourseName,
                    SemesterName = en.Course?.Semester?.SemesterName,
                    EnrollDate   = en.EnrollDate,
                    Status       = en.Status
                }).ToList();
            return r;
        });
        var data       = QueryHelper.ApplyFieldSelection(dtos, q.Fields);
        var pagination = PaginationMetadata.Create(q.Page, q.Size, result.TotalCount);
        return PagedApiResponse.Ok(data, pagination);
    }

    public async Task<ApiResponse<StudentResponse>> GetByIdAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity is null) return ApiResponse<StudentResponse>.Fail($"Student with ID {id} not found.");
        var response = ToResponse(ToBusinessModel(entity));
        response.Enrollments = entity.Enrollments?.Select(e => new EnrollmentSummaryResponse
        {
            EnrollmentId = e.EnrollmentId,
            CourseId     = e.CourseId,
            CourseName   = e.Course?.CourseName,
            SemesterName = e.Course?.Semester?.SemesterName,
            EnrollDate   = e.EnrollDate,
            Status       = e.Status
        }).ToList();
        return ApiResponse<StudentResponse>.Ok(response);
    }

    public async Task<ApiResponse<StudentResponse>> CreateAsync(StudentRequest request)
    {
        var created = await _repo.CreateAsync(new Student { FullName = request.FullName, Email = request.Email, DateOfBirth = request.DateOfBirth });
        return ApiResponse<StudentResponse>.Ok(ToResponse(ToBusinessModel(created)), "Student created successfully.");
    }

    public async Task<ApiResponse<StudentResponse>> UpdateAsync(int id, StudentRequest request)
    {
        var updated = await _repo.UpdateAsync(new Student { StudentId = id, FullName = request.FullName, Email = request.Email, DateOfBirth = request.DateOfBirth });
        if (updated is null) return ApiResponse<StudentResponse>.Fail($"Student with ID {id} not found.");
        return ApiResponse<StudentResponse>.Ok(ToResponse(ToBusinessModel(updated)), "Student updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var result = await _repo.DeleteAsync(id);
        return result
            ? ApiResponse<bool>.Ok(true, "Student deleted successfully.")
            : ApiResponse<bool>.Fail($"Student with ID {id} not found.");
    }
}
