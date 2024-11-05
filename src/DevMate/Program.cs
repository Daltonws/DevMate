// using Microsoft.AspNetCore.Mvc;
// using Octokit;
// using System.Net.Http.Headers;

// var builder = WebApplication.CreateBuilder(args);
// var app = builder.Build();

// app.MapGet("/", () => "Hello Copilot!");

// // make sure you change the App Name below
// string yourGitHubAppName = " DeveMate ";
// string githubCopilotCompletionsUrl = "https://api.githubcopilot.com/chat/completions";

// app.MapPost("/DevMate", async (
//     [FromHeader(Name = "X-GitHub-Token")] string githubToken, 
//     [FromBody] Request userRequest) =>
// {
//     // Identify the user using the GitHub API token provided in the request headers
//     var octokitClient = new GitHubClient(new Octokit.ProductHeaderValue(yourGitHubAppName))
//     {
//         Credentials = new Credentials(githubToken)
//     };
//     var user = await octokitClient.User.Current();

//     // Insert special system messages
//     userRequest.Messages.Insert(0, new Message
//     {
//         Role = "system",
//         Content = $"Start every response with the user's name, which is @{user.Login}"
//     });
//     userRequest.Messages.Insert(0, new Message
//     {
//         Role = "system",
//         Content = "You are a Sr Developer that is there to help other developer find the infomration that they are looking for."
//     });

//     // Use the HttpClient class to communicate back to Copilot
//     var httpClient = new HttpClient();
//     httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", githubToken);
//     userRequest.Stream = true;

//     // Use Copilot's LLM to generate a response to the user's messages
//     var copilotLLMResponse = await httpClient.PostAsJsonAsync(githubCopilotCompletionsUrl, userRequest);

//     // Stream the response straight back to the user
//     var responseStream = await copilotLLMResponse.Content.ReadAsStreamAsync();
//     return Results.Stream(responseStream, "application/json");
// });

// // Callback endpoint for GitHub app installation
// app.MapGet("/callback", () => "You may close this tab and " + 
//     "return to GitHub.com (where you should refresh the page " +
//     "and start a fresh chat). If you're using VS Code or " +
//     "Visual Studio, return there.");

// app.Run();

using Microsoft.AspNetCore.Mvc;
using Octokit;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello Copilot!");

// GitHub app name and WebSocket URL
string yourGitHubAppName = "DeveMate";
string awsWebSocketUrl = "wss://cs0esv0jte.execute-api.us-east-1.amazonaws.com/dev/";

app.MapPost("/agent", async (
    [FromHeader(Name = "X-GitHub-Token")] string githubToken, 
    [FromBody] Request userRequest) =>
{
    // Identify the user using the GitHub API token provided in the request headers
    var octokitClient = new GitHubClient(new Octokit.ProductHeaderValue(yourGitHubAppName))
    {
        Credentials = new Credentials(githubToken)
    };
    var user = await octokitClient.User.Current();

    // Insert special system messages
    userRequest.Messages.Insert(0, new Message
    {
        Role = "system",
        Content = $"Start every response with the user's name, which is @{user.Login}"
    });
    userRequest.Messages.Insert(0, new Message
    {
        Role = "system",
        Content = "You are a Sr Developer that is there to help other developers find the information they are looking for."
    });

    // Create a WebSocket client to connect to the AWS WebSocket API
    using var webSocket = new ClientWebSocket();
    await webSocket.ConnectAsync(new Uri(awsWebSocketUrl), CancellationToken.None);

    // Serialize userRequest to JSON
    var requestJson = JsonSerializer.Serialize(userRequest);
    var requestBytes = Encoding.UTF8.GetBytes(requestJson);
    var requestBuffer = new ArraySegment<byte>(requestBytes);

    // Send the user request to the WebSocket API
    await webSocket.SendAsync(requestBuffer, WebSocketMessageType.Text, true, CancellationToken.None);

    // Prepare a buffer to receive the response
    var responseBuffer = new ArraySegment<byte>(new byte[1024]);
    var receivedData = new StringBuilder();

    // Stream the response back to the client as it arrives
    WebSocketReceiveResult result;
    do
    {
        result = await webSocket.ReceiveAsync(responseBuffer, CancellationToken.None);
        if (responseBuffer.Array != null)
        {
            receivedData.Append(Encoding.UTF8.GetString(responseBuffer.Array, 0, result.Count));
        }
    } while (!result.EndOfMessage);

    // Close the WebSocket connection
    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", CancellationToken.None);

    // Return the response data as JSON
    return Results.Json(receivedData.ToString());
});

// Callback endpoint for GitHub app installation
app.MapGet("/callback", () => "You may close this tab and return to GitHub.com (where you should refresh the page and start a fresh chat). If you're using VS Code or Visual Studio, return there.");

app.Run();
