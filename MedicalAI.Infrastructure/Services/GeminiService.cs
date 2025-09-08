using System.Text;
using System.Text.Json;
using MedicalAI.Infrastructure.Interfaces; // Verifique seu namespace
using MedicalAI.Domain.Entities;
using Microsoft.Extensions.Options;

namespace MedicalAI.Infrastructure.Services;

// Classe auxiliar para desserializar o arquivo JSON
file class PromptFile
{
    public string Template { get; set; } = string.Empty;
}

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly GeminiSettings _settings;
    private readonly string _promptTemplate; // Campo para armazenar o template do prompt

    public GeminiService(HttpClient httpClient, IOptions<GeminiSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;

        // Carrega o prompt do arquivo JSON uma vez, durante a inicialização do serviço.
        _promptTemplate = LoadPromptTemplate();
    }

    private string LoadPromptTemplate()
    {
        try
        {
            // Constrói o caminho para o arquivo de forma robusta
            var promptFilePath = Path.Combine(AppContext.BaseDirectory, "Prompts", "prompt.json");
            var jsonContent = File.ReadAllText(promptFilePath);
            var promptFile = JsonSerializer.Deserialize<PromptFile>(jsonContent);

            if (string.IsNullOrWhiteSpace(promptFile?.Template))
            {
                throw new InvalidOperationException("O template do prompt está vazio ou não foi encontrado no arquivo prompt.json.");
            }
            
            return promptFile.Template;
        }
        catch (Exception ex)
        {
            // Logar o erro em um sistema de log real é fundamental aqui
            Console.WriteLine($"Erro crítico ao carregar o prompt.json: {ex.Message}");
            throw; // Relança a exceção para impedir que a aplicação inicie com uma configuração inválida.
        }
    }

    public async Task<AnaliseResultado> GerarAnaliseMedicaAsync(string dadosExame, string contextoAdicional, CancellationToken cancellationToken)
    {
        var prompt = ConstruirPrompt(dadosExame, contextoAdicional);
        
        var requestUrl = $"{_settings.ApiEndpoint}?key={_settings.ApiKey}";

        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            },
            generationConfig = new 
            {
                response_mime_type = "application/json", 
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(requestUrl, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
        var analise = ExtrairEConverterResultado(jsonResponse);

        return analise;
    }

    // O método agora é muito mais simples e limpo!
    private string ConstruirPrompt(string dadosExame, string contextoAdicional)
    {
        // Substitui os placeholders no template carregado do arquivo.
        return _promptTemplate
            .Replace("{{ContextoAdicional}}", contextoAdicional)
            .Replace("{{DadosExame}}", dadosExame);
    }

    private AnaliseResultado ExtrairEConverterResultado(string jsonResponse)
    {
        try
        {
            using var doc = JsonDocument.Parse(jsonResponse);
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
            
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var resultado = JsonSerializer.Deserialize<AnaliseResultado>(text, options);

            return resultado ?? new AnaliseResultado { ResumoAnalise = "Erro ao desserializar a resposta da IA." };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao processar JSON da IA: {ex.Message}");
            return new AnaliseResultado { ResumoAnalise = "Formato de resposta inválido recebido da IA." };
        }
    }
}