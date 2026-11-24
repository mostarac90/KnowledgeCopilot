import client from './client';


export const askAgent = (question: string) => {

  return client.post('/agent/ask', { question });

}