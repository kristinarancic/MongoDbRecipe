using BackEnd.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/author")]
public class AuthorController : ControllerBase
{
    private readonly IMongoDbService _mongoDbService;

    public AuthorController(IMongoDbService mongoDbService)
    {
        _mongoDbService = mongoDbService;
    }

    [HttpPost("create")]
    public async Task<ActionResult> CreateAuthor([FromBody] Author author)
    {
        try
        {
            author.Id = ObjectId.GenerateNewId().ToString();

            author.JoinedDate = DateTime.UtcNow;

            if (author == null)
            {
                return BadRequest("Author object can't be null");
            }
            await _mongoDbService.InsertDocumentAsync("authors", author);

            return CreatedAtAction(nameof(CreateAuthor), new { id = author.Id, joinedDate =  author.JoinedDate}, author);
        }
        catch (Exception exception)
        {
            return BadRequest("Error while creating author" + exception.Message);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Auth request)
    {
        if (string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.EmailAddress))
        {
            return BadRequest(new { message = "Name and email fields are required!" });
        }

        try
        {
            var user = await _mongoDbService.GetCollection<Author>("authors")
                .Find(u => u.Name == request.Name && u.EmailAddress == request.EmailAddress)
                .FirstOrDefaultAsync();

            Console.WriteLine(user);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid name or email address!" });
            }

            return Ok(new { message = "Login successful", user });

        }
        catch (Exception e)
        {
            return BadRequest("Error while creating author" + e.Message);
        }
    }

    [HttpGet("name/{authorId}")]
    public async Task<ActionResult<string>> GetAuthorName(string authorId)
    {
        try
        {
            var author = await _mongoDbService.GetCollection<Author>("authors")
                .Find(a => a.Id == authorId)
                .FirstOrDefaultAsync();

            if (author == null)
            {
                return NotFound($"Author with ID {authorId} not found.");
            }

            return Ok(author.Name);
        }
        catch (Exception exception)
        {
            return BadRequest($"Error fetching author name: {exception.Message}");
        }
    }

    [HttpGet("allNames")]
    public async Task<ActionResult<string>> GetAllAuthorNames()
    {
        try
        {
            var authors = await _mongoDbService.GetCollection<Author>("authors").Aggregate()
                .Project(p => p.Name)
                .ToListAsync();

            if (authors == null)
            {
                return NotFound($"Authors not found.");
            }

            return Ok(authors);
        }
        catch (Exception exception)
        {
            return BadRequest($"Error fetching author name: {exception.Message}");
        }
    }
}