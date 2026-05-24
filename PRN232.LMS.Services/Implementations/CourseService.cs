using PRN232.LMS.Models.BusinessModels;
using PRN232.LMS.Models.Entities;
using PRN232.LMS.Models.RequestModels;
using PRN232.LMS.Models.ResponseModels;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Helpers;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.Services.Implementations;

public class CourseService : ICourseService
{
    private readonly ICourseRepository    _repo;
    private readonly ISemesterRepository  _semesterRepo;

    public CourseService(ICourseRepository repo, ISemesterRepository semesterRepo)
    {
        _repo        = repo;
        _semesterRepo = semesterRepo;
    }

    private static CourseBM ToBusinessModel(Course e) => new()
    {
        CourseId        = e.CourseId,
        CourseName      = e.CourseName,
        SemesterId      = e.SemesterId,
        SemesterName    = e.Semester?.SemesterName,
        EnrollmentCount = e.Enrollments?.Count ?? 0
    };

    private static CourseResponse ToResponse(CourseBM bm) => new()
    {
        CourseId        = bm.CourseId,
        CourseName      = bm.CourseName,
        SemesterId      = bm.SemesterId,
        SemesterName    = bm.SemesterName,
        EnrollmentCount = bm.EnrollmentCount
    };

    public async Task<PagedApiResponse> GetAllAsync(QueryParams q, int? semesterId = null)
    {
        bool expandSemester = q.HasExpand("semester");
        var result     = await _repo.GetAllAsync(q.Search, q.Sort, q.Page, q.Size, semesterId, expandSemester);
        var dtos       = result.Items.Select(e => ToResponse(ToBusinessModel(e)));
        var data       = QueryHelper.ApplyFieldSelection(dtos, q.Fields);
        var pagination = PaginationMetadata.Create(q.Page, q.Size, result.TotalCount);
        return PagedApiResponse.Ok(data, pagination);
    }

    public async Task<ApiResponse<CourseResponse>> GetByIdAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity is null) return ApiResponse<CourseResponse>.Fail($"Course with ID {id} not found.");
        var response = ToResponse(ToBusinessModel(entity));
        response.Enrollments = entity.Enrollments?.Select(e => new StudentEnrollmentSummaryResponse
        {
            EnrollmentId    = e.EnrollmentId,
            StudentId       = e.StudentId,
            StudentFullName = e.Student?.FullName,
            StudentEmail    = e.Student?.Email,
            EnrollDate      = e.EnrollDate,
            Status          = e.Status
        }).ToList();
        return ApiResponse<CourseResponse>.Ok(response);
    }

    public async Task<ApiResponse<CourseResponse>> CreateAsync(CourseRequest request)
    {
        if (!await _semesterRepo.ExistsAsync(request.SemesterId))
            return ApiResponse<CourseResponse>.Fail($"Semester with ID {request.SemesterId} does not exist.");
        var created = await _repo.CreateAsync(new Course { CourseName = request.CourseName, SemesterId = request.SemesterId });
        var full    = await _repo.GetByIdAsync(created.CourseId);
        return ApiResponse<CourseResponse>.Ok(ToResponse(ToBusinessModel(full!)), "Course created successfully.");
    }

    public async Task<ApiResponse<CourseResponse>> UpdateAsync(int id, CourseRequest request)
    {
        if (!await _semesterRepo.ExistsAsync(request.SemesterId))
            return ApiResponse<CourseResponse>.Fail($"Semester with ID {request.SemesterId} does not exist.");
        var updated = await _repo.UpdateAsync(new Course { CourseId = id, CourseName = request.CourseName, SemesterId = request.SemesterId });
        if (updated is null) return ApiResponse<CourseResponse>.Fail($"Course with ID {id} not found.");
        var full = await _repo.GetByIdAsync(id);
        return ApiResponse<CourseResponse>.Ok(ToResponse(ToBusinessModel(full!)), "Course updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var result = await _repo.DeleteAsync(id);
        return result
            ? ApiResponse<bool>.Ok(true, "Course deleted successfully.")
            : ApiResponse<bool>.Fail($"Course with ID {id} not found.");
    }
}
