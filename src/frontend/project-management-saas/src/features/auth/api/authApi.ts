import { httpClient } from '../../../shared/api/httpClient';
import type { AuthenticationResponse, CurrentUser, SignInRequest } from '../model/authTypes';

export const authApi = {
  async signIn(request: SignInRequest): Promise<AuthenticationResponse> {
    const response = await httpClient.post<AuthenticationResponse>('/auth/token', request);

    return response.data;
  },

  async refreshSession(): Promise<AuthenticationResponse> {
    const response = await httpClient.post<AuthenticationResponse>('/auth/refresh');

    return response.data;
  },

  async revokeSession(): Promise<void> {
    await httpClient.post('/auth/revoke');
  },

  async getCurrentUser(): Promise<CurrentUser> {
    const response = await httpClient.get<CurrentUser>('/auth/me');

    return response.data;
  }
};
