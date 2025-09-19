import { Plus, Trash2 } from "lucide-react";
import { useCallback, useMemo, useRef, useState } from "react";
import { useFetcher } from "react-router";
import type { DataQueryDefinition } from "../../../types/schemas";

export function DataQueryDefinitionsForm({ dataQueries = [] }: { dataQueries: DataQueryDefinition[] }) {
  const fetcher = useFetcher<{ groundTruth?: any; error?: string }>();

  const initial = useMemo(() => structuredClone(dataQueries) as DataQueryDefinition[], [dataQueries]);
  const [draftQueries, setDraftQueries] = useState<DataQueryDefinition[]>(initial);
  const [baseline, setBaseline] = useState<DataQueryDefinition[]>(initial);
  const prevBaselineRef = useRef<DataQueryDefinition[] | null>(null);
  const serialize = (v: any) => JSON.stringify(v);
  const dirty = useMemo(() => serialize(baseline) !== serialize(draftQueries), [baseline, draftQueries]);

  const groundTruthId: string | undefined = dataQueries[0]?.GroundTruthId;

  const removeDataQuery = (id: string | undefined | null) => {
    setDraftQueries(prev => prev.filter(d => d.DataQueryId !== id));
  }
  const updateDataQuery = (id: string | undefined | null, key: keyof DataQueryDefinition, value: any) => {
    setDraftQueries(prev => prev.map(d => d.DataQueryId === id ? { ...d, [key]: value } : d));
  }
  const addDataQueryDefinition = () => {
    const newQuery: Partial<DataQueryDefinition> = {
      DataQueryId: crypto.randomUUID(),
      GroundTruthId: groundTruthId!,
      DatastoreType: 'Sql',
      DatastoreName: '',
      QueryTarget: '',
      QueryDefinition: '',
      IsFullQuery: false,
      RequiredProperties: [],
      UserCreated: 'current_user', // Placeholder, replace with actual user
      UserUpdated: 'current_user', // Placeholder, replace with actual user
      CreationDateTime: new Date().toISOString()
    };
    setDraftQueries(prev => [...prev, {
      ...newQuery
    } as DataQueryDefinition]);
  }


  const addRequiredProperty = (id: string | undefined | null, property: string) => {
    setDraftQueries(prev => prev.map(d => d.DataQueryId === id ? {
      ...d,
      RequiredProperties: Array.from(new Set([...(d.RequiredProperties || []), property]))
    } : d));
  }

  const removeRequiredProperty = (id: string | undefined | null, property: string) => {
    setDraftQueries(prev => prev.map(d => d.DataQueryId === id ? {
      ...d,
      RequiredProperties: (d.RequiredProperties || []).filter(p => p !== property)
    } : d));
  }



  const onSubmit = useCallback((e: React.FormEvent) => {
    e.preventDefault();
    if (!dirty || !groundTruthId) return;
    prevBaselineRef.current = baseline;
    setBaseline(draftQueries); // optimistic baseline
    const fd = new FormData();
    // Map DatastoreType string to backend enum integer (0=Sql,1=CosmosDb)
    fd.append('queries', JSON.stringify(draftQueries));
    fetcher.submit(fd, { method: 'post', action: `/ground-truths/${groundTruthId}/queries` });
  }, [dirty, groundTruthId, draftQueries, fetcher, baseline]);

  // Revert optimistic baseline if error returned
  if (fetcher.state === 'idle' && fetcher.data?.error && prevBaselineRef.current) {
    if (serialize(baseline) === serialize(draftQueries)) { // only revert once
      setBaseline(prevBaselineRef.current);
      prevBaselineRef.current = null;
    }
  }

  const submitting = fetcher.state === "submitting";
  return (
    <fetcher.Form
      method="post"
      onSubmit={onSubmit}
      action={`/ground-truths/${groundTruthId || ''}/queries`}
      className="space-y-6 border rounded-md p-4">
      {draftQueries.map((d) => (
        <div key={d.DataQueryId || d.QueryDefinition} className="border rounded-md p-4 space-y-2">
          <div className="flex justify-between items-center">
            <span className="text-sm font-medium">{d.DatastoreType} Query</span>
            <button onClick={() => removeDataQuery(d.DataQueryId)} className="text-red-600 hover:text-red-800 text-xs flex items-center gap-1"><Trash2 className="w-4 h-4" />Remove</button>
          </div>
          <div className="grid md:grid-cols-3 gap-3">
            <div>
              <label className="text-xs text-muted-foreground">Datastore Type</label>
              <select name="data_store_type" value={d.DatastoreType} onChange={e => updateDataQuery(d.DataQueryId, 'DatastoreType', e.target.value)} className="w-full p-2 border rounded-md text-sm bg-white">
                <option value="Sql">Sql</option>
                <option value="CosmosDb">CosmosDb</option>
              </select>
            </div>
            <div>
              <label className="text-xs text-muted-foreground">Datastore Name</label>
              <input name="data_store_name" value={d.DatastoreName} onChange={e => updateDataQuery(d.DataQueryId, 'DatastoreName', e.target.value)} className="w-full p-2 border rounded-md text-sm" placeholder="db name" />
            </div>
            <div>
              <label className="text-xs text-muted-foreground">Target</label>
              <input name="query_type" value={d.QueryTarget} onChange={e => updateDataQuery(d.DataQueryId, 'QueryTarget', e.target.value)} className="w-full p-2 border rounded-md text-sm" placeholder="schema.table" />
            </div>
          </div>
          <div>
            <label className="text-xs text-muted-foreground">Query Definition</label>
            <textarea name="query_definition" value={d.QueryDefinition} onChange={e => updateDataQuery(d.DataQueryId, 'QueryDefinition', e.target.value)} rows={5} className="w-full p-2 border rounded-md font-mono text-xs" placeholder="SELECT * FROM ..." />
          </div>
          <div className="flex items-center gap-2">
            <label className="text-xs text-muted-foreground">Is Full Query?</label>
            <input name="is_full_query" type="checkbox" onChange={e => updateDataQuery(d.DataQueryId, 'IsFullQuery', e.target.checked)} checked={d.IsFullQuery} />
          </div>
          <div className="space-y-2">
            <label className="text-xs text-muted-foreground">Required Properties</label>
            <div className="flex flex-wrap gap-2">
              {d.RequiredProperties.map(rp => (
                <span key={rp} className="flex items-center gap-1 px-2 py-1 bg-gray-100 rounded text-xs">
                  {rp}
                  <button onClick={() => removeRequiredProperty(d.DataQueryId, rp)} className="text-red-600 hover:text-red-800" title="Remove">×</button>
                </span>
              ))}
            </div>
            <AddRequiredProperty onAdd={(v) => addRequiredProperty(d.DataQueryId, v)} />
          </div>
        </div>

      ))}
      <button type="button" onClick={addDataQueryDefinition} className="flex items-center gap-2 px-4 py-2 border border-dashed border-gray-300 rounded-md text-sm text-muted-foreground hover:border-primary hover:text-primary transition-colors w-full justify-center"><Plus className="w-4 h-4" /> Add New Query</button>

      <div className="flex gap-3 justify-end">
        <button
          type="submit"
          disabled={!dirty || submitting || !groundTruthId}
          className="px-4 py-2 bg-primary text-primary-foreground rounded-md disabled:opacity-50"
        >{submitting ? "Saving..." : "Save Queries"}</button>
        {dirty && !submitting && (
          <span className="text-xs text-muted-foreground self-center">Unsaved changes</span>
        )}
      </div>
    </fetcher.Form >

  );
}

function AddRequiredProperty({ onAdd }: { onAdd: (v: string) => void }) {
  const [value, setValue] = useState('');
  return (
    <div className="flex gap-2">
      <input
        value={value}
        onChange={e => setValue(e.target.value)}
        placeholder="Add property"
        className="p-2 border rounded-md text-sm flex-1"
      />
      <button
        type="button"
        onClick={() => { onAdd(value); setValue(''); }}
        className="px-3 py-2 text-sm bg-secondary text-secondary-foreground rounded-md"
      >Add</button>
    </div>
  );
}
