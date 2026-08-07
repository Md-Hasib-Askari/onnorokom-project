using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Application.Common.Mappings;
using AssignmentSystem.Application.Services;
using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AssignmentSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
