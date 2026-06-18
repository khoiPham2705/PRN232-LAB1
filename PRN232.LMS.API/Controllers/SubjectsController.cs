using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.Services.RequestModels;
using PRN232.LMS.Services.ResponseModels;
using PRN232.LMS.Services.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;

namespace PRN232.LMS.API.Controllers;

/// <summary>
/// Manages academic subjects (independent of courses/semesters).
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Produces("application/json", "application/xml")]
public class SubjectsController : ControllerBase
{
    private readonly ISubjectService _service;
    public SubjectsController(ISubjectService service) => _service = service;

    /// <summary>Get all subjects.</summary>
    /// <remarks>
    /// Supports: <c>?search=</c> (searches subjectCode and subjectName)
    /// <c>?sort=subjectCode,-credit</c> <c>?page=1&amp;size=10</c>
    /// <c>?fields=subjectId,subjectCode,subjectName</c>
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

    /// <summary>Get a subject by ID.</summary>
    /// <param name="id">Subject ID</param>
    [HttpGet("{id:int}", Name = "GetSubjectById")]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Create a new subject.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>),          StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] SubjectRequest request)
    {
        var result = await _service.CreateAsync(request);
        return CreatedAtRoute("GetSubjectById", new { version = "1", id = result.Data!.SubjectId }, result);
    }

    /// <summary>Update an existing subject.</summary>
    /// <param name="id">Subject ID to update</param>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>),          StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] SubjectRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Delete a subject by ID.</summary>
    /// <param name="id">Subject ID to delete</param>
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
