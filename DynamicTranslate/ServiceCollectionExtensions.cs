using DynamicTranslate.DB;
using DynamicTranslate.Translation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;


namespace DynamicTranslate
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDynamicTranslate(this IServiceCollection services,
            Type dbContextType = null,
            ITranslateEngine CustomTranslateEngine = null)
        {
            if (dbContextType != null)
            {
                services.AddScoped<DbContext>(s => s.GetRequiredService(dbContextType) as DbContext);
                services.AddScoped(typeof(Repository<>), typeof(Repository<>));

            }

            if (CustomTranslateEngine == null)
            {
                services.AddScoped<ITranslateEngine, DefaultTranslateEngine>();
            }
            else
            {
                services.AddScoped(typeof(ITranslateEngine), CustomTranslateEngine.GetType());

            }
            ObjectExtensions.ServiceProvider = services.BuildServiceProvider();

            //var dbContext = ObjectExtensions.ServiceProvider.GetService<TranslationDbContext>();
            //if (dbContext != null)
            //{

            //    Process process = new Process();
            //    process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            //    process.StartInfo.FileName = "cmd.exe";
            //    process.StartInfo.Arguments = "/C dotnet ef migrations add initial_translation -Context TranslationDbContext -v";
            //    process.StartInfo.UseShellExecute = false;
            //    process.StartInfo.CreateNoWindow = true;
            //    process.StartInfo.RedirectStandardOutput = true;
            //    process.StartInfo.RedirectStandardError = true;
            //    process.OutputDataReceived += (sender, args) =>
            //    {
            //        Console.WriteLine(args.Data.ToString());
            //    };

            //    process.ErrorDataReceived += (sender, args) =>
            //    {
            //        Console.WriteLine(args.Data.ToString());
            //    };

            //    process.Start();
            //    process.BeginErrorReadLine();
            //    process.BeginOutputReadLine();

            //   process.WaitForExit();





            //    dbContext.Database.Migrate();
            //}


            return services;
        }
    }
}
