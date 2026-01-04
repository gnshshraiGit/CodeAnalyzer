using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Spectre.Console;
using System.Linq;
using Microsoft.SemanticKernel.Connectors.SqliteVec;
using CodeAnalyzer.Indexers;
using CodeAnalyzer.Agents;
using OpenAI.Chat;
using System;
using Microsoft.SemanticKernel.ChatCompletion;
using CodeAnalyzer.Plugins;

namespace CodeAnalyzer
{
    public interface ICodeAnalyzerCLI
    {
        Task StartAsync();
    }
    public class CodeAnalyzerCLI : ICodeAnalyzerCLI
    {
        private readonly Kernel _kernel;
        private readonly IConfiguration _config;
        private readonly SqliteVectorStore _vectorStore;
        private readonly ICodeFileIndexer _indexer;
        private readonly ICodeAnalystAgentBuilder _codeAnalystBuilder;

        public CodeAnalyzerCLI(
            Kernel kernel, 
            SqliteVectorStore sqliteVectorStore, 
            ICodeFileIndexer indexer, 
            ICodeAnalystAgentBuilder codeAnalystBuilder,
            IConfiguration config
        )
        {
            _kernel = kernel;
            _vectorStore = sqliteVectorStore;
            _config = config;
            _indexer = indexer;
            _codeAnalystBuilder = codeAnalystBuilder;
        }

        public async Task StartAsync()
        {
            var readingOptionsFileExtensions = _config.GetSection("ReadOptions:FileExtensions").GetChildren().Select(x => x.Value).ToList();
            var readingOptionsExcludePaths = _config.GetSection("ReadOptions:ExcludePaths").GetChildren().Select(x => x.Value).ToList();
            var includeSubDirectories = _config.GetSection("ReadOptions:IncludeSubDirectories").Value;
            var collectionName = _config["VectorStore:CollectionName"] ?? "snippets";

            AnsiConsole.Write(new FigletText("Code Analyzer CLI").Color(Color.Green));

            // 3. Indexing Logic
            List<string> fileExtensionFilter = readingOptionsFileExtensions;

            if (AnsiConsole.Confirm("Index local files?"))
            {
                if(AnsiConsole.Confirm("Indexing will overwrite existing data. Continue? (y/n)"))
                {
                    await _indexer.IndexFilesAsync();
                }
            }

            var agent = _codeAnalystBuilder.WithSearchCodePlugin().Build();

            var reducer = _kernel.GetChatSummarizerPlugin(_config);

            //Interaction Loop
            //Chat history setup
            var chatHistory = new ChatHistory();
            while (true)
            {
                var userQuery = AnsiConsole.Ask<string>("[blue]Ask about your code:[/]");
                chatHistory.AddUserMessage(userQuery);

                var usage = default(ChatTokenUsage);
                var aIfullMessage = "";

                if (userQuery == "exit") break;
                AnsiConsole.Markup($"[green]Assistant:[/] ");
                await foreach (var responseChunk in agent.InvokeStreamingAsync(chatHistory))
                {
                    aIfullMessage += responseChunk.Message.Content ?? string.Empty;
                    Console.Write(responseChunk.Message.Content ?? string.Empty);
                    usage = ((StreamingChatCompletionUpdate)responseChunk.Message.InnerContent).Usage;
                }
                chatHistory.AddAssistantMessage(aIfullMessage);

                AnsiConsole.MarkupLine($"\n\n[grey](Tokens Used: Prompt-{usage?.InputTokenCount}, Completion-{usage?.OutputTokenCount}, Total-{usage?.TotalTokenCount}) [/] \n\n");

                //Truncate history if needed
                var reducedMessage = await reducer.ReduceAsync(chatHistory);
                if (reducedMessage != null)
                {
                    chatHistory = new(reducedMessage);
                }
            }
        }
    }
}