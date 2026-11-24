import { useState } from 'react';
import { Link } from 'react-router';
import { getErrorMessage } from '../api/errors';
import { useDocuments, useIndexDocument } from '../hooks/useDocuments';

type IndexState =
  | { status: 'indexing' }
  | { status: 'done'; chunkCount: number }
  | { status: 'error'; message: string };

const dateFormatter = new Intl.DateTimeFormat(undefined, {
  dateStyle: 'medium',
  timeStyle: 'short',
});

export const DocumentsPage = () => {
  const { data: documents, isPending, error } = useDocuments();
  const index = useIndexDocument();
  const [indexStates, setIndexStates] = useState<Record<string, IndexState>>(
    {},
  );

  const handleIndex = (id: string) => {
    setIndexStates((s) => ({ ...s, [id]: { status: 'indexing' } }));
    index.mutate(id, {
      onSuccess: (result) =>
        setIndexStates((s) => ({
          ...s,
          [id]: { status: 'done', chunkCount: result.chunkCount },
        })),
      onError: (err) =>
        setIndexStates((s) => ({
          ...s,
          [id]: { status: 'error', message: getErrorMessage(err) },
        })),
    });
  };

  return (
    <section className="mx-auto w-full max-w-4xl px-4 py-6 text-left">
      <div className="mb-6 flex items-center justify-between gap-4">
        <h1 className="text-2xl font-semibold text-(--text-h)">Documents</h1>
        <Link
          to="/documents/upload"
          className="rounded-lg bg-(--accent) px-4 py-2 text-white"
        >
          Upload document
        </Link>
      </div>

      {isPending && <p>Loading documents…</p>}

      {error && (
        <p className="text-red-600 dark:text-red-400" role="alert">
          {getErrorMessage(error)}
        </p>
      )}

      {documents && documents.length === 0 && (
        <p>
          No documents yet.{' '}
          <Link to="/documents/upload" className="text-(--accent) underline">
            Upload your first PDF
          </Link>
          .
        </p>
      )}

      {documents && documents.length > 0 && (
        <div className="overflow-x-auto rounded-lg border border-(--border)">
          <table className="w-full text-sm">
            <thead className="bg-(--accent-bg) text-(--text-h)">
              <tr>
                <th className="px-4 py-2 text-left font-semibold">File</th>
                <th className="px-4 py-2 text-right font-semibold">Pages</th>
                <th className="px-4 py-2 text-left font-semibold">Uploaded</th>
                <th className="px-4 py-2" />
              </tr>
            </thead>
            <tbody>
              {documents.map((doc) => {
                const state = indexStates[doc.id];
                return (
                  <tr key={doc.id} className="border-t border-(--border)">
                    <td className="px-4 py-2">
                      <Link
                        to={`/documents/${doc.id}`}
                        className="break-all text-(--accent) underline"
                      >
                        {doc.fileName}
                      </Link>
                    </td>
                    <td className="px-4 py-2 text-right tabular-nums">
                      {doc.pageCount}
                    </td>
                    <td className="whitespace-nowrap px-4 py-2">
                      {dateFormatter.format(new Date(doc.uploadedAt))}
                    </td>
                    <td className="whitespace-nowrap px-4 py-2 text-right">
                      {state?.status === 'done' && (
                        <span className="mr-3 text-green-700 dark:text-green-400">
                          {state.chunkCount} chunks indexed
                        </span>
                      )}
                      {state?.status === 'error' && (
                        <span className="mr-3 text-red-600 dark:text-red-400">
                          {state.message}
                        </span>
                      )}
                      <button
                        type="button"
                        onClick={() => handleIndex(doc.id)}
                        disabled={state?.status === 'indexing'}
                        className="cursor-pointer rounded-lg border border-(--accent-border) px-3 py-1 text-(--accent) disabled:cursor-not-allowed disabled:opacity-50"
                      >
                        {state?.status === 'indexing'
                          ? 'Indexing…'
                          : state?.status === 'done'
                            ? 'Re-index'
                            : 'Index'}
                      </button>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
};
