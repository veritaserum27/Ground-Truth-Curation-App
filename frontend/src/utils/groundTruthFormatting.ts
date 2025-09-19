/**
 * groundTruthFormatting.tsx
 * Centralized formatting & display helpers for ground truth entities.
 */
import type { ValidationStatus } from '../types/schemas';

export function formatCategory(category: string): string {
  return category.replace(/_/g, ' ').replace(/\b\w/g, l => l.toUpperCase());
}

export function formatStatus(status: string): string {
  return status.replace(/_/g, ' ').replace(/\b\w/g, l => l.toUpperCase());
}

// Accept either canonical ValidationStatus value or legacy lowercase/underscore variants (e.g. 'validated', 'revisions_requested').
export function getStatusColorClass(statusInput: string | ValidationStatus): string {
  if (!statusInput) return 'bg-gray-100 text-gray-800';
  const raw = statusInput.toString();
  const normalized = raw
    .replace(/_/g, ' ')
    .replace(/\b\w/g, l => l.toUpperCase())
    .trim();

  switch (normalized) {
    case 'Validated':
      return 'bg-green-100 text-green-800';
    case 'Request Revisions':
    case 'Revisions Requested': // tolerate legacy phrasing
      return 'bg-yellow-100 text-yellow-800';
    case 'Pending':
      return 'bg-amber-100 text-amber-800';
    case 'New, Data Curated':
    case 'New':
      return 'bg-blue-100 text-blue-800';
    default:
      return 'bg-gray-100 text-gray-800';
  }
}
