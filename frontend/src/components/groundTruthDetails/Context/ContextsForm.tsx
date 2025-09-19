import { useCallback, useMemo, useRef, useState } from "react";
import { useFetcher } from "react-router";
import type { GroundTruthContext } from "../../../types/schemas";

interface ContextsFormProps {
  groundTruthId: string;
  contexts: GroundTruthContext[];
}

export function ContextsForm({ groundTruthId, contexts }: ContextsFormProps) {
  const fetcher = useFetcher<{ groundTruth?: any; error?: string }>();

  // Create a stable initial snapshot for dirty tracking
  const initial = useMemo(() => structuredClone(contexts) as GroundTruthContext[], [contexts]);
  const [draftContexts, setDraftContexts] = useState<GroundTruthContext[]>(initial);
  // Baseline used for dirty tracking (optimistically updated on submit)
  const [baseline, setBaseline] = useState<GroundTruthContext[]>(initial);
  const prevBaselineRef = useRef<GroundTruthContext[] | null>(null);

  // Deduplicate contexts by ContextId
  const uniqueDraftContexts = useMemo(() => {
    const seen = new Set<string>();
    return draftContexts.filter(c => {
      if (!c.ContextId) return true;
      if (seen.has(c.ContextId)) return false;
      seen.add(c.ContextId);
      return true;
    });
  }, [draftContexts]);

  const serialize = (value: any) => JSON.stringify(value);
  const dirty = useMemo(() => serialize(baseline) !== serialize(draftContexts), [baseline, draftContexts]);

  // CRUD helpers
  const addContext = () => {
    const id = crypto.randomUUID();
    const entryId = crypto.randomUUID();
    setDraftContexts(prev => ([
      ...prev,
      {
        ContextId: id,
        GroundTruthId: groundTruthId,
        GroundTruthEntryId: entryId,
        ContextType: "Default",
        ContextParameters: []
      }
    ]));
  };

  const removeContext = (contextId: string) => {
    setDraftContexts(prev => prev.filter(c => c.ContextId !== contextId));
  };

  const updateContextType = (contextId: string, type: string) => {
    setDraftContexts(prev => prev.map(c => c.ContextId === contextId ? { ...c, ContextType: type } : c));
  };

  const addParameter = (contextId: string) => {
    setDraftContexts(prev => prev.map(c => c.ContextId === contextId ? {
      ...c,
      ContextParameters: [...c.ContextParameters, {
        ParameterId: crypto.randomUUID(),
        ParameterName: "",
        ParameterValue: "",
        DataType: "string"
      }]
    } : c));
  };

  const updateParameter = (contextId: string, paramId: string, field: "ParameterName" | "ParameterValue" | "DataType", value: string) => {
    setDraftContexts(prev => prev.map(c => c.ContextId === contextId ? {
      ...c,
      ContextParameters: c.ContextParameters.map(p => p.ParameterId === paramId ? { ...p, [field]: value } : p)
    } : c));
  };

  const removeParameter = (contextId: string, paramId: string) => {
    setDraftContexts(prev => prev.map(c => c.ContextId === contextId ? {
      ...c,
      ContextParameters: c.ContextParameters.filter(p => p.ParameterId !== paramId)
    } : c));
  };

  // Submit handler using fetcher.Form
  const onSubmit = useCallback((e: React.FormEvent) => {
    e.preventDefault();
    if (!dirty) return;
    // Optimistically mark current draft as baseline
    prevBaselineRef.current = baseline;
    setBaseline(draftContexts);
    const fd = new FormData();
    fd.append("contexts", JSON.stringify(draftContexts));
    fetcher.submit(fd, { method: "post", action: `/ground-truths/${groundTruthId}/contexts` });
  }, [dirty, draftContexts, fetcher, groundTruthId, baseline]);

  // If server returns an error, restore previous baseline so dirty indicator shows again
  if (fetcher.state === 'idle' && fetcher.data?.error && prevBaselineRef.current) {
    if (serialize(baseline) === serialize(draftContexts)) {
      // revert optimistic baseline only once
      setBaseline(prevBaselineRef.current);
      prevBaselineRef.current = null;
    }
  }

  const submitting = fetcher.state === "submitting";
  const errorMessage = fetcher.data?.error;

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="text-lg font-semibold">Contexts</h3>
        <button type="button" onClick={addContext} className="text-sm px-3 py-2 border rounded-md hover:bg-muted">Add Context</button>
      </div>
      {!uniqueDraftContexts.length && (
        <p className="text-sm text-muted-foreground italic">No contexts yet. Add one.</p>
      )}
      <fetcher.Form method="post" action={`/ground-truths/${groundTruthId}/contexts`} onSubmit={onSubmit} className="space-y-6">
        <input type="hidden" name="contexts" value={JSON.stringify(draftContexts)} />
        <div className="space-y-4 border rounded-md p-4">
          {uniqueDraftContexts.map((context, index) => (
            <div key={context.ContextId} className="border rounded-md p-4 space-y-3">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <span className="px-2 py-1 bg-blue-100 text-blue-800 rounded text-sm">Context {index + 1}</span>
                  <input
                    value={context.ContextType}
                    onChange={e => updateContextType(context.ContextId, e.target.value)}
                    className="p-1 border rounded text-xs"
                    placeholder="Type"
                  />
                </div>
                <button type="button" onClick={() => removeContext(context.ContextId)} className="text-xs text-red-600 hover:text-red-800">Remove</button>
              </div>
              <div className="space-y-2">
                <div className="flex items-center justify-between">
                  <label className="text-xs text-muted-foreground">Parameters</label>
                  <button type="button" onClick={() => addParameter(context.ContextId)} className="text-xs text-primary hover:text-primary/80">Add Parameter</button>
                </div>
                {context.ContextParameters.length === 0 && (
                  <p className="text-xs text-muted-foreground italic">No parameters.</p>
                )}
                <div className="space-y-2">
                  {context.ContextParameters.map(param => (
                    <div key={param.ParameterId} className="flex items-start gap-2 p-2 border rounded-md">
                      <div className="grid grid-cols-3 gap-2 flex-1">
                        <input
                          value={param.ParameterName}
                          onChange={e => updateParameter(context.ContextId, param.ParameterId, "ParameterName", e.target.value)}
                          placeholder="Name"
                          className="p-2 border rounded-md text-xs"
                        />
                        <input
                          value={param.ParameterValue}
                          onChange={e => updateParameter(context.ContextId, param.ParameterId, "ParameterValue", e.target.value)}
                          placeholder="Value"
                          className="p-2 border rounded-md text-xs"
                        />
                        <select
                          value={param.DataType || "string"}
                          onChange={e => updateParameter(context.ContextId, param.ParameterId, "DataType", e.target.value)}
                          className="p-2 border rounded-md text-xs bg-white"
                        >
                          <option value="string">String</option>
                          <option value="integer">Integer</option>
                          <option value="float">Float</option>
                          <option value="decimal">Decimal</option>
                          <option value="boolean">Boolean</option>
                          <option value="datetime">Datetime</option>
                        </select>
                      </div>
                      <button
                        type="button"
                        onClick={() => removeParameter(context.ContextId, param.ParameterId)}
                        className="text-red-600 hover:text-red-800 text-xs px-2"
                        title="Remove parameter"
                      >×</button>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          ))}
        </div>
        {errorMessage && (
          <div className="text-sm text-red-600">{errorMessage}</div>
        )}
        <div className="flex gap-3 justify-end">
          <button
            type="submit"
            disabled={!dirty || submitting}
            className="px-4 py-2 bg-primary text-primary-foreground rounded-md disabled:opacity-50"
          >{submitting ? "Saving..." : "Save Contexts"}</button>
          {dirty && !submitting && (
            <span className="text-xs text-muted-foreground self-center">Unsaved changes</span>
          )}
        </div>
      </fetcher.Form>
    </div>
  );
}
