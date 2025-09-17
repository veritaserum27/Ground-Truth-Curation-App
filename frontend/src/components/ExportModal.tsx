import { Download, FileText, Filter, X } from 'lucide-react';
import { useState } from 'react';
import type { GroundTruth, GroundTruthStatus, Tag } from '../types';
import { formatStatus } from '../utils/groundTruthFormatting';

interface ExportModalProps {
  isOpen: boolean;
  onClose: () => void;
  groundTruths: GroundTruth[];
  tags: Tag[];
  getTagsForGroundTruth: (gt: GroundTruth) => Tag[];
}

type ExportFormat = 'jsonl' | 'csv';

export const ExportModal = ({ isOpen, onClose, groundTruths, tags, getTagsForGroundTruth }: ExportModalProps) => {
  const [format, setFormat] = useState<ExportFormat>('jsonl');
  const [isExporting, setIsExporting] = useState(false);

  // Export filtering state
  const [statusFilters, setStatusFilters] = useState<GroundTruthStatus[]>([]);
  const [tagFilters, setTagFilters] = useState<string[]>([]);

  // Get filtered ground truths based on export filters
  const getExportFilteredGroundTruths = () => {
    return groundTruths.filter(gt => {
      const statusMatch = statusFilters.length === 0 || statusFilters.includes(gt.status);
      const tagMatch = tagFilters.length === 0 || tagFilters.some(tagId => gt.tags.includes(tagId));
      return statusMatch && tagMatch;
    });
  };

  const filteredGroundTruths = getExportFilteredGroundTruths();

  // Handle status filter toggle
  const toggleStatusFilter = (status: GroundTruthStatus) => {
    setStatusFilters(prev =>
      prev.includes(status)
        ? prev.filter(s => s !== status)
        : [...prev, status]
    );
  };

  // Handle tag filter toggle
  const toggleTagFilter = (tagId: string) => {
    setTagFilters(prev =>
      prev.includes(tagId)
        ? prev.filter(t => t !== tagId)
        : [...prev, tagId]
    );
  };

  // Clear all filters
  const clearFilters = () => {
    setStatusFilters([]);
    setTagFilters([]);
  };

  // Status formatting delegated to shared util

  const formatExportData = (groundTruth: GroundTruth) => {
    const tags = getTagsForGroundTruth(groundTruth);

    // Get primary context and data query (first one if multiple exist)
    const primaryContext = groundTruth.contexts[0];
    const primaryDataQuery = groundTruth.dataQueryDefinitions[0];
    const primaryResponse = groundTruth.generatedResponses[0];

    return {
      id: groundTruth.id,
      userQuery: groundTruth.prompt,
      context: primaryContext ? {
        id: primaryContext.id,
        parameters: primaryContext.parameters
      } : null,
      rawData: primaryResponse?.rawData || '',
      response: primaryResponse?.content || '',
      dataQuery: primaryDataQuery ? {
        id: primaryDataQuery.id,
        name: primaryDataQuery.name || '',
        dataStoreType: primaryDataQuery.dataStoreType,
        query: primaryDataQuery.query
      } : null,
      allDataQueries: groundTruth.dataQueryDefinitions.map(dqd => ({
        id: dqd.id,
        name: dqd.name || '',
        dataStoreType: dqd.dataStoreType,
        query: dqd.query,
        contextId: dqd.contextId
      })),
      metadata: {
        category: groundTruth.category,
        status: groundTruth.status,
        tags: tags.map(tag => ({
          name: tag.name,
          isPredefined: tag.isPredefined
        })),
        createdAt: groundTruth.createdAt.toISOString(),
        updatedAt: groundTruth.updatedAt.toISOString(),
        createdBy: groundTruth.createdBy,
        dataCuratorNotes: groundTruth.dataCuratorNotes,
        reviewCount: groundTruth.reviews.length,
        responseCount: groundTruth.generatedResponses.length,
        confidence: primaryResponse?.confidence || 0,
        model: primaryResponse?.model || ''
      }
    };
  };

  const exportAsJSONL = () => {
    const exportData = filteredGroundTruths.map(gt => formatExportData(gt));
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
    const exportData = filteredGroundTruths.map(gt => formatExportData(gt));

    // CSV Headers
    const headers = [
      'ID',
      'User Query',
      'Context ID',
      'Context Parameters',
      'Raw Data',
      'Response',
      'Primary Data Query ID',
      'Primary Data Query Name',
      'Primary Data Store Type',
      'Primary Data Query',
      'All Data Queries',
      'Category',
      'Status',
      'Tags',
      'Created At',
      'Updated At',
      'Created By',
      'Data Curator Notes',
      'Review Count',
      'Response Count',
      'Confidence',
      'Model'
    ];

    const csvRows = [
      headers.join(','),
      ...exportData.map(item => [
        escapeCSV(item.id),
        escapeCSV(item.userQuery),
        escapeCSV(item.context?.id || ''),
        escapeCSV(JSON.stringify(item.context?.parameters || {})),
        escapeCSV(item.rawData),
        escapeCSV(item.response),
        escapeCSV(item.dataQuery?.id || ''),
        escapeCSV(item.dataQuery?.name || ''),
        escapeCSV(item.dataQuery?.dataStoreType || ''),
        escapeCSV(item.dataQuery?.query || ''),
        escapeCSV(JSON.stringify(item.allDataQueries)),
        escapeCSV(item.metadata.category),
        escapeCSV(item.metadata.status),
        escapeCSV(JSON.stringify(item.metadata.tags)),
        escapeCSV(item.metadata.createdAt),
        escapeCSV(item.metadata.updatedAt),
        escapeCSV(item.metadata.createdBy),
        escapeCSV(item.metadata.dataCuratorNotes),
        escapeCSV(item.metadata.reviewCount.toString()),
        escapeCSV(item.metadata.responseCount.toString()),
        escapeCSV(item.metadata.confidence.toString()),
        escapeCSV(item.metadata.model)
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
                {['new', 'revisions_requested', 'validated', 'out-of-scope'].map(status => (
                  <button
                    key={status}
                    onClick={() => toggleStatusFilter(status as GroundTruthStatus)}
                    className={`px-2 py-1 rounded text-xs ${statusFilters.includes(status as GroundTruthStatus)
                      ? 'bg-primary text-primary-foreground'
                      : 'bg-muted hover:bg-muted/50'
                      }`}
                  >
                    {formatStatus(status as GroundTruthStatus)}
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
                    key={tag.id}
                    onClick={() => toggleTagFilter(tag.id)}
                    className={`px-2 py-1 rounded text-xs ${tagFilters.includes(tag.id)
                      ? 'bg-primary text-primary-foreground'
                      : 'bg-muted hover:bg-muted/50'
                      }`}
                  >
                    {tag.name}
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
