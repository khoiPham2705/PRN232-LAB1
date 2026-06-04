using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.Services.RequestModels;
using PRN232.LMS.Services.ResponseModels;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.API.Controllers;

/// <summary>
/// Manages students enrolled in the LMS.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _service;
    public StudentsController(IStudentService service) => _service = service;

    /// <summary>Get all students.</summary>
    /// <remarks>
    /// Supports full query capabilities:
    /// - **Search**: <c>?search=nguyen</c> — filters fullName and email
    /// - **Sort**: <c>?sort=fullName,-dateOfBirth</c> — multi-field, prefix <c>-</c> for descending
    /// - **Paging**: <c>?page=2&amp;size=10</c>
    /// - **Fields**: <c>?fields=studentId,fullName,email</c> — returns only selected fields
    /// - **Expand**: <c>?expand=enrollments</c> — includes each student's enrollment list
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] QueryParams q)
        => Ok(await _service.GetAllAsync(q));

    /// <summary>Get a student by ID.</summary>
    /// <remarks>Returns full student detail including all enrollments (with course and semester names).</remarks>
    /// <param name="id">Student ID</param>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
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
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.StudentId }, result);
    }

    /// <summary>Update an existing student.</summary>
    /// <param name="id">Student ID to update</param>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>),          StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] StudentRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Delete a student by ID.</summary>
    /// <param name="id">Student ID to delete</param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
