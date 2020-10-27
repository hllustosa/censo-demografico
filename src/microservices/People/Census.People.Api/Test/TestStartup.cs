using System.Reflection;
using Census.People.Application.Behaviour;
using MediatR;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Census.People.Domain.Interfaces;
using Census.People.Infra.Repository;
using Census.People.Infra.Connection;
using Census.Shared.Bus;
using Census.Shared.Bus.Interfaces;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Census.People.Api
{
    public class TestStartup
    {
        public TestStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMediatR(typeof(ValidatorAssembly).Assembly);
            services.AddTransient<IMongoConnection, MongoConnection>();
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
            services.AddTransient<IPersonRepository, PersonRepository>();
            services.AddTransient<IEventBus, MockBus>();
            services.AddTransient<IGuidGenerator, MockGuidGenerator>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<ValidatorAssembly>());
        }

        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }


    public class MockBus : IEventBus
    {
        public void Publish(IntegrationEvent @event)
        {
            
        }

        public void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
           
        }

        public void Unsubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {

        }
    }

    class MockGuidGenerator : IGuidGenerator
    {
        public string GenerateGuid()
        {
            return "id";
        }
    }
}
