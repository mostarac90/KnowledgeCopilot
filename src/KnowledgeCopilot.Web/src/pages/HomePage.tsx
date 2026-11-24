import { Link } from 'react-router';
import { AskAgent } from '../components/AskAgent';

const steps = [
  {
    title: 'Upload your documents',
    body: (
      <>
        Add PDFs on the{' '}
        <Link to="/documents/upload" className="text-(--accent) underline">
          Upload
        </Link>{' '}
        page. They're indexed automatically so the assistant can search them.
      </>
    ),
  },
  {
    title: 'Ask a question',
    body: 'Type your question below in plain language and press Enter (Shift+Enter adds a new line).',
  },
  {
    title: 'Get an answer',
    body: 'The assistant searches your indexed documents and answers based on what it finds.',
  },
];

const tips = [
  'Be specific: "What is the notice period in the employment contract?" works better than "notice?".',
  'Mention the topic or document you have in mind to narrow the search.',
  'If an answer seems off, rephrase or ask a follow-up question.',
];

export const HomePage = () => (
  <>
    <section className="mx-auto w-full max-w-3xl px-4 pt-8 text-left">
      <h1 className="mb-2 text-2xl font-semibold text-(--text-h)">
        Ask your knowledge base
      </h1>
      <p className="mb-10">
        KnowledgeCopilot answers questions using the documents you've
        uploaded, so you don't have to search through them yourself.
      </p>

      <ol className="grid gap-3 sm:grid-cols-3">
        {steps.map((step, i) => (
          <li
            key={step.title}
            className="rounded-lg border border-(--border) p-4"
          >
            <span className="mb-2 flex h-7 w-7 items-center justify-center rounded-full bg-(--accent-bg) text-sm font-semibold text-(--accent)">
              {i + 1}
            </span>
            <h2 className="mb-1 text-base font-semibold text-(--text-h)">
              {step.title}
            </h2>
            <p className="text-sm">{step.body}</p>
          </li>
        ))}
      </ol>
    </section>

    <AskAgent />

    <section className="mx-auto w-full max-w-3xl px-4 pb-8 text-left">
      <details className="rounded-lg border border-(--border) p-4 text-sm">
        <summary className="cursor-pointer font-semibold text-(--text-h)">
          Tips for better answers
        </summary>
        <ul className="mt-3 list-disc space-y-1 pl-5">
          {tips.map((tip) => (
            <li key={tip}>{tip}</li>
          ))}
        </ul>
      </details>
    </section>
  </>
);
