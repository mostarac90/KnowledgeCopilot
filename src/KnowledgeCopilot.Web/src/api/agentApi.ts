import client from './client';

export class AskAgentRequest {
  question: string;

  constructor(question: string) {
    this.question = question;
  }
}

export class AskAgentResponse {
  question: string;
  answer: string;

  constructor(question: string, answer: string) {
    this.question = question;
    this.answer = answer;
  }
}

export const askAgent = async (
  request: AskAgentRequest,
): Promise<AskAgentResponse> => {
  const { data } = await client.post<AskAgentResponse>(
    '/api/agent/ask',
    request,
  );

  return new AskAgentResponse(data.question, data.answer);
};
