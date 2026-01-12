using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace BackEnd.Models;
public class Recipe
{
    // public ObjectId Id { get; set; }
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("title")]
    public string? Title { get; set; }

    [BsonElement("ingredients")]
    public List<Ingredient>? Ingredients { get; set; }

    [BsonElement("recipeAuthor")]
    public Author? RecipeAuthor { get; set; }

    [BsonElement("prepTime")]
    public int PrepTime { get; set; }

    [BsonElement("description")]
    public string? Description { get; set; }
}