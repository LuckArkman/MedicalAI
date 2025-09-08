using FluentValidation;

namespace MedicalAI.Application.Features.AnalyseExam;

public class AnalyseExamCommandValidator : AbstractValidator<AnalyseExamCommand>
{
    public AnalyseExamCommandValidator()
    {
        RuleFor(x => x.ExamType)
            .NotEmpty().WithMessage("O tipo de exame é obrigatório.")
            .MaximumLength(100).WithMessage("O tipo de exame não pode exceder 100 caracteres.");

        RuleFor(x => x.ExamData)
            .NotEmpty().WithMessage("Os dados do exame são obrigatórios.")
            .MinimumLength(20).WithMessage("Os dados do exame devem ter no mínimo 20 caracteres.");
    }
}