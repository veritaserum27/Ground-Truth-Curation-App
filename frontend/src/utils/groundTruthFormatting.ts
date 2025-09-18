/**
 * groundTruthFormatting.tsx
 * Centralized formatting & display helpers for ground truth entities.
 */
import type { ValidationStatus } from '../services/schemas';
import type { GroundTruthCategory } from '../types';

export function formatCategory(category: GroundTruthCategory): string {
  return category.replace(/_/g, ' ').replace(/\b\w/g, l => l.toUpperCase());
}

export function formatStatus(status: string): string {
  return status.replace(/_/g, ' ').replace(/\b\w/g, l => l.toUpperCase());
}

export function getStatusColorClass(status: ValidationStatus): string {
  switch (status) {
    case "New, Data Curated": return 'bg-gray-100 text-gray-800';
    case "Pending": return 'bg-gray-100 text-gray-800';
    case "Request Revisions": return 'bg-gray-100 text-gray-800';
    case "Validated": return 'bg-gray-100 text-gray-800';
    default: return 'bg-gray-100 text-gray-800'
  }
}
