using NUnit.Framework;
using Moq;
using System.Threading.Tasks;
using CodeAnalyzer;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using CodeAnalyzer.Indexers;
using CodeAnalyzer.Agents;
using Spectre.Console;
using System.Collections.Generic;
using OpenAI.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Agents;

namespace CodeAnalyzer.Tests
{
    [TestFixture]
    public class CodeAnalyzerCLITests
    {
        private Mock<ICodeFileIndexer> _indexerMock;
        private Mock<ICodeAnalystAgentBuilder> _agentBuilderMock;
        private Mock<IConfiguration> _configMock;
        private Mock<Kernel> _kernelMock;

        private CodeAnalyzerCLI _cli;

        [SetUp]
        public void Setup()
        {
            _indexerMock = new Mock<ICodeFileIndexer>();
            _agentBuilderMock = new Mock<ICodeAnalystAgentBuilder>();
            _configMock = new Mock<IConfiguration>();
            _kernelMock = new Mock<Kernel>();

            // Setup config mock for GetSection calls
            var fileExtensions = new List<IConfigurationSection>();
            var excludePaths = new List<IConfigurationSection>();

            var configSectionMockFE = new Mock<IConfigurationSection>();
            configSectionMockFE.Setup(s => s.GetChildren()).Returns(fileExtensions);
            _configMock.Setup(c => c.GetSection("ReadOptions:FileExtensions")).Returns(configSectionMockFE.Object);

            var configSectionMockEP = new Mock<IConfigurationSection>();
            configSectionMockEP.Setup(s => s.GetChildren()).Returns(excludePaths);
            _configMock.Setup(c => c.GetSection("ReadOptions:ExcludePaths")).Returns(configSectionMockEP.Object);

            var configSubDirMock = new Mock<IConfigurationSection>();
            configSubDirMock.Setup(s => s.Value).Returns("true");
            _configMock.Setup(c => c.GetSection("ReadOptions:IncludeSubDirectories")).Returns(configSubDirMock.Object);

            _configMock.Setup(c => c["VectorStore:CollectionName"]).Returns("snippets");

            _cli = new CodeAnalyzerCLI(_kernelMock.Object, null, _indexerMock.Object, _agentBuilderMock.Object, _configMock.Object);
        }

    //     [Test]
    //     public async Task StartAsync_WhenUserConfirmsIndexing_CallsIndexFilesAsync()
    //     {
    //         // Arrange
    //         // Simulate user inputs: confirm Index local files? Yes , confirm overwrite? Yes, then "exit" to break loop
    //         var inputs = new Queue<string>(new[] { "y", "y", "exit" });

    //         // Mock AnsiConsole static methods using delegate replacements for testing
    //         AnsiConsole.Confirm = (string prompt) => 
    //         {
    //             if (inputs.Count == 0) return false;
    //             var resp = inputs.Dequeue();
    //             return resp == "y";
    //         };

    //         AnsiConsole.Ask<string> = (string prompt) => 
    //         {
    //             if (inputs.Count == 0) return "exit";
    //             return inputs.Dequeue();
    //         };

    //         _agentBuilderMock.Setup(b => b.WithSearchCodePlugin()).Returns(_agentBuilderMock.Object);
    //         _agentBuilderMock.Setup(b => b.Build()).Returns(new DummyAgent());

    //         // Act
    //         await _cli.StartAsync();

    //         // Assert
    //         _indexerMock.Verify(i => i.IndexFilesAsync(), Times.Once);
    //     }
    // }

    // // Dummy implementation for agent to allow test to run without exceptions
    // public class DummyAgent : ChatCompletionAgent
    // {
    //     public async IAsyncEnumerable<StreamingChatCompletionUpdate> InvokeStreamingAsync(ChatHistory chatHistory)
    //     {
    //         yield break;
    //     }
    // }
    }
}