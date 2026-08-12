using AssignmentSystem.Application.Common.Interfaces;
using AssignmentSystem.Infrastructure.Email;
using AssignmentSystem.Infrastructure.Persistence;
using AssignmentSystem.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AssignmentSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUserRepository, AuthUserRepository>();
        services.AddScoped<IProfileRepository, ProfileRepository>();
        services.AddScoped<IGradeRepository, GradeRepository>();
        services.AddScoped<ISectionRepository, SectionRepository>();
        services.AddScoped<ISubjectRepository, SubjectRepository>();
        services.AddScoped<ISectionSubjectRepository, SectionSubjectRepository>();
        services.AddScoped<IAssignmentRepository, AssignmentRepository>();
        services.AddScoped<ISubmissionRepository, SubmissionRepository>();
        services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();
        services.AddScoped<IPasswordResetCodeRepository, PasswordResetCodeRepository>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        if (isDevelopment)
        {
            services.AddScoped<IEmailSender, LoggingEmailSender>();
        }
        else
        {
            services.AddScoped<IEmailSender, SmtpEmailSender>();
        }

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<BCryptSettings>(configuration.GetSection(BCryptSettings.SectionName));
        services.Configure<SmtpSettings>(configuration.GetSection(SmtpSettings.SectionName));

        return services;
    }
}
