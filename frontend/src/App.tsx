import { Routes, Route, Navigate } from 'react-router-dom';
import { Suspense } from 'react';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import { ErrorBoundary } from './components/common/ErrorBoundary';
import { AppLayout } from './components/layout/AppLayout';
import LoginPage from './pages/auth/LoginPage';
import RegisterPage from './pages/auth/RegisterPage';
import DashboardPage from './pages/dashboard/DashboardPage';
import SearchPage from './pages/search/SearchPage';
import SubscriptionPage from './pages/subscription/SubscriptionPage';
import HistoryPage from './pages/history/HistoryPage';
import ProfilePage from './pages/profile/ProfilePage';
import PetitionHistoryPage from './pages/petition/PetitionHistoryPage';
import { SubscriptionProvider } from './contexts/SubscriptionContext';
import { SearchProvider } from './contexts/SearchContext';

function ProtectedRoute({ children }: { children: React.ReactElement }) {
  const { state } = useAuth();
  if (!state.token) return <Navigate to="/login" replace />;
  return children;
}

export default function App() {
  return (
    <AuthProvider>
      <ErrorBoundary>
      <Suspense fallback={<div className="p-4">Yükleniyor...</div>}>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route
            path="/*"
            element={
              <ProtectedRoute>
                <SubscriptionProvider>
                  <SearchProvider>
                    <AppLayout>
                      <Routes>
                        <Route index element={<DashboardPage />} />
                        <Route path="search" element={<SearchPage />} />
                        <Route path="subscription" element={<SubscriptionPage />} />
                        <Route path="history" element={<HistoryPage />} />
                        <Route path="profile" element={<ProfilePage />} />
                        <Route path="petitions" element={<PetitionHistoryPage />} />
                      </Routes>
                    </AppLayout>
                  </SearchProvider>
                </SubscriptionProvider>
              </ProtectedRoute>
            }
          />
        </Routes>
  </Suspense>
  </ErrorBoundary>
    </AuthProvider>
  );
}
