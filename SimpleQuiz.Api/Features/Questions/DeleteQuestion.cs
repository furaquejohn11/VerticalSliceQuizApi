using Carter;
using MediatR;
using SimpleQuiz.Api.Abstractions;
using SimpleQuiz.Api.Abstractions.Operations;
using SimpleQuiz.Api.Entities;
using SimpleQuiz.Api.Shared;

namespace SimpleQuiz.Api.Features.Questions;

public class DeleteQuestion
{
    public record Command(int id) : ICommand;

    internal sealed class Handler : ICommandHandler<Command>
    {
        private readonly IRepository<Question> _questionsRepository;

        public Handler(IRepository<Question> questionsRepository)
        {
            _questionsRepository = questionsRepository;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {

            var question = await _questionsRepository.GetByIdAsync(request.id);

            if (question is null)
            {
                return Result.Failure(new Error("No Question Found","Question not found"));
            }

            await _questionsRepository.DeleteAsync(question);

            return Result.Success();
        }
    }
}

public class DeleteAnswerOptionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/question/delete", async (IMediator mediator, int id) =>
        {
            var result = await mediator.Send(new DeleteQuestion.Command(id));

            return result.IsSuccess
                   ? Result.Success()
                   : Result.Failure(result.Error);
        })
        .WithTags("Questions");
    }
}
