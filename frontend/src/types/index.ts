// User and Authentication Types
export type UserRole = 'data_validator' | 'data_curator';

export interface User {
  id: string;
  name: string;
  email: string;
  role: UserRole;
}

// Tag Types
export interface Tag {
  id: string;
  name: string;
  color: string;
  isPredefined: boolean;
  createdBy?: string;
  createdAt?: Date;
}

// Parameter Types
export type ParameterDataType = 'string' | 'float' | 'decimal' | 'datetime' | 'integer' | 'boolean';

export interface Parameter {
  name: string;
  value: string;
  dataType: ParameterDataType;
}

// Ground Truth Types
export type GroundTruthCategory = 'asset_knowledge' | 'unanswerable' | 'maintenance_request';
export type GroundTruthStatus = 'new' | 'revisions_requested' | 'validated' | 'out-of-scope';
export type DataStoreType = 'GraphQL' | 'SQL' | 'CosmosDB';

export interface Context {
  id: string;
  parameters: Parameter[]; // Array of parameters with name, value, and data type
}

export interface DataQueryDefinition {
  id: string;
  dataStoreType: DataStoreType;
  query: string;
  name?: string;
  contextId: string; // Reference to the context this query uses
}

export interface GeneratedResponse {
  id: string;
  content: string;
  rawData: string;
  model: string;
  timestamp: Date;
  confidence: number;
  dataQueryDefinitionId: string;
}

export interface Review {
  id: string;
  reviewerId: string;
  reviewerName: string;
  content: string;
  rating: number;
  timestamp: Date;
  userQueryAtTime: string;
  dataStoreTypeAtTime: DataStoreType;
  dataQueryDefinitionAtTime: string;
  formattedResponseAtTime: string;
}

export interface CuratorNote {
  id: string;
  curatorId: string;
  curatorName: string;
  content: string;
  timestamp: Date;
  dataQueryDefinitionAtTime: string;
  formattedResponseAtTime: string;
}

export interface GroundTruth {
  id: string;
  prompt: string;
  contexts: Context[];
  dataQueryDefinitions: DataQueryDefinition[];
  category: GroundTruthCategory;
  status: GroundTruthStatus;
  tags: string[]; // Array of tag IDs
  createdAt: Date;
  updatedAt: Date;
  createdBy: string;
  dataCuratorNotes: string;
  generatedResponses: GeneratedResponse[];
  reviews: Review[];
}