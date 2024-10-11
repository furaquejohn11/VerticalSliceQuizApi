using Carter;
using FluentValidation;
using MediatR;
using SimpleQuiz.Api.Abstractions;
using SimpleQuiz.Api.Abstractions.Operations;
using SimpleQuiz.Api.Entities;
using SimpleQuiz.Api.Shared;

namespace SimpleQuiz.Api.Features.Quizzes;

public static class DeleteQuiz
{
    public record Command(Guid id) : ICommand;
    public class Validator : AbstractValidator<Command>
    {
        public Validator() 
        {
            RuleFor(c => c.id).NotEmpty();
        }
    }

    internal sealed class Handler : ICommandHandler<Command>
    {
        private readonly IRepository<Quiz> _quizRepository;
        private readonly IValidator<Command> _validator;

        public Handler(IRepository<Quiz> quizRepository, IValidator<Command> validator)
        {
            _quizRepository = quizRepository;
            _validator = validator;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                return Result.Failure<string>(new Error(
                    "Quiz.Validation",
                    validationResult.ToString()));
            }

            var quiz = await _quizRepository.GetByIdAsync(request.id);

            if (quiz is null)
            {
                return Result.Failure(new Error("QUIZ NOT FOUND","QUIZ CANT BE FOUND"));
            }

            await _quizRepository.DeleteAsync(quiz);

            return Result.Success();
        }
    }
}

public class DeleteQuizEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/quiz/delete", async(IMediator mediator, Guid id) =>
        {
            var result = await mediator.Send(new DeleteQuiz.Command(id));

            return result.IsSuccess
                   ? Result.Success()
                   : Result.Failure(result.Error);
        })
        .WithTags("Quizzes");
    }
}