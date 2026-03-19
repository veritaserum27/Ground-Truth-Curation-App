import { FileText, HandHelping, MessageSquare } from "lucide-react";

interface CategoryIconProps {
  category: string | null | undefined;
}

export function CategoryIcon({ category }: CategoryIconProps) {
  switch (category) {
    case 'asset_knowledge': return <FileText className="w-4 h-4" />;
    case 'unanswerable': return <MessageSquare className="w-4 h-4" />;
    case 'maintenance_request': return <MessageSquare className="w-4 h-4" />;
    case 'Support': return <HandHelping className="w-4 h-4" />;
    default: return <FileText className="w-4 h-4" />;
  }
}
