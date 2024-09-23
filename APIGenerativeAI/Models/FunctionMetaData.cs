using Newtonsoft.Json;

namespace APIGenerativeAI.Models
{
    /// <summary>
    /// Contém metadados sobre uma função, incluindo o nome da função e seus parâmetros.
    /// </summary>
    internal class FunctionMetaData
    {
        /// <summary>
        /// Obtém ou define o nome da função.
        /// </summary>
        /// <value>
        /// O nome da função.
        /// </value>
        [JsonProperty(nameof(Function))]
        public string Function { get; set; }

        /// <summary>
        /// Obtém ou define os parâmetros da função.
        /// </summary>
        /// <value>
        /// Um dicionário onde as chaves são os nomes dos parâmetros e os valores são os valores dos parâmetros.
        /// </value>
        [JsonProperty(nameof(Parameters))]
        public Dictionary<string, object> Parameters { get; set; }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="FunctionMetaData"/>.
        /// </summary>
        public FunctionMetaData()
        {
            Parameters = new Dictionary<string, object>();
        }
    }
}
