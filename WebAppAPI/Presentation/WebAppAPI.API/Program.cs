using Microsoft.AspNetCore.Identity;
using WebAppAPI.API.Extensions;
using WebAppAPI.API.Extensions.Observability;
using WebAppAPI.API.Extensions.ServiceCollection;
using WebAppAPI.API.Middlewares;
using WebAppAPI.Domain.Entities.Identity;
using WebAppAPI.Persistence.Seeding;
using WebAppAPI.SignalR;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

#region Services
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