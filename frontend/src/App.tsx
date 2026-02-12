import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './contexts/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import RoleBasedRoute from './components/RoleBasedRoute';
import Layout from './components/Layout';
import Login from './pages/Login';
import Register from './pages/Register';
import Dashboard from './pages/Dashboard';
import Suppliers from './pages/Suppliers';
import Farms from './pages/Farms';
import Chickens from './pages/Chickens';
import Vehicles from './pages/Vehicles';
import Distributions from './pages/Distributions';
import Deliveries from './pages/Deliveries';
import Sales from './pages/Sales';
import Reports from './pages/Reports';
import Inventory from './pages/Inventory';
import StockMovements from './pages/StockMovements';
import Orders from './pages/Orders';
import Notifications from './pages/Notifications';
import Expenses from './pages/Expenses';
import Payments from './pages/Payments';
import Profile from './pages/Profile';
import './App.css';

/** Wraps a route with Layout and optional role check. */
function AppRoute({
  children,
  allowedRoles,
}: {
  children: React.ReactNode;
  allowedRoles?: string[];
}) {
  return (
    <ProtectedRoute>
      {allowedRoles && allowedRoles.length > 0 ? (
        <RoleBasedRoute allowedRoles={allowedRoles}>
          <Layout>{children}</Layout>
        </RoleBasedRoute>
      ) : (
        <Layout>{children}</Layout>
      )}
    </ProtectedRoute>
  );
}

const App: React.FC = () => {
  return (
    <AuthProvider>
      <Router>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />

          {/* Shared: all authenticated */}
          <Route
            path="/dashboard"
            element={
              <AppRoute>
                <Dashboard />
              </AppRoute>
            }
          />
          <Route
            path="/notifications"
            element={
              <AppRoute>
                <Notifications />
              </AppRoute>
            }
          />
          <Route
            path="/profile"
            element={
              <AppRoute>
                <Profile />
              </AppRoute>
            }
          />

          {/* Admin + ShopOwner */}
          <Route
            path="/orders"
            element={
              <AppRoute allowedRoles={['Admin', 'ShopOwner']}>
                <Orders />
              </AppRoute>
            }
          />
          <Route
            path="/deliveries"
            element={
              <AppRoute allowedRoles={['Admin', 'Driver', 'ShopOwner']}>
                <Deliveries />
              </AppRoute>
            }
          />
          <Route
            path="/sales"
            element={
              <AppRoute allowedRoles={['Admin', 'ShopOwner']}>
                <Sales />
              </AppRoute>
            }
          />
          <Route
            path="/payments"
            element={
              <AppRoute allowedRoles={['Admin', 'ShopOwner']}>
                <Payments />
              </AppRoute>
            }
          />

          {/* Admin + Driver */}
          <Route
            path="/distributions"
            element={
              <AppRoute allowedRoles={['Admin', 'Driver']}>
                <Distributions />
              </AppRoute>
            }
          />

          {/* Admin + FarmManager */}
          <Route
            path="/suppliers"
            element={
              <AppRoute allowedRoles={['Admin', 'FarmManager']}>
                <Suppliers />
              </AppRoute>
            }
          />
          <Route
            path="/farms"
            element={
              <AppRoute allowedRoles={['Admin', 'FarmManager']}>
                <Farms />
              </AppRoute>
            }
          />
          <Route
            path="/chickens"
            element={
              <AppRoute allowedRoles={['Admin', 'FarmManager']}>
                <Chickens />
              </AppRoute>
            }
          />
          <Route
            path="/inventory"
            element={
              <AppRoute allowedRoles={['Admin', 'FarmManager']}>
                <Inventory />
              </AppRoute>
            }
          />
          <Route
            path="/stock-movements"
            element={
              <AppRoute allowedRoles={['Admin', 'FarmManager']}>
                <StockMovements />
              </AppRoute>
            }
          />

          {/* Admin only */}
          <Route
            path="/vehicles"
            element={
              <AppRoute allowedRoles={['Admin']}>
                <Vehicles />
              </AppRoute>
            }
          />
          <Route
            path="/reports"
            element={
              <AppRoute allowedRoles={['Admin']}>
                <Reports />
              </AppRoute>
            }
          />
          <Route
            path="/expenses"
            element={
              <AppRoute allowedRoles={['Admin']}>
                <Expenses />
              </AppRoute>
            }
          />

          <Route path="/" element={<Navigate to="/dashboard" replace />} />
        </Routes>
      </Router>
    </AuthProvider>
  );
};

export default App;
