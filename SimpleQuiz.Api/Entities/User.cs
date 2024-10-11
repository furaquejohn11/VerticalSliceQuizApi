namespace SimpleQuiz.Api.Entities;

public class User
{
    private User(
        Guid id,
        string username,
        string password,
        string firstName,
        string lastName
        )
    {
        Id = id;
        Username = username;
        Password = password;
        FirstName = firstName;
        LastName = lastName;
    }
    public Guid Id { get; private set; }
    public string Username { get; private set; }
    public string Password { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }

    public static User Create(string username, string password, string firstName, string lastName)
    {
        var user = new User(
            Guid.NewGuid(),
            username,
            password,
            firstName,
            lastName
            );

        return user;
    }

    public void Update(string username, string password, string firstName, string lastName)
    {
        Username = username;
        Password = password;
        FirstName = firstName;
        LastName = lastName;
    }
}
