import { describe, expect, test } from 'vitest';
import { formatCategory, formatStatus, getStatusColorClass } from '../groundTruthFormatting';

describe('groundTruthFormatting', () => {
  test('formatCategory capitalizes and spaces', () => {
    expect(formatCategory('asset_knowledge')).toBe('Asset Knowledge');
  });
  test('formatStatus capitalizes and spaces', () => {
    expect(formatStatus('revisions_requested')).toBe('Revisions Requested');
  });
  test('getStatusColorClass returns class for canonical Validated', () => {
    expect(getStatusColorClass('Validated')).toContain('green');
  });
  test('getStatusColorClass accepts lowercase legacy form', () => {
    expect(getStatusColorClass('validated')).toContain('green');
  });
});
