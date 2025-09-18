
import { ChevronDown, Download, Eye, Tag as TagIcon } from 'lucide-react';
import { useMemo, useState } from 'react';
import { NavLink, useLoaderData } from "react-router";
import { ExportModal } from '../components/ExportModal';
import { CategoryIcon } from '../components/ui/categoryIcon';
import { useAuth } from '../contexts/AuthContext';
// Removed tag retrieval from EditingContext; tags are now derived from loaded ground truths
import type { GroundTruthsListLoaderResponse } from '../routes/loaders/groundTruthsListLoader';
import type { GroundTruthStatus } from '../types';
import { filterGroundTruths } from '../utils/groundTruthFilters';
import { getStatusColorClass } from "../utils/groundTruthFormatting";
// Re-export external loader so React Router picks it up
export { groundTruthsListLoader as clientLoader } from "../routes/loaders/groundTruthsListLoader";

// Filtering to be moved to utility in later task (placeholder for parity)

export default function GroundTruthListPage() {
  const { groundTruths } = useLoaderData() as GroundTruthsListLoaderResponse;
  const { user } = useAuth();
  // Derive unique tag list from ground truth DTOs (PascalCase: Tags -> Tag objects)
  const uniqueTags = useMemo(() => {
    const map = new Map<string, { TagId: string; Name: string; Description: string }>();
    for (const gt of groundTruths as any[]) {
      if (gt?.Tags) {
        for (const t of gt.Tags) {
          if (t?.TagId && !map.has(t.TagId)) {
            map.set(t.TagId, { TagId: t.TagId, Name: t.Name, Description: t.Description ?? '' });
          }
        }
      }
    }
    return Array.from(map.values()).sort((a, b) => a.Name.localeCompare(b.Name));
  }, [groundTruths]);

  // Local filter state (Phase 2 Task 2.1 step 1-3)
  const [categoryFilter, setCategoryFilter] = useState<string | 'all'>('all');
  const [statusFilter, setStatusFilter] = useState<string | 'all'>('all');
  const [tagFilter, setTagFilter] = useState<string | 'all'>('all');

  // Map backend Tag objects (PascalCase) to simple display tokens with deterministic colors
  const colorPalette = [
    'bg-purple-100 text-purple-800',
    'bg-indigo-100 text-indigo-800',
    'bg-pink-100 text-pink-800',
    'bg-orange-100 text-orange-800',
    'bg-teal-100 text-teal-800',
    'bg-cyan-100 text-cyan-800',
    'bg-amber-100 text-amber-800',
    'bg-lime-100 text-lime-800'
  ];

  const colorForTag = (tagId: string) => {
    let hash = 0;
    for (let i = 0; i < tagId.length; i++) hash = (hash * 31 + tagId.charCodeAt(i)) >>> 0;
    return colorPalette[hash % colorPalette.length];
  };

  const getTagsForGroundTruth = (gt: any) => (gt?.Tags || []).map((t: any) => ({
    id: t.TagId,
    name: t.Name,
    color: colorForTag(t.TagId)
  }));

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
            {/* <select
              value={categoryFilter}
              onChange={(e) => setCategoryFilter(e.target.value as GroundTruthCategory | 'all')}
              className="w-48 p-3 border rounded-md bg-white appearance-none cursor-pointer pr-10"
            >
              <option value="all">All Categories</option>
              <option value="asset_knowledge">Asset Knowledge</option>
              <option value="unanswerable">Unanswerable</option>
              <option value="maintenance_request">Maintenance Request</option>
            </select> */}
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
              {uniqueTags.map(tag => (
                <option key={tag.TagId} value={tag.TagId}>{tag.Name}</option>
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
            <div key={gt.GroundTruthId} className="bg-white rounded-lg border shadow hover:shadow-md transition-shadow">
              <div className="p-6 pb-3">
                <div className="flex items-start justify-between">
                  <div className="flex-1 space-y-1">
                    <h3 className="leading-tight">
                      {gt.UserQuery}
                    </h3>
                    <div className="flex items-center gap-2 text-sm text-muted-foreground">
                      <CategoryIcon category={gt.Category} />
                      <span>{gt.Category}</span>
                      <span>•</span>
                      <span>Created {new Date(gt.CreationDateTime).toLocaleDateString()}</span>
                      <span>•</span>
                      <span>Updated {new Date(gt.UserUpdated ?? "").toLocaleDateString()}</span>
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
                  <span className={`px-2 py-1 rounded text-sm ${getStatusColorClass(gt.ValidationStatus)}`}>
                    {gt.ValidationStatus}
                  </span>
                </div>
              </div>

              <div className="px-6 pb-6">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-4 text-sm text-muted-foreground">
                    <span>{gt.GroundTruthEntries.length} answers</span>
                    <span>{gt.Comments.length} reviews</span>
                  </div>

                  <NavLink
                    to={`/ground-truths/${gt.GroundTruthId}`}
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
          tags={uniqueTags}
          getTagsForGroundTruth={getTagsForGroundTruth} />
      )}
    </div>
  );
};
