export interface CurrentUser {
  id: string;
  email: string;
  displayName: string | null;
  roles: readonly string[];
  permissions: readonly string[];
}

export interface AuthenticationResponse {
  accessToken: string;
  accessTokenExpiresAtUtc: string;
  user: CurrentUser;
}

export interface SignInRequest {
  email: string;
  password: string;
}
