import { AuthProvider } from './hooks/useAuth';
import { ProtectedRoute } from './components/layout/ProtectedRoute';
import { Dashboard } from './components/dashboard/Dashboard';
import { Toaster } from './components/ui/toaster';
import './globals.css';

function App() {
  return (
    <AuthProvider>
      <div className="App">
        <ProtectedRoute>
          <Dashboard />
        </ProtectedRoute>
        <Toaster />
      </div>
    </AuthProvider>
  );
}

export default App;
