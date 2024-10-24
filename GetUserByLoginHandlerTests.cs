using FluentValidation;
using FluentValidation.Results;
using Moq;
using FluentAssertions;
using SimpleQuiz.Api.Abstractions;
using SimpleQuiz.Api.Entities;
using SimpleQuiz.Api.Services;
using SimpleQuiz.Api.Shared;
using Xunit;

namespace SimpleQuiz.Api.Features.Users.Tests;

public class GetUserByLoginHandlerTests
{
    private readonly Mock<IRepository<User>> _userRepositoryMock;
    private readonly Mock<IValidator<GetUserByLogin.Query>> _validatorMock;
    private readonly Mock<PasswordHasher> _passwordHasherMock;
    private readonly Mock<TokenProvider> _tokenProviderMock;
    private readonly GetUserByLogin.Handler _handler;

    // Define test credentials as constants
    private const string ValidUsername = "usersdsds1";
    private const string ValidPassword = "password1";

    public GetUserByLoginHandlerTests()
    {
        _userRepositoryMock = new Mock<IRepository<User>>();
        _validatorMock = new Mock<IValidator<GetUserByLogin.Query>>();
        _passwordHasherMock = new Mock<PasswordHasher>();
        _tokenProviderMock = new Mock<TokenProvider>();

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
            .Setup(r => r.FindAsync(It.IsAny<Func<User, bool>>()))
            .ReturnsAsync((User)null);

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
        var query = new GetUserByLogin.Query(ValidUsername, "wrongpassword");
        var user = new User { Username = ValidUsername, Password = "hashedpassword" };

        _validatorMock
            .Setup(v => v.Validate(query))
            .Returns(new ValidationResult());

        _userRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Func<User, bool>>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(h => h.VerifyPassword(query.password, user.Password))
            .Returns(false);

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
        var user = new User { Username = ValidUsername, Password = "hashedpassword" };
        var expectedToken = "jwt-token";

        _validatorMock
            .Setup(v => v.Validate(query))
            .Returns(new ValidationResult());

        _userRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Func<User, bool>>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(h => h.VerifyPassword(ValidPassword, user.Password))
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