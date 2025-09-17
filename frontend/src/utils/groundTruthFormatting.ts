/**
 * groundTruthFormatting.tsx
 * Centralized formatting & display helpers for ground truth entities.
 */
import type { GroundTruthCategory, GroundTruthStatus } from '../types';

export function formatCategory(category: GroundTruthCategory): string {
  return category.replace(/_/g, ' ').replace(/\b\w/g, l => l.toUpperCase());
}

export function formatStatus(status: GroundTruthStatus): string {
  return status.replace(/_/g, ' ').replace(/\b\w/g, l => l.toUpperCase());
}

export function getStatusColorClass(status: GroundTruthStatus): string {
  switch (status) {
    case 'new': return 'bg-gray-100 text-gray-800';
    case 'revisions_requested': return 'bg-red-100 text-red-800';
    case 'validated': return 'bg-green-100 text-green-800';
    case 'out-of-scope': return 'bg-yellow-100 text-yellow-800';
  }
}
