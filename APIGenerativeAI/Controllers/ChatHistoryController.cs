using APIGenerativeAI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace APIGenerativeAI.Controllers
{
    /// <summary>
    /// Controller responsible for managing the chat message history.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ChatHistoryController : ControllerBase
    {
        private readonly IChatHistoryService _chatHistoryService;

        /// <summary>
        /// Constructor for the ChatHistoryController class.
        /// </summary>
        /// <param name="chatHistoryService">Service responsible for managing chat history.</param>
        public ChatHistoryController(IChatHistoryService chatHistoryService)
        {
            _chatHistoryService = chatHistoryService ?? throw new ArgumentNullException(nameof(chatHistoryService));
        }

        /// <summary>
        /// Retrieves the chat message history for a specific chat session.
        /// </summary>
        /// <param name="sessionId">Chat session ID.</param>
        /// <returns>Chat history for the session.</returns>
        [HttpGet("{sessionId}")]
        public IActionResult GetChatHistory(string sessionId)
        {
            Microsoft.SemanticKernel.ChatCompletion.ChatHistory chatHistory = _chatHistoryService.GetChatHistory(sessionId);

            StringBuilder allMessages = new StringBuilder();

            // Iterate over each message in the chat history
            foreach (Microsoft.SemanticKernel.ChatMessageContent message in chatHistory)
            {
                // Append each message to the StringBuilder
                allMessages.AppendLine(message.Content);
            }

            // Convert the chat history to a readable string
            var messages = allMessages.ToString();

            return Ok(new { Messages = messages });
        }
    }
}
