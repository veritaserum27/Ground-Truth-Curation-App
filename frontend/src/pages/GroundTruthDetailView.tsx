import CommentsComponent from '@/components/groundTruthDetails/Comments';
import DataQueryDefinitions from '@/components/groundTruthDetails/DataQueryDefinitions';
import GeneratedResponse from '@/components/groundTruthDetails/GeneratedResponse';
import { useOutletContext } from 'react-router';
import type { GroundTruthDefinition, Tag } from '../services/schemas';

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
        <h3 className="text-lg font-semibold">Contexts</h3>
        {(() => {
          const contexts = (groundTruth.GroundTruthEntries || [])
            .map(e => e.GroundTruthContext)
            .filter((c): c is NonNullable<typeof c> => Boolean(c));

          if (!contexts.length) {
            return <p className="text-muted-foreground italic">No contexts.</p>;
          }

          // Optional de-duplication by ContextId (keep first occurrence)
          const seen = new Set<string>();
          const uniqueContexts = contexts.filter(c => {
            if (!c.ContextId) return true; // include if no id
            if (seen.has(c.ContextId)) return false;
            seen.add(c.ContextId);
            return true;
          });

          return (
            <div className="space-y-4 border rounded-md p-4">
              {uniqueContexts.map((context, index) => (
                <div key={context.ContextId || index} className="border rounded-md p-4">
                  <div className="space-y-2">
                    <div className="flex items-center gap-2 mb-3">
                      <span className="px-2 py-1 bg-blue-100 text-blue-800 rounded text-sm">Context {index + 1}</span>
                      {context.ContextType && (
                        <span className="text-xs text-muted-foreground">{context.ContextType}</span>
                      )}
                    </div>
                    <div className="space-y-2">
                      {context.ContextParameters?.length ? (
                        context.ContextParameters.map((p, paramIndex) => (
                          <div key={p.ParameterId || paramIndex} className="flex items-center gap-2 p-3 border rounded-md">
                            <span className="text-muted-foreground min-w-10">{p.ParameterName}:</span>
                            <span className="px-2 py-1 bg-muted rounded text-sm">{p.ParameterValue}</span>
                            <span className="px-1 py-0.5 bg-gray-100 text-gray-600 rounded text-xs">{p.DataType}</span>
                          </div>
                        ))
                      ) : (
                        <p className="text-sm text-muted-foreground italic">No parameters defined.</p>
                      )}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          );
        })()}
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
