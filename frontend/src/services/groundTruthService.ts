/**
 * groundTruthService
 * Goal: Keep it dead simple & explicit. No generic wrappers unless truly reusable.
 */

import {
  type GroundTruthContext,
  type GroundTruthDefinition,
  GroundTruthDefinitionDtoArraySchema,
  GroundTruthDefinitionDtoSchema
} from '../types/schemas';
import { apiError, buildUrl, fetchJson, normalizePascalCase } from './utils';
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

export async function updateGroundTruthContext(groundTruthId: string, contexts: GroundTruthContext[]): Promise<GroundTruthDefinition> {

  const url = buildUrl(`/api/groundtruth/definitions/${groundTruthId}/contexts`);

  try {
    const res = await fetch(url, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(contexts)
    })
    if (!res.ok) {
      throw apiError(res.status, `HTTP ${res.status}: ${res.statusText} - Issue updating contexts`);
    }

    let data: GroundTruthDefinition | null = null;
    try { data = await res.json(); } catch { /* ignore parse error */ }
    if (!data) {
      throw apiError(res.status, `HTTP ${res.status}: ${res.statusText} - Issue with updating contexts: ${data}`);
    }
    console.log({ data })
    const normalized = normalizePascalCase(data);

    const groundTruth = GroundTruthDefinitionDtoSchema.parse(normalized);
    return groundTruth;

  } catch (err) {
    // make sure to throw apiError incase it pops up.
    throw err;
  }
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
