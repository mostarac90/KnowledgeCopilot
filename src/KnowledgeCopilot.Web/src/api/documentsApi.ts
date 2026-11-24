import client from './client';

export interface DocumentSummary {
  id: string;
  fileName: string;
  pageCount: number;
  uploadedAt: string;
}

export interface DocumentPage {
  pageNumber: number;
  text: string;
}

export interface DocumentDetails {
  id: string;
  fileName: string;
  pageCount: number;
  pages: DocumentPage[];
}

export interface UploadDocumentResponse {
  id: string;
  fileName: string;
  pageCount: number;
  status: string;
}

export interface IndexDocumentResponse {
  documentId: string;
  status: string;
  chunkCount: number;
}

export const getDocuments = async (): Promise<DocumentSummary[]> => {
  const { data } = await client.get<DocumentSummary[]>('/api/documents');
  return data;
};

export const getDocument = async (id: string): Promise<DocumentDetails> => {
  const { data } = await client.get<DocumentDetails>(`/api/documents/${id}`);
  return data;
};

export const uploadDocument = async (
  file: File,
): Promise<UploadDocumentResponse> => {
  const formData = new FormData();
  formData.append('file', file);

  const { data } = await client.post<UploadDocumentResponse>(
    '/api/documents',
    formData,
    { headers: { 'Content-Type': 'multipart/form-data' } },
  );
  return data;
};

export const indexDocument = async (
  id: string,
): Promise<IndexDocumentResponse> => {
  const { data } = await client.post<IndexDocumentResponse>(
    `/api/documents/${id}/index`,
  );
  return data;
};
