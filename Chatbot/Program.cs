
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Chatbot;
using Chatbot.Services;

var builder = WebApplication.CreateBuilder(args);
// Use a fixed URL so we can open the browser to a known address
builder.WebHost.UseUrls("http://localhost:5000");

builder.Services.AddSingleton(provider =>
{
    var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? string.Empty;
    return new WeatherAgent(apiKey);
});

// register data store and disease service
var dataPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "diseases.json");
builder.Services.AddSingleton(new DiseaseStore(dataPath));
builder.Services.AddSingleton<IDiseaseService, DiseaseService>();
builder.Services.AddControllers();

// CRUD endpoints for diseases.json are registered after app is built

// (upload endpoint moved to after app build)

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();
app.UseCors();
app.MapControllers();
// Serve static files (wwwroot) so we can host a simple web chat UI
app.UseDefaultFiles();
app.UseStaticFiles();

// DiseasesController now handles CRUD via MVC controllers

// Khi ứng dụng đã khởi động, mở trình duyệt mặc định tới UI
app.Lifetime.ApplicationStarted.Register(() =>
{
    try
    {
        var url = "http://localhost:5000";
        Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
    }
    catch
    {
        // ignore failures to open browser
    }
});

// Ensure index.html is served at root and expose API info at /api/info
app.MapGet("/", () => Results.File(Path.Combine(app.Environment.ContentRootPath, "wwwroot", "index.html"), "text/html"));
// Explicit route for admin UI (fallback if static file not found)
app.MapGet("/admin.html", () => Results.File(Path.Combine(app.Environment.ContentRootPath, "wwwroot", "admin.html"), "text/html"));
app.MapGet("/favicon.ico", () => Results.NoContent());
app.MapGet("/api/info", () => Results.Ok("Chatbot API đang chạy"));
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapPost("/api/chat", async (ChatRequest request, WeatherAgent agent) =>
{
    if (request == null || string.IsNullOrWhiteSpace(request.Input))
        return Results.BadRequest(new { error = "Thiếu input" });

    var response = await agent.AskAsync(request.Input);

    if (!response.Success)
        return Results.Problem(detail: response.Error);

    var jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    return Results.Json(response, jsonOptions);
});

app.Run();

public class ChatRequest
{
    public string Input { get; set; } = string.Empty;
}

//using dotenv.net;
//using OpenAI.Chat;
//using OpenAI.Responses;

//DotEnv.Load();
//var openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
//if (openAiKey == null)
//    throw new InvalidOperationException("Missing OPENAI_API_KEY");

//ChatClient client = new(model: "gpt-4.1-nano", openAiKey);

//List<ChatMessage> messages = [
//    new AssistantChatMessage("Hello, what do you want to do today?")
//];

//Console.WriteLine(messages[0].Content[0].Text);

//while (true)
//{
//    Console.ForegroundColor = ConsoleColor.Blue;
//    var input = Console.ReadLine();
//    if (input == null || input?.ToLower() == "exit")
//        break;
//    Console.ResetColor();

//    messages.Add(new UserChatMessage(input));

//    ChatCompletion completion = client.CompleteChat(messages);

//    var response = completion.Content[0].Text;

//    messages.Add(new AssistantChatMessage(response));
//    Console.WriteLine(response);
//}
