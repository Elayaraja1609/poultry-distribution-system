import React, { useState, useEffect } from 'react';
import reportService, { DashboardSummary, SalesReport, ProfitLossReport, SalesTrend, CustomerAnalytics, InventoryAnalytics, PerformanceMetrics } from '../services/reportService';
import LoadingSpinner from '../components/LoadingSpinner';
import ErrorAlert from '../components/ErrorAlert';
import { LineChart, Line, BarChart, Bar, PieChart, Pie, Cell, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts';
import './Reports.css';

const Reports: React.FC = () => {
  const [dashboardSummary, setDashboardSummary] = useState<DashboardSummary | null>(null);
  const [salesReport, setSalesReport] = useState<SalesReport | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  
  // Profit & Loss Report states
  const [plStartDate, setPlStartDate] = useState('');
  const [plEndDate, setPlEndDate] = useState('');
  const [profitLossReport, setProfitLossReport] = useState<ProfitLossReport | null>(null);
  
  // Analytics states
  const [analyticsStartDate, setAnalyticsStartDate] = useState('');
  const [analyticsEndDate, setAnalyticsEndDate] = useState('');
  const [salesTrends, setSalesTrends] = useState<SalesTrend[]>([]);
  const [customerAnalytics, setCustomerAnalytics] = useState<CustomerAnalytics[]>([]);
  const [performanceMetrics, setPerformanceMetrics] = useState<PerformanceMetrics | null>(null);
  const [inventoryAnalytics, setInventoryAnalytics] = useState<InventoryAnalytics[]>([]);

  useEffect(() => {
    loadDashboardSummary();
  }, []);

  useEffect(() => {
    if (startDate && endDate) {
      loadSalesReport();
    }
  }, [startDate, endDate]);

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

  const loadSalesReport = async () => {
    try {
      setError('');
      const response = await reportService.getSalesReport(startDate, endDate);
      if (response.success && response.data) {
        setSalesReport(response.data);
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load sales report');
    }
  };

  const handleDateRangeChange = () => {
    if (startDate && endDate && new Date(startDate) <= new Date(endDate)) {
      loadSalesReport();
    } else if (startDate && endDate) {
      setError('Start date must be before end date');
    }
  };

  const loadProfitLossReport = async () => {
    try {
      setError('');
      const response = await reportService.getProfitLossReport(plStartDate, plEndDate);
      if (response.success && response.data) {
        setProfitLossReport(response.data);
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load profit & loss report');
    }
  };

  const handlePlDateRangeChange = () => {
    if (plStartDate && plEndDate && new Date(plStartDate) <= new Date(plEndDate)) {
      loadProfitLossReport();
    } else if (plStartDate && plEndDate) {
      setError('Start date must be before end date');
    }
  };

  const loadAnalytics = async () => {
    try {
      setError('');
      const [trendsRes, customersRes, metricsRes] = await Promise.all([
        reportService.getSalesTrends(analyticsStartDate, analyticsEndDate),
        reportService.getCustomerAnalytics(analyticsStartDate, analyticsEndDate),
        reportService.getPerformanceMetrics(analyticsStartDate, analyticsEndDate)
      ]);
      
      if (trendsRes.success && trendsRes.data) {
        setSalesTrends(trendsRes.data);
      }
      if (customersRes.success && customersRes.data) {
        setCustomerAnalytics(customersRes.data);
      }
      if (metricsRes.success && metricsRes.data) {
        setPerformanceMetrics(metricsRes.data);
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load analytics');
    }
  };

  const loadInventoryAnalytics = async () => {
    try {
      const response = await reportService.getInventoryAnalytics();
      if (response.success && response.data) {
        setInventoryAnalytics(response.data);
      }
    } catch (err: any) {
      console.error('Failed to load inventory analytics:', err);
    }
  };

  const COLORS = ['#667eea', '#f093fb', '#4facfe', '#00f2fe', '#43e97b', '#fa709a'];

  if (loading && !dashboardSummary) {
    return <LoadingSpinner />;
  }

  return (
    <div className="reports-page">
      <div className="page-header">
        <h1>Reports & Analytics</h1>
      </div>

      {error && <ErrorAlert message={error} onClose={() => setError('')} />}

      {dashboardSummary && (
        <div className="dashboard-summary">
          <h2>Dashboard Summary</h2>
          <div className="summary-grid">
            <div className="summary-card">
              <h3>Suppliers</h3>
              <p className="summary-value">{dashboardSummary.totalSuppliers}</p>
            </div>
            <div className="summary-card">
              <h3>Farms</h3>
              <p className="summary-value">{dashboardSummary.totalFarms}</p>
            </div>
            <div className="summary-card">
              <h3>Total Chickens</h3>
              <p className="summary-value">{dashboardSummary.totalChickens}</p>
            </div>
            <div className="summary-card">
              <h3>In Farms</h3>
              <p className="summary-value">{dashboardSummary.chickensInFarms}</p>
            </div>
            <div className="summary-card">
              <h3>Ready for Distribution</h3>
              <p className="summary-value">{dashboardSummary.chickensReadyForDistribution}</p>
            </div>
            <div className="summary-card">
              <h3>Vehicles</h3>
              <p className="summary-value">{dashboardSummary.totalVehicles}</p>
            </div>
            <div className="summary-card">
              <h3>Active Distributions</h3>
              <p className="summary-value">{dashboardSummary.activeDistributions}</p>
            </div>
            <div className="summary-card">
              <h3>Pending Deliveries</h3>
              <p className="summary-value">{dashboardSummary.pendingDeliveries}</p>
            </div>
            <div className="summary-card">
              <h3>Completed Deliveries</h3>
              <p className="summary-value">{dashboardSummary.completedDeliveries}</p>
            </div>
            <div className="summary-card">
              <h3>Total Sales</h3>
              <p className="summary-value">{dashboardSummary.totalSales}</p>
            </div>
            <div className="summary-card">
              <h3>Pending Payments</h3>
              <p className="summary-value">${dashboardSummary.pendingPayments.toFixed(2)}</p>
            </div>
            <div className="summary-card">
              <h3>Shops</h3>
              <p className="summary-value">{dashboardSummary.totalShops}</p>
            </div>
          </div>
        </div>
      )}

      <div className="sales-report-section">
        <h2>Sales Report</h2>
        <div className="date-range-selector">
          <div className="form-group">
            <label>Start Date</label>
            <input
              type="date"
              value={startDate}
              onChange={(e) => setStartDate(e.target.value)}
              onBlur={handleDateRangeChange}
            />
          </div>
          <div className="form-group">
            <label>End Date</label>
            <input
              type="date"
              value={endDate}
              onChange={(e) => setEndDate(e.target.value)}
              onBlur={handleDateRangeChange}
            />
          </div>
          <button className="btn-primary" onClick={handleDateRangeChange}>
            Generate Report
          </button>
        </div>

        {salesReport && (
          <div className="sales-report-content">
            <div className="report-summary">
              <div className="report-summary-item">
                <h3>Total Sales</h3>
                <p>${salesReport.totalAmount.toFixed(2)}</p>
              </div>
              <div className="report-summary-item">
                <h3>Paid Amount</h3>
                <p>${salesReport.paidAmount.toFixed(2)}</p>
              </div>
              <div className="report-summary-item">
                <h3>Pending Amount</h3>
                <p>${salesReport.pendingAmount.toFixed(2)}</p>
              </div>
            </div>

            {salesReport.salesByShop.length > 0 && (
              <div className="report-table-section">
                <h3>Sales by Shop</h3>
                <table className="report-table">
                  <thead>
                    <tr>
                      <th>Shop</th>
                      <th>Sales Count</th>
                      <th>Total Amount</th>
                    </tr>
                  </thead>
                  <tbody>
                    {salesReport.salesByShop.map((item, index) => (
                      <tr key={index}>
                        <td>{item.label}</td>
                        <td>{item.count}</td>
                        <td>${item.amount.toFixed(2)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}

            {salesReport.salesByDate.length > 0 && (
              <div className="report-table-section">
                <h3>Sales by Date</h3>
                <table className="report-table">
                  <thead>
                    <tr>
                      <th>Date</th>
                      <th>Sales Count</th>
                      <th>Total Amount</th>
                    </tr>
                  </thead>
                  <tbody>
                    {salesReport.salesByDate.map((item, index) => (
                      <tr key={index}>
                        <td>{item.label}</td>
                        <td>{item.count}</td>
                        <td>${item.amount.toFixed(2)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        )}
      </div>

      <div className="profit-loss-section">
        <h2>Profit & Loss Report</h2>
        <div className="date-range-selector">
          <div className="form-group">
            <label>Start Date</label>
            <input
              type="date"
              value={plStartDate}
              onChange={(e) => setPlStartDate(e.target.value)}
              onBlur={handlePlDateRangeChange}
            />
          </div>
          <div className="form-group">
            <label>End Date</label>
            <input
              type="date"
              value={plEndDate}
              onChange={(e) => setPlEndDate(e.target.value)}
              onBlur={handlePlDateRangeChange}
            />
          </div>
          <button className="btn-primary" onClick={handlePlDateRangeChange}>
            Generate P&L Report
          </button>
        </div>

        {profitLossReport && (
          <div className="pl-report-content">
            <div className="pl-summary">
              <div className="pl-summary-card revenue">
                <h3>Total Revenue</h3>
                <p className="pl-value">${profitLossReport.totalRevenue.toFixed(2)}</p>
              </div>
              <div className="pl-summary-card expense">
                <h3>Total Expenses</h3>
                <p className="pl-value">${profitLossReport.totalExpenses.toFixed(2)}</p>
              </div>
              <div className="pl-summary-card profit">
                <h3>Net Profit</h3>
                <p className={`pl-value ${profitLossReport.netProfit >= 0 ? 'positive' : 'negative'}`}>
                  ${profitLossReport.netProfit.toFixed(2)}
                </p>
              </div>
              <div className="pl-summary-card margin">
                <h3>Profit Margin</h3>
                <p className={`pl-value ${profitLossReport.profitMargin >= 0 ? 'positive' : 'negative'}`}>
                  {profitLossReport.profitMargin.toFixed(2)}%
                </p>
              </div>
            </div>

            <div className="pl-breakdown">
              <div className="breakdown-section">
                <h3>Revenue Breakdown</h3>
                <p>Total Sales: ${profitLossReport.revenueBreakdown.totalSales.toFixed(2)}</p>
                <p>Number of Sales: {profitLossReport.revenueBreakdown.totalSalesCount}</p>
                
                {Object.keys(profitLossReport.revenueBreakdown.salesByShop).length > 0 && (
                  <div className="breakdown-table">
                    <h4>Sales by Shop</h4>
                    <table className="report-table">
                      <thead>
                        <tr>
                          <th>Shop</th>
                          <th>Amount</th>
                        </tr>
                      </thead>
                      <tbody>
                        {Object.entries(profitLossReport.revenueBreakdown.salesByShop).map(([shop, amount]) => (
                          <tr key={shop}>
                            <td>{shop}</td>
                            <td>${(amount as number).toFixed(2)}</td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                )}
              </div>

              <div className="breakdown-section">
                <h3>Expense Breakdown</h3>
                <p>Total Expenses: ${profitLossReport.expenseBreakdown.totalExpenses.toFixed(2)}</p>
                
                {Object.keys(profitLossReport.expenseBreakdown.expensesByCategory).length > 0 && (
                  <div className="breakdown-table">
                    <h4>Expenses by Category</h4>
                    <table className="report-table">
                      <thead>
                        <tr>
                          <th>Category</th>
                          <th>Amount</th>
                        </tr>
                      </thead>
                      <tbody>
                        {Object.entries(profitLossReport.expenseBreakdown.expensesByCategory).map(([category, amount]) => (
                          <tr key={category}>
                            <td>{category}</td>
                            <td>${amount.toFixed(2)}</td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                )}
              </div>
            </div>
          </div>
        )}
      </div>

      <div className="analytics-section">
        <h2>Advanced Analytics</h2>
        <div className="date-range-selector">
          <div className="form-group">
            <label>Start Date</label>
            <input
              type="date"
              value={analyticsStartDate}
              onChange={(e) => setAnalyticsStartDate(e.target.value)}
            />
          </div>
          <div className="form-group">
            <label>End Date</label>
            <input
              type="date"
              value={analyticsEndDate}
              onChange={(e) => setAnalyticsEndDate(e.target.value)}
            />
          </div>
          <button className="btn-primary" onClick={loadAnalytics}>
            Load Analytics
          </button>
        </div>

        {performanceMetrics && (
          <div className="metrics-grid">
            <div className="metric-card">
              <h3>Total Revenue</h3>
              <p className="metric-value">${performanceMetrics.totalRevenue.toFixed(2)}</p>
            </div>
            <div className="metric-card">
              <h3>Net Profit</h3>
              <p className={`metric-value ${performanceMetrics.netProfit >= 0 ? 'positive' : 'negative'}`}>
                ${performanceMetrics.netProfit.toFixed(2)}
              </p>
            </div>
            <div className="metric-card">
              <h3>Profit Margin</h3>
              <p className={`metric-value ${performanceMetrics.profitMargin >= 0 ? 'positive' : 'negative'}`}>
                {performanceMetrics.profitMargin.toFixed(2)}%
              </p>
            </div>
            <div className="metric-card">
              <h3>Average Order Value</h3>
              <p className="metric-value">${performanceMetrics.averageOrderValue.toFixed(2)}</p>
            </div>
            <div className="metric-card">
              <h3>Delivery Success Rate</h3>
              <p className="metric-value">{performanceMetrics.deliverySuccessRate.toFixed(1)}%</p>
            </div>
            <div className="metric-card">
              <h3>Active Customers</h3>
              <p className="metric-value">{performanceMetrics.activeCustomers}</p>
            </div>
          </div>
        )}

        {salesTrends.length > 0 && (
          <div className="chart-section">
            <h3>Sales Trends</h3>
            <ResponsiveContainer width="100%" height={300}>
              <LineChart data={salesTrends}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="date" />
                <YAxis />
                <Tooltip />
                <Legend />
                <Line type="monotone" dataKey="amount" stroke="#667eea" name="Revenue ($)" />
                <Line type="monotone" dataKey="count" stroke="#f093fb" name="Sales Count" />
              </LineChart>
            </ResponsiveContainer>
          </div>
        )}

        {customerAnalytics.length > 0 && (
          <div className="chart-section">
            <h3>Top Customers</h3>
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={customerAnalytics.slice(0, 10)}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="shopName" />
                <YAxis />
                <Tooltip />
                <Legend />
                <Bar dataKey="totalSpent" fill="#667eea" name="Total Spent ($)" />
              </BarChart>
            </ResponsiveContainer>
          </div>
        )}

        {inventoryAnalytics.length > 0 && (
          <div className="chart-section">
            <h3>Inventory Utilization</h3>
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={inventoryAnalytics}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="farmName" />
                <YAxis />
                <Tooltip />
                <Legend />
                <Bar dataKey="utilizationRate" fill="#4facfe" name="Utilization (%)" />
                <Bar dataKey="turnoverRate" fill="#43e97b" name="Turnover (%)" />
              </BarChart>
            </ResponsiveContainer>
          </div>
        )}

        {performanceMetrics && Object.keys(performanceMetrics.expensesByCategory).length > 0 && (
          <div className="chart-section">
            <h3>Expenses by Category</h3>
            <ResponsiveContainer width="100%" height={300}>
              <PieChart>
                <Pie
                  data={Object.entries(performanceMetrics.expensesByCategory).map(([name, value]) => ({ name, value }))}
                  cx="50%"
                  cy="50%"
                  labelLine={false}
                  label={({ name, percent }) => `${name} ${((percent ?? 0) * 100).toFixed(0)}%`}
                  outerRadius={80}
                  fill="#8884d8"
                  dataKey="value"
                >
                  {Object.entries(performanceMetrics.expensesByCategory).map((entry, index) => (
                    <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                  ))}
                </Pie>
                <Tooltip />
              </PieChart>
            </ResponsiveContainer>
          </div>
        )}
      </div>
    </div>
  );
};

export default Reports;
