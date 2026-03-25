import { z } from "zod";
import {
  AuthTokensDtoSchema,
  CurrentUserDtoSchema,
} from "@diploma/validation";
import type { ApiClient } from "./http";
import type { SignInDto, SignUpDto, UpdateOwnProfileDto } from "@diploma/contracts/auth";

const AuthTokensResultSchema = z.object({
  isSuccess: z.boolean(),
  message: z.string().nullable(),
  statusCode: z.number(),
  data: AuthTokensDtoSchema,
  errors: z.array(z.unknown()),
});

const CurrentUserResultSchema = z.object({
  isSuccess: z.boolean(),
  message: z.string().nullable(),
  statusCode: z.number(),
  data: CurrentUserDtoSchema,
  errors: z.array(z.unknown()),
});

const BooleanResultSchema = z.object({
  isSuccess: z.boolean(),
  message: z.string().nullable(),
  statusCode: z.number(),
  data: z.boolean(),
  errors: z.array(z.unknown()),
});

export async function signUp(api: ApiClient, payload: SignUpDto) {
  await api.post("/api/users/signup", payload, BooleanResultSchema, { authMode: "none" });
}

export async function signUpWithAvatar(api: ApiClient, payload: SignUpDto, image: File) {
  const formData = new FormData();
  formData.append("user", JSON.stringify(payload));
  formData.append("image", image);
  await api.post("/api/users/signup-with-avatar", formData, BooleanResultSchema, { authMode: "none" });
}

export async function signIn(api: ApiClient, payload: SignInDto) {
  const result = await api.post("/api/users/signin", payload, AuthTokensResultSchema, {
    authMode: "none",
  });
  return result.data;
}

export async function refreshToken(api: ApiClient, refreshTokenValue: string) {
  const result = await api.post(
    "/api/users/refresh-token",
    { refreshToken: refreshTokenValue },
    AuthTokensResultSchema,
    { authMode: "none" },
  );
  return result.data;
}

export async function getCurrentUser(api: ApiClient) {
  const result = await api.get("/api/users/info", CurrentUserResultSchema);
  return result.data;
}

export async function updateOwnProfile(api: ApiClient, payload: UpdateOwnProfileDto) {
  const result = await api.put("/api/users/profile", payload, CurrentUserResultSchema);
  return result.data;
}

export async function uploadUserAvatar(api: ApiClient, image: File) {
  const formData = new FormData();
  formData.append("image", image);
  const result = await api.post("/api/users/avatar", formData, CurrentUserResultSchema);
  return result.data;
}
