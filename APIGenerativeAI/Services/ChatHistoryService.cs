using Microsoft.SemanticKernel.ChatCompletion;
using System.Collections.Generic;

namespace APIGenerativeAI.Services
{
    /// <summary>
    /// Interface para serviços de histórico de chat.
    /// </summary>
    public interface IChatHistoryService
    {
        /// <summary>
        /// Obtém o histórico de chat para uma sessão específica.
        /// </summary>
        /// <param name="sessionId">O identificador da sessão de chat.</param>
        /// <returns>O histórico de chat associado ao identificador da sessão.</returns>
        ChatHistory GetChatHistory(string sessionId);

        /// <summary>
        /// Adiciona uma mensagem do usuário ao histórico de chat para uma sessão específica.
        /// </summary>
        /// <param name="sessionId">O identificador da sessão de chat.</param>
        /// <param name="message">A mensagem do usuário a ser adicionada.</param>
        void AddUserMessage(string sessionId, string message);

        /// <summary>
        /// Adiciona uma mensagem do assistente ao histórico de chat para uma sessão específica.
        /// </summary>
        /// <param name="sessionId">O identificador da sessão de chat.</param>
        /// <param name="message">A mensagem do assistente a ser adicionada.</param>
        void AddAssistantMessage(string sessionId, string message);
    }

    /// <summary>
    /// Implementação da interface <see cref="IChatHistoryService"/> para gerenciamento do histórico de chat.
    /// </summary>
    public class ChatHistoryService : IChatHistoryService
    {
        private readonly Dictionary<string, ChatHistory> _chatHistories = new Dictionary<string, ChatHistory>();

        /// <summary>
        /// Obtém o histórico de chat para uma sessão específica. Se não existir, cria um novo histórico.
        /// </summary>
        /// <param name="sessionId">O identificador da sessão de chat.</param>
        /// <returns>O histórico de chat associado ao identificador da sessão.</returns>
        public ChatHistory GetChatHistory(string sessionId)
        {
            if (!_chatHistories.ContainsKey(sessionId))
            {
                _chatHistories[sessionId] = new ChatHistory();
            }
            return _chatHistories[sessionId];
        }

        /// <summary>
        /// Adiciona uma mensagem do usuário ao histórico de chat para uma sessão específica.
        /// </summary>
        /// <param name="sessionId">O identificador da sessão de chat.</param>
        /// <param name="message">A mensagem do usuário a ser adicionada.</param>
        public void AddUserMessage(string sessionId, string message)
        {
            var chatHistory = GetChatHistory(sessionId);
            chatHistory.AddUserMessage(message);
        }

        /// <summary>
        /// Adiciona uma mensagem do assistente ao histórico de chat para uma sessão específica.
        /// </summary>
        /// <param name="sessionId">O identificador da sessão de chat.</param>
        /// <param name="message">A mensagem do assistente a ser adicionada.</param>
        public void AddAssistantMessage(string sessionId, string message)
        {
            var chatHistory = GetChatHistory(sessionId);
            chatHistory.AddAssistantMessage(message);
        }
    }
}
