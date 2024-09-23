using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace APIGenerativeAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyzeSentimentController : Controller
    {
        private readonly Kernel _kernel;

        public AnalyzeSentimentController(Kernel kernel) => _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));

        /// <summary>
        /// Analyzes the sentiment of the provided message and returns whether it is positive or negative.
        /// </summary>
        /// <param name="userMessage">User message for sentiment analysis.</param>
        /// <returns>Returns <c>true</c> if the sentiment is positive, <c>false</c> if it is negative.</returns>
        [HttpGet]
        public async Task<bool> AnalyzeSentiment(string userMessage)
        {
            if (string.IsNullOrEmpty(userMessage))
            {
                throw new ArgumentException("The message is required.");
            }

            string initialPrompt = CreateInitialPrompt(userMessage);
            string response = await GetChatCompletionResponseAsync(initialPrompt);

            return DetermineSentiment(response);
        }

        /// <summary>
        /// Creates the initial prompt for sentiment analysis.
        /// </summary>
        /// <param name="message">User message.</param>
        /// <returns>Formatted prompt for sentiment analysis.</returns>
        private static string CreateInitialPrompt(string message)
        {
            return $"""
            Analyze sentiment of the following message: "{message}". Is this a positive or negative response? 
            Whenever it contains positive messages, yes, I agree, I want to move forward, it should return the word 'positive'.
            When the response is negative, it should respond only 'negative'.
            """;
        }

        /// <summary>
        /// Gets the AI response for the provided prompt.
        /// </summary>
        /// <param name="prompt">Prompt to be sent to the AI.</param>
        /// <returns>Response from the AI.</returns>
        private async Task<string> GetChatCompletionResponseAsync(string prompt)
        {
            var chatCompletionService = _kernel.Services.GetService<IChatCompletionService>();
            var settings = new OpenAIPromptExecutionSettings
            {
                Temperature = 0.5F,
                MaxTokens = 100
            };

            var result = chatCompletionService.GetStreamingChatMessageContentsAsync(
                prompt,
                executionSettings: settings,
                kernel: _kernel
            );

            string fullMessage = "";
            await foreach (var content in result)
            {
                fullMessage += content.Content;
            }

            return fullMessage.Trim().ToLower();
        }

        /// <summary>
        /// Determines the sentiment based on the AI response.
        /// </summary>
        /// <param name="response">AI response.</param>
        /// <returns>Returns <c>true</c> if the response is positive, <c>false</c> if it is negative.</returns>
        private static bool DetermineSentiment(string response)
        {
            if (response.Contains("positive"))
            {
                return true;
            }
            else if (response.Contains("negative"))
            {
                return false;
            }

            // In case the sentiment is not detected
            return false;
        }
    }
}
