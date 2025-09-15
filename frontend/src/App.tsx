import { AuthProvider, useAuth } from './contexts/AuthContext';
import { DataProvider } from './contexts/DataContext';
import { LoginPage } from './components/LoginPage';
import { Header } from './components/Header';
import { GroundTruthList } from './components/GroundTruthList';
import { GroundTruthDetail } from './components/GroundTruthDetail';
import { useData } from './contexts/DataContext';

function AppContent() {
  const { isAuthenticated } = useAuth();
  const { selectedGroundTruthId } = useData();

  if (!isAuthenticated) {
    return <LoginPage />;
  }

  return (
    <div className="min-h-screen bg-background">
      <Header />
      <main className="container mx-auto px-4 py-8">
        {selectedGroundTruthId ? <GroundTruthDetail /> : <GroundTruthList />}
      </main>
    </div>
  );
}

export default function App() {
  return (
    <AuthProvider>
      <DataProvider>
        <AppContent />
      </DataProvider>
    </AuthProvider>
  );
}