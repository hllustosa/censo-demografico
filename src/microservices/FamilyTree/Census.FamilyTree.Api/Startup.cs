using Census.FamilyTree.Application.Events;
using Census.FamilyTree.Domain.Repository;
using Census.FamilyTree.Infra.Connection;
using Census.FamilyTree.Infra.Repository;
using Census.Shared.Bus;
using Census.Shared.Bus.Event;
using Census.Shared.Bus.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static Census.FamilyTree.Application.Behaviour.LoggingBehaviour;

namespace Census.FamilyTree.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMediatR(typeof(LoggingBehavior<,>).Assembly);
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient<INeo4jConnection, Neo4jConnection>();
            services.AddTransient<IPersonFamilyTreeRepository, PersonFamilyTreeRepository>();

            services.AddTransient<PersonCreatedEventHandler>();
            services.AddTransient<PersonDeletedEventHandler>();
            services.AddTransient<PersonUpdatedEventHandler>();
            services.AddEventBus(Configuration);
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IEventBus eventBus)
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
            app.UseCors(option => option.AllowAnyOrigin());
            app.UseMvc();
            

            // Subscribing to events
            eventBus.Subscribe<PersonCreatedEvent, PersonCreatedEventHandler>();
            eventBus.Subscribe<PersonUpdatedEvent, PersonUpdatedEventHandler>();
            eventBus.Subscribe<PersonDeletedEvent, PersonDeletedEventHandler>();
        }
    }
}
