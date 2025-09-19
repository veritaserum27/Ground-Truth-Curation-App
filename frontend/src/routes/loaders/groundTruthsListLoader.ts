// Loader for ground truths list route (Phase 1 Task 1.1)
// Fetches all ground truth definitions and returns them for the list page.
import { listGroundTruthDefinitions } from "../../services/groundTruthService";
import type { GroundTruthDefinition } from "../../types/schemas";

export interface GroundTruthsListLoaderResponse {
  groundTruths: GroundTruthDefinition[]
}
export async function groundTruthsListLoader(): Promise<GroundTruthsListLoaderResponse> {
  const groundTruths = await listGroundTruthDefinitions();
  return { groundTruths };
}
