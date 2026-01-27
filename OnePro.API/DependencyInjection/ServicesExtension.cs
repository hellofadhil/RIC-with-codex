using Microsoft.Extensions.DependencyInjection.Extensions;
using OnePro.API.Interfaces;
using OnePro.API.Repositories;

namespace OnePro.API.DependencyInjection
{
    public static class ServicesExtension
    {
        public static IServiceCollection AddConfigureServices(this IServiceCollection services)
        {
            services.AddScoped<IRicRepository, RicRepository>();
            services.AddScoped<IGroupRepository, GroupRepository>();
            services.AddScoped<IRicRollOutRepository, RicRollOutRepository>();
            // services.AddScoped<IUserRepository, UserRepository>();
            // services.AddScoped<IFormRicApprovalRepository, FormRicApprovalRepository>();
            // services.AddScoped<IReviewFormRicRepository, ReviewFormRicRepository>();
            // services.AddScoped<IFormRicHistoryRepository, FormRicHistoryRepository>();
            services.AddScoped<IUndanganFormRicRepository, UndanganFormRicRepository>();

            return services;
        }
    }
}
