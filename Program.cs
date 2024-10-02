using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace TaskWorkerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((context, configuration) =>
                {
                    // Verifica se o diretório de logs existe, caso contrário, cria-o
                    string logPath = @"C:\logs";
                    if (!Directory.Exists(logPath))
                    {
                        Directory.CreateDirectory(logPath);
                    }

                    // Configura o Serilog para salvar logs no arquivo
                    configuration
                        .ReadFrom.Configuration(context.Configuration)
                        .WriteTo.File(Path.Combine(logPath, "log-.txt"), rollingInterval: RollingInterval.Day)
                        .Enrich.FromLogContext();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();

                    // Verificação do sistema operacional usando RuntimeInformation
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        services.Configure<HostOptions>(option => option.ShutdownTimeout = TimeSpan.FromSeconds(20));
                    }
                })
                .UseWindowsService(); // Para rodar como serviço no Windows
    }
}
