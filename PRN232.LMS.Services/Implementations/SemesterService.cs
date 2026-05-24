using PRN232.LMS.Models.BusinessModels;
using PRN232.LMS.Models.Entities;
using PRN232.LMS.Models.RequestModels;
using PRN232.LMS.Models.ResponseModels;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Helpers;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.Services.Implementations;

public class SemesterService : ISemesterService
{
    private readonly ISemesterRepository _repo;
    public SemesterService(ISemesterRepository repo) => _repo = repo;

    private static SemesterBM ToBusinessModel(Semester e) => new()
    {
        SemesterId   = e.SemesterId,
        SemesterName = e.SemesterName,
        StartDate    = e.StartDate,
        EndDate      = e.EndDate,
        CourseCount  = e.Courses?.Count ?? 0
    };

    private static SemesterResponse ToResponse(SemesterBM bm) => new()
    {
        SemesterId   = bm.SemesterId,
        SemesterName = bm.SemesterName,
        StartDate    = bm.StartDate,
        EndDate      = bm.EndDate,
        CourseCount  = bm.CourseCount
    };

    public async Task<PagedApiResponse> GetAllAsync(QueryParams q)
    {
        var result   = await _repo.GetAllAsync(q.Search, q.Sort, q.Page, q.Size);
        var dtos     = result.Items.Select(e => ToResponse(ToBusinessModel(e)));
        var data     = QueryHelper.ApplyFieldSelection(dtos, q.Fields);
        var pagination = PaginationMetadata.Create(q.Page, q.Size, result.TotalCount);
        return PagedApiResponse.Ok(data, pagination);
    }

    public async Task<ApiResponse<SemesterResponse>> GetByIdAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity is null)
            return ApiResponse<SemesterResponse>.Fail($"Semester with ID {id} not found.");
        var response = ToResponse(ToBusinessModel(entity));
        response.Courses = entity.Courses?.Select(c => new CourseSummaryResponse
        {
            CourseId        = c.CourseId,
            CourseName      = c.CourseName,
            EnrollmentCount = c.Enrollments?.Count ?? 0
        }).ToList();
        return ApiResponse<SemesterResponse>.Ok(response);
    }

    public async Task<ApiResponse<SemesterResponse>> CreateAsync(SemesterRequest request)
    {
        var entity  = new Semester { SemesterName = request.SemesterName, StartDate = request.StartDate, EndDate = request.EndDate };
        var created = await _repo.CreateAsync(entity);
        return ApiResponse<SemesterResponse>.Ok(ToResponse(ToBusinessModel(created)), "Semester created successfully.");
    }

    public async Task<ApiResponse<SemesterResponse>> UpdateAsync(int id, SemesterRequest request)
    {
        var entity  = new Semester { SemesterId = id, SemesterName = request.SemesterName, StartDate = request.StartDate, EndDate = request.EndDate };
        var updated = await _repo.UpdateAsync(entity);
        if (updated is null) return ApiResponse<SemesterResponse>.Fail($"Semester with ID {id} not found.");
        return ApiResponse<SemesterResponse>.Ok(ToResponse(ToBusinessModel(updated)), "Semester updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var result = await _repo.DeleteAsync(id);
        return result
            ? ApiResponse<bool>.Ok(true, "Semester deleted successfully.")
            : ApiResponse<bool>.Fail($"Semester with ID {id} not found.");
    }
}
