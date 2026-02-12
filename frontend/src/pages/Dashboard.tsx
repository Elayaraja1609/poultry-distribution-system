import React, { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import reportService, { DashboardSummary } from '../services/reportService';
import { LineChart, Line, BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts';
import LoadingSpinner from '../components/LoadingSpinner';
import ErrorAlert from '../components/ErrorAlert';
import './Dashboard.css';

const Dashboard: React.FC = () => {
  const { user } = useAuth();
  const [dashboardSummary, setDashboardSummary] = useState<DashboardSummary | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    loadDashboardSummary();
  }, []);

  const loadDashboardSummary = async () => {
    try {
      setLoading(true);
      setError('');
      const response = await reportService.getDashboardSummary();
      if (response.success && response.data) {
        setDashboardSummary(response.data);
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load dashboard summary');
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return <LoadingSpinner />;
  }

  // Mock sales trend data for chart
  const salesTrendData = [
    { month: 'Jan', revenue: 35000, orders: 120 },
    { month: 'Feb', revenue: 38000, orders: 135 },
    { month: 'Mar', revenue: 42000, orders: 150 },
    { month: 'Apr', revenue: 40000, orders: 142 },
    { month: 'May', revenue: 45000, orders: 165 },
    { month: 'Jun', revenue: 48000, orders: 180 },
  ];

  // Mock inventory by farm data
  const inventoryByFarm = [
    { name: 'Green Valley Alpha', percentage: 89 },
    { name: 'Oak Ridge Beta', percentage: 93 },
    { name: 'Sunset Farm', percentage: 76 },
    { name: 'Mountain View', percentage: 82 },
  ];

  // Mock recent orders
  const recentOrders = [
    { id: '#ORD-4801', customer: 'Green Grocers Inc.', amount: 2900.00, status: 'DELIVERED', date: 'Oct 24, 2024' },
    { id: '#ORD-4802', customer: 'Fresh Market Co.', amount: 1850.00, status: 'PROCESSING', date: 'Oct 24, 2024' },
    { id: '#ORD-4803', customer: 'City Poultry Shop', amount: 3200.00, status: 'SHIPPED', date: 'Oct 23, 2024' },
    { id: '#ORD-4804', customer: 'Village Store', amount: 1500.00, status: 'DELIVERED', date: 'Oct 23, 2024' },
  ];

  // Mock upcoming deliveries
  const upcomingDeliveries = [
    { route: 'RT-2024-001', driver: 'John Smith', status: 'In Transit', time: '10:45 AM' },
    { route: 'RT-2024-002', driver: 'Mike Johnson', status: 'Loading', time: '11:30 AM' },
    { route: 'RT-2024-003', driver: 'David Lee', status: 'Scheduled', time: '2:15 PM' },
  ];

  const getStatusBadgeClass = (status: string) => {
    const statusMap: { [key: string]: string } = {
      'DELIVERED': 'success',
      'PROCESSING': 'warning',
      'SHIPPED': 'info',
      'PENDING': 'pending',
    };
    return statusMap[status] || 'pending';
  };

  return (
    <div className="dashboard">
      {error && <ErrorAlert message={error} onClose={() => setError('')} />}

      <div className="dashboard-header">
        <h1 className="dashboard-title">Admin Dashboard</h1>
        <p className="dashboard-subtitle">Welcome back, {user?.fullName || user?.username}. Here's your overview for today.</p>
      </div>

      {/* Metric Cards */}
      <div className="metrics-grid">
        <div className="metric-card primary">
          <div className="metric-card-header">
            <div className="metric-title">Total Stock</div>
            <div className="metric-icon">📦</div>
          </div>
          <div className="metric-value">
            {dashboardSummary?.totalChickens?.toLocaleString() || '0'} units
          </div>
          <div className="metric-change positive">
            <span className="metric-change-icon">▲</span>
            <span>+5.2% from last month</span>
          </div>
        </div>

        <div className="metric-card">
          <div className="metric-card-header">
            <div className="metric-title">Active Orders</div>
            <div className="metric-icon">📋</div>
          </div>
          <div className="metric-value">{dashboardSummary?.activeDistributions || 0}</div>
          <div className="metric-change negative">
            <span className="metric-change-icon">▼</span>
            <span>-1.5% from yesterday</span>
          </div>
        </div>

        <div className="metric-card">
          <div className="metric-card-header">
            <div className="metric-title">Today Deliveries</div>
            <div className="metric-icon">🚚</div>
          </div>
          <div className="metric-value">{dashboardSummary?.pendingDeliveries || 0}</div>
          <div className="metric-change negative">
            <span className="metric-change-icon">▼</span>
            <span>-0.4% from average</span>
          </div>
        </div>

        <div className="metric-card">
          <div className="metric-card-header">
            <div className="metric-title">Monthly Sales</div>
            <div className="metric-icon">💰</div>
          </div>
          <div className="metric-value">${dashboardSummary?.totalSales?.toFixed(2) || '0.00'}</div>
          <div className="metric-change positive">
            <span className="metric-change-icon">▲</span>
            <span>+8.3% from last month</span>
          </div>
        </div>

        <div className="metric-card">
          <div className="metric-card-header">
            <div className="metric-title">Gross Profit</div>
            <div className="metric-icon">💵</div>
          </div>
          <div className="metric-value">${((dashboardSummary?.totalSales || 0) * 0.3).toFixed(2)}</div>
          <div className="metric-change negative">
            <span className="metric-change-icon">▼</span>
            <span>-0.2% variance</span>
          </div>
        </div>
      </div>

      {/* Content Grid */}
      <div className="content-grid">
        {/* Sales Trends Chart */}
        <div className="chart-card">
          <div className="chart-header">
            <div>
              <h3 className="chart-title">Sales Trends</h3>
              <p className="chart-subtitle">Revenue performance over the last 6 months</p>
            </div>
            <select className="chart-filter">
              <option>Monthly</option>
              <option>Weekly</option>
              <option>Daily</option>
            </select>
          </div>
          <div className="chart-body">
            <ResponsiveContainer width="100%" height={300}>
              <LineChart data={salesTrendData}>
                <CartesianGrid strokeDasharray="3 3" stroke="#E5E7EB" />
                <XAxis dataKey="month" stroke="#6B7280" />
                <YAxis stroke="#6B7280" />
                <Tooltip
                  contentStyle={{
                    backgroundColor: '#FFFFFF',
                    border: '1px solid #E5E7EB',
                    borderRadius: '8px',
                  }}
                />
                <Legend />
                <Line
                  type="monotone"
                  dataKey="revenue"
                  stroke="#10B981"
                  strokeWidth={2}
                  name="Revenue ($)"
                  dot={{ fill: '#10B981', r: 4 }}
                />
                <Line
                  type="monotone"
                  dataKey="orders"
                  stroke="#667EEA"
                  strokeWidth={2}
                  name="Orders"
                  dot={{ fill: '#667EEA', r: 4 }}
                />
              </LineChart>
            </ResponsiveContainer>
          </div>
        </div>

        {/* Inventory by Farm */}
        <div className="table-card">
          <div className="table-header">
            <h3 className="table-title">Inventory by Farm</h3>
            <a href="/inventory" className="table-link">View Full Report</a>
          </div>
          <div>
            {inventoryByFarm.map((farm, index) => (
              <div key={index} className="progress-item">
                <div className="progress-label">
                  <span>{farm.name}</span>
                  <span>{farm.percentage}%</span>
                </div>
                <div className="progress-bar-container">
                  <div
                    className="progress-bar"
                    style={{ width: `${farm.percentage}%` }}
                  />
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Bottom Grid */}
      <div className="content-grid">
        {/* Recent Orders Table */}
        <div className="table-card">
          <div className="table-header">
            <h3 className="table-title">Recent Orders</h3>
            <a href="/orders" className="table-link">See All</a>
          </div>
          <div className="table-container">
            <table>
              <thead>
                <tr>
                  <th>Order ID</th>
                  <th>Customer</th>
                  <th>Amount</th>
                  <th>Status</th>
                  <th>Date</th>
                </tr>
              </thead>
              <tbody>
                {recentOrders.map((order) => (
                  <tr key={order.id}>
                    <td>
                      <span style={{ color: '#10B981', fontWeight: 600 }}>{order.id}</span>
                    </td>
                    <td>{order.customer}</td>
                    <td>${order.amount.toFixed(2)}</td>
                    <td>
                      <span className={`status-badge ${getStatusBadgeClass(order.status)}`}>
                        {order.status}
                      </span>
                    </td>
                    <td>{order.date}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        {/* Upcoming Deliveries */}
        <div className="table-card">
          <div className="table-header">
            <h3 className="table-title">Upcoming Deliveries</h3>
          </div>
          <div>
            {upcomingDeliveries.map((delivery, index) => (
              <div key={index} className="activity-item">
                <div className="activity-icon info">🚚</div>
                <div className="activity-content">
                  <div className="activity-title">{delivery.route}</div>
                  <div className="activity-description">{delivery.driver}</div>
                  <div className="activity-time">
                    {delivery.status} • {delivery.time}
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;
