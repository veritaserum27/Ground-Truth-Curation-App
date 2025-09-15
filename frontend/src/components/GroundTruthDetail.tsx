import { useState } from "react";
import {
  ArrowLeft,
  Edit,
  Save,
  X,
  Star,
  FileText,
  MessageSquare,
  Clock,
  ChevronDown,
  Plus,
  Trash2,
} from "lucide-react";
import { useAuth } from "../contexts/AuthContext";
import { useData } from "../contexts/DataContext";
import { ReviewForm } from "./ReviewForm";
import { TagManager } from "./TagManager";
import {
  GroundTruthCategory,
  GroundTruthStatus,
  Context,
  DataQueryDefinition,
  DataStoreType,
} from "../types";

// Helper functions
const formatCategory = (category: GroundTruthCategory) =>
  category
    .replace(/_/g, " ")
    .replace(/\b\w/g, (l) => l.toUpperCase());

const formatStatus = (status: GroundTruthStatus) =>
  status
    .replace(/_/g, " ")
    .replace(/\b\w/g, (l) => l.toUpperCase());

const getStatusColor = (status: GroundTruthStatus) => {
  switch (status) {
    case "new":
      return "bg-gray-100 text-gray-800";
    case "revisions_requested":
      return "bg-red-100 text-red-800";
    case "validated":
      return "bg-green-100 text-green-800";
    case "out-of-scope":
      return "bg-yellow-100 text-yellow-800";
  }
};

const getCategoryIcon = (category: GroundTruthCategory) => {
  switch (category) {
    case "asset_knowledge":
      return <FileText className="w-4 h-4" />;
    case "unanswerable":
      return <MessageSquare className="w-4 h-4" />;
    case "maintenance_request":
      return <MessageSquare className="w-4 h-4" />;
  }
};

