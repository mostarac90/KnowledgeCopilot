import { useRef, useState, type ChangeEvent, type FormEvent } from 'react';
import { Link } from 'react-router';
import { getErrorMessage } from '../api/errors';
import type {
  IndexDocumentResponse,
  UploadDocumentResponse,
} from '../api/documentsApi';
import { useIndexDocument, useUploadDocument } from '../hooks/useDocuments';

const MAX_FILE_SIZE = 10 * 1024 * 1024;

type Stage = 'idle' | 'uploading' | 'indexing' | 'done' | 'error';

const validateFile = (file: File): string | null => {
  if (!file.name.toLowerCase().endsWith('.pdf')) {
    return 'Only PDF files are supported.';
  }
  if (file.size === 0) {
    return 'File is empty.';
  }
  if (file.size > MAX_FILE_SIZE) {
    return 'Maximum file size is 10 MB.';
  }
  return null;
};

const formatSize = (bytes: number) =>
  bytes < 1024 * 1024
    ? `${(bytes / 1024).toFixed(0)} KB`
    : `${(bytes / 1024 / 1024).toFixed(1)} MB`;

export const UploadDocumentPage = () => {
  const inputRef = useRef<HTMLInputElement>(null);
  const [file, setFile] = useState<File | null>(null);
  const [stage, setStage] = useState<Stage>('idle');
  const [error, setError] = useState<string | null>(null);
  const [uploaded, setUploaded] = useState<UploadDocumentResponse | null>(null);
  const [indexed, setIndexed] = useState<IndexDocumentResponse | null>(null);

  const upload = useUploadDocument();
  const index = useIndexDocument();

  const busy = stage === 'uploading' || stage === 'indexing';

  const handleFileChange = (e: ChangeEvent<HTMLInputElement>) => {
    const selected = e.target.files?.[0] ?? null;
    setFile(selected);
    setStage('idle');
    setUploaded(null);
    setIndexed(null);
    setError(selected ? validateFile(selected) : null);
  };

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!file || busy) return;

    const validationError = validateFile(file);
    if (validationError) {
      setError(validationError);
      return;
    }

    setError(null);

    try {
      setStage('uploading');
      const uploadResult = await upload.mutateAsync(file);
      setUploaded(uploadResult);

      setStage('indexing');
      const indexResult = await index.mutateAsync(uploadResult.id);
      setIndexed(indexResult);

      setStage('done');
      setFile(null);
      if (inputRef.current) inputRef.current.value = '';
    } catch (err) {
      setStage('error');
      setError(getErrorMessage(err as Error));
    }
  };

  return (
    <section className="mx-auto w-full max-w-3xl px-4 py-6 text-left">
      <h1 className="mb-4 text-2xl font-semibold text-(--text-h)">
        Upload document
      </h1>
      <p className="mb-6">
        Upload a PDF (max 10 MB). It will be processed and indexed so the
        agent can answer questions about it.
      </p>

      <form onSubmit={handleSubmit} className="flex flex-col gap-4">
        <label
          htmlFor="file"
          className="flex cursor-pointer flex-col items-center gap-2 rounded-lg border-2 border-dashed border-(--border) px-4 py-10 text-center hover:border-(--accent-border)"
        >
          <span className="font-semibold text-(--text-h)">
            {file ? file.name : 'Choose a PDF file'}
          </span>
          <span className="text-sm">
            {file ? formatSize(file.size) : 'Click to browse'}
          </span>
          <input
            ref={inputRef}
            id="file"
            type="file"
            accept=".pdf,application/pdf"
            onChange={handleFileChange}
            disabled={busy}
            className="sr-only"
          />
        </label>

        <button
          type="submit"
          disabled={!file || busy || !!validateFile(file)}
          className="self-end cursor-pointer rounded-lg bg-(--accent) px-5 py-2 text-white disabled:cursor-not-allowed disabled:opacity-50"
        >
          {stage === 'uploading'
            ? 'Uploading…'
            : stage === 'indexing'
              ? 'Indexing…'
              : 'Upload and index'}
        </button>
      </form>

      {error && (
        <div className="mt-4 text-red-600 dark:text-red-400" role="alert">
          <p>{error}</p>
          {uploaded && !indexed && (
            <p className="mt-1">
              The file was uploaded but indexing failed. You can retry from{' '}
              <Link to="/documents" className="underline">
                the documents list
              </Link>
              .
            </p>
          )}
        </div>
      )}

      {stage === 'done' && uploaded && indexed && (
        <div
          className="mt-6 rounded-lg border border-(--border) bg-(--accent-bg) p-4"
          role="status"
        >
          <p className="font-semibold text-(--text-h)">
            {uploaded.fileName} is ready
          </p>
          <p>
            {uploaded.pageCount} pages, {indexed.chunkCount} chunks indexed.
          </p>
          <div className="mt-3 flex gap-4">
            <Link
              to={`/documents/${uploaded.id}`}
              className="text-(--accent) underline"
            >
              View document
            </Link>
            <Link to="/documents" className="text-(--accent) underline">
              All documents
            </Link>
          </div>
        </div>
      )}
    </section>
  );
};
