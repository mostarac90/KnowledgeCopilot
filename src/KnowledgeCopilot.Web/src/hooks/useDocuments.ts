import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  getDocument,
  getDocuments,
  indexDocument,
  uploadDocument,
} from '../api/documentsApi';

export const documentKeys = {
  all: ['documents'] as const,
  detail: (id: string) => ['documents', id] as const,
};

export const useDocuments = () =>
  useQuery({
    queryKey: documentKeys.all,
    queryFn: getDocuments,
  });

export const useDocument = (id: string) =>
  useQuery({
    queryKey: documentKeys.detail(id),
    queryFn: () => getDocument(id),
    enabled: !!id,
  });

export const useUploadDocument = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: uploadDocument,
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: documentKeys.all }),
  });
};

export const useIndexDocument = () =>
  useMutation({
    mutationFn: indexDocument,
  });
