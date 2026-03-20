export type SignInDto = {
  email: string;
  password: string;
};

export type SignUpDto = {
  name: string;
  email: string;
  password: string;
  accountImageUrl?: string | null;
};

export type AuthTokensDto = {
  success?: boolean;
  token: string;
  refreshToken: string;
};

export type CurrentUserDto = {
  id: string;
  name: string;
  email: string;
  accountImageUrl: string | null;
  role: string;
};
