using System.Text.Json;
using Backend.Repository;
using Backend.Services;
using Backend.ViewModel;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Npgsql;

namespace Backend.ApiController;

[ApiController]
[Route("api/[controller]")]
public class FormController: ControllerBase
{
    private readonly IFormRepository _formRepository;
    private readonly IEmailService _email;
    public FormController
        (IFormRepository formRepository,
            IEmailService email)
    {
        _formRepository = formRepository;
        _email = email;
    }
    
    [HttpPost("submit")]
    [EnableRateLimiting("FormSubmitPolicy")]
    public async Task<IActionResult> SubmitForm([FromBody] SubmitFormViewModel viewModel)
    {

        var id = await _formRepository.InsertFormAsync( JsonSerializer.Serialize(viewModel));
        if (id != null)
        {
            await _email.SendAdminNotificationAsync();
            return Ok(new { message = "Form submitted", id });
        }
        return BadRequest();
    }

    [HttpGet("HealthCheck")]
    public IActionResult HealthCheck()
    {
        return Ok("Api health check");
    }
    
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = (await _formRepository.GetAllAsync()).Select(x=> new FormListViewModel(x)).ToList();
        return Ok(list);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var detail = await _formRepository.GetByIdAsync(id);
        if (detail == null)
            return NotFound();

        var res = new FormViewModel(detail);
     
        return Ok(res);
    }
}