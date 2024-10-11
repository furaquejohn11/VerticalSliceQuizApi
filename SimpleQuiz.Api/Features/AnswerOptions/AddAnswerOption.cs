using Carter;
using MediatR;
using SimpleQuiz.Api.Abstractions;
using SimpleQuiz.Api.Abstractions.Operations;
using SimpleQuiz.Api.Entities;
using SimpleQuiz.Api.Shared;

namespace SimpleQuiz.Api.Features.AnswerOptions;

public class AddAnswerOption
{
    public record Command(
        int questionId,
        string Text,
        bool isCorrect) : ICommand;

    internal sealed class Handler : ICommandHandler<Command>
    {
        private readonly IRepository<AnswerOption> _answerRepository;

        public Handler(IRepository<AnswerOption> answerRepository)
        {
            _answerRepository = answerRepository;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var answer = new AnswerOption(
                             request.questionId,
                             request.Text,
                             request.isCorrect);

            await _answerRepository.AddAsync(answer);

            return Result.Success();
        }
    }
}

public class AddAnswerOptionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/answeroption/add", async (IMediator mediator, AddAnswerOption.Command command) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? Results.Ok()
                : Results.BadRequest(result.Error);
        })
        .WithTags("AnswerOptions");
    }
}
