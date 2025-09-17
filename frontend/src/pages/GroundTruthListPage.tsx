
import { ChevronDown, Download, Eye, Tag as TagIcon } from 'lucide-react';
import { useMemo, useState } from 'react';
import { NavLink, useLoaderData } from "react-router";
import { ExportModal } from '~/components/ExportModal';
import { CategoryIcon } from '~/components/ui/categoryIcon';
import { useAuth } from '~/contexts/AuthContext';
import { useEditingData } from '~/contexts/EditingContext';
import type { GroundTruth, GroundTruthCategory, GroundTruthStatus } from '~/types';
import { filterGroundTruths } from '~/utils/groundTruthFilters';
import { formatCategory, formatStatus, getStatusColorClass } from "~/utils/groundTruthFormatting";
// Re-export external loader so React Router picks it up
export { groundTruthsListLoader as clientLoader } from "../routes/loaders/groundTruthsListLoader";

// Filtering to be moved to utility in later task (placeholder for parity)

export default function GroundTruthListPage() {
  const { groundTruths } = useLoaderData() as { groundTruths: GroundTruth[] };
  const { user } = useAuth();
  const { tags } = useEditingData(); // retain tags from context until tag loader added

  // Local filter state (Phase 2 Task 2.1 step 1-3)
  const [categoryFilter, setCategoryFilter] = useState<GroundTruthCategory | 'all'>('all');
  const [statusFilter, setStatusFilter] = useState<GroundTruthStatus | 'all'>('all');
  const [tagFilter, setTagFilter] = useState<string | 'all'>('all');

  // Tag resolution helper (avoid dependency on context groundTruths filtering)
  const getTagsForGroundTruth = (gt: any) => gt.tags
    ? gt.tags.map((tid: string) => tags.find(t => t.id === tid)).filter(Boolean)
    : [];

  const filteredGroundTruths = useMemo(() => filterGroundTruths(groundTruths as any, {
    category: categoryFilter === 'all' ? undefined : categoryFilter,
    status: statusFilter === 'all' ? undefined : statusFilter,
    tagIds: tagFilter === 'all' ? undefined : [tagFilter],
  }), [groundTruths, categoryFilter, statusFilter, tagFilter]);

  const [isExportModalOpen, setIsExportModalOpen] = useState(false);
  return (
    <div className="space-y-6">
      <div className="flex gap-4 items-center">
        <div className="flex-1">
          <h2>Ground Truth Curation</h2>
          <p className="text-muted-foreground">
            {filteredGroundTruths.length} of {groundTruths.length} items
          </p>
        </div>

        <div className="flex gap-2">
          <div className="relative">
            <select
              value={categoryFilter}
              onChange={(e) => setCategoryFilter(e.target.value as GroundTruthCategory | 'all')}
              className="w-48 p-3 border rounded-md bg-white appearance-none cursor-pointer pr-10"
            >
              <option value="all">All Categories</option>
              <option value="asset_knowledge">Asset Knowledge</option>
              <option value="unanswerable">Unanswerable</option>
              <option value="maintenance_request">Maintenance Request</option>
            </select>
            <ChevronDown className="absolute right-3 top-1/2 transform -translate-y-1/2 w-4 h-4 text-muted-foreground pointer-events-none" />
          </div>

          <div className="relative">
            <select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value as GroundTruthStatus | 'all')}
              className="w-48 p-3 border rounded-md bg-white appearance-none cursor-pointer pr-10"
            >
              <option value="all">All Statuses</option>
              <option value="new">New</option>
              <option value="revisions_requested">Revisions Requested</option>
              <option value="validated">Validated</option>
              <option value="out-of-scope">Out of Scope</option>
            </select>
            <ChevronDown className="absolute right-3 top-1/2 transform -translate-y-1/2 w-4 h-4 text-muted-foreground pointer-events-none" />
          </div>

          <div className="relative">
            <select
              value={tagFilter}
              onChange={(e) => setTagFilter(e.target.value as string | 'all')}
              className="w-48 p-3 border rounded-md bg-white appearance-none cursor-pointer pr-10"
            >
              <option value="all">All Tags</option>
              {tags.map(tag => (
                <option key={tag.id} value={tag.id}>{tag.name}</option>
              ))}
            </select>
            <ChevronDown className="absolute right-3 top-1/2 transform -translate-y-1/2 w-4 h-4 text-muted-foreground pointer-events-none" />
          </div>
        </div>
      </div>

      <div className="grid gap-4">
        {filteredGroundTruths.map(gt => {
          const groundTruthTags = getTagsForGroundTruth(gt) as { id: string; name: string; color: string }[];
          return (
            <div key={gt.id} className="bg-white rounded-lg border shadow hover:shadow-md transition-shadow">
              <div className="p-6 pb-3">
                <div className="flex items-start justify-between">
                  <div className="flex-1 space-y-1">
                    <h3 className="leading-tight">
                      {gt.prompt}
                    </h3>
                    <div className="flex items-center gap-2 text-sm text-muted-foreground">
                      <CategoryIcon category={gt.category} />
                      <span>{formatCategory(gt.category)}</span>
                      <span>•</span>
                      <span>Created {gt.createdAt.toLocaleDateString()}</span>
                      <span>•</span>
                      <span>Updated {gt.updatedAt.toLocaleDateString()}</span>
                    </div>
                    {groundTruthTags.length > 0 && (
                      <div className="flex items-center gap-2 pt-2">
                        <TagIcon className="w-4 h-4 text-muted-foreground" />
                        <div className="flex gap-1 flex-wrap">
                          {groundTruthTags.map((tag: { id: string; name: string; color: string }) => (
                            <span key={tag.id} className={`px-2 py-1 rounded text-xs ${tag.color}`}>
                              {tag.name}
                            </span>
                          ))}
                        </div>
                      </div>
                    )}
                  </div>
                  <span className={`px-2 py-1 rounded text-sm ${getStatusColorClass(gt.status)}`}>
                    {formatStatus(gt.status)}
                  </span>
                </div>
              </div>

              <div className="px-6 pb-6">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-4 text-sm text-muted-foreground">
                    <span>{gt.generatedResponses.length} answers</span>
                    <span>{gt.reviews.length} reviews</span>
                  </div>

                  <NavLink
                    to={`/ground-truths/${gt.id}`}
                    className="flex items-center gap-2 px-3 py-2 text-sm hover:bg-muted rounded-md"
                  >
                    <Eye className="w-4 h-4" />
                    View Details
                  </NavLink>
                </div>
              </div>
            </div>
          );
        })}

        {filteredGroundTruths.length === 0 && (
          <div className="bg-white rounded-lg border shadow">
            <div className="text-center py-8">
              <p className="text-muted-foreground">No ground truths match your filters.</p>
            </div>
          </div>
        )}
      </div>

      {user?.role === 'data_curator' && (
        <div className="flex justify-end">
          <button
            onClick={() => setIsExportModalOpen(true)}
            disabled={filteredGroundTruths.length === 0}
            className="flex items-center gap-2 px-4 py-2 bg-primary text-primary-foreground hover:bg-primary/90 rounded-md disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            <Download className="w-4 h-4" />
            Export Data
          </button>
        </div>
      )}

      {isExportModalOpen && (
        <ExportModal

          isOpen={isExportModalOpen}
          onClose={() => setIsExportModalOpen(false)}
          groundTruths={filteredGroundTruths}
          tags={tags}
          getTagsForGroundTruth={getTagsForGroundTruth} />
      )}
    </div>
  );
};
