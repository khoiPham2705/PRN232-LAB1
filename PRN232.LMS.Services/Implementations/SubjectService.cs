using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Services.RequestModels;
using PRN232.LMS.Services.ResponseModels;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Helpers;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.Services.Implementations;

public class SubjectService : ISubjectService
{
    private readonly ISubjectRepository _repo;
    public SubjectService(ISubjectRepository repo) => _repo = repo;

    private static SubjectBM ToBusinessModel(Subject e) => new()
    {
        SubjectId   = e.SubjectId,
        SubjectCode = e.SubjectCode,
        SubjectName = e.SubjectName,
        Credit      = e.Credit
    };

    private static SubjectResponse ToResponse(SubjectBM bm) => new()
    {
        SubjectId   = bm.SubjectId,
        SubjectCode = bm.SubjectCode,
        SubjectName = bm.SubjectName,
        Credit      = bm.Credit
    };

    public async Task<PagedApiResponse> GetAllAsync(QueryParams q)
    {
        var result     = await _repo.GetAllAsync(q.Search, q.Sort, q.Page, q.Size);
        var dtos       = result.Items.Select(e => ToResponse(ToBusinessModel(e)));
        var data       = QueryHelper.ApplyFieldSelection(dtos, q.Fields);
        var pagination = PaginationMetadata.Create(q.Page, q.Size, result.TotalCount);
        return PagedApiResponse.Ok(data, pagination);
    }

    public async Task<ApiResponse<SubjectResponse>> GetByIdAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity is null) return ApiResponse<SubjectResponse>.Fail($"Subject with ID {id} not found.");
        return ApiResponse<SubjectResponse>.Ok(ToResponse(ToBusinessModel(entity)));
    }

    public async Task<ApiResponse<SubjectResponse>> CreateAsync(SubjectRequest request)
    {
        var created = await _repo.CreateAsync(new Subject { SubjectCode = request.SubjectCode, SubjectName = request.SubjectName, Credit = request.Credit });
        return ApiResponse<SubjectResponse>.Ok(ToResponse(ToBusinessModel(created)), "Subject created successfully.");
    }

    public async Task<ApiResponse<SubjectResponse>> UpdateAsync(int id, SubjectRequest request)
    {
        var updated = await _repo.UpdateAsync(new Subject { SubjectId = id, SubjectCode = request.SubjectCode, SubjectName = request.SubjectName, Credit = request.Credit });
        if (updated is null) return ApiResponse<SubjectResponse>.Fail($"Subject with ID {id} not found.");
        return ApiResponse<SubjectResponse>.Ok(ToResponse(ToBusinessModel(updated)), "Subject updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var result = await _repo.DeleteAsync(id);
        return result
            ? ApiResponse<bool>.Ok(true, "Subject deleted successfully.")
            : ApiResponse<bool>.Fail($"Subject with ID {id} not found.");
    }
}
