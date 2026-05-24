using PRN232.LMS.Models.BusinessModels;
using PRN232.LMS.Models.Entities;
using PRN232.LMS.Models.RequestModels;
using PRN232.LMS.Models.ResponseModels;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Helpers;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.Services.Implementations;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _repo;
    private readonly IStudentRepository    _studentRepo;
    private readonly ICourseRepository     _courseRepo;

    public EnrollmentService(IEnrollmentRepository repo, IStudentRepository studentRepo, ICourseRepository courseRepo)
    {
        _repo        = repo;
        _studentRepo = studentRepo;
        _courseRepo  = courseRepo;
    }

    private static EnrollmentBM ToBusinessModel(Enrollment e) => new()
    {
        EnrollmentId    = e.EnrollmentId,
        StudentId       = e.StudentId,
        StudentFullName = e.Student?.FullName,
        CourseId        = e.CourseId,
        CourseName      = e.Course?.CourseName,
        EnrollDate      = e.EnrollDate,
        Status          = e.Status
    };

    private static EnrollmentResponse ToResponse(EnrollmentBM bm) => new()
    {
        EnrollmentId    = bm.EnrollmentId,
        StudentId       = bm.StudentId,
        StudentFullName = bm.StudentFullName,
        CourseId        = bm.CourseId,
        CourseName      = bm.CourseName,
        EnrollDate      = bm.EnrollDate,
        Status          = bm.Status
    };

    private static EnrollmentResponse MapFull(Enrollment e)
    {
        var r = ToResponse(ToBusinessModel(e));
        r.StudentEmail = e.Student?.Email;
        r.SemesterId   = e.Course?.SemesterId ?? 0;
        r.SemesterName = e.Course?.Semester?.SemesterName;
        return r;
    }

    public async Task<PagedApiResponse> GetAllAsync(QueryParams q, int? studentId = null, int? courseId = null)
    {
        bool expandStudent = q.HasExpand("student");
        bool expandCourse  = q.HasExpand("course");

        var result = await _repo.GetAllAsync(q.Search, q.Sort, q.Page, q.Size,
            studentId, courseId, expandStudent, expandCourse);

        var dtos = result.Items.Select(MapFull);
        var data       = QueryHelper.ApplyFieldSelection(dtos, q.Fields);
        var pagination = PaginationMetadata.Create(q.Page, q.Size, result.TotalCount);
        return PagedApiResponse.Ok(data, pagination);
    }

    public async Task<ApiResponse<EnrollmentResponse>> GetByIdAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity is null) return ApiResponse<EnrollmentResponse>.Fail($"Enrollment with ID {id} not found.");
        return ApiResponse<EnrollmentResponse>.Ok(MapFull(entity));
    }

    public async Task<ApiResponse<EnrollmentResponse>> CreateAsync(EnrollmentRequest request)
    {
        if (!await _studentRepo.ExistsAsync(request.StudentId))
            return ApiResponse<EnrollmentResponse>.Fail($"Student with ID {request.StudentId} does not exist.");
        if (!await _courseRepo.ExistsAsync(request.CourseId))
            return ApiResponse<EnrollmentResponse>.Fail($"Course with ID {request.CourseId} does not exist.");

        var created = await _repo.CreateAsync(new Enrollment
        {
            StudentId  = request.StudentId,
            CourseId   = request.CourseId,
            EnrollDate = request.EnrollDate,
            Status     = request.Status
        });
        var full = await _repo.GetByIdAsync(created.EnrollmentId);
        return ApiResponse<EnrollmentResponse>.Ok(MapFull(full!), "Enrollment created successfully.");
    }

    public async Task<ApiResponse<EnrollmentResponse>> UpdateAsync(int id, EnrollmentRequest request)
    {
        if (!await _studentRepo.ExistsAsync(request.StudentId))
            return ApiResponse<EnrollmentResponse>.Fail($"Student with ID {request.StudentId} does not exist.");
        if (!await _courseRepo.ExistsAsync(request.CourseId))
            return ApiResponse<EnrollmentResponse>.Fail($"Course with ID {request.CourseId} does not exist.");

        var updated = await _repo.UpdateAsync(new Enrollment
        {
            EnrollmentId = id,
            StudentId    = request.StudentId,
            CourseId     = request.CourseId,
            EnrollDate   = request.EnrollDate,
            Status       = request.Status
        });
        if (updated is null) return ApiResponse<EnrollmentResponse>.Fail($"Enrollment with ID {id} not found.");
        var full = await _repo.GetByIdAsync(id);
        return ApiResponse<EnrollmentResponse>.Ok(MapFull(full!), "Enrollment updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var result = await _repo.DeleteAsync(id);
        return result
            ? ApiResponse<bool>.Ok(true, "Enrollment deleted successfully.")
            : ApiResponse<bool>.Fail($"Enrollment with ID {id} not found.");
    }
}
