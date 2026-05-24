using PRN232.LMS.Models.RequestModels;
using PRN232.LMS.Models.ResponseModels;

namespace PRN232.LMS.Services.Interfaces;

public interface ISemesterService
{
    Task<PagedApiResponse> GetAllAsync(QueryParams q);
    Task<ApiResponse<SemesterResponse>> GetByIdAsync(int id);
    Task<ApiResponse<SemesterResponse>> CreateAsync(SemesterRequest request);
    Task<ApiResponse<SemesterResponse>> UpdateAsync(int id, SemesterRequest request);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}

public interface ICourseService
{
    Task<PagedApiResponse> GetAllAsync(QueryParams q, int? semesterId = null);
    Task<ApiResponse<CourseResponse>> GetByIdAsync(int id);
    Task<ApiResponse<CourseResponse>> CreateAsync(CourseRequest request);
    Task<ApiResponse<CourseResponse>> UpdateAsync(int id, CourseRequest request);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}

public interface ISubjectService
{
    Task<PagedApiResponse> GetAllAsync(QueryParams q);
    Task<ApiResponse<SubjectResponse>> GetByIdAsync(int id);
    Task<ApiResponse<SubjectResponse>> CreateAsync(SubjectRequest request);
    Task<ApiResponse<SubjectResponse>> UpdateAsync(int id, SubjectRequest request);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}

public interface IStudentService
{
    Task<PagedApiResponse> GetAllAsync(QueryParams q);
    Task<ApiResponse<StudentResponse>> GetByIdAsync(int id);
    Task<ApiResponse<StudentResponse>> CreateAsync(StudentRequest request);
    Task<ApiResponse<StudentResponse>> UpdateAsync(int id, StudentRequest request);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}

public interface IEnrollmentService
{
    Task<PagedApiResponse> GetAllAsync(QueryParams q, int? studentId = null, int? courseId = null);
    Task<ApiResponse<EnrollmentResponse>> GetByIdAsync(int id);
    Task<ApiResponse<EnrollmentResponse>> CreateAsync(EnrollmentRequest request);
    Task<ApiResponse<EnrollmentResponse>> UpdateAsync(int id, EnrollmentRequest request);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}
