import { z } from 'zod';

/**
 * Backend DTO Zod Schemas (PascalCase) — mirror C# exactly.
 * Any transformation to frontend domain models happens in the service layer.
 */

// Simple enums represented as strings in API responses (unvalidated pass-through for now)

// DatastoreType enum (backend: Sql | CosmosDb). Accept numeric indices defensively but normalize.
export const DatastoreTypeSchema = z.union([z.literal(0), z.literal(1), z.enum(['Sql', 'CosmosDb'])])
  .transform(v => (v === 0 || v === 'Sql') ? 'Sql' : 'CosmosDb');

// Category Enum
// ValidationStatus Enum
const validation = ['New, Data Curated', 'Validated', 'Request Revisions', 'Pending'] as const;
export const ValidationStatusSchema = z.enum(validation).default('New, Data Curated');
// TagDto
export const TagDtoSchema = z.object({
  TagId: z.string().uuid(),
  Name: z.string(),
  Description: z.string()
});

// CommentDto
export const CommentDtoSchema = z.object({
  CommentId: z.string().uuid(),
  CommentText: z.string(),
  CommentType: z.string(),
  UserCreated: z.string(),
  CreationDateTime: z.string()
});

// RawDataDto (collection of arbitrary records)
export const RawDataDtoSchema = z.object({
  DataQueryId: z.string().uuid(),
  RawData: z.array(z.record(z.any()))
});

// ContextParameterDto
export const ContextParameterDtoSchema = z.object({
  ParameterId: z.string().uuid(),
  ParameterName: z.string(),
  ParameterValue: z.string(),
  DataType: z.string().nullable().optional()
}).transform(p => ({
  ...p,
  DataType: p.DataType ?? 'string'
}));

// GroundTruthContextDto
export const GroundTruthContextDtoSchema = z.object({
  ContextId: z.string().uuid(),
  GroundTruthId: z.string().uuid(),
  GroundTruthEntryId: z.string().uuid(),
  ContextType: z.string(),
  ContextParameters: z.array(ContextParameterDtoSchema)
});

// DataQueryDefinitionDto
export const DataQueryDefinitionDtoSchema = z.object({
  DataQueryId: z.string().uuid().nullable().optional(),
  GroundTruthId: z.string().uuid(),
  DatastoreType: DatastoreTypeSchema,
  DatastoreName: z.string(),
  QueryTarget: z.string(),
  QueryDefinition: z.string(),
  IsFullQuery: z.boolean(),
  RequiredProperties: z.array(z.string()),
  UserCreated: z.string(),
  UserUpdated: z.string(),
  CreationDateTime: z.string()
});

// GroundTruthEntryDto
export const GroundTruthEntryDtoSchema = z.object({
  GroundTruthEntryId: z.string().uuid(),
  GroundTruthId: z.string().uuid(),
  GroundTruthContext: GroundTruthContextDtoSchema.nullable().optional(),
  Response: z.string(),
  RequiredValues: z.array(z.string()),
  RawData: z.array(RawDataDtoSchema),
  CreationDateTime: z.string()
});

// GroundTruthDefinitionDto
export const GroundTruthDefinitionDtoSchema = z.object({
  GroundTruthId: z.string().uuid(),
  UserQuery: z.string(),
  ValidationStatus: ValidationStatusSchema,
  Category: z.string().optional().nullable(),
  UserCreated: z.string(),
  UserUpdated: z.string().nullable().optional(),
  CreationDateTime: z.string(),
  GroundTruthEntries: z.array(GroundTruthEntryDtoSchema),
  DataQueryDefinitions: z.array(DataQueryDefinitionDtoSchema),
  Comments: z.array(CommentDtoSchema),
  Tags: z.array(TagDtoSchema)
});

export const GroundTruthDefinitionDtoArraySchema = z.array(GroundTruthDefinitionDtoSchema);
export const GroundTruthContextDtoArraySchema = z.array(GroundTruthContextDtoSchema);
// Export backend DTO types
export type Tag = z.infer<typeof TagDtoSchema>;
export type Comment = z.infer<typeof CommentDtoSchema>;
export type RawData = z.infer<typeof RawDataDtoSchema>;
export type ContextParameter = z.infer<typeof ContextParameterDtoSchema>;
export type GroundTruthContext = z.infer<typeof GroundTruthContextDtoSchema>;
export type DataQueryDefinition = z.infer<typeof DataQueryDefinitionDtoSchema>;
export type GroundTruthEntry = z.infer<typeof GroundTruthEntryDtoSchema>;
export type GroundTruthDefinition = z.infer<typeof GroundTruthDefinitionDtoSchema>;
export type ValidationStatus = z.infer<typeof ValidationStatusSchema>;


// Backward compatibility aliases (legacy names used elsewhere in code before refactor)
export const TagSchema = TagDtoSchema;
export const CommentSchema = CommentDtoSchema;
export const RawDataChunkSchema = RawDataDtoSchema; // name retained though shape changed to PascalCase inside
export const DataQueryDefinitionSchema = DataQueryDefinitionDtoSchema;
export const GroundTruthEntrySchema = GroundTruthEntryDtoSchema;
export const GroundTruthDefinitionSchema = GroundTruthDefinitionDtoSchema;
export const GroundTruthDefinitionArraySchema = GroundTruthDefinitionDtoArraySchema;
