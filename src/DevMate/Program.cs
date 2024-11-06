// using Microsoft.AspNetCore.Mvc;
// using Octokit;
// using System.Net.Http.Headers;



// var builder = WebApplication.CreateBuilder(args);
// var app = builder.Build();



// app.MapGet("/", () => "Hello Copilot!");


// // make sure you change the App Name below
// string yourGitHubAppName = " DevMate ";
// string githubCopilotCompletionsUrl = 
//     "https://api.githubcopilot.com/chat/completions";



// app.MapPost("/agent", async (
//     [FromHeader(Name = "X-GitHub-Token")] string githubToken, 
//     [FromBody] Request userRequest) =>
// {

// var octokitClient = 
//     new GitHubClient(
//         new Octokit.ProductHeaderValue(yourGitHubAppName))
// {
//     Credentials = new Credentials(githubToken)
// };
// var user = await octokitClient.User.Current();

// userRequest.Messages.Insert(0, new Message
// {
//     Role = "system",
//     Content = 
//         "Start every response with the user's name, " + 
//         $"which is @{user.Login}"
// });
// userRequest.Messages.Insert(0, new Message
// {
//     Role = "system",
//     Content = 
//         "You are a helpful Sr. Developer that replies to " +
//         "user who is a jr developer messages asking for access to developer and other technical documents to help assist them."
// });

// var httpClient = new HttpClient();
// httpClient.DefaultRequestHeaders.Authorization = 
//     new AuthenticationHeaderValue("Bearer", githubToken);
// userRequest.Stream = true;

// var copilotLLMResponse = await httpClient.PostAsJsonAsync(
//     githubCopilotCompletionsUrl, userRequest);

// var responseStream = 
//     await copilotLLMResponse.Content.ReadAsStreamAsync();
// return Results.Stream(responseStream, "application/json");



// });

// app.MapGet("/callback", () => "You may close this tab and " + 
//     "return to GitHub.com (where you should refresh the page " +
//     "and start a fresh chat). If you're using VS Code or " +
//     "Visual Studio, return there.");

// app.Run();

// // //working code
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
// // //End of  working code

// // using Microsoft.AspNetCore.Mvc;
// // using Octokit;
// // using System.Net.Http;
// // using System.Text;
// // using System.Text.Json;

// // var builder = WebApplication.CreateBuilder(args);
// // var app = builder.Build();

// // app.MapGet("/", () => "Hello Copilot!");

// // // GitHub app name and AWS RESTful API URL
// // string yourGitHubAppName = "DeveMate";
// // string awsApiUrl = "https://a6b66wpks5.execute-api.us-east-1.amazonaws.com/dev/agent";
// // string modelDeploymentId = "4da7b4be-db50-4ec3-b2cf-fbe94632ff69";  // Set this to a fixed ID or generate as needed

// // app.MapPost("/agent", async (
// //     [FromHeader(Name = "X-GitHub-Token")] string githubToken, 
// //     [FromBody] Request userRequest) =>
// // {
// //     // Generate a unique requestId for tracking
// //     string requestId = Guid.NewGuid().ToString();

// //     // Identify the user using the GitHub API token
// //     var octokitClient = new GitHubClient(new Octokit.ProductHeaderValue(yourGitHubAppName))
// //     {
// //         Credentials = new Credentials(githubToken)
// //     };
// //     var user = await octokitClient.User.Current();

// //     // Insert special system messages
// //     userRequest.Messages.Insert(0, new Message
// //     {
// //         Role = "system",
// //         Content = $"Start every response with the user's name, which is @{user.Login}"
// //     });
// //     userRequest.Messages.Insert(0, new Message
// //     {
// //         Role = "system",
// //         Content = "You are a Sr Developer that is there to help other developers find the information they are looking for."
// //     });

// //     // Add tracking information to the userRequest (if necessary)
// //     userRequest.Messages.Insert(0, new Message
// //     {
// //         Role = "system",
// //         Content = $"Tracking info: requestId = {requestId}, modelDeploymentId = {modelDeploymentId}"
// //     });

// //     // Set up HTTP client and send the request to the AWS API Gateway
// //     using var httpClient = new HttpClient();
// //     var requestJson = JsonSerializer.Serialize(userRequest);
// //     var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

// //     var response = await httpClient.PostAsync(awsApiUrl, content);
// //     var responseData = await response.Content.ReadAsStringAsync();

// //     // Return the response with the added identifiers
// //     return Results.Json(new
// //     {
// //         requestId,
// //         modelDeploymentId,
// //         data = responseData
// //     });
// // });

// // // Callback endpoint for GitHub app installation
// // app.MapGet("/callback", () => "You may close this tab and return to GitHub.com (where you should refresh the page and start a fresh chat). If you're using VS Code or Visual Studio, return there.");

// // app.Run();
// // 
// using Microsoft.AspNetCore.Mvc;
// using Octokit;
// using System.Net.Http;
// using System.Text;
// using System.Text.Json;

// var builder = WebApplication.CreateBuilder(args);
// var app = builder.Build();

// app.MapGet("/", () => "Hello Copilot!");

// // GitHub app name and AWS RESTful API URL
// string yourGitHubAppName = "DevMate";
// string awsApiUrl = "https://a6b66wpks5.execute-api.us-east-1.amazonaws.com/dev/agent";
// string modelDeploymentId = "4da7b4be-db50-4ec3-b2cf-fbe94632ff69";

// app.MapPost("/agent", async (
//     [FromHeader(Name = "X-GitHub-Token")] string githubToken, 
//     [FromBody] Request userRequest) =>
// {
//     // Generate a unique requestId for tracking
//     string requestId = Guid.NewGuid().ToString();

//     // Identify the user using the GitHub API token
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
//         Content = "You are a Sr Developer that is there to help other developers find the information they are looking for."
//     });

//     // Add tracking information to the userRequest
//     userRequest.Messages.Insert(0, new Message
//     {
//         Role = "system",
//         Content = $"Tracking info: requestId = {requestId}, modelDeploymentId = {modelDeploymentId}"
//     });

//     // Set up HTTP client and send the request to the AWS API Gateway
//     using var httpClient = new HttpClient();
//     var requestJson = JsonSerializer.Serialize(userRequest);
//     var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

//     var response = await httpClient.PostAsync(awsApiUrl, content);
    
//     // Stream the response directly back to the client
//     if (response.IsSuccessStatusCode)
//     {
//         var responseStream = await response.Content.ReadAsStreamAsync();
//         return Results.Stream(responseStream, "application/json");
//     }
//     else
//     {
//         // Handle errors if Lambda does not respond as expected
//         return Results.Json(new
//         {
//             requestId,
//             modelDeploymentId,
//             error = "Error receiving response from Lambda",
//             statusCode = response.StatusCode
//         });
//     }
// });

// // Callback endpoint for GitHub app installation
// app.MapGet("/callback", () => "You may close this tab and return to GitHub.com (where you should refresh the page and start a fresh chat). If you're using VS Code or Visual Studio, return there.");

// app.Run();
