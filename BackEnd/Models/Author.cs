using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace BackEnd.Models;
public class Author 
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("name")]
    public string? Name { get; set; }

    [BsonElement("bio")]
    public string? Bio { get; set; }

    [BsonElement("joinedDate")]
    public DateTime? JoinedDate { get; set; }

    [BsonElement("emailAddress")]
    [EmailAddress]
    public string? EmailAddress { get; set; }
}