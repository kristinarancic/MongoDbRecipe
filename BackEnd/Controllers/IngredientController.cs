using BackEnd.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/ingredients")]
public class IngredientController : ControllerBase
{
    private readonly IMongoDbService _mongoDbService;

    public IngredientController(IMongoDbService mongoDbService)
    {
        _mongoDbService = mongoDbService;
    }

    [HttpPost("create")]
    public async Task<ActionResult> CreateIngredient([FromBody] Ingredient ingredient)
    {
        try
        {
            ingredient.Id = ObjectId.GenerateNewId().ToString();

            if (ingredient == null)
            {
                return BadRequest("Ingredient can't be null!");
            }

            await _mongoDbService.InsertDocumentAsync<Ingredient>("ingredients", ingredient);

            return CreatedAtAction(nameof(CreateIngredient), new { id = ingredient.Id }, ingredient);
        }
        catch (Exception exception)
        {
            return BadRequest("Error while creating ingredient!" + exception.Message);
        }
    }

    [HttpGet("names")]
    public async Task<ActionResult<List<string>>> GetAllIngredientsNames()
    {
        try
        {
            var ingredients = await _mongoDbService.GetCollection<Ingredient>("ingredients")
                .Find(_ => true)
                .Project(ingredient => ingredient.Name)
                .ToListAsync();

            return Ok(ingredients);
        }
        catch (Exception exception)
        {
            return BadRequest("Error while creating ingredient!" + exception.Message);
        }
    }
}