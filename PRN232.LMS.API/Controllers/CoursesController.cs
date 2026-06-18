using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.Services.RequestModels;
using PRN232.LMS.Services.ResponseModels;
using PRN232.LMS.Services.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;

namespace PRN232.LMS.API.Controllers;

/// <summary>
/// Manages courses within semesters.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Produces("application/json", "application/xml")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _service;
    public CoursesController(ICourseService service) => _service = service;

    /// <summary>Get all courses.</summary>
    /// <remarks>
    /// Supports: <c>?search=</c> <c>?sort=courseName</c>
    /// <c>?page=1&amp;size=10</c> <c>?fields=courseId,courseName</c>
    /// <c>?expand=semester</c> <c>?semesterId=1</c> (filter by semester)
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] QueryParams q, 
        [FromQuery] int? semesterId = null,
        [FromHeader(Name = "X-Request-Id")] string? requestId = null)
    {
        if (requestId != null)
        {
            HttpContext.Response.Headers["X-Response-Id"] = requestId;
        }
        return Ok(await _service.GetAllAsync(q, semesterId));
    }

    /// <summary>Get a course by ID including enrolled students.</summary>
    /// <param name="id">Course ID</param>
    [HttpGet("{id:int}", Name = "GetCourseById")]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Get students enrolled in a specific course (nested resource).</summary>
    /// <param name="courseId">Course ID</param>
    [HttpGet("{courseId:int}/students")]
    [ProducesResponseType(typeof(ApiResponse<List<StudentEnrollmentSummaryResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentsByCourse([FromRoute] int courseId)
    {
        var courseResult = await _service.GetByIdAsync(courseId);
        if (!courseResult.Success)
        {
            return NotFound(ApiResponse<object>.Fail($"Course with ID {courseId} not found."));
        }

        var enrollments = courseResult.Data?.Enrollments ?? new List<StudentEnrollmentSummaryResponse>();
        return Ok(ApiResponse<List<StudentEnrollmentSummaryResponse>>.Ok(enrollments, "Students retrieved successfully."));
    }

    /// <summary>Create a new course.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>),         StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CourseRequest request)
    {
        var result = await _service.CreateAsync(request);
        if (!result.Success) return BadRequest(result);
        return CreatedAtRoute("GetCourseById", new { version = "1", id = result.Data!.CourseId }, result);
    }

    /// <summary>Update an existing course.</summary>
    /// <param name="id">Course ID to update</param>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>),         StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] CourseRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Delete a course by ID.</summary>
    /// <param name="id">Course ID to delete</param>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var result = await _service.DeleteAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
