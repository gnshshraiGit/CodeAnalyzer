# CodeAnalyzer
A code anylyzer using Semantic kernel and Azure OpenAI

1. **Code Snippet Model:**
   - Represents pieces of code with properties like Id, Filename, Content, and an AI-generated embedding vector for semantic search.

2. **Indexing Local Code Files:**
   - The `CodeFileIndexer` class reads source code files from a directory, applies filters (file extensions, exclude paths), generates embeddings for the code content via AI embedding services, and stores these embeddings in a vector store (SQLite-based).
   - This allows the codebase to be searched semantically later.

3. **Semantic Search Plugin:**
   - The `SearchCode` plugin adds a function to the AI kernel that uses embeddings to find relevant code snippets based on a natural language query.
   - It searches the indexed vector store and returns matched code content.

4. **CLI (Command-Line Interface):**
   - A user interface to interact with the tool.
   - Users can choose to index local files and then ask questions about their codebase.
   - The tool uses an AI chat agent that integrates the search plugin to answer queries about the code.
   - The chat maintains conversation history, streams AI responses, and summarizes chat history to manage token usage.

5. **AI Agent Builder:**
   - Sets up a chat agent with instructions to act as a senior software developer.
   - The agent uses the SearchCode plugin for codebase lookups and OpenAI-based chat completions for interactive Q&A.

In summary, this tool indexes your local codebase into an AI-searchable vector store. You can then query your codebase in natural language and get relevant code snippets and explanations from an AI agent. It's designed to improve developer productivity by making code easier to explore and understand through natural language interactions.

# Sample run

````console
CodeAnalyzer.CLI %  dotnet run
````

