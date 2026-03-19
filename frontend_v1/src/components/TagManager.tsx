import { Plus, Tag as TagIcon, X } from 'lucide-react';
import { useState } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { useEditingData } from '../contexts/EditingContext';

interface TagManagerProps {
  selectedTags: string[];
  onTagsChange: (tags: string[]) => void;
  disabled?: boolean;
}

// Predefined colors for new tags
const tagColors = [
  'bg-purple-100 text-purple-800',
  'bg-indigo-100 text-indigo-800',
  'bg-pink-100 text-pink-800',
  'bg-orange-100 text-orange-800',
  'bg-teal-100 text-teal-800',
  'bg-cyan-100 text-cyan-800',
  'bg-amber-100 text-amber-800',
  'bg-lime-100 text-lime-800'
];

export const TagManager = ({ selectedTags, onTagsChange, disabled = false }: TagManagerProps) => {
  const { tags, addTag } = useEditingData();
  const { user } = useAuth();
  const [isAddingTag, setIsAddingTag] = useState(false);
  const [newTagName, setNewTagName] = useState('');

  const handleToggleTag = (tagId: string) => {
    if (disabled) return;

    if (selectedTags.includes(tagId)) {
      onTagsChange(selectedTags.filter(id => id !== tagId));
    } else {
      onTagsChange([...selectedTags, tagId]);
    }
  };

  const handleRemoveTag = (tagId: string) => {
    if (disabled) return;
    onTagsChange(selectedTags.filter(id => id !== tagId));
  };

  const handleAddNewTag = () => {
    if (!newTagName.trim() || !user) return;

    // Check if tag already exists
    const existingTag = tags.find(tag => tag.name.toLowerCase() === newTagName.trim().toLowerCase());
    if (existingTag) {
      // If tag exists, just select it
      if (!selectedTags.includes(existingTag.id)) {
        onTagsChange([...selectedTags, existingTag.id]);
      }
      setNewTagName('');
      setIsAddingTag(false);
      return;
    }

    // Create new tag with a random color
    const randomColor = tagColors[Math.floor(Math.random() * tagColors.length)];
    addTag({
      name: newTagName.trim(),
      color: randomColor,
      isPredefined: false,
      createdBy: user.id
    });

    // Find the newly created tag and select it
    // We'll need to wait for the next render cycle, but for simplicity we'll assume the tag ID
    const newTagId = `tag-${Date.now()}`;
    onTagsChange([...selectedTags, newTagId]);

    setNewTagName('');
    setIsAddingTag(false);
  };

  const handleCancelAddTag = () => {
    setNewTagName('');
    setIsAddingTag(false);
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      handleAddNewTag();
    } else if (e.key === 'Escape') {
      handleCancelAddTag();
    }
  };

  // Separate selected and unselected tags
  const selectedTagsData = tags.filter(tag => selectedTags.includes(tag.id));
  const unselectedTags = tags.filter(tag => !selectedTags.includes(tag.id));

  return (
    <div className="space-y-3">
      <div className="flex items-center gap-2">
        <TagIcon className="w-4 h-4 text-muted-foreground" />
        <label>Tags</label>
      </div>

      {/* Selected Tags Section */}
      {selectedTagsData.length > 0 && (
        <div className="space-y-2">
          <div className="text-sm text-muted-foreground">Selected Tags:</div>
          <div className="flex flex-wrap gap-2">
            {selectedTagsData.map(tag => (
              <div
                key={tag.id}
                className={`px-3 py-1 rounded text-sm flex items-center gap-2 ${tag.color} ring-2 ring-primary/20`}
              >
                <span>
                  {tag.name}
                  {tag.isPredefined && (
                    <span className="ml-1 text-xs opacity-60">★</span>
                  )}
                </span>
                {!disabled && (
                  <button
                    onClick={() => handleRemoveTag(tag.id)}
                    className="hover:bg-black/10 rounded-full p-0.5 transition-colors"
                    title={`Remove ${tag.name} tag`}
                  >
                    <X className="w-3 h-3" />
                  </button>
                )}
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Available Tags Section */}
      {!disabled && unselectedTags.length > 0 && (
        <div className="space-y-2">
          <div className="text-sm text-muted-foreground">Available Tags:</div>
          <div className="flex flex-wrap gap-2">
            {unselectedTags.map(tag => (
              <button
                key={tag.id}
                onClick={() => handleToggleTag(tag.id)}
                className={`px-3 py-1 rounded text-sm transition-all cursor-pointer hover:scale-105 ${tag.color} ${tag.color
                  .split(' ')
                  .map(c =>
                    c.startsWith('bg-')
                      ? `hover:${c}`
                      : c.startsWith('text-')
                        ? `hover:${c}`
                        : ''
                  )
                  .join(' ')
                  }`}
              >
                {tag.name}
                {tag.isPredefined && (
                  <span className="ml-1 text-xs opacity-60">★</span>
                )}
              </button>
            ))}
          </div>
        </div>
      )}

      {/* Add New Tag Section */}
      {!disabled && (
        <div className="space-y-2">
          {isAddingTag ? (
            <div className="flex items-center gap-2">
              <input
                type="text"
                value={newTagName}
                onChange={(e) => setNewTagName(e.target.value)}
                onKeyPress={handleKeyPress}
                placeholder="Enter tag name"
                className="px-2 py-1 text-sm border rounded"
                autoFocus
              />
              <button
                onClick={handleAddNewTag}
                disabled={!newTagName.trim()}
                className="px-2 py-1 text-xs bg-primary text-primary-foreground rounded hover:bg-primary/90 disabled:opacity-50"
              >
                Add
              </button>
              <button
                onClick={handleCancelAddTag}
                className="px-2 py-1 text-xs bg-muted hover:bg-muted/80 rounded"
              >
                Cancel
              </button>
            </div>
          ) : (
            <button
              onClick={() => setIsAddingTag(true)}
              className="px-3 py-1 rounded text-sm border-2 border-dashed border-muted-foreground/30 text-muted-foreground hover:border-primary hover:text-primary transition-colors flex items-center gap-1"
            >
              <Plus className="w-3 h-3" />
              Add New Tag
            </button>
          )}
        </div>
      )}

      {/* Summary */}
      {selectedTags.length > 0 && (
        <div className="text-sm text-muted-foreground">
          {selectedTags.length} tag{selectedTags.length !== 1 ? 's' : ''} selected
        </div>
      )}
    </div>
  );
};
