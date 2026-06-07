using Microsoft.AspNetCore.Identity;
using WebAppAPI.API.Extensions.ApplicationBuilder;
using WebAppAPI.API.Extensions.Observability;
using WebAppAPI.API.Extensions.ServiceCollection;
using WebAppAPI.Domain.Entities.Identity;
using WebAppAPI.Persistence.Seeding;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

#region Services
builder.Services.AddApiConfigurationOptions(builder.Configuration);
builder.Services.AddWebAppApiDependencies(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddApiObservability(builder.Configuration);
builder.Services.AddApiAuthentication(builder.Configuration);
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
app.UseApiSwagger();
app.UseApiPipeline<Program>(builder.Configuration);
app.MapApiEndpoints();
#endregion

app.Run();