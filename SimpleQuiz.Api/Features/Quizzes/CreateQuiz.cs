using Carter;
using FluentValidation;
using MediatR;
using SimpleQuiz.Api.Abstractions.Operations;
using SimpleQuiz.Api.Database;
using SimpleQuiz.Api.Entities;
using SimpleQuiz.Api.Features.Users;
using SimpleQuiz.Api.Shared;

namespace SimpleQuiz.Api.Features.Quizzes;

public static class CreateQuiz
{
    public record Command(
        Guid UserId,
        string Title,
        string Description,
        bool IsPublic
        ) : ICommand;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.UserId)
            .NotEmpty().WithMessage("User ID is required.");

            RuleFor(c => c.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(100).WithMessage("Title must not exceed 100 characters.");

            RuleFor(c => c.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

            RuleFor(c => c.IsPublic)
                .NotNull().WithMessage("IsPublic flag must be set.");
        }
    }

    internal sealed class Handler : ICommandHandler<Command>
    {
        private readonly AppDbContext _appDbContext;
        private readonly IValidator<Command> _validator;

        public Handler(AppDbContext appDbContext, IValidator<Command> validator)
        {
            _appDbContext = appDbContext;
            _validator = validator;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                return Result.Failure(new Error(
                    "CreateUser.Validation",
                    validationResult.ToString()));
            }

            var quiz = Quiz.Create(
                request.UserId,
                request.Title,
                request.Description,
                request.IsPublic
                );

            await _appDbContext.Quizzes.AddAsync(quiz);
            await _appDbContext.SaveChangesAsync();

            return Result.Success(quiz);
        }
    }
}

public class CreateQuizEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/quiz/create", async (IMediator mediator, CreateQuiz.Command command) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? Results.Ok()
                : Results.BadRequest(result.Error);
        })
        .WithTags("Quizzes");
    }
}