```plaintext
  ____               _               _                      _                                  ____   _       ___ 
 / ___|   ___     __| |   ___       / \     _ __     __ _  | |  _   _   ____   ___   _ __     / ___| | |     |_ _|
| |      / _ \   / _` |  / _ \     / _ \   | '_ \   / _` | | | | | | | |_  /  / _ \ | '__|   | |     | |      | | 
| |___  | (_) | | (_| | |  __/    / ___ \  | | | | | (_| | | | | |_| |  / /  |  __/ | |      | |___  | |___   | | 
 \____|  \___/   \__,_|  \___|   /_/   \_\ |_| |_|  \__,_| |_|  \__, | /___|  \___| |_|       \____| |_____| |___|
                                                               |___/                                             
```
`Index local files?` [y/n] (y): y   
`Indexing will overwrite existing data. Continue?` (y/n) [y/n] (y): y   
`Select the project source file Enter to continue` (./): ./   
./CodeAnalyzerCLI.cs
./Program.cs                   
./Indexers/CodeFileIndexer.cs  
./Plugins/SearchCode.cs                 
./Plugins/ChatSummarizer.cs             
./Agents/CodeAnalyst.cs                 
./Models/CodeSnippet.cs                 
`Ask about your code:` Pull all the files in my vector store and prepare a report based by analyzing design patterns, functional and non functional features
`Assistant:` I analyzed all files loaded in your vector store. Here is a detailed report on the design patterns, functional and non-functional features of the codebase:

---

# Design Patterns

1. **Dependency Injection**  
   The application uses the Dependency Injection pattern extensively via Microsoft.Extensions.DependencyInjection. Services such as Kernel, IConfiguration, SqliteVectorStore, ICodeAnalyzerCLI, ICodeFileIndexer, and ICodeAnalystAgentBuilder are registered and injected. This promotes loose coupling and testability.

2. **Builder Pattern**  
   The Kernel builder pattern is used to construct a Kernel instance with Azure OpenAI chat completion and embedding generation services configured.

3. **Interface Segregation and Abstractions**  
   Interfaces such as ICodeAnalyzerCLI, ICodeFileIndexer, and ICodeAnalystAgentBuilder abstract implementation details from the caller, enabling easier future extensions or mocking.

4. **Adapter / Wrapper Pattern**  
   Wrapper/plugins such as SearchCode and ChatSummarizer adapt underlying kernel and vector store functionality into higher level callable entities.

5. **Factory Pattern**  
   Static methods in plugins provide factory-like methods that create and configure complex objects e.g., GetSearchCodePlugin and GetChatSummarizerPlugin.

---

# Functional Features

- **Code Indexing**  
  Local code files can be indexed into a Sqlite-based vector store with embeddings for semantic search.

- **Semantic Search**  
  Users can query the indexed codebase using natural language questions, leveraging OpenAI embedding-based search for relevant code snippets.

- **Chat-based Interface**  
  A CLI with chat history allows iterative interaction and refining queries about the codebase.

- **Adaptive Chat History Reduction**  
  Summarization plugin manages and prunes the chat history intelligently to stay within token limits.

- **Plugin-based Kernel Extensions**  
  Plugins extend kernel capabilities for search and summarization, providing modular functions.

- **Configuration Driven**  
  The app reads settings from appsettings.json via IConfiguration, enabling easy customization of file extensions, exclusion paths, Azure credentials, and logging.

---

# Non-Functional Features

- **Logging**  
  Console logging is available and configurable through appsettings.json for diagnostic monitoring.

- **Asynchronous and Streaming Support**  
  It employs async/await and streaming API consumption for responsive and efficient operation.

- **Console UI with User Prompts**  
  Uses Spectre.Console for rich CLI UI, including colored text, confirmation prompts, and Figlet headers.

- **Extensible Architecture**  
  The modular design with DI, plugins, and interfaces supports easy addition of more analyzers or vector stores.

- **Error Handling and Confirmations**  
  Multiple user confirmations before destructive actions keep the workflow safe and intentional.

---

Summary:  
The project is a modular, extensible semantic code analysis solution built with modern C# practices. It leverages AI embeddings for code search in a user-friendly CLI with configurability, robust async patterns, and maintainable design patterns such as DI, Builder, and Adapter. Non-functional aspects like logging, user prompts, and chat history summarization enhance usability and operation.

If you want, I can generate a detailed markdown report or help answer specific questions on any part of the codebase or design.

(Tokens Used: Prompt-1919, Completion-660, Total-2579)  


`Ask about your code:` give some improvements by analyzing some potential shortcomings 
`Assistant:` After analyzing key components like the SqliteVectorStore usage, SearchCode plugin implementation, indexing logic, dependency injection setup, and agent builder, here are some potential shortcomings and improvement suggestions:

1. **Indexing Scalability & Performance**  
   - The indexing process reads entire files and generates embeddings sequentially, which could be slow for large codebases.
   - **Improvement:** Process files in parallel using asynchronous parallelism (e.g., `Parallel.ForEachAsync` or TPL Dataflow) with batching to speed up indexing without blocking.
   - Also consider chunking very large files into smaller pieces before embedding to avoid hitting token limits and improve search granularity.

2. **Error Handling and Resilience**  
   - Current indexing code writes file content and queries embedding API without visible error handling.
   - **Improvement:** Add try-catch blocks around file I/O and embedding generation to handle corrupted files, API failures, or connectivity issues gracefully with retry/backoff and logging.

3. **VectorStore Collection Deletion Strategy**  
   - On each indexing run, the vector store collection is deleted and recreated, which may cause a full re-index and loss of incremental indexing.
   - **Improvement:** Support incremental or differential indexing by detecting changed files using file timestamps or checksums and updating only those vectors. This avoids costly full rebuilds.

4. **Embedding Generation Parameters Flexibility**  
   - The embedding generation hardcodes dimension=1536 and generation options per call.
   - **Improvement:** Move embedding options into configuration or constants and centralize embedding generation logic to allow easy modification or switching embedding models without code changes.

5. **Search Result Presentation**  
   - Search returns raw joined content snippets, which may overwhelm the user or provide insufficient context.
   - **Improvement:** Summarize or highlight relevant code parts, show source file names with line numbers, or paginate results for better UX.

6. **Configuration Defaulting and Validation**  
   - Some configuration keys have fallbacks (e.g., collection name) but overall config validation is minimal.
   - **Improvement:** Add robust config validation with defaults and explicit error messages for missing mandatory fields at startup to avoid runtime surprises.

7. **Logging Detail and Levels**  
   - Limited logging at key operations such as indexing start/end or errors.
   - **Improvement:** Add structured logging at info/debug/error levels throughout indexing, search, and agent actions to facilitate monitoring and troubleshooting in production.

8. **Extensibility for Multiple Vector Stores or Models**  
   - The code is strongly tied to SqliteVectorStore and Azure OpenAI embedding.
   - **Improvement:** Abstract vector store and embedding service behind interfaces to allow plugging in different vector databases or embedding providers.

9. **Chat History and State Management**  
   - While not fully explored here, chat history summarization appears used but details unclear.
   - **Improvement:** Ensure chat state management handles token limits and user session persistence cleanly for multi-turn conversations.

---

These improvements will enhance scalability, robustness, maintainability, and user experience of the codebase when analyzing large or complex source repositories. Let me know if you'd like me to generate specific code examples or implementation plans for any suggested improvements.

(Tokens Used: Prompt-2674, Completion-659, Total-3333)  


`Ask about your code:` 