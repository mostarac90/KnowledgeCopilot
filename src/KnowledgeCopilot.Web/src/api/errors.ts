import { isAxiosError } from 'axios';

/**
 * Extracts a readable message from an API error.
 * The backend returns either a plain string or `{ error: string }`.
 */
export const getErrorMessage = (error: Error): string => {
  if (isAxiosError(error)) {
    const data: unknown = error.response?.data;

    if (typeof data === 'string' && data.trim()) {
      return data;
    }

    if (
      data &&
      typeof data === 'object' &&
      'error' in data &&
      typeof data.error === 'string'
    ) {
      return data.error;
    }

    if (error.response?.status === 404) {
      return 'Not found.';
    }
  }

  return error.message;
};
