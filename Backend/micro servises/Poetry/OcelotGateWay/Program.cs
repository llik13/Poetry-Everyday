using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using MMLib.Ocelot.Provider.AppConfiguration;
using MMLib.SwaggerForOcelot.DependencyInjection;
using Ocelot.Middleware;

namespace OcelotGateWay
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Ocelot with the json configuration files
            builder.Configuration.AddOcelotWithSwaggerSupport(options =>
            {
                options.Folder = "OcelotConfiguration";
            });

            builder.Services.AddOcelot(builder.Configuration).AddAppConfiguration();
            builder.Services.AddSwaggerForOcelot(builder.Configuration);

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add CORS support
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowLocalhost3000", policy =>
                {
                    policy
                        .WithOrigins("http://localhost:3000") // Разрешаем запросы с этого домена
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials(); // Если работаешь с куками (например, для RefreshToken)
                });
            });

            var app = builder.Build();

            // Use Swagger UI for Ocelot
            app.UseSwaggerForOcelotUI(opt =>
            {
                opt.PathToSwaggerGenerator = "/swagger/docs";
            });

            app.UseWebSockets();

            // Use CORS before Ocelot middleware
            app.UseCors("AllowLocalhost3000");

            app.UseOcelot().Wait();

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
