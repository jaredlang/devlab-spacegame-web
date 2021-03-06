﻿using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using System.Reflection;
using TailSpin.SpaceGame.DBRepository; 

[assembly: FunctionsStartup(typeof(TailSpin.SpaceGame.LeaderboardFunction.Startup))]
namespace TailSpin.SpaceGame.LeaderboardFunction
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // https://github.com/Azure/azure-functions-host/issues/4517#issuecomment-497940595
            var actual_root = Environment.GetEnvironmentVariable("AzureWebJobsScriptRoot")  // local_root
                    ?? (Environment.GetEnvironmentVariable("HOME") == null
                        ? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                        : $"{Environment.GetEnvironmentVariable("HOME")}/site/wwwroot"); // azure_root
            
            var configBuilder = new ConfigurationBuilder()
                  .SetFileProvider(new PhysicalFileProvider(actual_root))
                  //.AddJsonFile("local.settings.json", optional: false, reloadOnChange: true);
                  .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var appSettingsConfig = configBuilder.Build();

            if (appSettingsConfig["Values:DBRepositoryType"] == "AzureSQL")
            {
                /*
                // if it is a local run, 
                // if (actual_root.Equals(Environment.GetEnvironmentVariable("AzureWebJobsScriptRoot")))
                if ("Development".Equals(Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT"), StringComparison.OrdinalIgnoreCase))
                {
                    configBuilder.AddUserSecrets<Startup>();
                    appSettingsConfig = configBuilder.Build();
                }
                */

                if (appSettingsConfig["Values:DatabaseConnection"] != null) 
                {
                    string connectionString = appSettingsConfig["Values:DatabaseConnection"];
                    builder.Services.AddSingleton<IDocumentDBRepository>(new RemoteDBRepository(connectionString));
                    return;
                }
            }

            builder.Services.AddSingleton<IDocumentDBRepository>(new LocalDocumentDBRepository(
                Path.Combine(actual_root, @"FileData/scores.json"), Path.Combine(actual_root, @"FileData/profiles.json")));

        }
    }
}
