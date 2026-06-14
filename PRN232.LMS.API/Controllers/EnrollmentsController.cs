using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.Services.RequestModels;
using PRN232.LMS.Services.ResponseModels;
using PRN232.LMS.Services.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;

namespace PRN232.LMS.API.Controllers;

/// <summary>
/// Manages student enrollments in courses.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _service;
    public EnrollmentsController(IEnrollmentService service) => _service = service;

    /// <summary>Get all enrollments.</summary>
    /// <remarks>
    /// Supports full query capabilities:
    /// - **Search**: <c>?search=active</c> — filters status, student name, course name
    /// - **Sort**: <c>?sort=-enrollDate,status</c>
    /// - **Paging**: <c>?page=1&amp;size=20</c>
    /// - **Fields**: <c>?fields=enrollmentId,status</c>
    /// - **Expand**: <c>?expand=student,course</c> — includes nested student and course objects
    /// - **Filter**: <c>?studentId=5</c> or <c>?courseId=3</c>
    /// </remarks>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] QueryParams q,
        [FromQuery] int? studentId = null,
        [FromQuery] int? courseId  = null,
        [FromHeader(Name = "X-Request-Id")] string? requestId = null)
    {
        if (requestId != null)
        {
            HttpContext.Response.Headers["X-Response-Id"] = requestId;
        }
        return Ok(await _service.GetAllAsync(q, studentId, courseId));
    }

    /// <summary>Get an enrollment by ID.</summary>
    /// <remarks>Returns complete enrollment detail including student name/email, course name, and semester name.</remarks>
    /// <param name="id">Enrollment ID</param>
    [HttpGet("{id:int}", Name = "GetEnrollmentById")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Enroll a student in a course.</summary>
    /// <remarks>Validates that both the student and course exist before creating the enrollment.</remarks>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>),             StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] EnrollmentRequest request)
    {
        var result = await _service.CreateAsync(request);
        if (!result.Success) return BadRequest(result);
        return CreatedAtRoute("GetEnrollmentById", new { version = "1", id = result.Data!.EnrollmentId }, result);
    }

    /// <summary>Update an existing enrollment.</summary>
    /// <param name="id">Enrollment ID to update</param>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>),             StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] EnrollmentRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Delete an enrollment by ID.</summary>
    /// <param name="id">Enrollment ID to delete</param>
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
