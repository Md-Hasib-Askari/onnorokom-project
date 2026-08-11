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
        services.AddScoped<IProfileProvisioningService, ProfileProvisioningService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAdminUserService, AdminUserService>();
        services.AddScoped<IGradeService, GradeService>();
        services.AddScoped<ISectionService, SectionService>();
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<ISectionSubjectService, SectionSubjectService>();
        services.AddScoped<IAdminQueryService, AdminQueryService>();
        services.AddScoped<ISystemSettingService, SystemSettingService>();
        services.AddScoped<ITeacherAssignmentService, TeacherAssignmentService>();
        return services;
    }
}
