using CodeAnalyzer.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.SqliteVec;

namespace CodeAnalyzer.Agents
{
    public interface ICodeAnalystAgentBuilder
    {
        ChatCompletionAgent Build();
        ICodeAnalystAgentBuilder WithSearchCodePlugin();
    }
    public class CodeAnalystAgentBuilder : ICodeAnalystAgentBuilder
    {
        private Kernel _kernel;
        private KernelPlugin SearchCodePlugin {get; set;}
        private IConfiguration _config;
        private SqliteVectorStore _vectorStore;
        public CodeAnalystAgentBuilder(Kernel kernel, IConfiguration config, SqliteVectorStore vectorStore)
        {
            _kernel = kernel;
            _config = config;
            _vectorStore = vectorStore;
        }
        public ChatCompletionAgent Build()
        {
            var agent = new ChatCompletionAgent()
            {
                Name = "CodeAnalyst",
                Instructions = $"You are a senior software developer and expert in various programming languages. Use the SearchCode tool to answer questions about the codebase.",
                Kernel = _kernel,
                Arguments = new KernelArguments(new AzureOpenAIPromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() }),
            };

            if (SearchCodePlugin != null)
            {
                agent.Kernel.Plugins.Add(SearchCodePlugin);
            }

            return agent;
        }

        public ICodeAnalystAgentBuilder WithSearchCodePlugin()
        {
            SearchCodePlugin = _kernel.GetSearchCodePlugin(_vectorStore, _config);
            return this;
        }
    }
}