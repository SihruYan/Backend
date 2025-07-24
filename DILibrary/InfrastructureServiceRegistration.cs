using Backend.Repository;
using Backend.Services;

namespace Backend.DILibrary;

public static class InfrastructureServiceRegistration
{
    /// <summary>
    /// Repository , Service 註冊
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        #region Service

        services.AddScoped<IEmailService, MailKitService>();

        #endregion
        #region repository

        services.AddScoped<IFormRepository,FormRepository>();


        #endregion
        return services;

    }
}