using MediatR;
using MedicalAI.Application.Features.AnalyseExam;
using Microsoft.AspNetCore.Mvc;

namespace MedicalAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalysisController : ControllerBase
{
    private readonly IMediator _mediator;

    public AnalysisController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AnalyzeExam([FromBody] AnalyseExamCommand command)
    {
        if (command == null || string.IsNullOrWhiteSpace(command.ExamData))
        {
            return BadRequest("Os dados do exame são obrigatórios.");
        }
        
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}