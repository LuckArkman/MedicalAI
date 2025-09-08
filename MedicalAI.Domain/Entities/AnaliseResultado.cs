namespace MedicalAI.Domain.Entities;

public class AnaliseResultado
{
    public string ResumoAnalise { get; set; } = string.Empty;
    public List<string> SugestoesTratamento { get; set; } = [];
    public List<string> FontesReferenciadas { get; set; } = [];
    public string Disclaimer { get; set; } = "Este resultado é gerado por IA e não substitui a avaliação de um profissional de saúde qualificado.";
}