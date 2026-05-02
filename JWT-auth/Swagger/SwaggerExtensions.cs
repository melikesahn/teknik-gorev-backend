using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace JWT_auth.Swagger;

public static class SwaggerExtensions
{
    public static IServiceCollection AddJwtAuthSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "JWT Auth API",
                Version = "v1"
            });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "POST /api/Auth/login yanıtındaki accessToken değerini yapıştır. Sadece token metni (önüne 'Bearer ' yazma; Swagger otomatik ekler).",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT"
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}
