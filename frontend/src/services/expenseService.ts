import apiClient from './api';

export interface Expense {
  id: string;
  expenseType: string;
  category: string;
  amount: number;
  description: string;
  expenseDate: string;
  vehicleId?: string;
  vehicleNumber?: string;
  farmId?: string;
  farmName?: string;
  receiptPath?: string;
  createdAt: string;
}

export interface CreateExpenseDto {
  expenseType: string;
  category: string;
  amount: number;
  description: string;
  expenseDate: string;
  vehicleId?: string;
  farmId?: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface ApiResponse<T> {
  success: boolean;
  message?: string;
  data?: T;
}

class ExpenseService {
  async getAll(pageNumber: number = 1, pageSize: number = 10): Promise<ApiResponse<PagedResult<Expense>>> {
    const response = await apiClient.get<ApiResponse<PagedResult<Expense>>>(
      `/expenses?pageNumber=${pageNumber}&pageSize=${pageSize}`
    );
    return response.data;
  }

  async getById(id: string): Promise<ApiResponse<Expense>> {
    const response = await apiClient.get<ApiResponse<Expense>>(`/expenses/${id}`);
    return response.data;
  }

  async getByCategory(category: string, pageNumber: number = 1, pageSize: number = 10): Promise<ApiResponse<PagedResult<Expense>>> {
    const response = await apiClient.get<ApiResponse<PagedResult<Expense>>>(
      `/expenses/category/${category}?pageNumber=${pageNumber}&pageSize=${pageSize}`
    );
    return response.data;
  }

  async getByDateRange(startDate: string, endDate: string, pageNumber: number = 1, pageSize: number = 10): Promise<ApiResponse<PagedResult<Expense>>> {
    const response = await apiClient.get<ApiResponse<PagedResult<Expense>>>(
      `/expenses/date-range?startDate=${startDate}&endDate=${endDate}&pageNumber=${pageNumber}&pageSize=${pageSize}`
    );
    return response.data;
  }

  async create(expense: CreateExpenseDto): Promise<ApiResponse<Expense>> {
    const response = await apiClient.post<ApiResponse<Expense>>('/expenses', expense);
    return response.data;
  }

  async update(id: string, expense: CreateExpenseDto): Promise<ApiResponse<Expense>> {
    const response = await apiClient.put<ApiResponse<Expense>>(`/expenses/${id}`, expense);
    return response.data;
  }

  async delete(id: string): Promise<ApiResponse<object>> {
    const response = await apiClient.delete<ApiResponse<object>>(`/expenses/${id}`);
    return response.data;
  }
}

export default new ExpenseService();
