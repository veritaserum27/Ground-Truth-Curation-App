import { Star } from 'lucide-react';
import { useState } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { useEditingData } from '../contexts/EditingContext';

import type { GroundTruth } from '../types';
interface ReviewFormProps { groundTruth: GroundTruth }

export const ReviewForm = ({ groundTruth }: ReviewFormProps) => {
  const [reviewContent, setReviewContent] = useState('');
  const [reviewRating, setReviewRating] = useState(0);
  const { user } = useAuth();
  const { addReview } = useEditingData();

  if (!user || user.role !== 'data_validator') {
    return null;
  }

  const handleSubmitReview = () => {
    if (!reviewContent.trim() || reviewRating === 0 || !user) return;

    const currentResponse = groundTruth.generatedResponses[0];
    addReview(groundTruth.id, {
      reviewerId: user.id,
      reviewerName: user.name,
      content: reviewContent.trim(),
      rating: reviewRating,
      userQueryAtTime: groundTruth.prompt,
      dataStoreTypeAtTime: groundTruth.dataQueryDefinitions[0]?.dataStoreType || 'GraphQL',
      dataQueryDefinitionAtTime: groundTruth.dataQueryDefinitions[0]?.query || '',
      formattedResponseAtTime: currentResponse?.content || 'No response available at time of review'
    });

    setReviewContent('');
    setReviewRating(0);
  };

  const handleClear = () => {
    setReviewContent('');
    setReviewRating(0);
  };

  return (
    <>
      <hr className="my-4 border-border" />
      <div className="space-y-4">
        <h4>Add Your Review</h4>

        <div className="space-y-2">
          <label>Rating</label>
          <div className="flex items-center gap-1">
            {Array.from({ length: 5 }, (_, i) => (
              <Star
                key={i}
                className={`w-6 h-6 cursor-pointer transition-colors ${i < reviewRating
                  ? 'fill-yellow-400 text-yellow-400'
                  : 'text-gray-300 hover:text-yellow-200'
                  }`}
                onClick={() => setReviewRating(i + 1)}
              />
            ))}
            <span className="ml-2 text-sm text-muted-foreground">
              {reviewRating > 0 ? `${reviewRating} star${reviewRating !== 1 ? 's' : ''}` : 'Click to rate'}
            </span>
          </div>
        </div>

        <div className="space-y-2">
          <label>Review Comments</label>
          <textarea
            value={reviewContent}
            onChange={(e) => setReviewContent(e.target.value)}
            placeholder="Share your thoughts on this ground truth..."
            rows={4}
            className="w-full p-3 border rounded-md resize-none"
          />
        </div>

        <div className="flex justify-end gap-2">
          <button
            onClick={handleClear}
            className="px-4 py-2 hover:bg-muted rounded-md"
          >
            Clear
          </button>
          <button
            onClick={handleSubmitReview}
            disabled={!reviewContent.trim() || reviewRating === 0}
            className="px-4 py-2 bg-primary text-primary-foreground rounded-md disabled:opacity-50 disabled:cursor-not-allowed hover:bg-primary/90"
          >
            Submit Review
          </button>
        </div>
      </div>
    </>
  );
};
