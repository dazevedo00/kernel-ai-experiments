namespace APIGenerativeAI.Models
{
    /// <summary>
    /// Representa uma mensagem do usuário, incluindo o identificador da sessão e o conteúdo da mensagem.
    /// </summary>
    public class UserMessage
    {
        /// <summary>
        /// Obtém ou define o identificador único da sessão do usuário.
        /// </summary>
        /// <value>
        /// O identificador da sessão do usuário.
        /// </value>
        public string SessionId { get; set; }

        /// <summary>
        /// Obtém ou define o conteúdo da mensagem enviada pelo usuário.
        /// </summary>
        /// <value>
        /// O texto da mensagem do usuário.
        /// </value>
        /// 
        public string Message { get; set; }
    }
}
