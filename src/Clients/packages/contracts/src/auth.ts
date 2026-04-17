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

export type VerifyEmailCodeDto = {
  email: string;
  code: string;
};

export type ResendEmailVerificationCodeDto = {
  email: string;
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
  phoneNumber?: string | null;
  emailConfirmed: boolean;
  hasPassword: boolean;
  isGoogleAccount: boolean;
  role: string;
};

export type UpdateOwnProfileDto = {
  name: string;
  phoneNumber?: string | null;
  accountImageUrl?: string | null;
};

export type ChangePasswordDto = {
  email: string;
  currentPassword?: string | null;
  newPassword: string;
};
