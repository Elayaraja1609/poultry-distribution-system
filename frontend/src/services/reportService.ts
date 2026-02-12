import apiClient from './api';

export interface DashboardSummary {
  totalSuppliers: number;
  totalFarms: number;
  totalChickens: number;
  chickensInFarms: number;
  chickensReadyForDistribution: number;
  totalVehicles: number;
  activeDistributions: number;
  pendingDeliveries: number;
  completedDeliveries: number;
  totalSales: number;
  pendingPayments: number;
  totalShops: number;
}

export interface SalesReport {
  reportDate: string;
  totalSales: number;
  totalAmount: number;
  paidAmount: number;
  pendingAmount: number;
  salesByShop: SalesReportItem[];
  salesByDate: SalesReportItem[];
}

export interface SalesReportItem {
  label: string;
  count: number;
  amount: number;
}

export interface ProfitLossReport {
  startDate: string;
  endDate: string;
  totalRevenue: number;
  totalExpenses: number;
  grossProfit: number;
  netProfit: number;
  profitMargin: number;
  revenueBreakdown: RevenueBreakdown;
  expenseBreakdown: ExpenseBreakdown;
}

export interface RevenueBreakdown {
  totalSales: number;
  totalSalesCount: number;
  salesByShop: { [key: string]: number };
  salesByMonth: { [key: string]: number };
}

export interface ExpenseBreakdown {
  totalExpenses: number;
  expensesByCategory: { [key: string]: number };
  expensesByMonth: { [key: string]: number };
}

export interface SalesTrend {
  date: string;
  amount: number;
  count: number;
}

export interface CustomerAnalytics {
  shopId: string;
  shopName: string;
  totalOrders: number;
  totalSpent: number;
  averageOrderValue: number;
  lastOrderDate: string;
  daysSinceLastOrder: number;
}

export interface InventoryAnalytics {
  farmId: string;
  farmName: string;
  currentStock: number;
  capacity: number;
  utilizationRate: number;
  stockIn: number;
  stockOut: number;
  stockLoss: number;
  turnoverRate: number;
}

export interface PerformanceMetrics {
  totalRevenue: number;
  totalExpenses: number;
  netProfit: number;
  profitMargin: number;
  totalOrders: number;
  completedDeliveries: number;
  deliverySuccessRate: number;
  averageOrderValue: number;
  activeCustomers: number;
  customerRetentionRate: number;
  revenueByCategory: { [key: string]: number };
  expensesByCategory: { [key: string]: number };
}

export interface ApiResponse<T> {
  success: boolean;
  message?: string;
  data?: T;
}

class ReportService {
  async getDashboardSummary(): Promise<ApiResponse<DashboardSummary>> {
    const response = await apiClient.get<ApiResponse<DashboardSummary>>('/reports/dashboard');
    return response.data;
  }

  async getSalesReport(startDate?: string, endDate?: string): Promise<ApiResponse<SalesReport>> {
    const params = new URLSearchParams();
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);
    
    const response = await apiClient.get<ApiResponse<SalesReport>>(`/reports/sales?${params.toString()}`);
    return response.data;
  }

  async getProfitLossReport(startDate: string, endDate: string): Promise<ApiResponse<ProfitLossReport>> {
    const params = new URLSearchParams();
    params.append('startDate', startDate);
    params.append('endDate', endDate);
    
    const response = await apiClient.get<ApiResponse<ProfitLossReport>>(`/reports/profit-loss?${params.toString()}`);
    return response.data;
  }

  async getRevenueBreakdown(startDate: string, endDate: string): Promise<ApiResponse<RevenueBreakdown>> {
    const params = new URLSearchParams();
    params.append('startDate', startDate);
    params.append('endDate', endDate);
    
    const response = await apiClient.get<ApiResponse<RevenueBreakdown>>(`/reports/revenue?${params.toString()}`);
    return response.data;
  }

  async getExpenseBreakdown(startDate: string, endDate: string): Promise<ApiResponse<ExpenseBreakdown>> {
    const params = new URLSearchParams();
    params.append('startDate', startDate);
    params.append('endDate', endDate);
    
    const response = await apiClient.get<ApiResponse<ExpenseBreakdown>>(`/reports/expenses?${params.toString()}`);
    return response.data;
  }

  async getSalesTrends(startDate: string, endDate: string): Promise<ApiResponse<SalesTrend[]>> {
    const params = new URLSearchParams();
    params.append('startDate', startDate);
    params.append('endDate', endDate);
    
    const response = await apiClient.get<ApiResponse<SalesTrend[]>>(`/reports/sales-trends?${params.toString()}`);
    return response.data;
  }

  async getCustomerAnalytics(startDate: string, endDate: string): Promise<ApiResponse<CustomerAnalytics[]>> {
    const params = new URLSearchParams();
    params.append('startDate', startDate);
    params.append('endDate', endDate);
    
    const response = await apiClient.get<ApiResponse<CustomerAnalytics[]>>(`/reports/customer-analytics?${params.toString()}`);
    return response.data;
  }

  async getInventoryAnalytics(): Promise<ApiResponse<InventoryAnalytics[]>> {
    const response = await apiClient.get<ApiResponse<InventoryAnalytics[]>>('/reports/inventory-analytics');
    return response.data;
  }

  async getPerformanceMetrics(startDate: string, endDate: string): Promise<ApiResponse<PerformanceMetrics>> {
    const params = new URLSearchParams();
    params.append('startDate', startDate);
    params.append('endDate', endDate);
    
    const response = await apiClient.get<ApiResponse<PerformanceMetrics>>(`/reports/performance-metrics?${params.toString()}`);
    return response.data;
  }
}

export default new ReportService();
