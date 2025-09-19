import type { DataQueryDefinition } from "@/types/schemas";

export default function DataQueryDefinitions({ dataQueries = [] }: { dataQueries: DataQueryDefinition[] }) {
  return (
    <>
      <h3 className="text-lg font-semibold">Data Query Definitions</h3>
      {dataQueries.length ? (
        <div className="space-y-4 border rounded-md p-4">
          {dataQueries.map((d: DataQueryDefinition) => (
            <div key={d.DataQueryId || d.QueryDefinition} className="border rounded-md p-4 space-y-2">
              <div className="flex items-center justify-between">
                <div className="font-medium">{d.DatastoreType} Query</div>
                <span className="text-xs text-muted-foreground">{d.DatastoreName}</span>
              </div>
              <div className="text-xs text-muted-foreground">Target: {d.QueryTarget}</div>
              <pre className="bg-muted p-3 rounded text-sm whitespace-pre-wrap overflow-auto max-h-64">{d.QueryDefinition}</pre>
              {d.RequiredProperties?.length > 0 && (
                <div className="text-xs"><span className="font-semibold">Required Properties:</span> {d.RequiredProperties.join(', ')}</div>
              )}
              <div className="flex gap-4 text-xs text-muted-foreground pt-1">
                <span>Created: {new Date(d.CreationDateTime).toLocaleString()}</span>
                <span>User: {d.UserCreated}</span>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <p className="text-muted-foreground italic">No data query definitions.</p>
      )}
    </>
  );
}
