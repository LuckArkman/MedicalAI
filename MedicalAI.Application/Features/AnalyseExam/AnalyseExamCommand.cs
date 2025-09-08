using MediatR;
using MedicalAI.Domain.Entities;

namespace MedicalAI.Application.Features.AnalyseExam;

public class AnalyseExamCommand : IRequest<AnaliseResultado>
{
    public string PatientId { get; set; } = string.Empty;
    public string ExamType { get; set; } = string.Empty;
    public string ExamData { get; set; } = string.Empty; // Texto do exame, resultados de laboratório, etc.
}