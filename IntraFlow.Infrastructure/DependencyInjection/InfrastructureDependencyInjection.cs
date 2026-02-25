using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using IntraFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using IntraFlow.Application.Abstractions;
using IntraFlow.Infrastructure.Email;

namespace IntraFlow.Infrastructure.DependencyInjection
{
    public static class InfrastructureDependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

             services.AddDbContext<AppDbContext>(options =>
             options.UseSqlServer(connectionString));

            services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
            services.AddScoped<IEmailSender, DevEmailSender>();

            return services;
        }
    }
}
