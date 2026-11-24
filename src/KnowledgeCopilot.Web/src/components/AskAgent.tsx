import { useState, type FormEvent } from 'react';
import { isAxiosError } from 'axios';
import { useAskAgent } from '../hooks/useAskAgent';

const MAX_QUESTION_LENGTH = 2000;

const getErrorMessage = (error: Error): string => {
  if (isAxiosError<{ error?: string }>(error) && error.response?.data?.error) {
    return error.response.data.error;
  }
  return error.message;
};

export const AskAgent = () => {
  const [question, setQuestion] = useState('');
  const { mutate, data, error, isPending } = useAskAgent();

  const trimmed = question.trim();

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!trimmed || isPending) return;
    mutate(trimmed);
  };

  return (
    <section className="mx-auto w-full max-w-3xl px-4 py-6 text-left">
      <form onSubmit={handleSubmit} className="flex flex-col gap-2">
        <label htmlFor="question" className="font-semibold text-(--text-h)">
          Ask the knowledge base
        </label>
        <textarea
          id="question"
          value={question}
          onChange={(e) => setQuestion(e.target.value)}
          onKeyDown={(e) => {
            if (e.key === 'Enter' && !e.shiftKey) {
              e.preventDefault();
              e.currentTarget.form?.requestSubmit();
            }
          }}
          placeholder="Type your question…"
          maxLength={MAX_QUESTION_LENGTH}
          rows={3}
          disabled={isPending}
          className="resize-y rounded-lg border border-(--border) bg-(--bg) px-3 py-2.5 text-(--text-h) focus:outline-2 focus:outline-offset-1 focus:outline-(--accent-border) disabled:opacity-60"
        />
        <button
          type="submit"
          disabled={!trimmed || isPending}
          className="self-end cursor-pointer rounded-lg bg-(--accent) px-5 py-2 text-white disabled:cursor-not-allowed disabled:opacity-50"
        >
          {isPending ? 'Asking…' : 'Ask'}
        </button>
      </form>

      {error && (
        <p className="mt-4 text-red-600 dark:text-red-400" role="alert">
          {getErrorMessage(error)}
        </p>
      )}

      {data && !isPending && (
        <article className="mt-6 rounded-lg border border-(--border) bg-(--accent-bg) p-4">
          <h2 className="mb-2 text-base font-semibold text-(--text-h)">
            {data.question}
          </h2>
          <p className="whitespace-pre-wrap text-(--text-h)">{data.answer}</p>
        </article>
      )}
    </section>
  );
};
