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
