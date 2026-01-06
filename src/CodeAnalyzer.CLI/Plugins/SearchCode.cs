using System.Linq;
using CodeAnalyzer.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.SqliteVec;

namespace CodeAnalyzer.Plugins
{

    public static class SearchCode
    {
        public static KernelPlugin GetSearchCodePlugin(this Kernel kernel, SqliteVectorStore _vectorStore, IConfiguration config)
        {
            var collectionName = config["VectorStore:CollectionName"] ?? "snippets";
            var collection = _vectorStore.GetCollection<string, CodeSnippet>(collectionName);

            return kernel.CreatePluginFromFunctions("SearchCode", [
                kernel.CreateFunctionFromMethod(async (string query) => {
                    var embedService = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
                    var queryEmbedding = await embedService.GenerateAsync(query, new EmbeddingGenerationOptions() {
                        Dimensions = 1536
                    });
                    var results = collection.SearchAsync(queryEmbedding.Vector, 5);
                    return string.Join("\n---\n", await results.Select(r => r.Record.Content).ToListAsync());
                }, "SearchCode", "Searches the local codebase for relevant logic.")
            ]);
        }
    }
}