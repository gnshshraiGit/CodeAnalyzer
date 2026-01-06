using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.SemanticKernel.Connectors.SqliteVec;
using CodeAnalyzer;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using CodeAnalyzer.Indexers;
using CodeAnalyzer.Agents;
using Microsoft.Extensions.Logging;

internal class Program
{
    static async Task Main(string[] args)
    {
        var services = new ServiceCollection();
        // 1. Load Configuration
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        services.AddKernel();

        services.AddSingleton<IConfiguration>(config);

        services.AddSingleton<Kernel>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var endpoint = config["AzureOpenAI:Endpoint"];
            var deploymentName = config["AzureOpenAI:ModelId"];
            var apiKey = config["AzureOpenAI:Apikey"];
            var embedDeploymentName = config["AzureOpenAI:EmbedDeploymentModelId"];
            var allowLogging = config.GetValue<bool>("Logging:Enabled");
            var kernelBuilder = Kernel.CreateBuilder();
            kernelBuilder.AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);
            kernelBuilder.AddAzureOpenAIEmbeddingGenerator(embedDeploymentName, endpoint, apiKey);
            if(allowLogging)
                kernelBuilder.Services.AddLogging(s => s.AddConsole());
            return kernelBuilder.Build();
        });

        services.AddSingleton<SqliteVectorStore>(sp => {
            var config = sp.GetRequiredService<IConfiguration>();
            var connectionString = config["VectorStore:ConnectionString"];
            return new SqliteVectorStore(connectionString);
        });

        services.AddSingleton<ICodeAnalyzerCLI, CodeAnalyzerCLI>();
        services.AddSingleton<ICodeFileIndexer, CodeFileIndexer>();
        services.AddSingleton<ICodeAnalystAgentBuilder, CodeAnalystAgentBuilder>();

        var serviceProvider = services.BuildServiceProvider();

        var cli = serviceProvider.GetRequiredService<ICodeAnalyzerCLI>();

        await cli.StartAsync();
    }
}