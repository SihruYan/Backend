using Backend.Data;
using Backend.Model;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controller;

[ApiController]
[Route("api/[controller]")]
public class InternalController: ControllerBase
{
    
    private readonly DapperDb _db;

    public InternalController(DapperDb db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var products = await _db.GetProductsAsync();
        return Ok(products);
    }

    [HttpPost]
    public async Task<IActionResult> Post(Product product)
    {
        await _db.InsertProductAsync(product);
        return Ok(new { message = "Created successfully" });
    }
}