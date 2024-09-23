namespace APIGenerativeAI.Models
{
    /// <summary>
    /// Representa um parâmetro de função com detalhes sobre sua obrigatoriedade, descrição, valor e tipo.
    /// </summary>
    public class Parameter
    {
        /// <summary>
        /// Obtém ou define se o parâmetro é obrigatório.
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Obtém ou define a descrição do parâmetro.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Obtém ou define o valor do parâmetro.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Obtém ou define o tipo do parâmetro.
        /// </summary>
        public Type TypeOff { get; set; }
    }

    /// <summary>
    /// Representa um serviço de API com detalhes sobre seu nome, URL, descrição, operação e parâmetros.
    /// </summary>
    public class APIService : APIItemGPT
    {
        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="APIService"/> com os detalhes fornecidos.
        /// </summary>
        /// <param name="name">O nome associado ao serviço da API.</param>
        /// <param name="url">A URL do serviço da API.</param>
        /// <param name="description">A descrição do serviço da API.</param>
        /// <param name="operation">A operação associada ao serviço da API.</param>
        public APIService(string name, string url, string description, string operation)
        {
            Name = name;
            URL = url;
            Description = description;
            Operation = operation;
        }

        /// <summary>
        /// Obtém ou define a descrição do serviço da API.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Obtém ou define a operação associada ao serviço da API.
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// Obtém ou define a URL do serviço da API.
        /// </summary>
        public string URL { get; set; }

        /// <summary>
        /// Obtém ou define um dicionário de parâmetros associados ao serviço da API.
        /// </summary>
        public Dictionary<string, Parameter> Parameters { get; set; }

        /// <summary>
        /// Obtém ou define uma mensagem associada ao serviço da API.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Obtém ou define o nome do método void associado ao serviço da API.
        /// </summary>
        public string VoidName { get; set; }
    }
}
