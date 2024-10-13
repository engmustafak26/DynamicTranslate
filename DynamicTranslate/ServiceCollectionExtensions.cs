using DynamicTranslate.DB;
using DynamicTranslate.Translation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;


namespace DynamicTranslate
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDynamicTranslate(this IServiceCollection services,
            Action<DbContextOptionsBuilder> optionsAction = null,
            ITranslateEngine CustomTranslateEngine = null)
        {
            if (optionsAction != null)
            {
                services.AddDbContext<DB.TranslationDbContext>(optionsAction);
            }

            if (CustomTranslateEngine == null)
            {
                services.AddScoped<ITranslateEngine, DefaultTranslateEngine>();
            }
            else
            {
                services.AddScoped(typeof(ITranslateEngine), CustomTranslateEngine.GetType());

            }
            services.AddHttpClient(nameof(DefaultTranslateEngine));
            ObjectExtensions.ServiceProvider = services.BuildServiceProvider();

            //var dbContext = ObjectExtensions.ServiceProvider.GetService<TranslationDbContext>();
            //if(dbContext != null)
            //{
            //    dbContext.Database.Migrate();
            //}


            return services;
        }
    }
}
