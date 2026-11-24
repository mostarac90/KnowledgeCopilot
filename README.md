# KnowledgeCopilot

KnowledgeCopilot is an AI assistant that answers questions about your own documents. You upload PDFs, they are split up and indexed, and then you can ask questions in plain language. The agent searches the documents and answers based on what it finds.

The agent is told to only answer from the documents. If the information isn't there, it says so instead of making something up. I wanted an assistant you can actually trust with company documents, where a wrong answer is worse than no answer.

For now it only supports text-based PDFs up to 10 MB. Scanned PDFs need OCR, which isn't supported yet. There is no login, so everyone shares the same knowledge base.

## How it works

1. You upload a PDF in the web app. The API checks that it is a real PDF, extracts the text page by page and stores the file and the text in PostgreSQL.
2. The document is indexed. The text is split into overlapping chunks (1,000 characters with 200 characters of overlap), and each chunk gets an embedding that is stored in PostgreSQL with pgvector.
3. You ask a question on the front page, and the web app sends it to the API.
4. The agent decides to call its `SearchKnowledgeBase` tool. The question is turned into an embedding and the five most similar chunks are found using cosine similarity.
5. The agent writes an answer based on those chunks and the web app shows it.

## Project structure

| Project | What it does |
|---|---|
| `KnowledgeCopilot.Core` | Domain models and interfaces (documents, chunks, search, embeddings, chat). |
| `KnowledgeCopilot.Infrastructure` | The agent and its tools, PDF text extraction, chunking, OpenAI embeddings, semantic search and the PostgreSQL repositories. |
| `KnowledgeCopilot.Api` | ASP.NET Core API for uploading, indexing, searching and asking questions. |
| `KnowledgeCopilot.Web` | React frontend. |

## Techniques used

### Backend

- **.NET 10 and ASP.NET Core Minimal APIs** for the REST API, with Swagger UI.
- **Retrieval-augmented generation (RAG).** Answers are grounded in chunks retrieved from your own documents instead of the model's general knowledge.
- **Microsoft Agent Framework** with **OpenAI tool calling**. The agent gets a knowledge base search tool and decides itself when to use it. The default model is gpt-4o-mini, and it can be changed in configuration.
- **OpenAI embeddings** (`text-embedding-3-small`, 1,536 dimensions) for turning chunks and questions into vectors.
- **PostgreSQL with pgvector** for storing documents, pages and chunk embeddings, and for cosine similarity search directly in the database.
- **PdfPig** for extracting text from PDFs, page by page.
- **Prompt rules against hallucination and prompt injection.** The model is told to only use the retrieved documents, to say when it can't find an answer, and to treat document content as data, not instructions.
- **Docker Compose** for running PostgreSQL with pgvector locally.

### Frontend

- **React 19 + TypeScript**, built with Vite.
- **React Router** for the pages: ask, documents, upload and document details.
- **TanStack Query** for fetching data and handling uploads, indexing and questions.
- **Axios** for HTTP and **Tailwind CSS** for styling.

## Deployment

The app is deployed to Azure. The backend is deployed to Azure App Service, the frontend to Azure Static Web Apps, and the database runs on Azure Database for PostgreSQL Flexible Server with the pgvector extension.

Preview link:

<!-- TODO: add the Static Web Apps URL -->
