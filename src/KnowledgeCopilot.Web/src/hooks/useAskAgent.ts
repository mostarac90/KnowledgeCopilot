import { useMutation } from '@tanstack/react-query';
import { askAgent, AskAgentRequest, AskAgentResponse } from '../api/agentApi';

export const useAskAgent = () =>
  useMutation<AskAgentResponse, Error, string>({
    mutationFn: (question) => askAgent(new AskAgentRequest(question)),
  });
