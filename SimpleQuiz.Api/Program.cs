using Carter;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using SimpleQuiz.Api.Database;
using SimpleQuiz.Api.Extensions;
using SimpleQuiz.Api.Services;
using SimpleQuiz.Api.Features.Users.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SimpleQuiz.Api.Persistence;
using SimpleQuiz.Api.Abstractions;
using FluentAssertions.Common;
using SimpleQuiz.Api.Abstractions.Authorizations;

var builder = WebApplication.CreateBuilder(args);

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builder =>
    {
        builder.WithOrigins("http://localhost:5173") // specify your allowed origins
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGenWithAuth();

var connectionString = builder.Configuration.GetConnectionString("AppDbConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

var assembly = typeof(Program).Assembly;

builder.Services.AddMediatR(config => config.RegisterServicesFromAssembly(assembly));

builder.Services.AddCarter();
builder.Services.AddValidatorsFromAssembly(assembly);

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IPasswordHasher,PasswordHasher>();
builder.Services.AddSingleton<ITokenProvider,TokenProvider>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContextService, UserContextService>();


// TODO FIX THIS
builder.Services.AddTransient(typeof(IAuthorizationService<>), typeof(AuthorizationService<>));
// Registering all strategies for different types dynamically
builder.Services.AddScoped<IAuthorizationStrategy<Guid>, QuizAuthorizationStrategy>();
builder.Services.AddScoped<IAuthorizationStrategy<int>, QuestionAuthorizationStrategy>();
builder.Services.AddScoped<IAuthorizationStrategy<int>, AnswerOptionAuthorizationStrategy>();



builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.RequireHttpsMetadata = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ClockSkew = TimeSpan.Zero
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowSpecificOrigin");

app.UseAuthentication();
app.UseAuthorization();

app.MapCarter();

app.Run();
