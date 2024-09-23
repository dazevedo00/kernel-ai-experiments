using APIGenerativeAI.Cache;
using APIGenerativeAI.Controllers;
using APIGenerativeAI.Models;
using APIGenerativeAI.Services;
using Azure;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

[Route("api/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
    private readonly FindFunctionalityController _findFunctionalityController;
    private readonly FunctionParameterController _functionParameterController;
    private readonly IChatHistoryService _chatHistoryService;
    private readonly IFunctionStateService _functionStateService;

    public ChatController(
        FindFunctionalityController findFunctionalityController,
        FunctionParameterController functionParameterController,
        IChatHistoryService chatHistoryService,
        IFunctionStateService functionStateService)
    {
        _findFunctionalityController = findFunctionalityController ?? throw new ArgumentNullException(nameof(findFunctionalityController));
        _functionParameterController = functionParameterController ?? throw new ArgumentNullException(nameof(functionParameterController));
        _chatHistoryService = chatHistoryService ?? throw new ArgumentNullException(nameof(chatHistoryService));
        _functionStateService = functionStateService ?? throw new ArgumentNullException(nameof(functionStateService));
    }

    /// <summary>
    /// Receives a user message, identifies the appropriate function, and returns the response.
    /// </summary>
    /// <param name="userMessage">User message and session ID.</param>
    /// <returns>Processed and formatted response.</returns>
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] UserMessage userMessage)
    {
        if (userMessage == null || string.IsNullOrEmpty(userMessage.Message) || string.IsNullOrEmpty(userMessage.SessionId))
        {
            return BadRequest("SessionId and Message are required.");
        }

        string functionFound = await GetFunctionAsync(userMessage.SessionId);

        if (string.IsNullOrWhiteSpace(functionFound))
        {
            functionFound = await FindServiceAsync(userMessage);
            if (string.IsNullOrWhiteSpace(functionFound))
            {
                return Ok(new { Response = "You need to provide more information to identify the service." });
            }

            await _functionStateService.SetFunctionAsync(userMessage.SessionId, functionFound);
        }

        var parameterResponse = await FillFunctionParametersAsync(functionFound, userMessage.Message);

        string response = GenerateResponse(parameterResponse, functionFound);
        return Ok(new { Response = response });
    }

    /// <summary>
    /// Retrieves the identified function from the cache.
    /// </summary>
    /// <param name="sessionId">User session ID.</param>
    /// <returns>Identified function.</returns>
    private async Task<string> GetFunctionAsync(string sessionId)
    {
        return await _functionStateService.GetFunctionAsync(sessionId);
    }

    /// <summary>
    /// Fills the function parameters using the FunctionParameterController.
    /// </summary>
    /// <param name="functionName">Function name.</param>
    /// <param name="prompt">User message.</param>
    /// <returns>Filled parameters and missing parameters.</returns>
    private async Task<(Dictionary<string, object> filledParameters, List<string> missingParameters)> FillFunctionParametersAsync(string functionName, string prompt)
    {
        IActionResult result = await _functionParameterController.GetFilledParameters(functionName, new UserMessage { SessionId = "2", Message = prompt });
        return ParseFunctionParametersResult(result);
    }

    /// <summary>
    /// Analyzes the result obtained from the FunctionParameterController.
    /// </summary>
    /// <param name="result">Result from the FunctionParameterController call.</param>
    /// <returns>Filled parameters and missing parameters.</returns>
    private (Dictionary<string, object> filledParameters, List<string> missingParameters) ParseFunctionParametersResult(IActionResult result)
    {
        if (result is OkObjectResult okResult)
        {
            var response = okResult.Value;
            var filledParameters = response.GetType().GetProperty("filledParameters")?.GetValue(response) as Dictionary<string, object>;
            var missingParameters = response.GetType().GetProperty("missingParameters")?.GetValue(response) as List<string>;

            return (filledParameters ?? new Dictionary<string, object>(), missingParameters ?? new List<string>());
        }

        return (new Dictionary<string, object>(), new List<string>());
    }

    /// <summary>
    /// Finds the appropriate function using the FindFunctionalityController.
    /// </summary>
    /// <param name="userMessage">User message.</param>
    /// <returns>Identified function.</returns>
    private async Task<string> FindServiceAsync(UserMessage userMessage)
    {
        _chatHistoryService.AddUserMessage(userMessage.SessionId, userMessage.Message);
        IActionResult result = await _findFunctionalityController.Post(userMessage);
        return ParseFindFunctionalityResult(result, userMessage.SessionId);
    }

    /// <summary>
    /// Analyzes the result obtained from the FindFunctionalityController.
    /// </summary>
    /// <param name="result">Result from the FindFunctionalityController call.</param>
    /// <param name="sessionId">User session ID.</param>
    /// <returns>Identified function or error message.</returns>
    private string ParseFindFunctionalityResult(IActionResult result, string sessionId)
    {
        if (result is OkObjectResult okResult)
        {
            var response = okResult.Value;
            var responseValue = response.GetType().GetProperty("Response")?.GetValue(response)?.ToString();

            if (!string.IsNullOrWhiteSpace(responseValue))
            {
                _chatHistoryService.AddAssistantMessage(sessionId, responseValue);
                return responseValue != "NotFound" ? responseValue : string.Empty;
            }
        }

        _chatHistoryService.AddAssistantMessage(sessionId, "You need to provide more information to identify the service.");
        return "Error processing the response.";
    }

    /// <summary>
    /// Generates the final response based on the filled parameters and the found function.
    /// </summary>
    /// <param name="parameterResponse">Response with filled and missing parameters.</param>
    /// <param name="functionFound">Identified function.</param>
    /// <returns>Formatted response.</returns>
    private string GenerateResponse(
        (Dictionary<string, object> filledParameters, List<string> missingParameters) parameterResponse,
        string functionFound)
    {
        if (parameterResponse.missingParameters.Count > 0)
        {
            return $"The following parameters are still missing: {string.Join(", ", parameterResponse.missingParameters)}";
        }

        return ConvertToJson(parameterResponse.filledParameters, functionFound);
    }

    /// <summary>
    /// Converts filled parameters and function into JSON.
    /// </summary>
    /// <param name="filledParameters">Filled parameters.</param>
    /// <param name="functionFound">Identified function.</param>
    /// <returns>Result in JSON format.</returns>
    private static string ConvertToJson(Dictionary<string, object> filledParameters, string functionFound)
    {
        var result = new
        {
            Parameters = filledParameters,
            Function = functionFound
        };

        return JsonConvert.SerializeObject(result, Formatting.Indented);
    }
}
