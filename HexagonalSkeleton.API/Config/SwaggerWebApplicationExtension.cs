namespace HexagonalSkeleton.API.Config
{
    /// <summary>
    /// This class holds extension methods for the WebApplication type.
    /// </summary>
    public static class SwaggerWebApplicationExtension
    {
        /// <summary>
        /// Extension method to add Swagger to a WebApplication instance.
        /// </summary>
        /// <param name="app">Web application used to configure the HTTP pipeline, and routes.</param>
        /// <returns>Return the modified WebApplication instance.</returns>
        public static WebApplication MapSwagger(this WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API");
                c.RoutePrefix = "api";
            });

            return app;
        }
    }
}
