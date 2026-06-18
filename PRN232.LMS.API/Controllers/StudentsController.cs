using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.Services.RequestModels;
using PRN232.LMS.Services.ResponseModels;
using PRN232.LMS.Services.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;

namespace PRN232.LMS.API.Controllers;

/// <summary>
/// Manages students enrolled in the LMS.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Produces("application/json", "application/xml")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _service;
    public StudentsController(IStudentService service) => _service = service;

    /// <summary>Get all students (V1).</summary>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] QueryParams q,
        [FromHeader(Name = "X-Request-Id")] string? requestId)
    {
        if (requestId != null)
        {
            HttpContext.Response.Headers["X-Response-Id"] = requestId;
        }
        return Ok(await _service.GetAllAsync(q));
    }

    /// <summary>Get all students (V2 - Beta info).</summary>
    [HttpGet]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(ApiResponse<V2BetaResponse>), StatusCodes.Status200OK)]
    public IActionResult GetAllV2()
    {
        return Ok(ApiResponse<V2BetaResponse>.Ok(new V2BetaResponse { Info = "LMS Students API Version 2.0 (Beta)", SupportedFormat = "JSON/XML" }, "Welcome to API Version 2.0"));
    }

    /// <summary>Get a student by ID.</summary>
    /// <remarks>Returns full student detail including all enrollments (with course and semester names).</remarks>
    /// <param name="id">Student ID</param>
    [HttpGet("{id:int}", Name = "GetStudentById")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Create a new student.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>),          StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] StudentRequest request)
    {
        var result = await _service.CreateAsync(request);
        return CreatedAtRoute("GetStudentById", new { version = "1", id = result.Data!.StudentId }, result);
    }

    /// <summary>Update an existing student.</summary>
    /// <param name="id">Student ID to update</param>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>),          StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] StudentRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Delete a student by ID.</summary>
    /// <param name="id">Student ID to delete</param>
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
