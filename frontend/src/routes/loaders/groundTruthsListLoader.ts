// Loader for ground truths list route (Phase 1 Task 1.1)
// Fetches all ground truth definitions and returns them for the list page.
import { listGroundTruthDefinitions } from "@/services/groundTruthService";

export async function groundTruthsListLoader() {
  const groundTruths = await listGroundTruthDefinitions();
  return { groundTruths };
}
