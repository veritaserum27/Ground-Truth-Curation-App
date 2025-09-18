import { CategoryIcon } from '@/components/ui/categoryIcon';
import { getStatusColorClass } from '@/utils/groundTruthFormatting';
import { NavLink, Outlet, useLoaderData, useNavigate, useParams } from 'react-router';
import { useAuth } from '../contexts/AuthContext';
import { groundTruthDetailLoader as clientLoader } from '../routes/loaders/groundTruthDetailLoader';
import type { GroundTruthDefinition } from '../services/schemas';

export { clientLoader };

// Formatting helpers (will later be moved to a shared util if reused elsewhere)
const formatCategory = (category: string) => category?.replace(/_/g, ' ').replace(/\b\w/g, l => l.toUpperCase());
const formatStatus = (status: string) => status?.replace(/_/g, ' ').replace(/\b\w/g, l => l.toUpperCase());

export default function GroundTruthDetailLayout() {
  const { groundTruth } = useLoaderData() as { groundTruth: GroundTruthDefinition };
  const { user } = useAuth();
  const navigate = useNavigate();
  const { id } = useParams();
  const canEdit = user?.role === 'data_curator' || user?.role === 'data_validator';
  const isOnEdit = location.pathname.endsWith('/edit');

  if (!groundTruth) {
    return (
      <div className="text-center py-8 space-y-4">
        <p className="text-muted-foreground">Ground truth not found.</p>
        <NavLink to="/ground-truths" className="px-4 py-2 bg-primary text-primary-foreground rounded-md">Back to List</NavLink>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <NavLink to="/ground-truths" className="text-sm text-primary hover:underline">&larr; Back to List</NavLink>

        {canEdit && (
          <div className="flex gap-2">
            {!isOnEdit && (
              <button
                onClick={() => navigate(`/ground-truths/${id}/edit`)}
                className="px-4 py-2 bg-primary text-primary-foreground rounded-md hover:bg-primary/90"
              >Edit</button>
            )}
            {isOnEdit && (
              <button
                onClick={() => navigate(`/ground-truths/${id}`)}
                className="px-4 py-2 border rounded-md hover:bg-muted"
              >Cancel</button>
            )}
          </div>
        )}
      </div>
      {/* Main Content */}
      <section className='bg-white rounded-lg border shadow p-6 space-y-6'>
        <div className="flex items-center justify-between p-6 border-b">
          <div className="space-y-2">
            <div className="flex items-center gap-2">
              <CategoryIcon category={groundTruth.Category} />
              <span className="text-sm text-muted-foreground">
                {groundTruth.Category}
              </span>
            </div>
            <h2 className="text-lg font-semibold">Ground Truth Details</h2>
          </div>
          <span
            className={`px-3 py-2 rounded-lg text-md align-center ${getStatusColorClass(groundTruth.ValidationStatus)}`}
          >
            {groundTruth.ValidationStatus}
          </span>
        </div>

        <Outlet context={{ groundTruth }} />
      </section>
    </div>
  );
}
