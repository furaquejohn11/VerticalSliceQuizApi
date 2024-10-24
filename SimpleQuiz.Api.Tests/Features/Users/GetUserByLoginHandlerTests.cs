using FluentValidation;
using FluentValidation.Results;
using Moq;
using FluentAssertions;
using SimpleQuiz.Api.Abstractions;
using SimpleQuiz.Api.Entities;
using SimpleQuiz.Api.Services;
using SimpleQuiz.Api.Features.Users.Infrastructure;
using SimpleQuiz.Api.Shared;
using Xunit;
using System.Linq.Expressions;
using Microsoft.Extensions.Configuration;

namespace SimpleQuiz.Api.Features.Users.Tests;

public class GetUserByLoginHandlerTests
{
    private readonly Mock<IRepository<User>> _userRepositoryMock;
    private readonly Mock<IValidator<GetUserByLogin.Query>> _validatorMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ITokenProvider> _tokenProviderMock;
    private readonly GetUserByLogin.Handler _handler;

    // Define test credentials as constants
    private const string ValidUsername = "user1";
    private const string ValidPassword = "password1";

    public GetUserByLoginHandlerTests()
    {
        _userRepositoryMock = new Mock<IRepository<User>>();
        _validatorMock = new Mock<IValidator<GetUserByLogin.Query>>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _tokenProviderMock = new Mock<ITokenProvider>();

        // Pass the mocks to the handler
        _handler = new GetUserByLogin.Handler(
            _userRepositoryMock.Object,
            _validatorMock.Object,
            _passwordHasherMock.Object,
            _tokenProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ReturnsFailureResult()
    {
        // Arrange
        var query = new GetUserByLogin.Query(ValidUsername, "short");
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("password", "Password must be at least 8 characters")
        };
        var validationResult = new ValidationResult(validationFailures);
        
        _validatorMock
            .Setup(v => v.Validate(query))
            .Returns(validationResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Login.Validation");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsLoginError()
    {
        // Arrange
        var query = new GetUserByLogin.Query("nonexistentuser", ValidPassword);
        
        _validatorMock
            .Setup(v => v.Validate(query))
            .Returns(new ValidationResult());

        _userRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Login Error");
        result.Error.Message.Should().Be("Your username or password is incorrect");
    }

    [Fact]
    public async Task Handle_WhenPasswordIncorrect_ReturnsLoginError()
    {
        // Arrange
        var query = new GetUserByLogin.Query(ValidUsername, ValidPassword);

        string hashedPassword = _passwordHasherMock.Object.HashPassword(ValidPassword);
        var user = User.Create(ValidUsername, hashedPassword, "John", "Doe");

        _validatorMock
            .Setup(v => v.Validate(query))
            .Returns(new ValidationResult());

        _userRepositoryMock
            .Setup(r => r.FindAsync(It.Is<Expression<Func<User, bool>>>(expr =>
                expr.Compile().Invoke(user))))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Login Error");
        result.Error.Message.Should().Be("Your username or password is incorrect");
    }

    [Fact]
    public async Task Handle_WhenCredentialsValid_ReturnsSuccessWithToken()
    {
        // Arrange
        var query = new GetUserByLogin.Query(ValidUsername, ValidPassword);

        // Simulate the user existing in the database with the correct username
        string hashedPassword = _passwordHasherMock.Object.HashPassword(ValidPassword);

        // Use DDD-style user creation (with factory method)
        var user = User.Create(ValidUsername, hashedPassword, "John", "Doe");

        var expectedToken = "jwt-token";

        _validatorMock
            .Setup(v => v.Validate(query))
            .Returns(new ValidationResult());

        // Modify the repository mock setup to ensure it returns the user only if the username matches
        _userRepositoryMock
            .Setup(r => r.FindAsync(It.Is<Expression<Func<User, bool>>>(expr =>
                expr.Compile().Invoke(user))))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(h => h.VerifyPassword(ValidPassword, hashedPassword))
            .Returns(true);

        _tokenProviderMock
            .Setup(t => t.Create(user))
            .Returns(expectedToken);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedToken);
    }


    [Fact]
    public void Validator_WhenUsernameEmpty_HasValidationError()
    {
        // Arrange
        var validator = new GetUserByLogin.Validator();
        var query = new GetUserByLogin.Query("", ValidPassword);

        // Act
        var result = validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "username");
    }

    [Fact]
    public void Validator_WhenPasswordTooShort_HasValidationError()
    {
        // Arrange
        var validator = new GetUserByLogin.Validator();
        var query = new GetUserByLogin.Query(ValidUsername, "short");

        // Act
        var result = validator.Validate(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "password");
    }
}