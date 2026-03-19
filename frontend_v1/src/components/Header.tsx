import { LogOut, User } from 'lucide-react';
import { useAuth } from '../contexts/AuthContext';

export const Header = () => {
  const { user, logout } = useAuth();

  if (!user) return null;

  return (
    <header className="border-b bg-background">
      <div className="container mx-auto px-4 py-4 flex items-center justify-between">
        <div>
          <h1>Ground Truth Management</h1>
          <p className="text-muted-foreground">Q&A Pairs Curation System</p>
        </div>

        <div className="flex items-center gap-4">
          <div className="flex items-center gap-2">
            <User className="w-4 h-4" />
            <span>{user.name}</span>
            <span className={`px-2 py-1 rounded text-sm ${user.role === 'data_curator' ? 'bg-primary text-primary-foreground' : 'bg-secondary text-secondary-foreground'}`}>
              {user.role === 'data_curator' ? 'Data Curator' : 'Data Validator'}
            </span>
          </div>

          <button
            onClick={logout}
            className="flex items-center gap-2 px-3 py-2 text-sm hover:bg-muted rounded-md"
          >
            <LogOut className="w-4 h-4" />
            Logout
          </button>
        </div>
      </div>
    </header>
  );
};
