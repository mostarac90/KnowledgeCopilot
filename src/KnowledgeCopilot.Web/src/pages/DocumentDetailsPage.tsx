import { Link, useParams } from 'react-router';
import { getErrorMessage } from '../api/errors';
import { useDocument, useIndexDocument } from '../hooks/useDocuments';

export const DocumentDetailsPage = () => {
  const { id = '' } = useParams();
  const { data: document, isPending, error } = useDocument(id);
  const index = useIndexDocument();

  return (
    <section className="mx-auto w-full max-w-4xl px-4 py-6 text-left">
      <Link to="/documents" className="text-(--accent) underline">
        ← All documents
      </Link>

      {isPending && <p className="mt-6">Loading document…</p>}

      {error && (
        <p className="mt-6 text-red-600 dark:text-red-400" role="alert">
          {getErrorMessage(error)}
        </p>
      )}

      {document && (
        <>
          <div className="mt-4 mb-6 flex flex-wrap items-start justify-between gap-4">
            <div>
              <h1 className="break-all text-2xl font-semibold text-(--text-h)">
                {document.fileName}
              </h1>
              <p>{document.pageCount} pages</p>
            </div>

            <div className="flex flex-col items-end gap-1">
              <button
                type="button"
                onClick={() => index.mutate(document.id)}
                disabled={index.isPending}
                className="cursor-pointer rounded-lg bg-(--accent) px-4 py-2 text-white disabled:cursor-not-allowed disabled:opacity-50"
              >
                {index.isPending ? 'Indexing…' : 'Index document'}
              </button>
              {index.data && (
                <span className="text-sm text-green-700 dark:text-green-400">
                  {index.data.chunkCount} chunks indexed
                </span>
              )}
              {index.error && (
                <span className="text-sm text-red-600 dark:text-red-400">
                  {getErrorMessage(index.error)}
                </span>
              )}
            </div>
          </div>

          <div className="flex flex-col gap-4">
            {document.pages.map((page) => (
              <article
                key={page.pageNumber}
                className="rounded-lg border border-(--border) p-4"
              >
                <h2 className="mb-2 text-sm font-semibold text-(--text-h)">
                  Page {page.pageNumber}
                </h2>
                <p className="whitespace-pre-wrap text-sm">
                  {page.text.trim() || (
                    <em className="opacity-70">No text on this page.</em>
                  )}
                </p>
              </article>
            ))}
          </div>
        </>
      )}
    </section>
  );
};
