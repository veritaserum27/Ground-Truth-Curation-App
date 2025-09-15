import { useState } from 'react';
import { ChevronDown } from 'lucide-react';
import { useAuth } from '../contexts/AuthContext';

// Mock users for the login dropdown
const mockUsers = [
  { name: 'Alice Smith', email: 'alice@company.com', role: 'Data Curator' },
  { name: 'Bob Johnson', email: 'bob@company.com', role: 'Data Validator' },
  { name: 'Carol Williams', email: 'carol@company.com', role: 'Data Validator' }
];

export const LoginPage = () => {
  const [selectedEmail, setSelectedEmail] = useState('');
  const { login } = useAuth();

  const handleLogin = () => {
    if (selectedEmail) {
      login(selectedEmail);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-muted">
      <div className="w-full max-w-md bg-white rounded-lg shadow border p-6">
        <div className="text-center mb-6">
          <h1 className="mb-2">Ground Truth Management</h1>
          <p className="text-muted-foreground">Sign in with Microsoft Entra</p>
        </div>
        
        <div className="space-y-4">
          <div className="space-y-2">
            <label>Select User (Demo Mode)</label>
            <div className="relative">
              <select 
                value={selectedEmail} 
                onChange={(e) => setSelectedEmail(e.target.value)}
                className="w-full p-3 border rounded-md bg-white appearance-none cursor-pointer pr-10"
              >
                <option value="">Choose a user...</option>
                {mockUsers.map(mockUser => (
                  <option key={mockUser.email} value={mockUser.email}>
                    {mockUser.name} - {mockUser.role}
                  </option>
                ))}
              </select>
              <ChevronDown className="absolute right-3 top-1/2 transform -translate-y-1/2 w-4 h-4 text-muted-foreground pointer-events-none" />
            </div>
          </div>
          
          <button 
            onClick={handleLogin} 
            disabled={!selectedEmail}
            className="w-full p-3 bg-primary text-primary-foreground rounded-md disabled:opacity-50 disabled:cursor-not-allowed hover:bg-primary/90"
          >
            Sign In with Microsoft
          </button>
          
          <div className="text-center text-sm text-muted-foreground">
            <p>Demo Application</p>
            <p>In production, this would redirect to Microsoft Entra</p>
          </div>
        </div>
      </div>
    </div>
  );
};