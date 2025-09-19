import CommentsComponent from '@/components/groundTruthDetails/Comments';
import DataQueryDefinitions from '@/components/groundTruthDetails/DataQueryDefinitions';
import GeneratedResponse from '@/components/groundTruthDetails/GeneratedResponse';
import { useOutletContext } from 'react-router';
import Contexts from '../components/groundTruthDetails/Context/Contexts';
import type { GroundTruthDefinition, Tag } from '../types/schemas';

// Read-only detail view (child index route)
export default function GroundTruthDetailView() {
  const { groundTruth } = useOutletContext<{ groundTruth: GroundTruthDefinition }>();

  return (
    <div className="bg-white rounded-lg border shadow p-6 space-y-6">
      <section className="space-y-2">
        <h3 className="text-lg font-semibold">User Query</h3>
        <p className="bg-muted p-3 rounded-md whitespace-pre-wrap">{groundTruth.UserQuery}</p>
      </section>

      <section className="space-y-2">
        <h3 className="text-lg font-semibold">Category</h3>
        <p className="bg-muted p-3 rounded-md">{groundTruth.Category || 'N/A'}</p>
      </section>

      <section className="space-y-2">
        <h3 className="text-lg font-semibold">Status</h3>
        <p className="px-3 py-1 inline-block rounded bg-gray-100">{groundTruth.ValidationStatus}</p>
      </section>

      <section className="space-y-2">
        <h3 className="text-lg font-semibold">Tags</h3>
        <div className="flex flex-wrap gap-2">
          {groundTruth.Tags?.length ? groundTruth.Tags.map((tag: Tag) => (
            <span key={tag.TagId} className="px-2 py-1 bg-blue-100 text-blue-800 rounded text-sm">{tag.Name}</span>
          )) : <p className="text-muted-foreground italic">No tags assigned</p>}
        </div>
      </section>

      <section className="space-y-3 ">
        <Contexts groundTruth={groundTruth} />
      </section>

      <section className="space-y-3">
        <DataQueryDefinitions dataQueries={groundTruth.DataQueryDefinitions} />
      </section>
      <GeneratedResponse groundTruth={groundTruth} />
      <section className="space-y-3 mt-4">
        <CommentsComponent comments={groundTruth.Comments} />
      </section>
    </div>
  );
}
