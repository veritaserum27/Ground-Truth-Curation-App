import { z } from 'zod';

// Enumerations / constrained value sets inferred from frontend domain types
export const ValidationStatusSchema = z.enum(['new', 'revisions_requested', 'validated', 'out-of-scope']);

// camelCase version matching ASP.NET Core default JSON output
export const TagSchema = z.object({
  tagId: z.string().uuid(),
  name: z.string(),
  description: z.string().optional().default('')
});

export const CommentSchema = z.object({
  commentId: z.string().uuid(),
  groundTruthId: z.string().uuid(),
  commentText: z.string(),
  commentDateTime: z.string(),
  userId: z.string(),
  commentType: z.string().optional().default('')
});

export const DataQueryDefinitionSchema = z.object({
  dataQueryId: z.string().uuid(),
  groundTruthId: z.string().uuid(),
  datastoreType: z.string(),
  datastoreName: z.string().optional().default(''),
  queryTarget: z.string().optional().default(''),
  queryDefinition: z.string(),
  isFullQuery: z.boolean(),
  requiredPropertiesJson: z.string().optional().default(''),
  userCreated: z.string().optional().default(''),
  userUpdated: z.string().optional().default(''),
  creationDateTime: z.string(),
  startDateTime: z.string(),
  endDateTime: z.string()
});

export const GroundTruthEntrySchema = z.object({
  groundTruthEntryId: z.string().uuid(),
  groundTruthId: z.string().uuid(),
  response: z.string().optional().default(''),
  requiredValuesJson: z.string().optional().default(''),
  rawDataJson: z.string().optional().default(''),
  creationDateTime: z.string(),
  startDateTime: z.string(),
  endDateTime: z.string()
});

export const GroundTruthDefinitionSchema = z.object({
  groundTruthId: z.string().uuid(),
  userQuery: z.string(),
  // Accept any string from backend then canonicalize later in service mapping
  validationStatus: z.string(),
  userCreated: z.string().optional().default(''),
  userUpdated: z.string().optional().default(''),
  creationDateTime: z.string(),
  startDateTime: z.string(),
  endDateTime: z.string(),
  groundTruthEntries: z.array(GroundTruthEntrySchema).default([]),
  dataQueryDefinitions: z.array(DataQueryDefinitionSchema).default([]),
  comments: z.array(CommentSchema).default([]),
  tags: z.array(TagSchema).default([])
});

export const GroundTruthDefinitionArraySchema = z.array(GroundTruthDefinitionSchema);

export type BackendValidationStatus = z.infer<typeof ValidationStatusSchema>;
export type BackendGroundTruthDefinition = z.infer<typeof GroundTruthDefinitionSchema>;
export type BackendGroundTruthEntry = z.infer<typeof GroundTruthEntrySchema>;
export type BackendDataQueryDefinition = z.infer<typeof DataQueryDefinitionSchema>;
export type BackendTag = z.infer<typeof TagSchema>;
export type BackendComment = z.infer<typeof CommentSchema>;
