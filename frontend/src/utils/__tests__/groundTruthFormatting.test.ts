import { formatCategory, formatStatus, getStatusColorClass } from '../groundTruthFormatting';

describe('groundTruthFormatting', () => {
  test('formatCategory capitalizes and spaces', () => {
    expect(formatCategory('asset_knowledge')).toBe('Asset Knowledge');
  });
  test('formatStatus capitalizes and spaces', () => {
    expect(formatStatus('revisions_requested')).toBe('Revisions Requested');
  });
  test('getStatusColorClass returns class for known status', () => {
    expect(getStatusColorClass('validated')).toContain('green');
  });
});
