using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using WebAppAPI.API.Extensions;
using WebAppAPI.API.Filters;
using WebAppAPI.API.Middlewares;
using WebAppAPI.API.Options.Observability;
using WebAppAPI.API.Options.Observability.Validation;
using WebAppAPI.Application;
using WebAppAPI.Application.Validators.Products;
using WebAppAPI.Domain.Constants;
using WebAppAPI.Domain.Entities.Identity;
using WebAppAPI.Infrastructure;
using WebAppAPI.Infrastructure.Filters;
using WebAppAPI.Infrastructure.Services.Storage.Local;
using WebAppAPI.Persistence;
using WebAppAPI.Persistence.Seeding;
using WebAppAPI.SignalR;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

#region variables
var tokenValidationParameters = new TokenValidationParameters
{
    ValidateAudience = true,
    ValidateIssuer = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,

    ValidAudience = builder.Configuration["Token:Audience"],
    ValidIssuer = builder.Configuration["Token:Issuer"],
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Token:SecurityKey"])),

    // If you have 'token lifetime problem' try this.
    //LifetimeValidator = (notBefore, expires, SecurityToken, validationParameters)
    //        => expires != null ? expires > DateTime.UtcNow : false,
    ClockSkew = TimeSpan.Zero,

    NameClaimType = ClaimTypes.Name
};
#endregion

#region Services
builder.Services.AddHttpContextAccessor(); // A service that provides access to the HttpContext from the business logic layer.

// Here we call the extension method that adds services to the IoC Container.
// However, in order to use this extension method here, we need to add the Presentation Project(Layer) as a reference to this project.
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddInfrastructureServices();
builder.Services.AddApplicationServices();
builder.Services.AddSignalRServices();

//builder.Services.AddStorage<AzureStorage>();
builder.Services.AddStorage<LocalStorage>();

var allowedOrigin = builder.Configuration["AngularClientUrl"] ?? throw new InvalidOperationException("AngularClientUrl must be configured in appsettings or environment variables.");
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins(allowedOrigin).AllowAnyHeader().AllowAnyMethod().AllowCredentials()
));

builder.Services
    .AddOptions<ObservabilityOptions>()
    .Bind(builder.Configuration.GetSection(ObservabilityOptions.SectionName));
//.ValidateOnStart();

builder.Services.AddSingleton<IValidateOptions<ObservabilityOptions>, ObservabilityOptionsValidator>();

builder.Services.AddSingleton(tokenValidationParameters);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
    options.Filters.Add<RolePermissionFilter>();
}).ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true);
builder.Services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters().AddValidatorsFromAssemblyContaining<ProductCreateValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(AuthSchemes.Authenticated, options =>
    {
        options.TokenValidationParameters = tokenValidationParameters;

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Cookies["accessToken"];
                if (!string.IsNullOrEmpty(accessToken))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });
#endregion

var app = builder.Build();

#region Seed Sample Entity Data
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var userManager = sp.GetRequiredService<UserManager<AppUser>>();
    await DemoSeed.CheckAndSeedAsync(sp, userManager);
}
#endregion

#region Middlewares
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.ConfigureExceptionHandler<Program>(app.Services.GetRequiredService<ILogger<Program>>());

app.UseStaticFiles();
app.UseCors();
app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<EndpointAdminCheckMiddleware>();

app.UseStatusCodePages();

app.MapControllers();
app.MapHubs();
#endregion

app.Run();