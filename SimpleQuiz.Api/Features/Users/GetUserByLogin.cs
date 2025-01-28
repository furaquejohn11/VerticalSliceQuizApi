using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SimpleQuiz.Api.Abstractions;
using SimpleQuiz.Api.Abstractions.Operations;
using SimpleQuiz.Api.Database;
using SimpleQuiz.Api.Entities;
using SimpleQuiz.Api.Shared;

namespace SimpleQuiz.Api.Features.Users;

public static class GetUserByLogin
{
    public record Query(
        string username,
        string password) : IQuery<string>;

    public class Validator : AbstractValidator<Query>
    {
        public Validator() 
        {
            RuleFor(q => q.username).NotEmpty();
            RuleFor(q => q.password).NotEmpty().MinimumLength(8);
        }

    }
    internal sealed class Handler : IQueryHandler<Query, string>
    { 
        private readonly IRepository<User> _userRepository;
        private readonly IValidator<Query> _validator;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenProvider _tokenProvider;

        public Handler(
           
            IRepository<User> userRepository,
            IValidator<Query> validator,
            IPasswordHasher passwordHasher,
            ITokenProvider tokenProvider)
        {
           
            _userRepository = userRepository;
            _validator = validator;
            _passwordHasher = passwordHasher;
            _tokenProvider = tokenProvider;
        }

        public async Task<Result<string>> Handle(Query request, CancellationToken cancellationToken)
        {
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                return Result.Failure<string>(new Error(
                    "Login.Validation",
                    validationResult.ToString()));
            }

            var user = await _userRepository.FindAsync(u => u.Username == request.username);

            if (user is not null && _passwordHasher.VerifyPassword(request.password, user.Password))
            {
                var token = _tokenProvider.Create(user);
                return Result.Success(token);
            }
            return Result.Failure<string>(new Error("Login Error", "Your username or password is incorrect"));

        }
    }
}

public class GetUserByLoginEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/users/login", async (IMediator mediator, GetUserByLogin.Query query) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        })
        .WithTags("Users");
    }
}

