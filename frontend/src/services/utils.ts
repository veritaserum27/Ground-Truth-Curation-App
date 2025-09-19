import { config } from "../utils/config";
export const API_BASE = config.apiUrl || 'http://localhost:5105';

// ---- Config ----
// Resolve API base URL from (in order): import.meta.env (Vite), process.env, fallback default.

// ---- Lightweight error helper ----
export interface ApiError extends Error { status: number; details?: unknown }
export function apiError(status: number, message: string, details?: unknown): ApiError {
  const e = new Error(message) as ApiError; e.status = status; e.details = details; return e;
}

// ---- Small helpers ----
export function buildUrl(path: string, params?: Record<string, string | undefined>) {
  const qs = params ? Object.entries(params)
    .filter(([, v]) => v != null && v !== '')
    .map(([k, v]) => `${encodeURIComponent(k)}=${encodeURIComponent(v!)}`)
    .join('&') : '';
  return `${API_BASE}${path.startsWith('/') ? path : '/' + path}${qs ? `?${qs}` : ''}`;
}

export async function fetchJson<T>(url: string, signal?: AbortSignal): Promise<T> {
  const resp = await fetch(url, { signal });
  let data: T | undefined;
  try { data = await resp.json(); } catch { /* ignore parse error */ }
  if (!data) throw apiError(resp.status, `HTTP ${resp.status} (no JSON response Body)`);
  if (!resp.ok) throw apiError(resp.status, (data as any)?.message || `HTTP ${resp.status}`, data);
  return data;
}

// ---- Casing Normalization ----
// Some environments/serializers return camelCase even though backend DTOs are PascalCase.
// We normalize only the top-level and nested object keys that start with lowercase -> uppercase first letter.
export function normalizePascalCase<T>(value: T): T {
  if (Array.isArray(value)) return value.map(v => normalizePascalCase(v)) as unknown as T;
  if (value && typeof value === 'object') {
    const obj = value as Record<string, unknown>;
    const out: Record<string, unknown> = {}; // avoid mutating original
    for (const [k, v] of Object.entries(obj)) {
      const first = k.charAt(0);
      const needs = first >= 'a' && first <= 'z';
      const newKey = needs ? first.toUpperCase() + k.slice(1) : k;
      // Only assign if target key not already present (prefer existing PascalCase)
      if (!(newKey in out)) {
        out[newKey] = normalizePascalCase(v);
      }
    }
    return out as unknown as T;
  }
  return value;
}
