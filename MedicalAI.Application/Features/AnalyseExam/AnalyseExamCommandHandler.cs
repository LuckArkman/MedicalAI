using MediatR;
using MedicalAI.Domain.Entities;
using MedicalAI.Infrastructure.Interfaces;

namespace MedicalAI.Application.Features.AnalyseExam;

public class AnalyseExamCommandHandler : IRequestHandler<AnalyseExamCommand, AnaliseResultado>
{
    private readonly IGeminiService _geminiService;
    // Futuramente, injetar um serviço de busca de documentos (RAG)
    // private readonly IDocumentRetrievalService _documentRetrievalService;

    public AnalyseExamCommandHandler(IGeminiService geminiService)
    {
        _geminiService = geminiService;
    }

    public async Task<AnaliseResultado> Handle(AnalyseExamCommand request, CancellationToken cancellationToken)
    {
        // Passo 1: Recuperar documentos relevantes (RAG) - Simulação
        // Em um sistema real, você buscaria em um banco de vetores por protocolos e artigos
        // baseados no request.ExamType e request.ExamData.
        var contexto = RecuperarContextoRelevante(request.ExamType);

        // Passo 2: Chamar o serviço Gemini com os dados e o contexto
        var resultado = await _geminiService.GerarAnaliseMedicaAsync(request.ExamData, contexto, cancellationToken);

        return resultado;
    }

    private string RecuperarContextoRelevante(string tipoExame)
    {
        // LÓGICA DE RAG (SIMULADA):
        // 1. Converter `tipoExame` em um embedding.
        // 2. Buscar em um banco de dados vetorial (ex: Pinecone, Weaviate) por documentos similares.
        // 3. Retornar os trechos de texto mais relevantes.
        
        // Exemplo estático:
        return """
               --- INÍCIO DO CONTEXTO DE REFERÊNCIA ---
               Protocolo SUS para Tratamento de Diabetes Tipo 2 (Resumo):
               1. Diagnóstico: Glicemia de jejum >= 126 mg/dL.
               2. Tratamento inicial: Mudança de estilo de vida (MEV) + Metformina 500-850mg.
               3. Se meta não atingida em 3 meses, associar sulfonilureia (ex: Glibenclamida).
               
               Artigo PubMed (PMID: 12345678):
               "Estudos recentes indicam que o uso de inibidores de SGLT2 demonstra benefícios cardiovasculares em pacientes com diabetes tipo 2 e alto risco cardiovascular."
               --- FIM DO CONTEXTO DE REFERÊNCIA ---
               """;
    }
}