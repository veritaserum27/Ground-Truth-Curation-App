import { createContext, useContext, useState, type ReactNode } from 'react';
import type { GroundTruth, Review, Tag } from '../types';

interface EditingContextType {
  editForm: Partial<GroundTruth>;
  setEditForm: React.Dispatch<React.SetStateAction<Partial<GroundTruth>>>;
  applyLocalEdits: (groundTruthId: string, updated: Partial<GroundTruth>) => void;
  getMergedGroundTruth: (original: GroundTruth) => GroundTruth;
  tags: Tag[];
  addTag: (tag: Omit<Tag, 'id' | 'createdAt'>) => void;
  addReview: (groundTruthId: string, review: Omit<Review, 'id' | 'timestamp'>) => void;
  getLocalReviews: (groundTruthId: string) => Review[];
  getMergedReviews: (groundTruthId: string, original: Review[]) => Review[];
  deepCopyGroundTruth: (gt: GroundTruth) => Partial<GroundTruth>;
}

const EditingContext = createContext<EditingContextType | undefined>(undefined);

// Primary hook
export const useEditingData = () => {
  const ctx = useContext(EditingContext);
  if (!ctx) throw new Error('useEditingData must be used within EditingProvider');
  return ctx;
};

const predefinedTags: Tag[] = [
  { id: 'tag-answerable', name: 'Answerable', color: 'bg-green-100 text-green-800', isPredefined: true },
  { id: 'tag-unanswerable', name: 'Unanswerable', color: 'bg-red-100 text-red-800', isPredefined: true },
  { id: 'tag-multiple-sources', name: 'Multiple Data Sources', color: 'bg-blue-100 text-blue-800', isPredefined: true }
];

type LocalEditsMap = Record<string, Partial<GroundTruth>>;
type LocalReviewsMap = Record<string, Review[]>;

interface EditingProviderProps { children: ReactNode }

export const EditingProvider = ({ children }: EditingProviderProps) => {
  // Deprecated state (to be removed after consumers migrate to route-based editing)
  const [editForm, setEditForm] = useState<Partial<GroundTruth>>({});
  const [tags, setTags] = useState<Tag[]>(predefinedTags);
  const [localEdits, setLocalEdits] = useState<LocalEditsMap>({});
  const [localReviews, setLocalReviews] = useState<LocalReviewsMap>({});

  const applyLocalEdits = (groundTruthId: string, updated: Partial<GroundTruth>) => {
    setLocalEdits(prev => ({ ...prev, [groundTruthId]: { ...prev[groundTruthId], ...updated } }));
  };
  const getMergedGroundTruth = (original: GroundTruth): GroundTruth => {
    const edits = localEdits[original.id];
    if (!edits) return original;
    return { ...original, ...edits }; // keep server updatedAt authoritative
  };
  const addTag = (tagData: Omit<Tag, 'id' | 'createdAt'>) => {
    const newTag: Tag = { ...tagData, id: `tag-${Date.now()}`, createdAt: new Date() };
    setTags(prev => [...prev, newTag]);
  };
  const addReview = (groundTruthId: string, reviewData: Omit<Review, 'id' | 'timestamp'>) => {
    const newReview: Review = { ...reviewData, id: Date.now().toString(), timestamp: new Date() };
    setLocalReviews(prev => ({ ...prev, [groundTruthId]: [...(prev[groundTruthId] || []), newReview] }));
  };
  const getLocalReviews = (groundTruthId: string) => localReviews[groundTruthId] || [];
  const getMergedReviews = (groundTruthId: string, original: Review[]) => [...original, ...getLocalReviews(groundTruthId)];
  const deepCopyGroundTruth = (gt: GroundTruth): Partial<GroundTruth> => ({
    prompt: gt.prompt,
    contexts: gt.contexts.map(c => ({ id: c.id, parameters: JSON.parse(JSON.stringify(c.parameters)) })),
    dataQueryDefinitions: gt.dataQueryDefinitions.map(d => ({ id: d.id, dataStoreType: d.dataStoreType, query: d.query, name: d.name, contextId: d.contextId })),
    category: gt.category,
    status: gt.status,
    tags: [...gt.tags],
    dataCuratorNotes: gt.dataCuratorNotes
  });

  return (
    <EditingContext.Provider value={{
      editForm,
      setEditForm,
      applyLocalEdits,
      getMergedGroundTruth,
      tags,
      addTag,
      addReview,
      getLocalReviews,
      getMergedReviews,
      deepCopyGroundTruth
    }}>
      {children}
    </EditingContext.Provider>
  );
};
