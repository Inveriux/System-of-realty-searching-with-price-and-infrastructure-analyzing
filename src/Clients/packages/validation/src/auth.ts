import { z } from "zod";
import type {
  AuthTokensDto,
  ChangePasswordDto,
  CurrentUserDto,
  ResendEmailVerificationCodeDto,
  SignInDto,
  SignUpDto,
  VerifyEmailCodeDto,
  UpdateOwnProfileDto,
} from "@diploma/contracts/auth";

export const SignInDtoSchema: z.ZodType<SignInDto> = z.object({
  email: z.string().email(),
  password: z.string().min(8),
});

export const SignUpDtoSchema: z.ZodType<SignUpDto> = z.object({
  name: z.string().min(1),
  email: z.string().email(),
  password: z.string().min(8),
  accountImageUrl: z.string().url().nullable().optional(),
});

export const VerifyEmailCodeDtoSchema: z.ZodType<VerifyEmailCodeDto> = z.object({
  email: z.string().email(),
  code: z.string().regex(/^\d{6}$/),
});

export const ResendEmailVerificationCodeDtoSchema: z.ZodType<ResendEmailVerificationCodeDto> = z.object({
  email: z.string().email(),
});

export const AuthTokensDtoSchema: z.ZodType<AuthTokensDto> = z.object({
  success: z.boolean().optional(),
  token: z.string(),
  refreshToken: z.string(),
});

export const CurrentUserDtoSchema: z.ZodType<CurrentUserDto> = z.object({
  id: z.string(),
  name: z.string(),
  email: z.string().email(),
  accountImageUrl: z.string().nullable(),
  phoneNumber: z.string().nullable().optional(),
  emailConfirmed: z.boolean(),
  hasPassword: z.boolean(),
  isGoogleAccount: z.boolean(),
  role: z.string(),
});

export const UpdateOwnProfileDtoSchema: z.ZodType<UpdateOwnProfileDto> = z.object({
  name: z.string().min(1),
  phoneNumber: z.string().nullable().optional(),
});

export const ChangePasswordDtoSchema: z.ZodType<ChangePasswordDto> = z.object({
  email: z.string().email(),
  currentPassword: z.string().min(1).nullable().optional(),
  newPassword: z.string().min(8),
});
