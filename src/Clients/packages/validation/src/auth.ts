import { z } from "zod";
import type {
  AuthTokensDto,
  CurrentUserDto,
  SignInDto,
  SignUpDto,
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
  role: z.string(),
});

export const UpdateOwnProfileDtoSchema: z.ZodType<UpdateOwnProfileDto> = z.object({
  name: z.string().min(1),
  phoneNumber: z.string().nullable().optional(),
});
