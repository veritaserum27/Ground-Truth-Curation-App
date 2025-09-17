import type { GroundTruth } from '../types';

export interface GroundTruthFilterOptions {
  status?: string;
  category?: string;
  tagIds?: string[]; // tags must ALL be present
  search?: string;   // matches id or title substring (case-insensitive)
}

export function filterGroundTruths(items: GroundTruth[], opts: GroundTruthFilterOptions): GroundTruth[] {
  const { status, category, tagIds, search } = opts;
  const searchLower = search?.trim().toLowerCase();
  return items.filter(gt => {
    if (status && gt.status !== status) return false;
    if (category && gt.category !== category) return false;
    if (tagIds && tagIds.length) {
      const allPresent = tagIds.every(t => gt.tags?.includes(t));
      if (!allPresent) return false;
    }
    if (searchLower) {
      const composite = `${gt.id}|${gt.prompt || ''}`.toLowerCase();
      if (!composite.includes(searchLower)) return false;
    }
    return true;
  });
}
