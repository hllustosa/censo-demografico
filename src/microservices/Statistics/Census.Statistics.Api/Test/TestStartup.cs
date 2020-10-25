using Census.People.Infra.Repository;
using Census.Shared.Bus;
using Census.Shared.Bus.Event;
using Census.Shared.Bus.Interfaces;
using Census.Statistics.Application.Behaviour;
using Census.Statistics.Application.Events;
using Census.Statistics.Domain.Interfaces;
using Census.Statistics.Infra.Connection;
using Census.Statistics.Infra.Repository;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Census.Statistics.Api
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
            services.AddMediatR(typeof(BaseEventHandler).Assembly);
            services.AddTransient<IMongoConnection, MongoConnection>();
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

            services.AddTransient<ITransactionManager, MongoTransactionManager>();
            services.AddTransient<IPersonCategoryRepository, PersonCategoryRepository>();
            services.AddTransient<IPersonPerCityCounterRepository, PersonPerCityCounterRepository>();

            services.AddTransient<IEventBus, MockBus>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
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
}
