using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CodeAnalyzer.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.SqliteVec;
using Spectre.Console;

namespace CodeAnalyzer.Indexers
{
    public interface ICodeFileIndexer
    {
        Task IndexFilesAsync();
    }
    public class CodeFileIndexer : ICodeFileIndexer
    {
        private readonly Kernel _kernel;
        private readonly IConfiguration _config;
        private readonly SqliteVectorStore _vectorStore;
        public CodeFileIndexer(Kernel kernel, SqliteVectorStore sqliteVectorStore, IConfiguration config)
        {
            _kernel = kernel;
            _vectorStore = sqliteVectorStore;
            _config = config;
        }

        public async Task IndexFilesAsync()
        {
            var readingOptionsFileExtensions = _config.GetSection("ReadOptions:FileExtensions").GetChildren().Select(x => x.Value).ToList();
            var readingOptionsExcludePaths = _config.GetSection("ReadOptions:ExcludePaths").GetChildren().Select(x => x.Value).ToList();
            var includeSubDirectories = _config.GetSection("ReadOptions:IncludeSubDirectories").Value;
            var collectionName = _config.GetSection("VectorStore:CollectionName").Value ?? "snippets";

            await _vectorStore.EnsureCollectionDeletedAsync(collectionName);
            var collection = _vectorStore.GetCollection<string, CodeSnippet>(collectionName);
            await collection.EnsureCollectionExistsAsync();

            var codeBaseRootDirectory = AnsiConsole.Prompt(new TextPrompt<string>("Select the project source file [green]Enter[/] to continue").DefaultValue("./"));

            var fileExtensionFilter = AnsiConsole.Prompt(new MultiSelectionPrompt<string>()
                    .Title("Select file extensions to include for indexing:")
                    .AddChoices(readingOptionsFileExtensions)
                    .Required().InstructionsText(
                        "[grey](Press [blue]<space>[/] to toggle an extension, " +
                        "[green]<enter>[/] to accept)[/]"));

            await AnsiConsole.Status().StartAsync("Indexing...", async ctx =>
            {
                var files = Directory.GetFiles(codeBaseRootDirectory, "*", SearchOption.AllDirectories).Except(
                    readingOptionsExcludePaths.SelectMany(excludePath =>
                        Directory.GetFiles(codeBaseRootDirectory, "*", SearchOption.AllDirectories)
                            .Where(f => f.Contains(excludePath))
                    )
                ).Where(file => fileExtensionFilter.Contains(Path.GetExtension(file))).ToList();
                var embedService = _kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();

                foreach (var file in files)
                {
                    AnsiConsole.WriteLine(file);
                    ctx.Status = $"Indexing [green]{file}[/]";
                    var content = await File.ReadAllTextAsync(file);
                    var embedding = await embedService.GenerateAsync(content, new EmbeddingGenerationOptions() {
                        Dimensions = 1536
                    });
                    await collection.UpsertAsync(new CodeSnippet
                    {
                        FileName = file,
                        Content = content,
                        Embedding = embedding.Vector
                    });
                }
            });
        }
    }
}