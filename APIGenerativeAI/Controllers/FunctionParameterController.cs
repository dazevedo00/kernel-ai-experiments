using APIGenerativeAI.Models;
using APIGenerativeAI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;

namespace APIGenerativeAI.Controllers
{
    /// <summary>
    /// Controller responsible for processing and filling function parameters based on user input
    /// using AI and caching to improve performance.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class FunctionParameterController : Controller
    {
        private readonly Kernel _kernel;
        private readonly List<dynamic> _functionDefinitions;
        private readonly IChatHistoryService _chatHistoryService;
        private readonly IMemoryCache _memoryCache;

        /// <summary>
        /// Constructor for the FunctionParameterController.
        /// </summary>
        /// <param name="kernel">Instance of the AI Kernel.</param>
        /// <param name="chatHistoryService">Chat history service.</param>
        /// <param name="memoryCache">Memory cache to store parameters.</param>
        public FunctionParameterController(Kernel kernel, IChatHistoryService chatHistoryService, IMemoryCache memoryCache)
        {
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
            _chatHistoryService = chatHistoryService ?? throw new ArgumentNullException(nameof(chatHistoryService));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));

            _functionDefinitions = LoadFunctionDefinitions();
        }

        /// <summary>
        /// Loads simulated function definitions for the system, including their parameters.
        /// </summary>
        /// <returns>List of function definitions.</returns>
        private List<dynamic> LoadFunctionDefinitions()
        {
            return new List<dynamic>
            {
                new
                {
                    Name = "RHPFerias",
                    VoidName = "SaveVacaciones",
                    Parameters = new Dictionary<string, dynamic>
                    {
                        { "CodigoFuncionario", new { Required = true, TypeOff = "System.String", Description = "Identifier of the employee, it's an identification code." } },
                        { "DataDaFeria", new { Required = true, TypeOff = "System.DateTime", Description = "Vacation date in the format dd/MM/yyyy." } }
                    }
                },
                new
                {
                    Name = "RHPSendPaySlip",
                    VoidName = "SendPaySlip",
                    Parameters = new Dictionary<string, dynamic>
                    {
                        { "CodigoFuncionario", new { Required = true, TypeOff = "System.String", Description = "Employee identifier." } },
                        { "year", new { Required = true, TypeOff = "System.Int32", Description = "Receipt year." } },
                        { "month", new { Required = true, TypeOff = "System.Int32", Description = "Receipt month." } }
                    }
                }
            };
        }

        /// <summary>
        /// Processes the user's message and attempts to fill the parameters for the provided function.
        /// Uses caching to store already filled parameters.
        /// </summary>
        /// <param name="functionName">Name of the function for which parameters should be filled.</param>
        /// <param name="userMessage">User message containing context for parameter filling.</param>
        /// <returns>List of filled parameters and a list of missing parameters, if any.</returns>
        [HttpPost("{functionName}")]
        public async Task<IActionResult> GetFilledParameters(string functionName, [FromBody] UserMessage userMessage)
        {
            // Check if parameters for this function are already in cache
            if (_memoryCache.TryGetValue($"{functionName}_parameters", out (Dictionary<string, object>? filledParameters, List<string> missingParameters) cachedParameters))
            {
                if (cachedParameters.missingParameters == null || !cachedParameters.missingParameters.Any())
                {
                    if (cachedParameters.filledParameters?.Count > 0)
                    {
                        return Ok(new
                        {
                            filledParameters = cachedParameters.filledParameters,
                            missingParameters = new List<string>() // No parameters are missing
                        });
                    }
                }
            }
            else
            {
                cachedParameters = (new Dictionary<string, object>(), new List<string>());
            }

            cachedParameters.missingParameters ??= new List<string>();
            cachedParameters.filledParameters ??= new Dictionary<string, object>();

            // Add user's message to chat history
            _chatHistoryService.AddUserMessage(userMessage.SessionId, userMessage.Message);

            // Find the function definition
            var functionDefinition = _functionDefinitions.FirstOrDefault(f => f.Name == functionName);
            if (functionDefinition == null)
            {
                var errorResponse = "Function not found.";
                _chatHistoryService.AddAssistantMessage(userMessage.SessionId, errorResponse);
                return NotFound(errorResponse);
            }

            // Fill parameters using AI or mark as missing
            foreach (var param in functionDefinition.Parameters)
            {
                string paramName = param.Key;
                var paramDetails = param.Value;

                if (!cachedParameters.filledParameters.ContainsKey(paramName))
                {
                    string filledValue = await TryFillParameterUsingAI(userMessage.Message, paramName, paramDetails.Description, Type.GetType(paramDetails.TypeOff));

                    if (filledValue != null && !filledValue.Contains("NaoEncontrado"))
                    {
                        try
                        {
                            filledValue = RemoveParameterFromValue(paramName, filledValue);

                            if (Type.GetType(paramDetails.TypeOff) == typeof(DateTime))
                            {
                                cachedParameters.filledParameters[paramName] = ConvertStringToDatetime(filledValue);
                                cachedParameters.missingParameters.Remove(paramName);
                            }
                            else
                            {
                                cachedParameters.filledParameters[paramName] = Convert.ChangeType(filledValue, Type.GetType(paramDetails.TypeOff));
                                cachedParameters.missingParameters.Remove(paramName);
                            }
                        }
                        catch (Exception)
                        {
                            cachedParameters.missingParameters.Add(paramName);
                        }
                    }
                    else if (paramDetails.Required)
                    {
                        cachedParameters.missingParameters.Add(paramName);
                    }
                }
            }

            // Store filled parameters in cache
            _memoryCache.Set($"{functionName}_parameters", cachedParameters, TimeSpan.FromMinutes(30));

            return Ok(new
            {
                cachedParameters.filledParameters,
                cachedParameters.missingParameters
            });
        }

        /// <summary>
        /// Removes the parameter name from the filled value string, if necessary.
        /// </summary>
        /// <param name="paramName">Parameter name.</param>
        /// <param name="filledValue">Filled value.</param>
        /// <returns>Value without the parameter name.</returns>
        private string RemoveParameterFromValue(string paramName, string filledValue)
        {
            if (string.IsNullOrEmpty(paramName) || string.IsNullOrEmpty(filledValue))
            {
                return filledValue;
            }

            // Remove all occurrences of paramName and ":" from filledValue
            string result = filledValue.Replace(paramName, string.Empty).Replace(":", string.Empty).Trim();
            return result;
        }

        /// <summary>
        /// Tries to fill a parameter using AI, based on the user's message and history.
        /// </summary>
        /// <param name="prompt">User message.</param>
        /// <param name="paramName">Parameter name.</param>
        /// <param name="paramDescription">Parameter description.</param>
        /// <param name="paramType">Parameter type.</param>
        /// <returns>Filled value or "NaoEncontrado".</returns>
        private async Task<string> TryFillParameterUsingAI(string prompt, string paramName, string paramDescription, Type paramType)
        {
            // Adjust the prompt to provide appropriate context to AI
            string initialPrompt = $"From the following prompt: '{prompt}', fill in the value for parameter '{paramName}' described as: {paramDescription}. " +
                                   $"Return only the found value in {paramType} format. If not found, return 'NaoEncontrado'.";

            // Configure chat service and execute the query
            var chatCompletionService = _kernel.Services.GetService<IChatCompletionService>();
            var settings = new OpenAIPromptExecutionSettings
            {
                Temperature = 0.5F,
                MaxTokens = 150
            };

            // Add initial prompt to history
            _chatHistoryService.AddUserMessage("3", initialPrompt);
            ChatHistory chatHistory = _chatHistoryService.GetChatHistory("3");

            string generatedResponse = "";
            var result = chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory, executionSettings: settings, kernel: _kernel);

            await foreach (var content in result)
            {
                generatedResponse += content.Content;
            }

            // Remove parameter name from filled value and return the found value
            return string.IsNullOrEmpty(generatedResponse) ? "NaoEncontrado" : RemoveParameterFromValue(paramName, generatedResponse.Trim());
        }

        /// <summary>
        /// Converts a string to a DateTime object.
        /// </summary>
        /// <param name="dateTimeString">String containing the date.</param>
        /// <returns>DateTime object.</returns>
        private DateTime ConvertStringToDatetime(string dateTimeString)
        {
            try
            {
                // Default date format dd/MM/yyyy
                string format = "dd/MM/yyyy";
                var culture = System.Globalization.CultureInfo.InvariantCulture;
                return DateTime.ParseExact(dateTimeString, format, culture);
            }
            catch (Exception)
            {
                throw new Exception("Invalid date format, please use dd/MM/yyyy.");
            }
        }
    }
}
