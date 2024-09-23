using APIGenerativeAI.Models;
using Newtonsoft.Json;

namespace APIGenerativeAI.Data
{
    internal static class DataContext
    {
        internal static List<APIService> ApiData()
        {
            string caminhoDoArquivo = Path.Combine("Data", "Data.json");

            // Obtém o caminho completo do arquivo, considerando a raiz do projeto
            string caminhoCompleto = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, caminhoDoArquivo);

            if (File.Exists(caminhoCompleto))
            {
                string json = File.ReadAllText(caminhoCompleto);

                // Desserializa o JSON para uma lista de objetos MyClass
                List<APIService> myClassList = JsonConvert.DeserializeObject<List<APIService>>(json);
                return myClassList;
            }
            else
            {
                throw new Exception("Não foi encontrado o ficheiro Data.json");
            }
        }

        internal static string GetApiDataToSendGPT(List<APIService> myClassList)
        {
            string text = string.Join(Environment.NewLine + Environment.NewLine,
           myClassList.Select(item =>
               $"\"Name\": \"{item.Name}\"," + Environment.NewLine +
               $"\"Description\": \"{item.Description}\""));

            return text;
        }

    }
}