export const GroundTruthDetail = () => {
  const { user } = useAuth();
  const {
    getSelectedGroundTruth,
    setSelectedGroundTruth,
    isEditing,
    setIsEditing,
    editForm,
    setEditForm,
    updateGroundTruth,
    deepCopyGroundTruth,
  } = useData();

  const [activeResponseTab, setActiveResponseTab] = useState<{
    [key: string]: "formatted" | "raw" | "metadata";
  }>({});
  const [expandedReviews, setExpandedReviews] = useState<{
    [key: string]: boolean;
  }>({});
  const [expandedCuratorNotes, setExpandedCuratorNotes] =
    useState<{ [key: string]: boolean }>({});

  const selectedGroundTruth = getSelectedGroundTruth();

  if (!selectedGroundTruth) {
    return (
      <div className="text-center py-8">
        <p className="text-muted-foreground">
          Ground truth not found.
        </p>
        <button
          onClick={() => setSelectedGroundTruth(null)}
          className="mt-4 px-4 py-2 bg-primary text-primary-foreground rounded-md"
        >
          Back to List
        </button>
      </div>
    );
  }

  const handleSave = () => {
    updateGroundTruth(selectedGroundTruth.id, editForm);
    setIsEditing(false);
    setEditForm({});
  };

  const handleCancel = () => {
    setIsEditing(false);
    setEditForm({});
  };

  const handleEdit = () => {
    setEditForm(deepCopyGroundTruth(selectedGroundTruth));
    setIsEditing(true);
  };

  // Check if user can edit (curators can edit everything, validators can only edit status and tags)
  const canEdit = user?.role === "data_curator" || user?.role === "data_validator";
  const isDataValidator = user?.role === "data_validator";
  const isDataCurator = user?.role === "data_curator";

  // Context editing helpers (only for data curators)
  const updateContexts = (newContexts: Context[]) => {
    setEditForm((prev) => ({ ...prev, contexts: newContexts }));
  };

  const addNewContext = () => {
    const currentContexts = [
      ...(editForm.contexts || selectedGroundTruth.contexts),
    ];
    const newContext: Context = {
      id: Date.now().toString(),
      parameters: [],
    };
    updateContexts([...currentContexts, newContext]);
  };

  const removeContext = (contextIndex: number) => {
    const currentContexts = [
      ...(editForm.contexts || selectedGroundTruth.contexts),
    ];
    currentContexts.splice(contextIndex, 1);
    updateContexts(currentContexts);
  };

  const updateContextParameter = (
    contextIndex: number,
    paramIndex: number,
    field: "name" | "value" | "dataType",
    newValue: string,
  ) => {
    const currentContexts = [
      ...(editForm.contexts || selectedGroundTruth.contexts),
    ];
    const newParameters = [
      ...currentContexts[contextIndex].parameters,
    ];
    newParameters[paramIndex] = {
      ...newParameters[paramIndex],
      [field]: newValue,
    };

    currentContexts[contextIndex] = {
      ...currentContexts[contextIndex],
      parameters: newParameters,
    };
    updateContexts(currentContexts);
  };

  const addParameterToContext = (contextIndex: number) => {
    const currentContexts = [
      ...(editForm.contexts || selectedGroundTruth.contexts),
    ];
    const newParameter = {
      name: "newParam",
      value: "",
      dataType: "string" as const,
    };
    currentContexts[contextIndex] = {
      ...currentContexts[contextIndex],
      parameters: [
        ...currentContexts[contextIndex].parameters,
        newParameter,
      ],
    };
    updateContexts(currentContexts);
  };

  const removeParameterFromContext = (
    contextIndex: number,
    paramIndex: number,
  ) => {
    const currentContexts = [
      ...(editForm.contexts || selectedGroundTruth.contexts),
    ];
    const newParameters = [
      ...currentContexts[contextIndex].parameters,
    ];
    newParameters.splice(paramIndex, 1);
    currentContexts[contextIndex] = {
      ...currentContexts[contextIndex],
      parameters: newParameters,
    };
    updateContexts(currentContexts);
  };

  return (
    <div className="space-y-6">
      {/* Detail Header */}
      <div className="flex items-center justify-between">
        <button
          onClick={() => setSelectedGroundTruth(null)}
          className="flex items-center gap-2 px-3 py-2 hover:bg-muted rounded-md"
        >
          <ArrowLeft className="w-4 h-4" />
          Back to List
        </button>

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
                {getCategoryIcon(selectedGroundTruth.category)}
                <span className="text-sm text-muted-foreground">
                  {formatCategory(selectedGroundTruth.category)}
                </span>
              </div>
              <h2>Ground Truth Details</h2>
            </div>
            <span
              className={`px-2 py-1 rounded text-sm ${getStatusColor(selectedGroundTruth.status)}`}
            >
              {formatStatus(selectedGroundTruth.status)}
            </span>
          </div>
        </div>

        <div className="p-6 space-y-6">
          {/* User Query - Only editable by data curators */}
          <div className="space-y-2">
            <label>User Query</label>
            {isEditing && isDataCurator ? (
              <textarea
                value={editForm.prompt || ""}
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
                {selectedGroundTruth.prompt}
              </p>
            )}
          </div>

          {/* Category - Only editable by data curators */}
          <div className="space-y-2">
            <label>Category</label>
            {isEditing && isDataCurator ? (
              <div className="relative">
                <select
                  value={editForm.category || ""}
                  onChange={(e) =>
                    setEditForm((prev) => ({
                      ...prev,
                      category: e.target.value as GroundTruthCategory,
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
                {formatCategory(selectedGroundTruth.category)}
              </p>
            )}
          </div>

          {/* Status - Editable by both data curators and data validators */}
          <div className="space-y-2">
            <label>Status</label>
            {isEditing ? (
              <div className="relative">
                <select
                  value={editForm.status || ""}
                  onChange={(e) =>
                    setEditForm((prev) => ({
                      ...prev,
                      status: e.target.value as GroundTruthStatus,
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
                  className={`px-3 py-1 rounded ${getStatusColor(selectedGroundTruth.status)}`}
                >
                  {formatStatus(selectedGroundTruth.status)}
                </span>
              </div>
            )}
          </div>

          {/* Tags - Editable by both data curators and data validators */}
          <div className="space-y-2">
            <label>Tags</label>
            {isEditing ? (
              <TagManager
                selectedTags={editForm.tags || selectedGroundTruth.tags}
                onTagsChange={(tags) =>
                  setEditForm((prev) => ({ ...prev, tags }))
                }
              />
            ) : (
              <div className="flex flex-wrap gap-2">
                {selectedGroundTruth.tags?.length > 0 ? (
                  selectedGroundTruth.tags.map((tagId) => {
                    // This would need to be replaced with actual tag lookup
                    return (
                      <span
                        key={tagId}
                        className="px-2 py-1 bg-blue-100 text-blue-800 rounded text-sm"
                      >
                        {tagId}
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
          {(!isDataValidator || !isEditing) && (
            <div className="space-y-2">
              <label>Contexts</label>
              {isEditing && isDataCurator ? (
                <div className="space-y-4">
                  {(
                    editForm.contexts ||
                    selectedGroundTruth.contexts
                  ).length === 0 ? (
                    <p className="text-muted-foreground text-center py-4">
                      No contexts. Click "Add New Context" below
                      to get started.
                    </p>
                  ) : (
                    (
                      editForm.contexts ||
                      selectedGroundTruth.contexts
                    ).map((context, index) => (
                      <div
                        key={context.id}
                        className="border rounded-md p-4"
                      >
                        <div className="flex items-center justify-between mb-3">
                          <span className="px-2 py-1 bg-blue-100 text-blue-800 rounded text-sm">
                            Context {index + 1}
                          </span>
                          <button
                            onClick={() => removeContext(index)}
                            className="flex items-center gap-2 text-sm text-red-600 hover:text-red-800"
                          >
                            <Trash2 className="w-4 h-4" />
                            Remove Context
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
                              {context.parameters.length === 0 ? (
                                <p className="text-sm text-muted-foreground italic">
                                  No parameters. Click "Add
                                  Parameter" to add one.
                                </p>
                              ) : (
                                context.parameters.map(
                                  (parameter, paramIndex) => (
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
                                            value={parameter.name}
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
                                            value={
                                              parameter.value
                                            }
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
                                              value={
                                                parameter.dataType
                                              }
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
                    Add New Context
                  </button>
                </div>
              ) : (
                <div className="space-y-4">
                  {selectedGroundTruth.contexts.length === 0 ? (
                    <p className="text-muted-foreground text-center py-4">
                      No contexts.
                    </p>
                  ) : (
                    selectedGroundTruth.contexts.map(
                      (context, index) => (
                        <div
                          key={context.id}
                          className="border rounded-md p-4"
                        >
                          <div className="space-y-2">
                            <div className="flex items-center gap-2 mb-3">
                              <span className="px-2 py-1 bg-blue-100 text-blue-800 rounded text-sm">
                                Context {index + 1}
                              </span>
                            </div>
                            <div className="space-y-2">
                              {context.parameters.length === 0 ? (
                                <p className="text-sm text-muted-foreground italic">
                                  No parameters defined.
                                </p>
                              ) : (
                                context.parameters.map(
                                  (parameter, paramIndex) => (
                                    <div
                                      key={paramIndex}
                                      className="flex items-center gap-2 text-sm"
                                    >
                                      <span className="text-muted-foreground min-w-20">
                                        {parameter.name}:
                                      </span>
                                      <span className="px-2 py-1 bg-muted rounded text-sm">
                                        {parameter.value}
                                      </span>
                                      <span className="px-1 py-0.5 bg-gray-100 text-gray-600 rounded text-xs">
                                        {parameter.dataType}
                                      </span>
                                    </div>
                                  ),
                                )
                              )}
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

          {/* Data Query Definitions - Only displayed/editable by data curators */}
          {(!isDataValidator || !isEditing) && (
            <div className="space-y-2">
              <label>Data Query Definitions</label>
              {isEditing && isDataCurator ? (
                <div className="space-y-4">
                  {(
                    editForm.dataQueryDefinitions ||
                    selectedGroundTruth.dataQueryDefinitions
                  ).length === 0 ? (
                    <p className="text-muted-foreground text-center py-4">
                      No data query definitions. Click "Add New
                      Query Definition" below to get started.
                    </p>
                  ) : (
                    (
                      editForm.dataQueryDefinitions ||
                      selectedGroundTruth.dataQueryDefinitions
                    ).map((definition, index) => (
                      <div
                        key={definition.id}
                        className="border rounded-md p-4"
                      >
                        <div className="flex items-center justify-between mb-3">
                          <div className="flex items-center gap-2">
                            <span>
                              {definition.name ||
                                `Query ${index + 1}`}
                            </span>
                            <span className="px-2 py-1 bg-gray-100 text-gray-800 rounded text-sm">
                              {definition.dataStoreType}
                            </span>
                          </div>
                          <button
                            onClick={() => {
                              const currentDefinitions =
                                editForm.dataQueryDefinitions ||
                                selectedGroundTruth.dataQueryDefinitions;
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
                              value={definition.name || ""}
                              onChange={(e) => {
                                const currentDefinitions =
                                  editForm.dataQueryDefinitions || [
                                    ...selectedGroundTruth.dataQueryDefinitions,
                                  ];
                                const newDefinitions = [
                                  ...currentDefinitions,
                                ];
                                newDefinitions[index] = {
                                  ...newDefinitions[index],
                                  name: e.target.value,
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
                                value={definition.dataStoreType}
                                onChange={(e) => {
                                  const currentDefinitions =
                                    editForm.dataQueryDefinitions || [
                                      ...selectedGroundTruth.dataQueryDefinitions,
                                    ];
                                  const newDefinitions = [
                                    ...currentDefinitions,
                                  ];
                                  newDefinitions[index] = {
                                    ...newDefinitions[index],
                                    dataStoreType: e.target
                                      .value as DataStoreType,
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
                              value={definition.query}
                              onChange={(e) => {
                                const currentDefinitions =
                                  editForm.dataQueryDefinitions || [
                                    ...selectedGroundTruth.dataQueryDefinitions,
                                  ];
                                const newDefinitions = [
                                  ...currentDefinitions,
                                ];
                                newDefinitions[index] = {
                                  ...newDefinitions[index],
                                  query: e.target.value,
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
                      const currentDefinitions =
                        editForm.dataQueryDefinitions || [
                          ...selectedGroundTruth.dataQueryDefinitions,
                        ];
                      const newDefinition: DataQueryDefinition = {
                        id: Date.now().toString(),
                        dataStoreType: "GraphQL",
                        query: "",
                        name: "",
                        contextId: selectedGroundTruth.contexts[0]?.id || "",
                      };
                      setEditForm((prev) => ({
                        ...prev,
                        dataQueryDefinitions: [
                          ...currentDefinitions,
                          newDefinition,
                        ],
                      }));
                    }}
                    className="flex items-center gap-2 px-4 py-2 border border-dashed border-gray-300 rounded-md text-sm text-muted-foreground hover:border-primary hover:text-primary transition-colors w-full justify-center"
                  >
                    <Plus className="w-4 h-4" />
                    Add New Query Definition
                  </button>
                </div>
              ) : (
                <div className="space-y-4">
                  {selectedGroundTruth.dataQueryDefinitions.length === 0 ? (
                    <p className="text-muted-foreground text-center py-4">
                      No data query definitions.
                    </p>
                  ) : (
                    selectedGroundTruth.dataQueryDefinitions.map(
                      (definition, index) => (
                        <div
                          key={definition.id}
                          className="border rounded-md p-4"
                        >
                          <div className="space-y-2">
                            <div className="flex items-center gap-2 mb-3">
                              <span>
                                {definition.name ||
                                  `Query ${index + 1}`}
                              </span>
                              <span className="px-2 py-1 bg-gray-100 text-gray-800 rounded text-sm">
                                {definition.dataStoreType}
                              </span>
                            </div>
                            <div>
                              <p className="text-sm text-muted-foreground mb-1">
                                Query:
                              </p>
                              <pre className="bg-muted p-3 rounded text-sm whitespace-pre-wrap">
                                {definition.query}
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
          <div className="space-y-2">
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
              <div className="bg-muted p-3 rounded-md">
                {selectedGroundTruth.dataCuratorNotes || (
                  <span className="text-muted-foreground italic">
                    No curator notes
                  </span>
                )}
              </div>
            )}
          </div>

          {/* Generated Responses */}
          <div className="space-y-4">
            <h3>Generated Responses</h3>
            {selectedGroundTruth.generatedResponses?.length > 0 ? (
              selectedGroundTruth.generatedResponses.map((response, index) => (
                <div key={response.id} className="border rounded-lg p-4">
                  <div className="flex items-center justify-between mb-3">
                    <div className="flex items-center gap-2">
                      <span>Response {index + 1}</span>
                      <span className="px-2 py-1 bg-purple-100 text-purple-800 rounded text-sm">
                        {response.model}
                      </span>
                      <span className="flex items-center gap-1 text-sm text-muted-foreground">
                        <Star className="w-4 h-4" />
                        {response.confidence.toFixed(2)}
                      </span>
                    </div>
                    <span className="flex items-center gap-1 text-sm text-muted-foreground">
                      <Clock className="w-4 h-4" />
                      {response.timestamp.toLocaleDateString()}
                    </span>
                  </div>

                  <div className="space-y-3">
                    <div className="flex gap-2 border-b">
                      {(["formatted", "raw", "metadata"] as const).map((tab) => (
                        <button
                          key={tab}
                          onClick={() =>
                            setActiveResponseTab((prev) => ({
                              ...prev,
                              [response.id]: tab,
                            }))
                          }
                          className={`px-3 py-2 text-sm border-b-2 ${
                            (activeResponseTab[response.id] || "formatted") === tab
                              ? "border-primary text-primary"
                              : "border-transparent text-muted-foreground hover:text-foreground"
                          }`}
                        >
                          {tab.charAt(0).toUpperCase() + tab.slice(1)}
                        </button>
                      ))}
                    </div>

                    <div>
                      {(activeResponseTab[response.id] || "formatted") === "formatted" && (
                        <div className="bg-muted p-3 rounded">
                          {response.content}
                        </div>
                      )}
                      {activeResponseTab[response.id] === "raw" && (
                        <pre className="bg-muted p-3 rounded text-sm whitespace-pre-wrap">
                          {response.rawData}
                        </pre>
                      )}
                      {activeResponseTab[response.id] === "metadata" && (
                        <div className="bg-muted p-3 rounded space-y-2">
                          <div className="flex justify-between">
                            <span className="text-muted-foreground">Model:</span>
                            <span>{response.model}</span>
                          </div>
                          <div className="flex justify-between">
                            <span className="text-muted-foreground">Confidence:</span>
                            <span>{response.confidence.toFixed(2)}</span>
                          </div>
                          <div className="flex justify-between">
                            <span className="text-muted-foreground">Timestamp:</span>
                            <span>{response.timestamp.toLocaleString()}</span>
                          </div>
                        </div>
                      )}
                    </div>
                  </div>
                </div>
              ))
            ) : (
              <p className="text-muted-foreground text-center py-4">
                No generated responses yet.
              </p>
            )}
          </div>

          {/* Reviews */}
          <div className="space-y-4">
            <h3>Reviews</h3>
            {selectedGroundTruth.reviews?.length > 0 ? (
              selectedGroundTruth.reviews.map((review) => (
                <div key={review.id} className="border rounded-lg p-4">
                  <div className="flex items-center justify-between mb-3">
                    <div className="flex items-center gap-2">
                      <span>{review.reviewerName}</span>
                      <div className="flex items-center gap-1">
                        {[...Array(5)].map((_, i) => (
                          <Star
                            key={i}
                            className={`w-4 h-4 ${
                              i < review.rating
                                ? "text-yellow-500 fill-current"
                                : "text-gray-300"
                            }`}
                          />
                        ))}
                      </div>
                    </div>
                    <div className="flex items-center gap-2">
                      <span className="text-sm text-muted-foreground">
                        {review.timestamp.toLocaleDateString()}
                      </span>
                      <button
                        onClick={() =>
                          setExpandedReviews((prev) => ({
                            ...prev,
                            [review.id]: !prev[review.id],
                          }))
                        }
                        className="text-sm text-primary hover:text-primary/80"
                      >
                        {expandedReviews[review.id] ? "Collapse" : "Expand"}
                      </button>
                    </div>
                  </div>

                  <div className="space-y-2">
                    <p>{review.content}</p>
                    
                    {expandedReviews[review.id] && (
                      <div className="mt-4 pt-4 border-t space-y-2">
                        <h4 className="text-sm">Review Context</h4>
                        <div className="space-y-2 text-sm text-muted-foreground">
                          <div>
                            <span>User Query at time of review:</span>
                            <p className="bg-muted p-2 rounded mt-1">
                              {review.userQueryAtTime}
                            </p>
                          </div>
                          <div>
                            <span>Data Store Type:</span>
                            <span className="ml-2 px-2 py-1 bg-gray-100 text-gray-800 rounded text-xs">
                              {review.dataStoreTypeAtTime}
                            </span>
                          </div>
                          <div>
                            <span>Data Query Definition at time of review:</span>
                            <pre className="bg-muted p-2 rounded mt-1 text-xs whitespace-pre-wrap">
                              {review.dataQueryDefinitionAtTime}
                            </pre>
                          </div>
                          <div>
                            <span>Formatted Response at time of review:</span>
                            <p className="bg-muted p-2 rounded mt-1">
                              {review.formattedResponseAtTime}
                            </p>
                          </div>
                        </div>
                      </div>
                    )}
                  </div>
                </div>
              ))
            ) : (
              <p className="text-muted-foreground text-center py-4">
                No reviews yet.
              </p>
            )}
          </div>

          {/* Add Review Form - For both data validators and curators */}
          <div>
            <ReviewForm groundTruthId={selectedGroundTruth.id} />
          </div>
        </div>
      </div>
    </div>
  );
};