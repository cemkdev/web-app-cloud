using WebAppAPI.API.Middlewares;
using WebAppAPI.SignalR;

namespace WebAppAPI.API.Extensions.ApplicationBuilder
{
    public static class ApiApplicationBuilderExtensions
    {
        public static WebApplication UseApiPipeline<TProgram>(this WebApplication app)
        {
            // TODO-SECTION-OBSERVABILITY: Revisit exception handling pipeline.
            // Current generic setup is preserved in Section 2B to avoid behavior changes.
            // Remove the generic dependency if the exception handling model is redesigned.
            app.ConfigureExceptionHandler<TProgram>(app.Services.GetRequiredService<ILogger<TProgram>>());

            app.UseStaticFiles();
            app.UseCors();
            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<EndpointAdminCheckMiddleware>();

            app.UseStatusCodePages();

            return app;
        }

        public static WebApplication MapApiEndpoints(this WebApplication app)
        {
            app.MapControllers();
            app.MapHubs();

            return app;
        }
    }
}
