import { FileText, MessageSquare } from "lucide-react";
import type { GroundTruthCategory } from "~/types";

interface CategoryIconProps {
  category: GroundTruthCategory;
}
export function CategoryIcon({ category }: CategoryIconProps) {
  switch (category) {
    case 'asset_knowledge': return <FileText className="w-4 h-4" />;
    case 'unanswerable': return <MessageSquare className="w-4 h-4" />;
    case 'maintenance_request': return <MessageSquare className="w-4 h-4" />;
  }
}
