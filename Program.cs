using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NotificationApi.Configurations;
using NotificationApi.Services.Implementations;
using NotificationApi.Services.Interfaces;
using NotificationApi.Services.Settings;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Tokens from either Auth0 application are accepted.
string[] authSchemes = ["Auth0App1", "Auth0App2"];

builder.Services.AddAutoMapper(typeof(NotificationApi.Mappers.ContractMapping));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Auth0 access token"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddAWSService<Amazon.SimpleEmail.IAmazonSimpleEmailService>();
builder.Services.AddTransient(sp => new Lazy<Amazon.SimpleEmail.IAmazonSimpleEmailService>(
    () => sp.GetRequiredService<Amazon.SimpleEmail.IAmazonSimpleEmailService>()));
builder.Services.AddTransient<INotificationService, NotificationService>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var authentication = builder.Services.AddAuthentication();
foreach (string scheme in authSchemes)
{
    authentication.AddJwtBearer(scheme, options =>
    {
        string? issuer = configuration[$"{scheme}:Issuer"] ?? Environment.GetEnvironmentVariable($"{scheme}.Issuer");
        options.Audience = configuration[$"{scheme}:Audience"] ?? Environment.GetEnvironmentVariable($"{scheme}.Audience");
        options.Authority = issuer;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer
        };
    });
}

builder.Services.AddAuthorizationBuilder()
    .SetDefaultPolicy(new AuthorizationPolicyBuilder(authSchemes)
        .RequireAuthenticatedUser()
        .Build());

builder.Services.Configure<SMTPSetting>(configuration.GetSection(nameof(SMTPSetting)));
builder.Services.Configure<AWSSetting>(configuration.GetSection(nameof(AWSSetting)));

builder.Services.AddControllers();

var app = builder.Build();

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
