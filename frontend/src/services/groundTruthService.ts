/**
 * groundTruthService
 * Goal: Keep it dead simple & explicit. No generic wrappers unless truly reusable.
 */

import {
  type GroundTruthDefinition,
  GroundTruthDefinitionDtoArraySchema,
  GroundTruthDefinitionDtoSchema
} from '../types/schemas';

const API_BASE = import.meta.env.VITE_API_URL || 'http://localhost:5105';

// ---- Config ----
// Resolve API base URL from (in order): import.meta.env (Vite), process.env, fallback default.

// ---- Lightweight error helper ----
export interface ApiError extends Error { status: number; details?: unknown }
function apiError(status: number, message: string, details?: unknown): ApiError {
  const e = new Error(message) as ApiError; e.status = status; e.details = details; return e;
}

// ---- Small helpers ----
function buildUrl(path: string, params?: Record<string, string | undefined>) {
  const qs = params ? Object.entries(params)
    .filter(([, v]) => v != null && v !== '')
    .map(([k, v]) => `${encodeURIComponent(k)}=${encodeURIComponent(v!)}`)
    .join('&') : '';
  return `${API_BASE}${path.startsWith('/') ? path : '/' + path}${qs ? `?${qs}` : ''}`;
}

async function fetchJson(url: string, signal?: AbortSignal) {
  const resp = await fetch(url, { signal });
  let data: unknown = null;
  try { data = await resp.json(); } catch { /* ignore parse error */ }
  if (!resp.ok) throw apiError(resp.status, (data as any)?.message || `HTTP ${resp.status}`, data);
  return data;
}

// ---- Casing Normalization ----
// Some environments/serializers return camelCase even though backend DTOs are PascalCase.
// We normalize only the top-level and nested object keys that start with lowercase -> uppercase first letter.
function normalizePascalCase<T>(value: T): T {
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



// ---- Public functions (explicit, minimal) ----
export async function listGroundTruthDefinitions(filter?: { userId?: string; validationStatus?: string }, signal?: AbortSignal): Promise<GroundTruthDefinition[]> {
  const url = buildUrl('/api/GroundTruth/definitions', filter);
  const raw = await fetchJson(url, signal);
  const normalized = normalizePascalCase(raw);
  const parsed = GroundTruthDefinitionDtoArraySchema.parse(normalized);
  return parsed;
}

export async function getGroundTruthDefinition(id: string, signal?: AbortSignal): Promise<GroundTruthDefinition> {
  const url = buildUrl(`/api/GroundTruth/definitions/${id}`);
  const raw = await fetchJson(url, signal);
  const normalized = normalizePascalCase(raw);
  const groundTruthDefinition = GroundTruthDefinitionDtoSchema.parse(normalized);
  return groundTruthDefinition
}

// Placeholders until backend endpoints exist
export async function createGroundTruthDefinition(): Promise<never> { throw apiError(501, 'Not implemented'); }
export async function addGroundTruthEntry(): Promise<never> { throw apiError(501, 'Not implemented'); }
export async function updateValidationStatus(): Promise<never> { throw apiError(501, 'Not implemented'); }

// Optional grouped export
export const groundTruthService = {
  listGroundTruthDefinitions,
  getGroundTruthDefinition,
  createGroundTruthDefinition,
  addGroundTruthEntry,
  updateValidationStatus,
  // Expose for tests/dev tools
  _internal: { normalizePascalCase }
};

// Usage example (pseudo):
// const defs = await listGroundTruthDefinitions();
// console.log(defs);
