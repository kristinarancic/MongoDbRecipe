using MongoDB.Driver;
using MongoDB.Bson;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()  // Dozvoli sve izvore
              .AllowAnyMethod()  // Dozvoli sve HTTP metode (GET, POST, itd.)
              .AllowAnyHeader(); // Dozvoli sve zaglavlja
    });
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// var app = builder.Build();

const string connectionUri = "mongodb+srv://rkristinavl:EzeoKtqhvoZQXgOJ@cluster0.tkiqf.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0";
var settings = MongoClientSettings.FromConnectionString(connectionUri);
settings.ServerApi = new ServerApi(ServerApiVersion.V1);
var client = new MongoClient(settings);

try
{
    // Proverite konekciju
    var result = client.GetDatabase("admin").RunCommand<BsonDocument>(new BsonDocument("ping", 1));
    Console.WriteLine("Pinged your deployment. You successfully connected to MongoDB!");
}
catch (Exception ex)
{
    Console.WriteLine($"Error connecting to MongoDB: {ex.Message}");
}

// Registrujte MongoDB klijenta, bazu, i servis
builder.Services.AddSingleton<IMongoClient>(client);
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    // Zamenite "ImeBaze" stvarnim imenom baze
    return client.GetDatabase("Recipe-Manager");
});
builder.Services.AddSingleton<IMongoDbService, MongoDbService>();

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
