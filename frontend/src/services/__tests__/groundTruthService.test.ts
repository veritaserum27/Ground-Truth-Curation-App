import { describe, expect, it } from 'vitest';
import { GroundTruthDefinitionDtoSchema } from '../../types/schemas';
import { groundTruthService } from '../groundTruthService';

// We import service lazily to avoid circular issues; mapping function not exported so we validate schema only for now.

const sample = {
  GroundTruthId: '11111111-1111-1111-1111-111111111111',
  UserQuery: 'How many widgets?',
  ValidationStatus: 'New, Data Curated',
  UserCreated: 'alice',
  UserUpdated: null,
  CreationDateTime: '2025-01-01T12:00:00Z',
  GroundTruthEntries: [
    {
      GroundTruthEntryId: '22222222-2222-2222-2222-222222222222',
      GroundTruthId: '11111111-1111-1111-1111-111111111111',
      GroundTruthContext: {
        ContextId: '33333333-3333-3333-3333-333333333333',
        GroundTruthId: '11111111-1111-1111-1111-111111111111',
        GroundTruthEntryId: '22222222-2222-2222-2222-222222222222',
        ContextType: 'Query',
        ContextParameters: [
          {
            ParameterId: '44444444-4444-4444-4444-444444444444',
            ParameterName: 'threshold',
            ParameterValue: '5',
            DataType: 'integer'
          }
        ]
      },
      Response: '42',
      RequiredValues: ['count'],
      RawData: [
        {
          DataQueryId: '55555555-5555-5555-5555-555555555555',
          RawData: [{ widgetCount: 42 }]
        }
      ],
      CreationDateTime: '2025-01-01T12:00:05Z'
    }
  ],
  DataQueryDefinitions: [
    {
      DataQueryId: '55555555-5555-5555-5555-555555555555',
      GroundTruthId: '11111111-1111-1111-1111-111111111111',
      DatastoreType: 'Sql',
      DatastoreName: 'mainDb',
      QueryTarget: 'Widgets',
      QueryDefinition: 'SELECT COUNT(*) as widgetCount FROM Widgets',
      IsFullQuery: true,
      RequiredProperties: ['widgetCount'],
      UserCreated: 'alice',
      UserUpdated: 'alice',
      CreationDateTime: '2025-01-01T11:59:59Z'
    }
  ],
  Comments: [
    {
      CommentId: '66666666-6666-6666-6666-666666666666',
      CommentText: 'Looks good',
      CommentType: 'Review',
      UserCreated: 'bob',
      CreationDateTime: '2025-01-01T12:00:10Z'
    }
  ],
  Tags: [
    { TagId: '77777777-7777-7777-7777-777777777777', Name: 'metrics', Description: 'Metric related' }
  ]
};

describe('GroundTruthDefinitionDtoSchema', () => {
  it('parses a valid backend DTO payload', () => {
    const parsed = GroundTruthDefinitionDtoSchema.parse(sample);
    expect(parsed.UserQuery).toBe('How many widgets?');
    expect(parsed.DataQueryDefinitions[0].RequiredProperties).toContain('widgetCount');
  });

  it('rejects invalid GUID', () => {
    const invalid = { ...sample, GroundTruthId: 'not-a-guid' };
    expect(() => GroundTruthDefinitionDtoSchema.parse(invalid)).toThrow();
  });

  it('normalizes and parses a camelCase payload via normalization utility', () => {
    const camel = {
      groundTruthId: sample.GroundTruthId,
      userQuery: sample.UserQuery,
      validationStatus: sample.ValidationStatus,
      userCreated: sample.UserCreated,
      userUpdated: sample.UserUpdated,
      creationDateTime: sample.CreationDateTime,
      groundTruthEntries: sample.GroundTruthEntries.map(e => ({
        groundTruthEntryId: e.GroundTruthEntryId,
        groundTruthId: e.GroundTruthId,
        groundTruthContext: e.GroundTruthContext && {
          contextId: e.GroundTruthContext.ContextId,
          groundTruthId: e.GroundTruthContext.GroundTruthId,
          groundTruthEntryId: e.GroundTruthContext.GroundTruthEntryId,
          contextType: e.GroundTruthContext.ContextType,
          contextParameters: e.GroundTruthContext.ContextParameters.map(p => ({
            parameterId: p.ParameterId,
            parameterName: p.ParameterName,
            parameterValue: p.ParameterValue,
            dataType: p.DataType
          }))
        },
        response: e.Response,
        requiredValues: e.RequiredValues,
        rawData: e.RawData.map(r => ({ dataQueryId: r.DataQueryId, rawData: r.RawData })),
        creationDateTime: e.CreationDateTime
      })),
      dataQueryDefinitions: sample.DataQueryDefinitions.map(d => ({
        dataQueryId: d.DataQueryId,
        groundTruthId: d.GroundTruthId,
        datastoreType: d.DatastoreType,
        datastoreName: d.DatastoreName,
        queryTarget: d.QueryTarget,
        queryDefinition: d.QueryDefinition,
        isFullQuery: d.IsFullQuery,
        requiredProperties: d.RequiredProperties,
        userCreated: d.UserCreated,
        userUpdated: d.UserUpdated,
        creationDateTime: d.CreationDateTime
      })),
      comments: sample.Comments.map(c => ({
        commentId: c.CommentId,
        commentText: c.CommentText,
        commentType: c.CommentType,
        userCreated: c.UserCreated,
        creationDateTime: c.CreationDateTime
      })),
      tags: sample.Tags.map(t => ({ tagId: t.TagId, name: t.Name, description: t.Description }))
    };
    const norm = (groundTruthService as any)._internal.normalizePascalCase(camel);
    const parsed = GroundTruthDefinitionDtoSchema.parse(norm);
    expect(parsed.UserQuery).toBe(sample.UserQuery);
    expect(parsed.GroundTruthEntries[0].GroundTruthContext?.ContextParameters[0].ParameterName).toBe('threshold');
  });

  it('defaults null DataType to string in context parameters', () => {
    const withNull = JSON.parse(JSON.stringify(sample));
    withNull.GroundTruthEntries[0].GroundTruthContext.ContextParameters.push({
      ParameterId: '99999999-9999-9999-9999-999999999999',
      ParameterName: 'flag',
      ParameterValue: 'true',
      DataType: null
    });
    const parsed = GroundTruthDefinitionDtoSchema.parse(withNull);
    const last = parsed.GroundTruthEntries[0].GroundTruthContext?.ContextParameters.at(-1);
    expect(last?.DataType).toBe('string');
  });
});
