import { describe, expect, test } from 'vitest';
import type { GroundTruthDefinition } from '../../types/schemas';
import { filterGroundTruths } from '../groundTruthFilters';

describe('filterGroundTruths', () => {
  const base: GroundTruthDefinition = {
    GroundTruthId: '1',
    UserQuery: 'How to replace a filter?',
    ValidationStatus: 'New, Data Curated',
    Category: 'maintenance_request',
    UserCreated: 'user',
    UserUpdated: 'user',
    CreationDateTime: new Date('2024-01-01').toISOString(),
    GroundTruthEntries: [],
    DataQueryDefinitions: [],
    Comments: [],
    Tags: [{ TagId: 't1', Name: 'T1', Description: '' }, { TagId: 't2', Name: 'T2', Description: '' }]
  } as any;

  const items: GroundTruthDefinition[] = [
    base,
    { ...base, GroundTruthId: '2', ValidationStatus: 'Validated', Category: 'asset_knowledge', Tags: [{ TagId: 't2', Name: 'T2', Description: '' }] },
    { ...base, GroundTruthId: '3', ValidationStatus: 'Request Revisions', Category: 'unanswerable', Tags: [] }
  ];

  test('filters by status', () => {
    const res = filterGroundTruths(items, { status: 'Validated' });
    expect(res.map(r => r.GroundTruthId)).toEqual(['2']);
  });

  test('filters by multiple tags (AND)', () => {
    const res = filterGroundTruths(items, { tagIds: ['t1', 't2'] });
    expect(res.map(r => r.GroundTruthId)).toEqual(['1']);
  });

  test('search matches id or prompt', () => {
    const res = filterGroundTruths(items, { search: 'replace' });
    expect(res.length).toBe(items.length); // all prompts contain word
  });
});
