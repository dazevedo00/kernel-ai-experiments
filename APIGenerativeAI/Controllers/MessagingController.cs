using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace APIGenerativeAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagingController : ControllerBase
    {
        private readonly Kernel _kernel;

        public MessagingController(Kernel kernel)
        {
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] string userMessage)
        {
            if (string.IsNullOrEmpty(userMessage))
            {
                return BadRequest("Message is required.");
            }

            // Build the initial prompt
            string initialPrompt = GetInitialPrompt(userMessage);

            // Set up the chat completion service
            var chatCompletionService = _kernel.Services.GetService<IChatCompletionService>();
            var settings = new OpenAIPromptExecutionSettings();

            var result = chatCompletionService.GetStreamingChatMessageContentsAsync(
                initialPrompt,
                executionSettings: settings,
                kernel: _kernel
            );

            string fullMessage = "";
            bool isFirstMessage = true;

            // Collect the AI's response
            await foreach (var content in result)
            {
                if (content.Role.HasValue && isFirstMessage)
                {
                    fullMessage += "AI > ";
                    isFirstMessage = false;
                }
                fullMessage += content.Content;
            }

            return Ok(new { Response = fullMessage });
        }

        /// <summary>
        /// Creates the initial prompt for the AI based on the user's message.
        /// </summary>
        /// <param name="message">The user's message.</param>
        /// <returns>A formatted prompt string.</returns>
        public static string GetInitialPrompt(string message)
        {
            return $"""
            Create a short yet professional message.
            The text to use as a basis is: {message}
            The message should be in chatbot language with correct text formatting in PT-PT.
            """;
        }
    }
}
