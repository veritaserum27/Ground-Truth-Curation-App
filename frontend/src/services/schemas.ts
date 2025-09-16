import { z } from 'zod';

// Enumerations / constrained value sets inferred from frontend domain types
export const ValidationStatusSchema = z.enum(['new', 'revisions_requested', 'validated', 'out-of-scope']);

export const TagSchema = z.object({
  TagId: z.string().uuid(),
  Name: z.string(),
  Description: z.string().optional().default('')
});

export const CommentSchema = z.object({
  CommentId: z.string().uuid(),
  GroundTruthId: z.string().uuid(),
  CommentText: z.string(),
  CommentDateTime: z.string(),
  UserId: z.string(),
  CommentType: z.string().optional().default('')
});

export const DataQueryDefinitionSchema = z.object({
  DataQueryId: z.string().uuid(),
  GroundTruthId: z.string().uuid(),
  DatastoreType: z.string(),
  DatastoreName: z.string().optional().default(''),
  QueryTarget: z.string().optional().default(''),
  QueryDefinition: z.string(),
  IsFullQuery: z.boolean(),
  RequiredPropertiesJson: z.string().optional().default(''),
  UserCreated: z.string().optional().default(''),
  UserUpdated: z.string().optional().default(''),
  CreationDateTime: z.string(),
  StartDateTime: z.string(),
  EndDateTime: z.string()
});

export const GroundTruthEntrySchema = z.object({
  GroundTruthEntryId: z.string().uuid(),
  GroundTruthId: z.string().uuid(),
  Response: z.string().optional().default(''),
  RequiredValuesJson: z.string().optional().default(''),
  RawDataJson: z.string().optional().default(''),
  CreationDateTime: z.string(),
  StartDateTime: z.string(),
  EndDateTime: z.string()
});

export const GroundTruthDefinitionSchema = z.object({
  GroundTruthId: z.string().uuid(),
  UserQuery: z.string(),
  ValidationStatus: ValidationStatusSchema,
  UserCreated: z.string().optional().default(''),
  UserUpdated: z.string().optional().default(''),
  CreationDateTime: z.string(),
  StartDateTime: z.string(),
  EndDateTime: z.string(),
  GroundTruthEntries: z.array(GroundTruthEntrySchema).default([]),
  DataQueryDefinitions: z.array(DataQueryDefinitionSchema).default([]),
  Comments: z.array(CommentSchema).default([]),
  Tags: z.array(TagSchema).default([])
});

export const GroundTruthDefinitionArraySchema = z.array(GroundTruthDefinitionSchema);

export type BackendValidationStatus = z.infer<typeof ValidationStatusSchema>;
export type BackendGroundTruthDefinition = z.infer<typeof GroundTruthDefinitionSchema>;
export type BackendGroundTruthEntry = z.infer<typeof GroundTruthEntrySchema>;
export type BackendDataQueryDefinition = z.infer<typeof DataQueryDefinitionSchema>;
export type BackendTag = z.infer<typeof TagSchema>;
export type BackendComment = z.infer<typeof CommentSchema>;
