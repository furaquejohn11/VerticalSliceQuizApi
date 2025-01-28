using Carter;
using FluentValidation;
using MediatR;
using SimpleQuiz.Api.Abstractions.Operations;
using SimpleQuiz.Api.Abstractions;
using SimpleQuiz.Api.Entities;
using SimpleQuiz.Api.Shared;
using SimpleQuiz.Api.Services;
using SimpleQuiz.Api.Abstractions.Authorizations;

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
        private readonly IAuthorizationService<Guid> _authorizationService;

        public Handler(
            IRepository<Quiz> quizRepository,
            IValidator<Command> validator,
            IAuthorizationService<Guid> authorizationService)
        {
            _quizRepository = quizRepository;
            _validator = validator;
            _authorizationService = authorizationService;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            // Validate the command
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                return Result.Failure<string>(new Error(
                    "Quiz.Validation",
                    validationResult.ToString()));
            }

            // Check if the user is authorized to delete the quiz
            var isAuthorized = await _authorizationService
                                    .IsUserAuthorizedToModifyAsync<QuizAuthorizationStrategy>(request.id);
            if (!isAuthorized)
            {
                return Result.Failure(new Error("Authorization.Failure", "User is not authorized to delete this quiz."));
            }

            // Check if the quiz exists
            var quiz = await _quizRepository.GetByIdAsync(request.id);
            if (quiz is null)
            {
                return Result.Failure(new Error("Quiz.NotFound", "Quiz cannot be found."));
            }

            // Delete the quiz
            await _quizRepository.DeleteAsync(quiz);

            return Result.Success();
        }
    }
}

public class DeleteQuizEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/quiz/delete", async (IMediator mediator, Guid id) =>
        {
            var result = await mediator.Send(new DeleteQuiz.Command(id));

            return result.IsSuccess
                ? Results.Ok(result)
                : Results.BadRequest(result.Error);
        })
        .WithTags("Quizzes");
    }
}
