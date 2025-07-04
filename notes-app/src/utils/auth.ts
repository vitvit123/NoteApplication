// src/utils/auth.ts

export function getToken(): string | null {
  return localStorage.getItem("token");
}

export function getUserId(): string | null {
  return localStorage.getItem("userId");
}

export function setToken(token: string): void {
  localStorage.setItem("token", token);
}

export function setUserId(userId: string): void {
  localStorage.setItem("userId", userId);
}

export function clearAuth(): void {
  localStorage.removeItem("token");
  localStorage.removeItem("userId");
}
