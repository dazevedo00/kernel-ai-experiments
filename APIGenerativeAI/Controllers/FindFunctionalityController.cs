using APIGenerativeAI.Models;
using APIGenerativeAI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel;

namespace APIGenerativeAI.Controllers
{
    /// <summary>
    /// Controller responsible for identifying the functionality requested by the user based on chat messages.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class FindFunctionalityController : Controller
    {
        private readonly Kernel _kernel;
        private readonly IChatHistoryService _chatHistoryService;

        /// <summary>
        /// Constructor for the FindFunctionalityController class.
        /// </summary>
        /// <param name="kernel">Kernel from Microsoft.SemanticKernel used for natural language processing.</param>
        /// <param name="chatHistoryService">Service responsible for chat message history.</param>
        public FindFunctionalityController(Kernel kernel, IChatHistoryService chatHistoryService)
        {
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
            _chatHistoryService = chatHistoryService ?? throw new ArgumentNullException(nameof(chatHistoryService));
        }

        /// <summary>
        /// Returns the list of available services in JSON format.
        /// </summary>
        /// <returns>JSON string containing available services.</returns>
        private string GetServicesJson()
        {
            return @"
        [
            { 'Name': 'GetAllGPTOperations', 'Description': 'Provide a list of all available operations in the chat. Allow the user to know which operations can be used in the chat.' },
            { 'Name': 'NotFound', 'Description': 'No data found for the search performed, please provide more details.' },
            { 'Name': 'RHPGetArticleInformation', 'Description': 'Allows retrieving article information. The identifier is required.' },
            { 'Name': 'RHPVacation', 'Description': 'Allows scheduling vacation for the employee. Parameters EmployeeCode and VacationDate are required.' },
            { 'Name': 'RHPSendPaySlip', 'Description': 'Sends the payslip for a specific year and month.' },
            { 'Name': 'RHPCreateAbsence', 'Description': 'Creates an absence for an employee on a specific date. Only for absences.' },
            { 'Name': 'ClientList', 'Description': 'List of clients from the ERP. Returns information about clients including name, client number, address, and tax number.' },
            { 'Name': 'EmployeeList', 'Description': 'Allows finding detailed data about employees, including information on work history, skills, and performance evaluations.' }
        ]";
        }

        /// <summary>
        /// Generates the initial prompt for service search.
        /// </summary>
        /// <param name="servicesJson">JSON string containing available services.</param>
        /// <returns>String containing the initial prompt.</returns>
        private string GetInitialPrompt(string servicesJson) =>
            "Search only in the list of data for the requested information." + Environment.NewLine +
            "If not found in the list, return the item \"NotFound\"." + Environment.NewLine +
            "Only consider the data from the following list:" + Environment.NewLine +
            servicesJson + Environment.NewLine +
            "Return the exact \"Name\" if you find information by the item's \"Description\". " +
            "If the description does not match any item, always return the item \"NotFound\".";

        /// <summary>
        /// Processes the user's message and identifies the associated functionality.
        /// </summary>
        /// <param name="userMessage">Message sent by the user.</param>
        /// <returns>Response containing the name of the identified functionality.</returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserMessage userMessage)
        {
            if (userMessage == null || string.IsNullOrEmpty(userMessage.Message) || string.IsNullOrEmpty(userMessage.SessionId))
            {
                return BadRequest("SessionId and Message are required.");
            }

            // Defining the list of services in JSON as context
            string servicesJson = GetServicesJson();

            // Construct the initial prompt
            string initialPrompt = GetInitialPrompt(servicesJson);

            // Add the initial prompt to history
            _chatHistoryService.AddUserMessage(userMessage.SessionId, initialPrompt);

            // Add the user's message to the chat history
            _chatHistoryService.AddUserMessage(userMessage.SessionId, userMessage.Message);
            _chatHistoryService.AddUserMessage(userMessage.SessionId, "Should return only 'NotFound' or if found, return the function name. Example 'RHPVacation'");

            // Configure the kernel and chat completion service
            var chatCompletionService = _kernel.Services.GetService<IChatCompletionService>();
            var settings = new OpenAIPromptExecutionSettings();

            string fullMessage = string.Empty;

            var result = chatCompletionService.GetStreamingChatMessageContentsAsync(
                initialPrompt + Environment.NewLine + userMessage.Message,
                executionSettings: settings,
                kernel: _kernel
            );

            await foreach (var content in result)
            {
                fullMessage += content.Content;
            }

            // Add the assistant's message to the chat history
            _chatHistoryService.AddAssistantMessage(userMessage.SessionId, fullMessage);

            return Ok(new { Response = fullMessage });
        }
    }
}
