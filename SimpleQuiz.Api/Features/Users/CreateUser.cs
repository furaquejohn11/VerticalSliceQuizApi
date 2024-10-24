using Carter;
using FluentValidation;
using MediatR;
using SimpleQuiz.Api.Abstractions;
using SimpleQuiz.Api.Abstractions.Operations;
using SimpleQuiz.Api.Database;
using SimpleQuiz.Api.Entities;
using SimpleQuiz.Api.Services;
using SimpleQuiz.Api.Shared;

namespace SimpleQuiz.Api.Features.Users;

public static class CreateUser
{
    public record Command(
        string Username,
        string Password,
        string FirstName,
        string LastName
    ) : ICommand;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Username).NotEmpty();
            RuleFor(c => c.Password).NotEmpty().MinimumLength(8);
            RuleFor(c => c.FirstName).NotEmpty();
            RuleFor(c => c.LastName).NotEmpty();
        }
    }

    internal sealed class Handler : ICommandHandler<Command>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IValidator<Command> _validator;
        private readonly IPasswordHasher _passwordHasher;

        public Handler(
            IRepository<User> userRepository,
            IValidator<Command> validator,
            IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _validator = validator;
            _passwordHasher = passwordHasher;
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

            string hashedPassword = _passwordHasher.HashPassword(request.Password);

            var user = User.Create(
                request.Username,
                hashedPassword,
                request.FirstName,
                request.LastName
            );

            await _userRepository.AddAsync(user);

            return Result.Success();
        }
    }
}

public class CreateUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/users/signup", async (IMediator mediator, CreateUser.Command command) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? Results.Ok()
                : Results.BadRequest(result.Error);
        })
        .WithTags("Users");
    }
}
