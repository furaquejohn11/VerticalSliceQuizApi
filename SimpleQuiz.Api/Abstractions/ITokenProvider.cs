using SimpleQuiz.Api.Entities;

namespace SimpleQuiz.Api.Abstractions;

public interface ITokenProvider
{
    string Create(User user);
}
