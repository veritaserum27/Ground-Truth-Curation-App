import { Download, FileText, Filter, X } from 'lucide-react';
import { useState } from 'react';
import type { GroundTruthDefinition, Tag, ValidationStatus } from '../services/schemas';
import { formatStatus } from '../utils/groundTruthFormatting';

interface ExportModalProps {
  isOpen: boolean;
  onClose: () => void;
  groundTruths: GroundTruthDefinition[];
  tags: Tag[];
  getTagsForGroundTruth: (gt: GroundTruthDefinition) => Tag[];
}

type ExportFormat = 'jsonl' | 'csv';

export const ExportModal = ({ isOpen, onClose, groundTruths, tags, getTagsForGroundTruth }: ExportModalProps) => {
  const [format, setFormat] = useState<ExportFormat>('jsonl');
  const [isExporting, setIsExporting] = useState(false);

  // Export filtering state
  const [statusFilters, setStatusFilters] = useState<ValidationStatus[]>([]);
  const [tagFilters, setTagFilters] = useState<string[]>([]);

  // Get filtered ground truths based on export filters
  const getExportFilteredGroundTruths = () => {
    return groundTruths.filter(gt => {
      const statusMatch = statusFilters.length === 0 || statusFilters.includes(gt.ValidationStatus);
      const tagMatch = tagFilters.length === 0 || tagFilters.some(tagId => gt.Tags.some(t => t.TagId === tagId));
      return statusMatch && tagMatch;
    });
  };

  const filteredGroundTruths = getExportFilteredGroundTruths();

  // Handle status filter toggle
  const toggleStatusFilter = (status: ValidationStatus) => {
    setStatusFilters(prev => prev.includes(status) ? prev.filter(s => s !== status) : [...prev, status]);
  };

  // Handle tag filter toggle
  const toggleTagFilter = (tagId: string) => {
    setTagFilters(prev => prev.includes(tagId) ? prev.filter(t => t !== tagId) : [...prev, tagId]);
  };

  // Clear all filters
  const clearFilters = () => {
    setStatusFilters([]);
    setTagFilters([]);
  };

  // Status formatting delegated to shared util

  // Build entry-level export objects (one JSONL line per GroundTruthEntry)
  const buildEntryExportObjects = (groundTruth: GroundTruthDefinition) => {
    const tagsForGt = getTagsForGroundTruth(groundTruth);
    return groundTruth.GroundTruthEntries.map(entry => {
      const ctx = entry.GroundTruthContext;
      const primaryDataQuery = groundTruth.DataQueryDefinitions[0];
      return {
        id: entry.GroundTruthEntryId, // entry-specific id
        groundTruthId: groundTruth.GroundTruthId,
        userQuery: groundTruth.UserQuery,
        response: entry.Response,
        requiredValues: entry.RequiredValues,
        context: ctx ? {
          id: ctx.ContextId,
          type: ctx.ContextType,
          parameters: ctx.ContextParameters.reduce((acc, param) => {
            acc[param.ParameterName] = param.ParameterValue;
            return acc;
          }, {} as Record<string, string>)
        } : null,
        rawData: entry.RawData.map(r => ({ dataQueryId: r.DataQueryId, rows: r.RawData })),
        // Include a snapshot of ALL data queries (definition-level) for reproducibility
        dataQueries: groundTruth.DataQueryDefinitions.map(dqd => ({
          id: dqd.DataQueryId || '',
          datastoreType: dqd.DatastoreType,
          datastoreName: dqd.DatastoreName,
          queryTarget: dqd.QueryTarget,
          queryDefinition: dqd.QueryDefinition,
          isFullQuery: dqd.IsFullQuery
        })),
        primaryDataQuery: primaryDataQuery ? {
          id: primaryDataQuery.DataQueryId || '',
          datastoreType: primaryDataQuery.DatastoreType,
          datastoreName: primaryDataQuery.DatastoreName,
          queryTarget: primaryDataQuery.QueryTarget,
          queryDefinition: primaryDataQuery.QueryDefinition,
          isFullQuery: primaryDataQuery.IsFullQuery
        } : null,
        metadata: {
          category: groundTruth.Category,
          validationStatus: groundTruth.ValidationStatus,
          tags: tagsForGt.map(t => ({ id: t.TagId, name: t.Name, description: t.Description })),
          createdAt: groundTruth.CreationDateTime,
          updatedAt: groundTruth.UserUpdated ?? groundTruth.CreationDateTime,
          entryCreatedAt: entry.CreationDateTime,
          createdBy: groundTruth.UserCreated,
          commentCount: groundTruth.Comments.length,
          totalEntriesForDefinition: groundTruth.GroundTruthEntries.length
        }
      };
    });
  };

  const exportAsJSONL = () => {
    // Flatten all entries across filtered definitions
    const exportData = filteredGroundTruths.flatMap(gt => buildEntryExportObjects(gt));
    const jsonlContent = exportData.map(item => JSON.stringify(item)).join('\n');

    const blob = new Blob([jsonlContent], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `ground-truth-export-${new Date().toISOString().split('T')[0]}.jsonl`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
  };

  const escapeCSV = (value: string) => {
    if (value.includes('"') || value.includes(',') || value.includes('\n') || value.includes('\r')) {
      return `"${value.replace(/"/g, '""')}"`;
    }
    return value;
  };

  const exportAsCSV = () => {
    // For CSV we currently retain one row per definition (could be extended per entry if needed)
    const exportData = filteredGroundTruths.map(gt => {
      // Reuse the first entry to approximate prior behavior
      const firstEntry = gt.GroundTruthEntries[0];
      const ctx = firstEntry?.GroundTruthContext || null;
      const primaryDataQuery = gt.DataQueryDefinitions[0];
      return {
        id: gt.GroundTruthId,
        userQuery: gt.UserQuery,
        context: ctx ? {
          id: ctx.ContextId,
          type: ctx.ContextType,
          parameters: ctx.ContextParameters.reduce((acc, param) => {
            acc[param.ParameterName] = param.ParameterValue;
            return acc;
          }, {} as Record<string, string>)
        } : null,
        rawData: JSON.stringify(firstEntry?.RawData.map(r => ({ dataQueryId: r.DataQueryId, rows: r.RawData })) || []),
        response: firstEntry?.Response || '',
        dataQuery: primaryDataQuery ? {
          id: primaryDataQuery.DataQueryId || '',
          datastoreType: primaryDataQuery.DatastoreType,
          datastoreName: primaryDataQuery.DatastoreName,
          queryTarget: primaryDataQuery.QueryTarget,
          queryDefinition: primaryDataQuery.QueryDefinition,
          isFullQuery: primaryDataQuery.IsFullQuery
        } : null,
        allDataQueries: gt.DataQueryDefinitions.map(dqd => ({
          id: dqd.DataQueryId || '',
          datastoreType: dqd.DatastoreType,
          datastoreName: dqd.DatastoreName,
          queryTarget: dqd.QueryTarget,
          queryDefinition: dqd.QueryDefinition,
          isFullQuery: dqd.IsFullQuery
        })),
        metadata: {
          category: gt.Category,
          validationStatus: gt.ValidationStatus,
          tags: gt.Tags.map(t => ({ id: t.TagId, name: t.Name, description: t.Description })),
          createdAt: gt.CreationDateTime,
          updatedAt: gt.UserUpdated ?? gt.CreationDateTime,
          createdBy: gt.UserCreated,
          commentCount: gt.Comments.length,
          entryCount: gt.GroundTruthEntries.length
        }
      };
    });

    // CSV Headers
    const headers = [
      'ID',
      'User Query',
      'Context ID',
      'Context Type',
      'Context Parameters',
      'Raw Data',
      'Response',
      'Primary Data Query ID',
      'Primary Datastore Type',
      'Primary Datastore Name',
      'Primary Query Target',
      'Primary Query Definition',
      'Primary Is Full Query',
      'All Data Queries',
      'Category',
      'Validation Status',
      'Tags',
      'Created At',
      'Updated At',
      'Created By',
      'Comment Count',
      'Entry Count'
    ];

    const csvRows = [
      headers.join(','),
      ...exportData.map(item => [
        escapeCSV(item.id),
        escapeCSV(item.userQuery),
        escapeCSV(item.context?.id || ''),
        escapeCSV(item.context?.type || ''),
        escapeCSV(JSON.stringify(item.context?.parameters || {})),
        escapeCSV(item.rawData),
        escapeCSV(item.response),
        escapeCSV(item.dataQuery?.id || ''),
        escapeCSV(item.dataQuery?.datastoreType || ''),
        escapeCSV(item.dataQuery?.datastoreName || ''),
        escapeCSV(item.dataQuery?.queryTarget || ''),
        escapeCSV(item.dataQuery?.queryDefinition || ''),
        escapeCSV(item.dataQuery?.isFullQuery?.toString() || ''),
        escapeCSV(JSON.stringify(item.allDataQueries)),
        escapeCSV(item.metadata.category || ''),
        escapeCSV(item.metadata.validationStatus),
        escapeCSV(JSON.stringify(item.metadata.tags)),
        escapeCSV(item.metadata.createdAt),
        escapeCSV(item.metadata.updatedAt),
        escapeCSV(item.metadata.createdBy),
        escapeCSV(item.metadata.commentCount.toString()),
        escapeCSV(item.metadata.entryCount.toString())
      ].join(','))
    ];

    const csvContent = csvRows.join('\n');

    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `ground-truth-export-${new Date().toISOString().split('T')[0]}.csv`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
  };

  const handleExport = async () => {
    setIsExporting(true);

    // Add a small delay for UX
    await new Promise(resolve => setTimeout(resolve, 500));

    if (format === 'jsonl') {
      exportAsJSONL();
    } else {
      exportAsCSV();
    }

    setIsExporting(false);
    onClose();
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg shadow-xl w-full max-w-lg mx-4 max-h-[90vh] overflow-y-auto">
        <div className="flex items-center justify-between p-6 border-b">
          <h2 className="flex items-center gap-2">
            <Download className="w-5 h-5" />
            Export Ground Truth Data
          </h2>
          <button
            onClick={onClose}
            className="text-muted-foreground hover:text-foreground"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        <div className="p-6 space-y-6">
          <div className="space-y-4">
            <div>
              <h3>Export Details</h3>
              <p className="text-muted-foreground text-sm">
                Exporting {filteredGroundTruths.length} ground truth entries
                {(statusFilters.length > 0 || tagFilters.length > 0) && (
                  <span className="text-xs"> (filtered from {groundTruths.length} total)</span>
                )}
              </p>
              {filteredGroundTruths.length === 0 && (statusFilters.length > 0 || tagFilters.length > 0) && (
                <p className="text-destructive text-sm mt-1">No entries match the selected filters.</p>
              )}
            </div>

            <div className="space-y-3">
              <label>Export Format</label>
              <div className="space-y-2">
                <label className="flex items-center gap-2 cursor-pointer">
                  <input
                    type="radio"
                    name="format"
                    value="jsonl"
                    checked={format === 'jsonl'}
                    onChange={(e) => setFormat(e.target.value as ExportFormat)}
                    className="w-4 h-4"
                  />
                  <div className="flex items-center gap-2">
                    <FileText className="w-4 h-4" />
                    <span>JSONL (JSON Lines)</span>
                  </div>
                </label>
                <p className="text-xs text-muted-foreground ml-6">
                  Each ground truth entry as a single JSON object per line
                </p>

                <label className="flex items-center gap-2 cursor-pointer">
                  <input
                    type="radio"
                    name="format"
                    value="csv"
                    checked={format === 'csv'}
                    onChange={(e) => setFormat(e.target.value as ExportFormat)}
                    className="w-4 h-4"
                  />
                  <div className="flex items-center gap-2">
                    <FileText className="w-4 h-4" />
                    <span>CSV (Comma-separated values)</span>
                  </div>
                </label>
                <p className="text-xs text-muted-foreground ml-6">
                  Tabular format compatible with spreadsheet applications
                </p>
              </div>
            </div>

            <div className="bg-muted/50 p-3 rounded text-sm">
              <h4>Export includes:</h4>
              <ul className="text-muted-foreground mt-1 space-y-1">
                <li>• User query (prompt)</li>
                <li>• Context parameters</li>
                <li>• Raw data and generated response</li>
                <li>• Data query definitions</li>
                <li>• Metadata (category, status, tags, timestamps)</li>
              </ul>
            </div>
          </div>

          <div className="space-y-3">
            <h3 className="font-medium">Filter Export</h3>

            <div className="space-y-2">
              <label className="flex items-center gap-2">
                <Filter className="w-4 h-4" />
                <span>Status Filters</span>
              </label>
              <div className="flex flex-wrap gap-1">
                {(['New, Data Curated', 'Request Revisions', 'Validated', 'Pending'] as ValidationStatus[]).map(status => (
                  <button
                    key={status}
                    onClick={() => toggleStatusFilter(status)}
                    className={`px-2 py-1 rounded text-xs ${statusFilters.includes(status)
                      ? 'bg-primary text-primary-foreground'
                      : 'bg-muted hover:bg-muted/50'
                      }`}
                  >
                    {formatStatus(status as any)}
                  </button>
                ))}
              </div>
            </div>

            <div className="space-y-2">
              <label className="flex items-center gap-2">
                <Filter className="w-4 h-4" />
                <span>Tag Filters</span>
              </label>
              <div className="flex flex-wrap gap-1">
                {tags.map(tag => (
                  <button
                    key={tag.TagId}
                    onClick={() => toggleTagFilter(tag.TagId)}
                    className={`px-2 py-1 rounded text-xs ${tagFilters.includes(tag.TagId)
                      ? 'bg-primary text-primary-foreground'
                      : 'bg-muted hover:bg-muted/50'
                      }`}
                  >
                    {tag.Name}
                  </button>
                ))}
              </div>
            </div>

            <button
              onClick={clearFilters}
              className="px-2 py-1 text-xs text-muted-foreground hover:bg-muted rounded"
            >
              Clear All Filters
            </button>
          </div>
        </div>

        <div className="flex justify-end gap-3 p-6 border-t bg-muted/25">
          <button
            onClick={onClose}
            disabled={isExporting}
            className="px-4 py-2 text-sm hover:bg-muted rounded-md"
          >
            Cancel
          </button>
          <button
            onClick={handleExport}
            disabled={isExporting || filteredGroundTruths.length === 0}
            className="flex items-center gap-2 px-4 py-2 text-sm bg-primary text-primary-foreground hover:bg-primary/90 rounded-md disabled:opacity-50"
          >
            {isExporting ? (
              <>
                <div className="w-4 h-4 border-2 border-current border-t-transparent animate-spin rounded-full" />
                Exporting...
              </>
            ) : (
              <>
                <Download className="w-4 h-4" />
                Export {format.toUpperCase()}
              </>
            )}
          </button>
        </div>
      </div>
    </div>
  );
};
