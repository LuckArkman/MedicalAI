using MedicalAI.Domain.Entities;

namespace MedicalAI.Infrastructure.Interfaces;

public interface IGeminiService
{
    Task<AnaliseResultado> GerarAnaliseMedicaAsync(string dadosExame, string contextoAdicional, CancellationToken cancellationToken);
}