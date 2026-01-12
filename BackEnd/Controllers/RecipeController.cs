using BackEnd.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/recipe")]
public class RecipeController : ControllerBase
{
    private readonly IMongoDbService _mongoDbService;

    public RecipeController(IMongoDbService mongoDbService)
    {
        _mongoDbService = mongoDbService;
    }

    [HttpPost("create/{authorId}")]
    public async Task<ActionResult> CreateRecipe([FromBody] Recipe recipe, string authorId, [FromQuery] List<string> ingredientNames)
    {
        try
        {
            if (recipe == null)
            {
                return BadRequest("Recipe can't be null!");
            }

            recipe.Id = ObjectId.GenerateNewId().ToString();

            var author = await _mongoDbService.GetCollection<Author>("authors")
            .Find(p => p.Id == authorId)
            .FirstOrDefaultAsync();

            if (author == null)
            {
                return BadRequest($"Author with ID {authorId} does not exist.");
            }

            // Povezivanje autora sa receptom
            recipe.RecipeAuthor = author;

            // Provera i dodavanje sastojaka
            if (ingredientNames != null && ingredientNames.Count > 0)
            {
                var ingredients = await _mongoDbService.GetCollection<Ingredient>("ingredients")
                    .Find(p => ingredientNames.Contains(p.Name))
                    .ToListAsync();

                if (ingredients.Count != ingredientNames.Count)
                {
                    return BadRequest("One or more ingredient IDs are invalid.");
                }

                recipe.Ingredients = ingredients;
            }
            else
            {
                recipe.Ingredients = new List<Ingredient>();
            }

            await _mongoDbService.InsertDocumentAsync<Recipe>("recipes", recipe);
            return CreatedAtAction(nameof(CreateRecipe), new { id = recipe.Id }, recipe); //recipe.recipeAuthor.Id = authorId
        }
        catch (Exception exception)
        {
            return BadRequest("Error while creating recipe!" + exception.Message);
        }
    }

    [HttpGet("returnAll")]
    public async Task<ActionResult> ReturnAllRecipes()
    {
        try
        {
            var recipes = await _mongoDbService.GetCollection<Recipe>("recipes").Find(_ => true).ToListAsync();

            var recipesWithCalories = new List<object>();

            foreach (Recipe r in recipes)
            {
                var totalCalories = await Task.FromResult(r.Ingredients!.Sum(ingredient => ingredient.Calories));
                recipesWithCalories.Add(new
                {
                    r.Id,
                    r.Title,
                    r.RecipeAuthor,
                    r.PrepTime,
                    r.Description,
                    r.Ingredients,
                    TotalCalories = totalCalories
                });
            }

            return Ok(recipesWithCalories);
        }
        catch (Exception e)
        {
            return BadRequest("Error: " + e.Message);
        }
    }

    [HttpGet("filter")]
    public async Task<ActionResult> FilterRecipes([FromQuery] List<string> authorNames, [FromQuery] List<string> ingredientsNames, [FromQuery] List<int> prepTimes)
    {
        try
        {
            // Pribavljanje kolekcije recepata
            var collection = _mongoDbService.GetCollection<Recipe>("recipes");

            // Lista filtera koji će se koristiti
            var filters = new List<FilterDefinition<Recipe>>();

            // Dodaj filter za autore ako je prosleđen
            if (authorNames != null && authorNames.Any())
            {
                var authorFilter = Builders<Recipe>.Filter.In(r => r.RecipeAuthor!.Name, authorNames);
                filters.Add(authorFilter);
            }

            // Dodaj filter za sastojke ako je prosleđen
            if (ingredientsNames != null && ingredientsNames.Any())
            {
                var ingredientsFilter = Builders<Recipe>.Filter.ElemMatch(
                    r => r.Ingredients,
                    ingredient => ingredientsNames.Contains(ingredient.Name)
                );
                filters.Add(ingredientsFilter);
            }

            // Dodaj filter za vreme pripreme ako je prosleđen
            if (prepTimes != null && prepTimes.Count >= 2)
            {
                var prepTimeFilter = Builders<Recipe>.Filter.And(
                    Builders<Recipe>.Filter.Gte(r => r.PrepTime, prepTimes[0]),
                    Builders<Recipe>.Filter.Lte(r => r.PrepTime, prepTimes[1])
                );
                filters.Add(prepTimeFilter);
            }

            // Kombinuj sve filtere sa AND operatorom, ili koristi prazan filter ako nema uslova
            var combinedFilter = filters.Any()
                ? Builders<Recipe>.Filter.And(filters)
                : Builders<Recipe>.Filter.Empty;

            var filterRecipes = await collection.Find(combinedFilter).ToListAsync();

            var recipesWithCalories = new List<object>();

            foreach (Recipe r in filterRecipes)
            {
                var totalCalories = await Task.FromResult(r.Ingredients!.Sum(ingredient => ingredient.Calories));
                recipesWithCalories.Add(new
                {
                    r.Id,
                    r.Title,
                    r.RecipeAuthor,
                    r.PrepTime,
                    r.Description,
                    r.Ingredients,
                    TotalCalories = totalCalories
                });
            }

            return Ok(recipesWithCalories);
        }
        catch (Exception e)
        {
            return BadRequest(new { Error = e.Message });
        }
    }

    [HttpPost("updateDescription")]
    public async Task<ActionResult> UpdateDescription([FromBody] Recipe recipe)
    {
        try
        {
            var result = await _mongoDbService.GetCollection<Recipe>("recipes")
                .UpdateOneAsync(
                    Builders<Recipe>.Filter.Eq(p => p.Id, recipe.Id),
                    Builders<Recipe>.Update.Set(p => p.Description, recipe.Description)
                );

            if (result.MatchedCount == 0)
            {
                return NotFound(new { Message = "Recept nije pronadjen!" });
            }

            return Ok(new { Message = "Opis recepta uspesno azuriran" });
        }
        catch (Exception e)
        {
            return BadRequest(new { Error = e.Message });
        }
    }

    [HttpDelete("deletePost/{postId}")]
    public async Task<ActionResult> DeletePost(string postId)
    {
        try
        {
            var result = await _mongoDbService.GetCollection<Recipe>("recipes")
                .DeleteOneAsync(
                    Builders<Recipe>.Filter.Eq(p => p.Id, postId)
                );

            if (result.DeletedCount == 0)
            {
                return BadRequest("No document was deleted. Check if postId is correct.");
            }

            return Ok("Recipe deleted successfully.");

        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

}