using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace IntraFlow.Application.DependencyInjection
{
    public static class ApplicationDependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // services.AddScoped<IExampleService, ExampleService>();
            return services;
        }
    }
}
