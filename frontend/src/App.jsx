import { Navigate, Route, Routes } from 'react-router-dom';
import { useAuth } from './auth/AuthContext';
import { Layout } from './components/Layout';
import { Loading, ResourceError } from './components/Feedback';
import { AuthPage } from './pages/AuthPage';
import { DashboardPage } from './pages/DashboardPage';
import { CustomersPage } from './pages/CustomersPage';
import { ItemsPage } from './pages/ItemsPage';
import { OrdersPage } from './pages/OrdersPage';
import { ProfilePage } from './pages/ProfilePage';

export function App() {
  const { user, loading, connectionError, reload } = useAuth();
  if (loading) return <div className="boot"><Loading /></div>;
  if (connectionError) return <div className="boot"><ResourceError error={connectionError} refresh={reload} /></div>;
  if (!user) return <AuthPage />;
  return <Routes><Route element={<Layout />}>
    <Route index element={<DashboardPage />} /><Route path="customers" element={<CustomersPage />} />
    <Route path="items" element={<ItemsPage />} /><Route path="orders" element={<OrdersPage />} />
    <Route path="profile" element={<ProfilePage />} /><Route path="*" element={<Navigate to="/" replace />} />
  </Route></Routes>;
}
