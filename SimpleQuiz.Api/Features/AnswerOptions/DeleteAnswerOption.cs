using Carter;
using MediatR;
using SimpleQuiz.Api.Abstractions;
using SimpleQuiz.Api.Abstractions.Operations;
using SimpleQuiz.Api.Entities;
using SimpleQuiz.Api.Shared;

namespace SimpleQuiz.Api.Features.AnswerOptions;

public static class DeleteAnswerOption
{
    public record Command(int id) : ICommand;

    internal sealed class Handler : ICommandHandler<Command>
    {
        private readonly IRepository<AnswerOption> _answerRepository;

        public Handler(IRepository<AnswerOption> answerRepository)
        {
            _answerRepository = answerRepository;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var answer = await _answerRepository.GetByIdAsync(request.id);

            if (answer is null)
            {
                return Result.Failure(new Error("Invalid answer option id","No answer id has found"));
            }

            await _answerRepository.DeleteAsync(answer);

            return Result.Success();
        }
    }
}

public class DeleteAnswerOptionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/answeroption/delete", async (IMediator mediator, int id) =>
        {
            var result = await mediator.Send(new DeleteAnswerOption.Command(id));

            return result.IsSuccess
                   ? Result.Success()
                   : Result.Failure(result.Error);
        })
        .WithTags("AnswerOptions");
    }
}
