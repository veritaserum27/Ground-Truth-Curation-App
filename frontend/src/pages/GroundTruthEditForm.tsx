import GeneratedResponse from '@/components/groundTruthDetails/GeneratedResponse';
import { Plus, Trash2 } from 'lucide-react';
import { useState } from 'react';
import { useNavigate, useOutletContext } from 'react-router';
import { TagManager } from '../components/TagManager';
import { useEditingData } from '../contexts/EditingContext';

import type { Comment, DataQueryDefinition, GroundTruthDefinition } from '../services/schemas';

export default function GroundTruthEditForm() {
  const { groundTruth } = useOutletContext<{ groundTruth: GroundTruthDefinition }>();

  const navigate = useNavigate();
  const { applyLocalEdits } = useEditingData();
  // Local editing state mirrors DTO shape (PascalCase) for merge submission
  const [form, setForm] = useState(() => ({
    UserQuery: groundTruth.UserQuery || '',
    Category: groundTruth.Category || '',
    ValidationStatus: groundTruth.ValidationStatus,
    Tags: (groundTruth.Tags || []).map(t => t.TagId),
    DataQueryDefinitions: groundTruth.DataQueryDefinitions?.map(d => ({ ...d })) || [],
    Comments: groundTruth.Comments?.map(c => ({ ...c })) || [],
    GroundTruthEntries: groundTruth.GroundTruthEntries?.map(e => ({
      GroundTruthEntryId: e.GroundTruthEntryId,
      GroundTruthContext: e.GroundTruthContext ? { ...e.GroundTruthContext, ContextParameters: e.GroundTruthContext.ContextParameters.map(p => ({ ...p })) } : null,
      Response: e.Response,
      RequiredValues: [...(e.RequiredValues || [])],
      RawData: e.RawData, // read-only display, keep pass-through
      CreationDateTime: e.CreationDateTime
    })) || []
  }));

  const handleSave = () => {
    applyLocalEdits(groundTruth.GroundTruthId, form as any);
    navigate(`/ground-truths/${groundTruth.GroundTruthId}`);
  };
  const handleCancel = () => navigate(`/ground-truths/${groundTruth.GroundTruthId}`);

  // ---------- Data Query Definitions CRUD ----------
  const addDataQueryDefinition = () => {
    setForm(f => ({
      ...f,
      DataQueryDefinitions: [
        ...f.DataQueryDefinitions,
        {
          DataQueryId: crypto.randomUUID(),
          GroundTruthId: groundTruth.GroundTruthId,
          DatastoreType: 'Sql',
          DatastoreName: '',
          QueryTarget: '',
          QueryDefinition: '',
          IsFullQuery: true,
          RequiredProperties: [],
          UserCreated: groundTruth.UserCreated,
          UserUpdated: groundTruth.UserUpdated || groundTruth.UserCreated,
          CreationDateTime: new Date().toISOString()
        } as DataQueryDefinition
      ]
    }));
  };

  const updateDataQuery = (id: string | undefined | null, key: keyof DataQueryDefinition, value: any) => {
    setForm(f => ({
      ...f,
      DataQueryDefinitions: f.DataQueryDefinitions.map(d => (d.DataQueryId === id ? { ...d, [key]: value } : d))
    }));
  };

  const removeDataQuery = (id: string | undefined | null) => {
    setForm(f => ({ ...f, DataQueryDefinitions: f.DataQueryDefinitions.filter(d => d.DataQueryId !== id) }));
  };

  const addRequiredProperty = (id: string | undefined | null, prop: string) => {
    if (!prop.trim()) return;
    setForm(f => ({
      ...f,
      DataQueryDefinitions: f.DataQueryDefinitions.map(d => d.DataQueryId === id ? { ...d, RequiredProperties: [...d.RequiredProperties, prop.trim()] } : d)
    }));
  };
  const removeRequiredProperty = (id: string | undefined | null, prop: string) => {
    setForm(f => ({
      ...f,
      DataQueryDefinitions: f.DataQueryDefinitions.map(d => d.DataQueryId === id ? { ...d, RequiredProperties: d.RequiredProperties.filter(p => p !== prop) } : d)
    }));
  };

  // ---------- Contexts (per Entry) ----------
  const addContextToEntry = (entryId: string) => {
    setForm(f => ({
      ...f,
      GroundTruthEntries: f.GroundTruthEntries.map(e => (
        e.GroundTruthEntryId === entryId
          ? {
            ...e,
            GroundTruthContext: e.GroundTruthContext || {
              ContextId: crypto.randomUUID(),
              GroundTruthId: groundTruth.GroundTruthId,
              GroundTruthEntryId: entryId,
              ContextType: 'Default',
              ContextParameters: []
            }
          }
          : e
      ))
    }));
  };

  const updateContextType = (entryId: string, value: string) => {
    setForm(f => ({
      ...f,
      GroundTruthEntries: f.GroundTruthEntries.map(e => e.GroundTruthEntryId === entryId && e.GroundTruthContext ? { ...e, GroundTruthContext: { ...e.GroundTruthContext, ContextType: value } } : e)
    }));
  };

  const addContextParameter = (entryId: string) => {
    setForm(f => ({
      ...f,
      GroundTruthEntries: f.GroundTruthEntries.map(e => {
        if (e.GroundTruthEntryId !== entryId || !e.GroundTruthContext) return e;
        return {
          ...e,
          GroundTruthContext: {
            ...e.GroundTruthContext,
            ContextParameters: [
              ...e.GroundTruthContext.ContextParameters,
              { ParameterId: crypto.randomUUID(), ParameterName: '', ParameterValue: '', DataType: 'string' }
            ]
          }
        };
      })
    }));
  };

  const updateContextParameter = (entryId: string, paramId: string, field: 'ParameterName' | 'ParameterValue' | 'DataType', value: string) => {
    setForm(f => ({
      ...f,
      GroundTruthEntries: f.GroundTruthEntries.map(e => {
        if (e.GroundTruthEntryId !== entryId || !e.GroundTruthContext) return e;
        return {
          ...e,
          GroundTruthContext: {
            ...e.GroundTruthContext,
            ContextParameters: e.GroundTruthContext.ContextParameters.map(p => p.ParameterId === paramId ? { ...p, [field]: value } : p)
          }
        };
      })
    }));
  };

  const removeContextParameter = (entryId: string, paramId: string) => {
    setForm(f => ({
      ...f,
      GroundTruthEntries: f.GroundTruthEntries.map(e => {
        if (e.GroundTruthEntryId !== entryId || !e.GroundTruthContext) return e;
        return {
          ...e,
          GroundTruthContext: {
            ...e.GroundTruthContext,
            ContextParameters: e.GroundTruthContext.ContextParameters.filter(p => p.ParameterId !== paramId)
          }
        };
      })
    }));
  };

  const removeContextFromEntry = (entryId: string) => {
    setForm(f => ({
      ...f,
      GroundTruthEntries: f.GroundTruthEntries.map(e => e.GroundTruthEntryId === entryId ? { ...e, GroundTruthContext: null } : e)
    }));
  };

  // ---------- Comments (Curator Notes & Reviews) ----------
  const addComment = (type: string, text: string) => {
    if (!text.trim()) return;
    setForm(f => ({
      ...f,
      Comments: [
        ...f.Comments,
        {
          CommentId: crypto.randomUUID(),
          CommentText: text.trim(),
          CommentType: type,
          UserCreated: groundTruth.UserUpdated || groundTruth.UserCreated,
          CreationDateTime: new Date().toISOString()
        } as Comment
      ]
    }));
  };
  const removeComment = (id: string) => setForm(f => ({ ...f, Comments: f.Comments.filter(c => c.CommentId !== id) }));
  const updateComment = (id: string, text: string) => setForm(f => ({ ...f, Comments: f.Comments.map(c => c.CommentId === id ? { ...c, CommentText: text } : c) }));

  const [newCommentText, setNewCommentText] = useState('');
  const [newCommentType, setNewCommentType] = useState('DataCuratorNote');

  // Add brand new context by creating a new entry stub (since contexts belong to entries)
  const addNewContext = () => {
    const newEntryId = crypto.randomUUID();
    const newEntry = {
      GroundTruthEntryId: newEntryId,
      GroundTruthContext: {
        ContextId: crypto.randomUUID(),
        GroundTruthId: groundTruth.GroundTruthId,
        GroundTruthEntryId: newEntryId,
        ContextType: 'Default',
        ContextParameters: [] as { ParameterId: string; ParameterName: string; ParameterValue: string; DataType: string }[]
      },
      Response: '',
      RequiredValues: [] as string[],
      RawData: [] as any[],
      CreationDateTime: new Date().toISOString()
    };
    setForm(f => ({ ...f, GroundTruthEntries: [...f.GroundTruthEntries, newEntry] }));
  };

  return (
    <div className="bg-white rounded-lg border shadow p-6 space-y-8">
      <section className="space-y-2">
        <label className="font-medium">User Query</label>
        <textarea
          value={form.UserQuery}
          onChange={e => setForm(f => ({ ...f, UserQuery: e.target.value }))}
          rows={3}
          className="w-full p-3 border rounded-md resize-none"
        />
      </section>

      <section className="space-y-2">
        <label className="font-medium">Category</label>
        <div className="relative">
          <select
            value={form.Category}
            onChange={e => setForm(f => ({ ...f, Category: e.target.value }))}
            className="w-full p-3 border rounded-md bg-white appearance-none cursor-pointer pr-10"
          >
            <option value="">Select Category</option>
            <option value="asset_knowledge">Asset Knowledge</option>
            <option value="unanswerable">Unanswerable</option>
            <option value="maintenance_request">Maintenance Request</option>
          </select>
        </div>
      </section>

      <section className="space-y-2">
        <label className="font-medium">Status</label>
        <div className="relative">
          <select
            value={form.ValidationStatus}
            onChange={e => setForm(f => ({ ...f, ValidationStatus: e.target.value as any }))}
            className="w-full p-3 border rounded-md bg-white appearance-none cursor-pointer pr-10"
          >
            <option value="new">New</option>
            <option value="revisions_requested">Revisions Requested</option>
            <option value="validated">Validated</option>
            <option value="out-of-scope">Out of Scope</option>
          </select>
        </div>
      </section>

      <section className="space-y-2">
        <label className="font-medium">Tags</label>
        <TagManager
          selectedTags={form.Tags}
          onTagsChange={(tags) => setForm(f => ({ ...f, Tags: tags }))}
        />
      </section>

      {/* Contexts (moved up) */}
      <section className="space-y-4 mt-4">
        <h3 className="text-lg font-semibold">Contexts</h3>
        {form.GroundTruthEntries.filter(e => e.GroundTruthContext).length === 0 && (
          <p className="text-sm text-muted-foreground italic">No contexts defined yet. Add one below.</p>
        )}
        <div className="space-y-4 border rounded-md p-4">
          {form.GroundTruthEntries.filter(e => e.GroundTruthContext).map((entry, idx) => (
            <div key={entry.GroundTruthEntryId} className="border rounded-md p-4 space-y-4">
              <div className="flex items-center justify-between">
                <span className="px-2 py-1 bg-blue-100 text-blue-800 rounded text-sm font-medium">Context {idx + 1}</span>
                <button onClick={() => removeContextFromEntry(entry.GroundTruthEntryId)} className="flex items-center gap-1 text-xs text-red-600 hover:text-red-800"><Trash2 className="w-4 h-4" />Remove Context</button>
              </div>
              <div className="space-y-3">
                <div>
                  <label className="text-xs text-muted-foreground">Context Type</label>
                  <input value={entry.GroundTruthContext!.ContextType} onChange={e => updateContextType(entry.GroundTruthEntryId, e.target.value)} className="w-full p-2 border rounded-md text-sm" />
                </div>
                <div className="space-y-2">
                  <div className="flex items-center justify-between">
                    <label className="text-xs text-muted-foreground">Parameters</label>
                    <button onClick={() => addContextParameter(entry.GroundTruthEntryId)} className="flex items-center gap-1 text-xs text-primary hover:text-primary/80"><Plus className="w-3 h-3" /> Add Parameter</button>
                  </div>
                  {entry.GroundTruthContext!.ContextParameters.length === 0 && (
                    <p className="text-xs text-muted-foreground italic">No parameters yet.</p>
                  )}
                  <div className="space-y-2">
                    {entry.GroundTruthContext!.ContextParameters.map(p => (
                      <div key={p.ParameterId} className="flex items-start gap-2 p-2 border rounded-md">
                        <div className="grid grid-cols-3 gap-2 flex-1">
                          <input value={p.ParameterName} onChange={e => updateContextParameter(entry.GroundTruthEntryId, p.ParameterId, 'ParameterName', e.target.value)} placeholder="Name" className="p-2 border rounded-md text-xs" />
                          <input value={p.ParameterValue} onChange={e => updateContextParameter(entry.GroundTruthEntryId, p.ParameterId, 'ParameterValue', e.target.value)} placeholder="Value" className="p-2 border rounded-md text-xs" />
                          <div className="relative">
                            <select value={p.DataType || 'string'} onChange={e => updateContextParameter(entry.GroundTruthEntryId, p.ParameterId, 'DataType', e.target.value)} className="w-full p-2 border rounded-md text-xs pr-8 bg-white">
                              <option value="string">String</option>
                              <option value="integer">Integer</option>
                              <option value="float">Float</option>
                              <option value="decimal">Decimal</option>
                              <option value="boolean">Boolean</option>
                              <option value="datetime">Datetime</option>
                            </select>
                          </div>
                        </div>
                        <button onClick={() => removeContextParameter(entry.GroundTruthEntryId, p.ParameterId)} className="text-red-600 hover:text-red-800 w-8 h-8 flex items-center justify-center" title="Remove parameter"><Trash2 className="w-4 h-4" /></button>
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
        <button type="button" onClick={addNewContext} className="flex items-center gap-2 px-4 py-2 border border-dashed border-gray-300 rounded-md text-sm text-muted-foreground hover:border-primary hover:text-primary transition-colors w-full justify-center"><Plus className="w-4 h-4" /> Add New Context</button>
      </section>

      {/* Data Query Definitions (moved below contexts) */}
      <section className="space-y-4 mt-4">
        <div className="flex items-center justify-between">
          <h3 className="text-lg font-semibold">Data Query Definitions</h3>
        </div>
        {form.DataQueryDefinitions.length === 0 && (
          <p className="text-sm text-muted-foreground italic">No data query definitions. Add one.</p>
        )}
        <div className="space-y-6 border rounded-md p-4">
          {form.DataQueryDefinitions.map(d => (
            <div key={d.DataQueryId} className="border rounded-md p-4 space-y-3">
              <div className="flex justify-between items-center">
                <span className="text-sm font-medium">{d.DatastoreType} Query</span>
                <button onClick={() => removeDataQuery(d.DataQueryId)} className="text-red-600 hover:text-red-800 text-xs flex items-center gap-1"><Trash2 className="w-4 h-4" />Remove</button>
              </div>
              <div className="grid md:grid-cols-3 gap-3">
                <div>
                  <label className="text-xs text-muted-foreground">Datastore Type</label>
                  <select value={d.DatastoreType} onChange={e => updateDataQuery(d.DataQueryId, 'DatastoreType', e.target.value)} className="w-full p-2 border rounded-md text-sm bg-white">
                    <option value="Sql">Sql</option>
                    <option value="CosmosDb">CosmosDb</option>
                  </select>
                </div>
                <div>
                  <label className="text-xs text-muted-foreground">Datastore Name</label>
                  <input value={d.DatastoreName} onChange={e => updateDataQuery(d.DataQueryId, 'DatastoreName', e.target.value)} className="w-full p-2 border rounded-md text-sm" placeholder="db name" />
                </div>
                <div>
                  <label className="text-xs text-muted-foreground">Target</label>
                  <input value={d.QueryTarget} onChange={e => updateDataQuery(d.DataQueryId, 'QueryTarget', e.target.value)} className="w-full p-2 border rounded-md text-sm" placeholder="schema.table" />
                </div>
              </div>
              <div>
                <label className="text-xs text-muted-foreground">Query Definition</label>
                <textarea value={d.QueryDefinition} onChange={e => updateDataQuery(d.DataQueryId, 'QueryDefinition', e.target.value)} rows={5} className="w-full p-2 border rounded-md font-mono text-xs" placeholder="SELECT * FROM ..." />
              </div>
              <div className="flex items-center gap-2">
                <label className="text-xs text-muted-foreground">Is Full Query?</label>
                <input type="checkbox" checked={d.IsFullQuery} onChange={e => updateDataQuery(d.DataQueryId, 'IsFullQuery', e.target.checked)} />
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
        </div>
        <button type="button" onClick={addDataQueryDefinition} className="flex items-center gap-2 px-4 py-2 border border-dashed border-gray-300 rounded-md text-sm text-muted-foreground hover:border-primary hover:text-primary transition-colors w-full justify-center"><Plus className="w-4 h-4" /> Add New Query</button>

      </section>


      {/* Comments */}
      <section className="space-y-4 mt-4">
        <h3 className="text-lg font-semibold">Comments & Notes</h3>
        {form.Comments.length === 0 && <p className="text-sm text-muted-foreground italic">No comments yet.</p>}
        <div className="space-y-4">
          {form.Comments.map(c => (
            <div key={c.CommentId} className="border rounded-md p-3 space-y-2">
              <div className="flex justify-between items-center">
                <span className="text-xs px-2 py-1 bg-gray-100 rounded">{c.CommentType}</span>
                <button onClick={() => removeComment(c.CommentId)} className="text-red-600 hover:text-red-800 text-xs flex items-center gap-1"><Trash2 className="w-4 h-4" />Delete</button>
              </div>
              <textarea value={c.CommentText} onChange={e => updateComment(c.CommentId, e.target.value)} rows={2} className="w-full p-2 border rounded-md text-sm" />
              <div className="flex gap-4 text-[10px] text-muted-foreground">
                <span>{c.UserCreated}</span>
                <span>{new Date(c.CreationDateTime).toLocaleString()}</span>
              </div>
            </div>
          ))}
        </div>
        <GeneratedResponse groundTruth={groundTruth} />
        <div className="border rounded-md p-3 space-y-2">
          <div className="flex gap-2">
            <select value={newCommentType} onChange={e => setNewCommentType(e.target.value)} className="p-2 border rounded-md text-sm bg-white">
              <option value="DataCuratorNote">DataCuratorNote</option>
              <option value="Review">Review</option>
            </select>
            <input value={newCommentText} onChange={e => setNewCommentText(e.target.value)} placeholder="Add comment text" className="flex-1 p-2 border rounded-md text-sm" />
            <button onClick={() => { addComment(newCommentType, newCommentText); setNewCommentText(''); }} className="px-3 py-2 bg-primary text-primary-foreground rounded-md text-sm">Add</button>
          </div>
        </div>
      </section>

      <div className="flex gap-3 justify-end pt-4">
        <button onClick={handleCancel} className="px-4 py-2 border rounded-md hover:bg-muted">Cancel</button>
        <button onClick={handleSave} className="px-4 py-2 bg-primary text-primary-foreground rounded-md hover:bg-primary/90">Save Changes</button>
      </div>
    </div>
  );
}

// Small inline component for adding required properties
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
