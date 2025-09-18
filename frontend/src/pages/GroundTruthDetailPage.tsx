import {
  ChevronDown,
  Edit,
  Plus,
  Save,
  Trash2,
  X
} from "lucide-react";
import { useState } from "react";
import { NavLink, useLoaderData } from "react-router";
import { TagManager } from "../components/TagManager";
import { useAuth } from "../contexts/AuthContext";
import { useEditingData } from "../contexts/EditingContext";
import type { ContextParameterCreate, ContextParameterUpdate, GroundTruthContextCreate, GroundTruthContextUpdate, GroundTruthDefinition, Tag, ValidationStatus } from "../services/schemas";

// Helper functions
const formatCategory = (category: string) =>
  category
    .replace(/_/g, " ")
    .replace(/\b\w/g, (l) => l.toUpperCase());

const formatStatus = (status: ValidationStatus) =>
  status
    .replace(/_/g, " ")
    .replace(/\b\w/g, (l) => l.toUpperCase());

// Re-export external loader implementation
export { groundTruthDetailLoader as clientLoader } from "../routes/loaders/groundTruthDetailLoader";

function BackToList({ groundTruth }: { groundTruth: GroundTruthDefinition | undefined }) {
  return (
    <div className="text-center py-8">
      {!groundTruth && (
        <p className="text-muted-foreground">
          Ground truth not found.
        </p>
      )}
      <NavLink to="/ground-truths" className="mt-4 px-4 py-2 bg-primary text-primary-foreground rounded-md">
        Back to List
      </NavLink>
    </div>
  )
}
export default function GroundTruthDetail() {
  const { user } = useAuth();
  const { isEditing, setIsEditing, editForm, setEditForm, applyLocalEdits, deepCopyGroundTruth, getMergedGroundTruth, getMergedReviews } = useEditingData();
  const { groundTruth: baseGroundTruth } = useLoaderData() as { groundTruth: GroundTruthDefinition };
  const groundTruth = getMergedGroundTruth(baseGroundTruth);


  const [activeResponseTab, setActiveResponseTab] = useState<{
    [key: string]: "formatted" | "raw" | "metadata";
  }>({});
  const [expandedReviews, setExpandedReviews] = useState<{
    [key: string]: boolean;
  }>({});
  const [expandedCuratorNotes, setExpandedCuratorNotes] =
    useState<{ [key: string]: boolean }>({});


  if (!groundTruth) {
    return (
      <BackToList groundTruth={groundTruth} />
    );
  }

  const handleSave = () => {
    const updatePayload = buildUpdatePayload(groundTruth);
    // For now just stash the update locally; API submission will be wired later
    applyLocalEdits(groundTruth.GroundTruthId, updatePayload);
    setIsEditing(false);
    setEditForm({} as any);
  };

  const handleCancel = () => {
    setIsEditing(false);
    setEditForm({} as any);
  };

  const handleEdit = () => {
    // Seed edit form with a shallow copy including legacy convenience fields
    setEditForm({
      GroundTruthId: groundTruth.GroundTruthId,
      UserQuery: groundTruth.UserQuery,
      ValidationStatus: groundTruth.ValidationStatus,
      Category: groundTruth.Category,
      prompt: groundTruth.UserQuery,
      status: groundTruth.ValidationStatus,
      category: groundTruth.Category ?? undefined,
      tags: groundTruth.Tags?.map(t => ({ id: t.TagId, name: t.Name, description: t.Description }))
    } as any);
    setIsEditing(true);
  };

  // Check if user can edit (curators can edit everything, validators can only edit status and tags)
  const canEdit = user?.role === "data_curator" || user?.role === "data_validator";
  const isDataValidator = user?.role === "data_validator";
  const isDataCurator = user?.role === "data_curator";

  // GroundTruthContext editing helpers (only for data curators)
  // TODO: migrate context editing to operate on GroundTruthEntries[].GroundTruthContext
  const updateContexts = (newContexts: (GroundTruthContextCreate | GroundTruthContextUpdate)[]) => {
    setEditForm((prev: any) => ({ ...prev, contexts: newContexts }));
  };

  const addNewContext = () => {
    const currentContexts: (GroundTruthContextCreate | GroundTruthContextUpdate)[] = (editForm as any).contexts || [];
    const newContext: GroundTruthContextCreate = {
      ContextParameters: [],
      ContextType: "",
    } as GroundTruthContextCreate;
    updateContexts([...currentContexts, newContext]);
  };

  const removeContext = (contextIndex: number) => {
    const currentContexts: (GroundTruthContextCreate | GroundTruthContextUpdate)[] = (editForm as any).contexts || [];
    updateContexts(currentContexts.filter((_, i) => i !== contextIndex));
  };

  const updateContextParameter = (
    contextIndex: number,
    paramIndex: number,
    field: "name" | "value" | "dataType",
    newValue: string,
  ) => {
    const current = (editForm as any).contexts as (GroundTruthContextCreate | GroundTruthContextUpdate)[] || [];
    if (!current[contextIndex]) return;
    const ctx = current[contextIndex];
    const params: (ContextParameterCreate | ContextParameterUpdate)[] = (ctx as any).ContextParameters || [];
    const existing = params[paramIndex];
    if (!existing) return;
    const updatedParam = { ...existing } as any;
    if (field === 'name') updatedParam.ParameterName = newValue;
    if (field === 'value') updatedParam.ParameterValue = newValue;
    if (field === 'dataType') updatedParam.DataType = newValue;
    const newParams = [...params];
    newParams[paramIndex] = updatedParam;
    const newContexts = [...current];
    (newContexts[contextIndex] as any) = { ...ctx, ContextParameters: newParams };
    updateContexts(newContexts);
  };

  const addParameterToContext = (contextIndex: number) => {
    const current = (editForm as any).contexts as (GroundTruthContextCreate | GroundTruthContextUpdate)[] || [];
    if (!current[contextIndex]) return;
    const ctx = current[contextIndex];
    const params: (ContextParameterCreate | ContextParameterUpdate)[] = (ctx as any).ContextParameters || [];
    const newParameter: ContextParameterCreate = {
      ParameterName: "NewParam",
      ParameterValue: "",
      DataType: "string"
    };
    const newContexts = [...current];
    (newContexts[contextIndex] as any) = { ...ctx, ContextParameters: [...params, newParameter] };
    updateContexts(newContexts);
  };

  const removeParameterFromContext = (contextIndex: number, paramIndex: number) => {
    const current = (editForm as any).contexts as (GroundTruthContextCreate | GroundTruthContextUpdate)[] || [];
    if (!current[contextIndex]) return;
    const ctx = current[contextIndex];
    const params: (ContextParameterCreate | ContextParameterUpdate)[] = (ctx as any).ContextParameters || [];
    const newParams = params.filter((_, i) => i !== paramIndex);
    const newContexts = [...current];
    (newContexts[contextIndex] as any) = { ...ctx, ContextParameters: newParams };
    updateContexts(newContexts);
  };

  return (
    <div className="space-y-6">
      {/* Detail Header */}
      <div className="flex items-center justify-between">
        <BackToList groundTruth={groundTruth} />
        {canEdit && !isEditing && (
          <button
            onClick={handleEdit}
            className="flex items-center gap-2 px-4 py-2 bg-primary text-primary-foreground rounded-md hover:bg-primary/90"
          >
            <Edit className="w-4 h-4" />
            {isDataValidator ? "Update Status & Tags" : "Edit Ground Truth"}
          </button>
        )}

        {isEditing && (
          <div className="flex gap-2">
            <button
              onClick={handleSave}
              className="flex items-center gap-2 px-4 py-2 bg-primary text-primary-foreground rounded-md hover:bg-primary/90"
            >
              <Save className="w-4 h-4" />
              Save Changes
            </button>
            <button
              onClick={handleCancel}
              className="flex items-center gap-2 px-4 py-2 hover:bg-muted rounded-md"
            >
              <X className="w-4 h-4" />
              Cancel
            </button>
          </div>
        )}
      </div>

      {/* Main Content */}
      <div className="bg-white rounded-lg border shadow">
        <div className="p-6 border-b">
          <div className="flex items-start justify-between">
            <div className="space-y-2">
              <div className="flex items-center gap-2">
                {/* Category icon placeholder (legacy getCategoryIcon removed) */}
                <span className="text-sm text-muted-foreground">
                  {formatCategory(groundTruth.Category || "")}
                </span>
              </div>
              <h2>Ground Truth Details</h2>
            </div>
            <span
              className={`px-2 py-1 rounded text-sm bg-gray-100`}
            >
              {formatStatus(groundTruth.ValidationStatus)}
            </span>
          </div>
        </div>

        <div className="p-6 space-y-6">
          {/* User Query - Only editable by data curators */}
          <div className="space-y-2">
            <label>User Query</label>
            {isEditing && isDataCurator ? (
              <textarea
                value={(editForm as any).prompt || (editForm as any).UserQuery || groundTruth.UserQuery || ""}
                onChange={(e) =>
                  setEditForm((prev) => ({
                    ...prev,
                    prompt: e.target.value,
                  }))
                }
                rows={3}
                className="w-full p-3 border rounded-md resize-none"
              />
            ) : (
              <p className="bg-muted p-3 rounded-md">
                {groundTruth.UserQuery}
              </p>
            )}
          </div>

          {/* Category - Only editable by data curators */}
          <div className="space-y-2">
            <label>Category</label>
            {isEditing && isDataCurator ? (
              <div className="relative">
                <select
                  value={(editForm as any).category || groundTruth.Category || ""}
                  onChange={(e) =>
                    setEditForm((prev) => ({
                      ...prev,
                      category: e.target.value,
                    }))
                  }
                  className="w-full p-3 border rounded-md bg-white appearance-none cursor-pointer pr-10"
                >
                  <option value="asset_knowledge">Asset Knowledge</option>
                  <option value="unanswerable">Unanswerable</option>
                  <option value="maintenance_request">
                    Maintenance Request
                  </option>
                </select>
                <ChevronDown className="absolute right-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-muted-foreground pointer-events-none" />
              </div>
            ) : (
              <p className="bg-muted p-3 rounded-md">
                {groundTruth.Category || "N/A"}
              </p>
            )}
          </div>

          {/* Status - Editable by both data curators and data validators */}
          <div className="space-y-2">
            <label>Status</label>
            {isEditing ? (
              <div className="relative">
                <select
                  value={editForm.ValidationStatus || ""}
                  onChange={(e) =>
                    setEditForm((prev) => ({
                      ...prev,
                      status: e.target.value as ValidationStatus,
                    }))
                  }
                  className="w-full p-3 border rounded-md bg-white appearance-none cursor-pointer pr-10"
                >
                  <option value="new">New</option>
                  <option value="revisions_requested">
                    Revisions Requested
                  </option>
                  <option value="validated">Validated</option>
                  <option value="out-of-scope">Out of Scope</option>
                </select>
                <ChevronDown className="absolute right-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-muted-foreground pointer-events-none" />
              </div>
            ) : (
              <div>
                <span
                  className={`px-3 py-1 rounded bg-gray-100`}
                >
                  {formatStatus(groundTruth.ValidationStatus)}
                </span>
              </div>
            )}
          </div>

          {/* Tags - Editable by both data curators and data validators */}
          <div className="space-y-2">
            <label>Tags</label>
            {isEditing ? (
              <TagManager
                selectedTags={(editForm as any).tags || groundTruth.Tags?.map(t => ({ id: t.TagId, name: t.Name, description: t.Description })) || []}
                onTagsChange={(tags) =>
                  setEditForm((prev: any) => ({ ...prev, tags }))
                }
              />
            ) : (
              <div className="flex flex-wrap gap-2">
                {groundTruth.Tags?.length ? (
                  groundTruth.Tags.map((tag: Tag) => {
                    // This would need to be replaced with actual tag lookup
                    return (
                      <span
                        key={tag.TagId}
                        className="px-2 py-1 bg-blue-100 text-blue-800 rounded text-sm"
                      >
                        {tag.Name}
                      </span>
                    );
                  })
                ) : (
                  <p className="text-muted-foreground italic">No tags assigned</p>
                )}
              </div>
            )}
          </div>

          {/* Contexts - Only displayed/editable by data curators */}
          {/* Context editing temporarily disabled until migrated to DTO structure */}
          {false && (!isDataValidator || !isEditing) && (
            <div className="space-y-2">
              <label>Contexts</label>
              {isEditing && isDataCurator ? (
                <div className="space-y-4">
                  {(
                    (editForm as any).contexts || []
                  ).length === 0 ? (
                    <p className="text-muted-foreground text-center py-4">
                      No contexts. Click "Add New GroundTruthContext" below
                      to get started.
                    </p>
                  ) : (
                    (
                      (editForm as any).contexts || []
                    ).map((context: any, index: number) => (
                      <div
                        key={context.ContextId || index}
                        className="border rounded-md p-4"
                      >
                        <div className="flex items-center justify-between mb-3">
                          <span className="px-2 py-1 bg-blue-100 text-blue-800 rounded text-sm">
                            GroundTruthContext {index + 1}
                          </span>
                          <button
                            onClick={() => removeContext(index)}
                            className="flex items-center gap-2 text-sm text-red-600 hover:text-red-800"
                          >
                            <Trash2 className="w-4 h-4" />
                            Remove GroundTruthContext
                          </button>
                        </div>

                        <div className="space-y-3">
                          <div>
                            <div className="flex items-center justify-between mb-2">
                              <label className="text-sm">
                                Parameters
                              </label>
                              <button
                                onClick={() =>
                                  addParameterToContext(index)
                                }
                                className="flex items-center gap-1 text-sm text-primary hover:text-primary/80"
                              >
                                <Plus className="w-3 h-3" />
                                Add Parameter
                              </button>
                            </div>
                            <div className="space-y-2">
                              {context.ContextParameters?.length === 0 ? (
                                <p className="text-sm text-muted-foreground italic">
                                  No parameters. Click "Add
                                  Parameter" to add one.
                                </p>
                              ) : (
                                context.ContextParameters.map(
                                  (parameter: any, paramIndex: number) => (
                                    <div
                                      key={paramIndex}
                                      className="flex items-center gap-2 p-3 border rounded-md"
                                    >
                                      <div className="flex-1 grid grid-cols-3 gap-2">
                                        <div>
                                          <label className="text-xs text-muted-foreground">
                                            Name
                                          </label>
                                          <input
                                            value={parameter.ParameterName}
                                            onChange={(e) =>
                                              updateContextParameter(
                                                index,
                                                paramIndex,
                                                "name",
                                                e.target.value,
                                              )
                                            }
                                            placeholder="Parameter name"
                                            className="w-full p-2 border rounded-md text-sm"
                                          />
                                        </div>
                                        <div>
                                          <label className="text-xs text-muted-foreground">
                                            Value
                                          </label>
                                          <input
                                            value={parameter.ParameterValue}
                                            onChange={(e) =>
                                              updateContextParameter(
                                                index,
                                                paramIndex,
                                                "value",
                                                e.target.value,
                                              )
                                            }
                                            placeholder="Parameter value"
                                            className="w-full p-2 border rounded-md text-sm"
                                          />
                                        </div>
                                        <div>
                                          <label className="text-xs text-muted-foreground">
                                            Type
                                          </label>
                                          <div className="relative">
                                            <select
                                              value={parameter.DataType}
                                              onChange={(e) =>
                                                updateContextParameter(
                                                  index,
                                                  paramIndex,
                                                  "dataType",
                                                  e.target.value,
                                                )
                                              }
                                              className="w-full p-2 border rounded-md text-sm bg-white appearance-none cursor-pointer pr-8"
                                            >
                                              <option value="string">
                                                String
                                              </option>
                                              <option value="float">
                                                Float
                                              </option>
                                              <option value="decimal">
                                                Decimal
                                              </option>
                                              <option value="datetime">
                                                Datetime
                                              </option>
                                              <option value="integer">
                                                Integer
                                              </option>
                                              <option value="boolean">
                                                Boolean
                                              </option>
                                            </select>
                                            <ChevronDown className="absolute right-2 top-1/2 transform -translate-y-1/2 w-4 h-4 text-muted-foreground pointer-events-none" />
                                          </div>
                                        </div>
                                      </div>
                                      <button
                                        onClick={() =>
                                          removeParameterFromContext(
                                            index,
                                            paramIndex,
                                          )
                                        }
                                        className="flex items-center justify-center w-8 h-8 text-red-600 hover:text-red-800 hover:bg-red-50 rounded-md transition-colors"
                                        title="Remove parameter"
                                      >
                                        <Trash2 className="w-4 h-4" />
                                      </button>
                                    </div>
                                  ),
                                )
                              )}
                            </div>
                          </div>
                        </div>
                      </div>
                    ))
                  )}

                  <button
                    onClick={addNewContext}
                    className="flex items-center gap-2 px-4 py-2 border border-dashed border-gray-300 rounded-md text-sm text-muted-foreground hover:border-primary hover:text-primary transition-colors w-full justify-center"
                  >
                    <Plus className="w-4 h-4" />
                    Add New GroundTruthContext
                  </button>
                </div>
              ) : (
                <div className="space-y-4">
                  {/* contexts display removed */}
                  {(false) ? (
                    <p className="text-muted-foreground text-center py-4">
                      No contexts.
                    </p>
                  ) : (
                    null
                  )}
                </div>
              )}
            </div>
          )}

          {/* Data Query Definitions - Only displayed/editable by data curators */}
          {false && (!isDataValidator || !isEditing) && (
            <div className="space-y-2">
              <label>Data Query Definitions</label>
              {isEditing && isDataCurator ? (
                <div className="space-y-4">
                  {(
                    editForm.dataQueryDefinitions ||
                    (groundTruth as any).DataQueryDefinitions
                  ).length === 0 ? (
                    <p className="text-muted-foreground text-center py-4">
                      No data query definitions. Click "Add New
                      Query Definition" below to get started.
                    </p>
                  ) : (
                    (
                      editForm.dataQueryDefinitions ||
                      (groundTruth as any).DataQueryDefinitions
                    ).map((definition, index) => (
                      <div
                        key={(definition as any).DataQueryId || index}
                        className="border rounded-md p-4"
                      >
                        <div className="flex items-center justify-between mb-3">
                          <div className="flex items-center gap-2">
                            <span>
                              {(definition as any).Name ||
                                `Query ${index + 1}`}
                            </span>
                            <span className="px-2 py-1 bg-gray-100 text-gray-800 rounded text-sm">
                              {(definition as any).DatastoreType}
                            </span>
                          </div>
                          <button
                            onClick={() => {
                              const currentDefinitions =
                                editForm.dataQueryDefinitions ||
                                (groundTruth as any).DataQueryDefinitions;
                              const newDefinitions =
                                currentDefinitions.filter(
                                  (_, i) => i !== index,
                                );
                              setEditForm((prev) => ({
                                ...prev,
                                dataQueryDefinitions:
                                  newDefinitions,
                              }));
                            }}
                            className="flex items-center gap-2 text-sm text-red-600 hover:text-red-800"
                          >
                            <Trash2 className="w-4 h-4" />
                            Remove
                          </button>
                        </div>

                        <div className="space-y-3">
                          <div>
                            <label className="text-sm">
                              Name
                            </label>
                            <input
                              value={(definition as any).Name || ""}
                              onChange={(e) => {
                                const currentDefinitions =
                                  editForm.dataQueryDefinitions || [
                                    ...(groundTruth as any).DataQueryDefinitions,
                                  ];
                                const newDefinitions = [
                                  ...currentDefinitions,
                                ];
                                newDefinitions[index] = {
                                  ...newDefinitions[index],
                                  Name: e.target.value,
                                };
                                setEditForm((prev) => ({
                                  ...prev,
                                  dataQueryDefinitions:
                                    newDefinitions,
                                }));
                              }}
                              placeholder="Enter query name (optional)"
                              className="w-full p-2 border rounded-md text-sm"
                            />
                          </div>

                          <div>
                            <label className="text-sm">
                              Data Store Type
                            </label>
                            <div className="relative">
                              <select
                                value={(definition as any).DatastoreType}
                                onChange={(e) => {
                                  const currentDefinitions =
                                    editForm.dataQueryDefinitions || [
                                      ...(groundTruth as any).DataQueryDefinitions,
                                    ];
                                  const newDefinitions = [
                                    ...currentDefinitions,
                                  ];
                                  newDefinitions[index] = {
                                    ...newDefinitions[index],
                                    DatastoreType: e.target.value,
                                  };
                                  setEditForm((prev) => ({
                                    ...prev,
                                    dataQueryDefinitions:
                                      newDefinitions,
                                  }));
                                }}
                                className="w-full p-2 border rounded-md bg-white appearance-none cursor-pointer pr-8 text-sm"
                              >
                                <option value="GraphQL">
                                  GraphQL
                                </option>
                                <option value="SQL">SQL</option>
                                <option value="CosmosDB">
                                  CosmosDB
                                </option>
                              </select>
                              <ChevronDown className="absolute right-2 top-1/2 transform -translate-y-1/2 w-4 h-4 text-muted-foreground pointer-events-none" />
                            </div>
                          </div>

                          <div>
                            <label className="text-sm">
                              Query
                            </label>
                            <textarea
                              value={(definition as any).QueryDefinition || ""}
                              onChange={(e) => {
                                const currentDefinitions =
                                  editForm.dataQueryDefinitions || [
                                    ...(groundTruth as any).DataQueryDefinitions,
                                  ];
                                const newDefinitions = [
                                  ...currentDefinitions,
                                ];
                                newDefinitions[index] = {
                                  ...newDefinitions[index],
                                  QueryDefinition: e.target.value,
                                };
                                setEditForm((prev) => ({
                                  ...prev,
                                  dataQueryDefinitions:
                                    newDefinitions,
                                }));
                              }}
                              rows={4}
                              placeholder="Enter your query definition..."
                              className="w-full p-2 border rounded-md resize-none text-sm"
                            />
                          </div>
                        </div>
                      </div>
                    ))
                  )}

                  <button
                    onClick={() => {
                      // Adding new data query definition disabled during migration
                    }}
                    className="flex items-center gap-2 px-4 py-2 border border-dashed border-gray-300 rounded-md text-sm text-muted-foreground hover:border-primary hover:text-primary transition-colors w-full justify-center"
                  >
                    <Plus className="w-4 h-4" />
                    Add New Query Definition
                  </button>
                </div>
              ) : (
                <div className="space-y-4">
                  {(groundTruth as any).DataQueryDefinitions?.length === 0 ? (
                    <p className="text-muted-foreground text-center py-4">
                      No data query definitions.
                    </p>
                  ) : (
                    (groundTruth as any).DataQueryDefinitions.map(
                      (definition: any, index: number) => (
                        <div
                          key={definition.DataQueryId || index}
                          className="border rounded-md p-4"
                        >
                          <div className="space-y-2">
                            <div className="flex items-center gap-2 mb-3">
                              <span>
                                {definition.Name ||
                                  `Query ${index + 1}`}
                              </span>
                              <span className="px-2 py-1 bg-gray-100 text-gray-800 rounded text-sm">
                                {definition.DatastoreType}
                              </span>
                            </div>
                            <div>
                              <p className="text-sm text-muted-foreground mb-1">
                                Query:
                              </p>
                              <pre className="bg-muted p-3 rounded text-sm whitespace-pre-wrap">
                                {definition.QueryDefinition}
                              </pre>
                            </div>
                          </div>
                        </div>
                      ),
                    )
                  )}
                </div>
              )}
            </div>
          )}

          {/* Data Curator Notes - Only editable by data curators */}
          {false && <div className="space-y-2">
            <label>Data Curator Notes</label>
            {isEditing && isDataCurator ? (
              <textarea
                value={editForm.dataCuratorNotes || ""}
                onChange={(e) =>
                  setEditForm((prev) => ({
                    ...prev,
                    dataCuratorNotes: e.target.value,
                  }))
                }
                rows={3}
                placeholder="Add notes for internal tracking..."
                className="w-full p-3 border rounded-md resize-none"
              />
            ) : (
              <div className="bg-muted p-3 rounded-md" />
            )}
          </div>}

          {/* Generated Responses */}
          {/* Generated responses section removed during DTO alignment */}

          {/* Reviews */}
          {/* Reviews section removed during DTO alignment */}

          {/* Add Review Form - For both data validators and curators */}
          {/* Review form temporarily removed */}
        </div>
      </div>
    </div>
  );
};
