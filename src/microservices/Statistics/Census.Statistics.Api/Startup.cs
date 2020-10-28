using Census.People.Infra.Repository;
using Census.Shared.Bus;
using Census.Shared.Bus.Event;
using Census.Shared.Bus.Interfaces;
using Census.Statistics.Api.Hubs;
using Census.Statistics.Application;
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
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMediatR(typeof(BaseEventHandler).Assembly);
            services.AddTransient<IMongoConnection, MongoConnection>();
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
            services.AddTransient<IGuidGenerator, GuidGenerator>();

            services.AddTransient<ITransactionManager, MongoTransactionManager>();
            services.AddTransient<IPersonCategoryRepository, PersonCategoryRepository>();
            services.AddTransient<IPersonPerCityCounterRepository, PersonPerCityCounterRepository>();

            services.AddTransient<PersonCreatedEventHandler>();
            services.AddTransient<PersonDeletedEventHandler>();
            services.AddTransient<PersonUpdatedEventHandler>();

            services.AddSignalR(o => o.EnableDetailedErrors = true);
            services.AddScoped<NotificationHub>();
            services.AddScoped<INotificationSender, NotificationHub>();

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                    builder.SetIsOriginAllowed(_ => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });

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
                //app.UseHsts();
            }

            app.UseCors();
            app.UseSignalR(builder => { builder.MapHub<NotificationHub>("/hubs/notification"); });
            app.UseMvc();

            //Subscribing to events
            eventBus.Subscribe<PersonCreatedEvent, PersonCreatedEventHandler>();
            eventBus.Subscribe<PersonUpdatedEvent, PersonUpdatedEventHandler>();
            eventBus.Subscribe<PersonDeletedEvent, PersonDeletedEventHandler>();
        }
    }
}
