﻿// See https://aka.ms/new-console-template for more information

using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StatementProcessor.OutputConnector;
using StatementProcessor.StatementConnector;

namespace StatementProcessor;

internal static class Program
{
    public static void Main(string[] args)
    {
        // var builder = new ConfigurationBuilder();
            
        // BuildConfig(builder);

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddTransient<StatementProcessorService>();
                services.AddTransient<IBankStatementFactory, CSVBankStatementFactory>();
                services.AddTransient<IOutputConnector, GoogleSheetsConnector>();
                services.AddTransient<IFileSystem, FileSystem>();
                services.AddOptions<AppSettings>().Bind(context.Configuration.GetSection(AppSettings.SectionName));
                services.AddTransient<AppSettings>();
            })
            .Build();

        var svc = ActivatorUtilities.CreateInstance<StatementProcessorService>(host.Services);
        svc.Run();
    }

    /*
static void BuildConfig(IConfigurationBuilder builder)
{
    builder.SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile(
            $"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development"}.json",
            optional: true)
        .AddEnvironmentVariables()
        .Build();
}*/
}