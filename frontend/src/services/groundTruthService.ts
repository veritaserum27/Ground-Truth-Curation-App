/**
 * groundTruthService
 * Goal: Keep it dead simple & explicit. No generic wrappers unless truly reusable.
 */

import type { DataStoreType, GroundTruth, GroundTruthStatus } from '../types';
import { GroundTruthDefinitionArraySchema, GroundTruthDefinitionSchema, type BackendDataQueryDefinition, type BackendGroundTruthDefinition, type BackendTag } from './schemas';

// ---- Config ----
const rawBase = (import.meta as unknown as { env?: { VITE_API_BASE_URL?: string } }).env?.VITE_API_BASE_URL;
const API_BASE = (rawBase ? rawBase.replace(/\/$/, '') : 'http://localhost:5000');

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

// ---- Mapping ----
const STATUS_VALUES: GroundTruthStatus[] = ['new', 'revisions_requested', 'validated', 'out-of-scope'];
const STATUS_SET = new Set<string>(STATUS_VALUES);

const mapStatus = (s: string): GroundTruthStatus => (STATUS_SET.has(s.toLowerCase()) ? s.toLowerCase() : 'new') as GroundTruthStatus;
const mapDataStoreType = (v: string): DataStoreType => v.toLowerCase().includes('graph') ? 'GraphQL' : v.toLowerCase().includes('cosmos') ? 'CosmosDB' : 'SQL';
function mapDataQueryDefinition(d: BackendDataQueryDefinition) {
  return {
    id: d.DataQueryId,
    dataStoreType: mapDataStoreType(d.DatastoreType),
    query: d.QueryDefinition,
    name: d.QueryTarget || d.DatastoreName || 'Query',
    contextId: 'default'
  };
}
function mapDefinition(dto: BackendGroundTruthDefinition): GroundTruth {
  return {
    id: dto.GroundTruthId,
    prompt: dto.UserQuery,
    status: mapStatus(dto.ValidationStatus),
    category: 'asset_knowledge',
    createdAt: new Date(dto.CreationDateTime),
    updatedAt: new Date(dto.StartDateTime || dto.CreationDateTime),
    createdBy: dto.UserCreated || 'unknown',
    dataCuratorNotes: '',
    contexts: [],
    dataQueryDefinitions: dto.DataQueryDefinitions.map(mapDataQueryDefinition),
    tags: dto.Tags.map((t: BackendTag) => t.TagId),
    generatedResponses: [],
    reviews: []
  };
}

// ---- Public functions (explicit, minimal) ----
export async function listGroundTruthDefinitions(filter?: { userId?: string; validationStatus?: string }, signal?: AbortSignal): Promise<GroundTruth[]> {
  const url = buildUrl('/api/GroundTruth/definitions', filter);
  const raw = await fetchJson(url, signal);
  const parsed = GroundTruthDefinitionArraySchema.parse(raw);
  return parsed.map(mapDefinition);
}

export async function getGroundTruthDefinition(id: string, signal?: AbortSignal): Promise<GroundTruth> {
  const url = buildUrl(`/api/GroundTruth/definitions/${id}`);
  const raw = await fetchJson(url, signal);
  const parsed = GroundTruthDefinitionSchema.parse(raw);
  return mapDefinition(parsed);
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
  updateValidationStatus
};

// Usage example (pseudo):
// const defs = await listGroundTruthDefinitions();
// console.log(defs);
