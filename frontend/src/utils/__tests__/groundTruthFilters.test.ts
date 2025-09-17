import type { GroundTruth } from '../../types';
import { filterGroundTruths } from '../groundTruthFilters';

describe('filterGroundTruths', () => {
  const base: GroundTruth = {
    id: '1',
    prompt: 'How to replace a filter?',
    category: 'maintenance_request',
    status: 'new',
    tags: ['t1', 't2'],
    createdAt: new Date('2024-01-01'),
    updatedAt: new Date('2024-01-02'),
    generatedResponses: [],
    reviews: [],
    contexts: [],
    dataQueryDefinitions: []
  } as any;

  const items: GroundTruth[] = [
    base,
    { ...base, id: '2', status: 'validated', category: 'asset_knowledge', tags: ['t2'] },
    { ...base, id: '3', status: 'revisions_requested', category: 'unanswerable', tags: [] }
  ];

  test('filters by status', () => {
    const res = filterGroundTruths(items, { status: 'validated' });
    expect(res.map(r => r.id)).toEqual(['2']);
  });

  test('filters by multiple tags (AND)', () => {
    const res = filterGroundTruths(items, { tagIds: ['t1', 't2'] });
    expect(res.map(r => r.id)).toEqual(['1']);
  });

  test('search matches id or prompt', () => {
    const res = filterGroundTruths(items, { search: 'replace' });
    expect(res.length).toBe(items.length); // all prompts contain word
  });
});
