import type { GroundTruthDefinition } from "../services/schemas";

export interface GroundTruthFilterOptions {
  status?: string;
  category?: string;
  tagIds?: string[]; // tags must ALL be present
  search?: string;   // matches id or title substring (case-insensitive)
}

export function filterGroundTruths(items: GroundTruthDefinition[], opts: GroundTruthFilterOptions): GroundTruthDefinition[] {
  const { status, category, tagIds, search } = opts;
  const searchLower = search?.trim().toLowerCase();
  return items.filter(gt => {
    if (status && gt.ValidationStatus !== status) return false;
    // TODO: if (category && gt.Category !== category) return false;
    if (tagIds && tagIds.length) {
      const allPresent = tagIds.every(t => gt.Tags?.includes(t));
      if (!allPresent) return false;
    }
    if (searchLower) {
      const composite = `${gt.GroundTruthId}|${gt.UserQuery || ''}`.toLowerCase();
      if (!composite.includes(searchLower)) return false;
    }
    return true;
  });
}
