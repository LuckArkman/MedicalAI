using System.Text;
using System.Text.Json;
using MedicalAI.Domain.Entities;
using MedicalAI.Infrastructure.Interfaces;
using Microsoft.Extensions.Options;

namespace MedicalAI.Infrastructure.Services;

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly GeminiSettings _settings;

    public GeminiService(HttpClient httpClient, IOptions<GeminiSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task<AnaliseResultado> GerarAnaliseMedicaAsync(string dadosExame, string contextoAdicional, CancellationToken cancellationToken)
    {
        var prompt = ConstruirPrompt(dadosExame, contextoAdicional);
        
        var requestUrl = $"{_settings.ApiEndpoint}?key={_settings.ApiKey}";

        // Estrutura do corpo da requisição para a API do Gemini
        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            },
            generationConfig = new 
            {
                // Configurações para forçar a saída em JSON
                response_mime_type = "application/json", 
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(requestUrl, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);

        // Extrair e desserializar a resposta JSON do Gemini
        var analise = ExtrairEConverterResultado(jsonResponse);

        return analise;
    }

    private string ConstruirPrompt(string dadosExame, string contextoAdicional)
    {
        return $"""
                Você é um assistente de IA especializado em análise de dados médicos, treinado para auxiliar profissionais de saúde.
                Sua tarefa é analisar os dados de um exame médico, com base estritamente no CONTEXTO DE REFERÊNCIA fornecido.
                Você NÃO DEVE usar conhecimento externo ao que foi fornecido.

                {contextoAdicional}

                --- INÍCIO DOS DADOS DO EXAME ---
                {dadosExame}
                --- FIM DOS DADOS DO EXAME ---

                Com base nos DADOS DO EXAME e utilizando APENAS o CONTEXTO DE REFERÊNCIA, forneça uma análise.
                
                Siga o seguinte formato JSON OBRIGATORIAMENTE:
                {{
                  "resumoAnalise": "Um resumo conciso da sua análise com base nos dados e no contexto.",
                  "sugestoesTratamento":
                  [
                    "Sugestão 1 baseada no protocolo X.",
                    "Sugestão 2 baseada no artigo Y."
                  ],
                  "fontesReferenciadas":
                  [
                    "Protocolo SUS para Tratamento de Diabetes Tipo 2",
                    "Artigo PubMed (PMID: 12345678)"
                  ]
                }}
                """;
    }

    private AnaliseResultado ExtrairEConverterResultado(string jsonResponse)
    {
        try
        {
            using var doc = JsonDocument.Parse(jsonResponse);
            // Navega na estrutura de resposta do Gemini para encontrar o texto
            var text = doc.RootElement
                          .GetProperty("candidates")[0]
                          .GetProperty("content")
                          .GetProperty("parts")[0]
                          .GetProperty("text")
                          .GetString();

            if (string.IsNullOrEmpty(text))
            {
                throw new InvalidOperationException("A resposta da API do Gemini não contém texto.");
            }
            
            // O texto retornado já é o JSON que pedimos
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var resultado = JsonSerializer.Deserialize<AnaliseResultado>(text, options);

            return resultado ?? new AnaliseResultado { ResumoAnalise = "Erro ao desserializar a resposta da IA." };
        }
        catch (Exception ex)
        {
            // Logar o erro e o jsonResponse para depuração
            Console.WriteLine($"Erro ao processar JSON da IA: {ex.Message}");
            return new AnaliseResultado { ResumoAnalise = "Formato de resposta inválido recebido da IA." };
        }
    }
}