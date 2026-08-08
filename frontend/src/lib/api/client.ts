import axios from "axios";
import { ERROR_MESSAGES } from "@/lib/messages";
import { HttpStatus } from "./http-status";

const DEFAULT_API_BASE_URL = "http://localhost:5128";
const API_BASE_URL = process.env.API_BASE_URL ?? DEFAULT_API_BASE_URL;

/** Bounds every backend call so a stalled .NET API can't hang a Next.js server worker (e.g. proxy.ts on every matched navigation). */
const REQUEST_TIMEOUT_MS = 10_000;

export class ApiError extends Error {
  readonly status: number;
  readonly fieldErrors?: Record<string, string>;

  constructor(status: number, message: string, fieldErrors?: Record<string, string>) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.fieldErrors = fieldErrors;
  }
}

interface BackendErrorBody {
  error?: string;
  errors?: Record<string, string>;
}

/** Server-side only: every call the Next.js server makes to the .NET API goes through this. */
export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: { "Content-Type": "application/json" },
  timeout: REQUEST_TIMEOUT_MS,
});

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (axios.isAxiosError(error)) {
      const status = error.response?.status ?? HttpStatus.NoResponse;
      const body = error.response?.data as BackendErrorBody | undefined;
      const message = body?.error || error.message || ERROR_MESSAGES.generic;
      return Promise.reject(new ApiError(status, message, body?.errors));
    }
    return Promise.reject(
      new ApiError(
        HttpStatus.NoResponse,
        error instanceof Error ? error.message : ERROR_MESSAGES.generic
      )
    );
  }
);

export function authHeaders(accessToken: string) {
  return { Authorization: `Bearer ${accessToken}` };
}
