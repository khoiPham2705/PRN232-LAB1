using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.Services.RequestModels;
using PRN232.LMS.Services.ResponseModels;
using PRN232.LMS.Services.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;

namespace PRN232.LMS.API.Controllers;

/// <summary>
/// Manages academic semesters.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Produces("application/json", "application/xml")]
public class SemestersController : ControllerBase
{
    private readonly ISemesterService _service;
    public SemestersController(ISemesterService service) => _service = service;

    /// <summary>Get all semesters.</summary>
    /// <remarks>
    /// Supports: <c>?search=</c> <c>?sort=semesterName,-startDate</c>
    /// <c>?page=1&amp;size=10</c> <c>?fields=semesterId,semesterName</c>
    /// </remarks>
    [HttpGet]
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

    /// <summary>Get a semester by ID including its nested course list.</summary>
    /// <param name="id">Semester ID</param>
    [HttpGet("{id:int}", Name = "GetSemesterById")]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Create a new semester.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>),           StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] SemesterRequest request)
    {
        var result = await _service.CreateAsync(request);
        return CreatedAtRoute("GetSemesterById", new { version = "1", id = result.Data!.SemesterId }, result);
    }

    /// <summary>Update an existing semester.</summary>
    /// <param name="id">Semester ID to update</param>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>),           StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] SemesterRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Delete a semester by ID.</summary>
    /// <param name="id">Semester ID to delete</param>
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
