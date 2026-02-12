import apiClient from './api';

export interface Sale {
  id: string;
  deliveryId: string;
  shopId: string;
  shopName: string;
  saleDate: string;
  totalAmount: number;
  paymentStatus: string;
  paymentMethod: string;
  paidAmount: number;
  remainingAmount: number;
  createdAt: string;
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

class SalesService {
  async getAll(pageNumber: number = 1, pageSize: number = 10): Promise<ApiResponse<PagedResult<Sale>>> {
    const response = await apiClient.get<ApiResponse<PagedResult<Sale>>>(
      `/sales?pageNumber=${pageNumber}&pageSize=${pageSize}`
    );
    return response.data;
  }

  async getByShop(shopId: string, pageNumber: number = 1, pageSize: number = 10): Promise<ApiResponse<PagedResult<Sale>>> {
    const response = await apiClient.get<ApiResponse<PagedResult<Sale>>>(
      `/sales/shop/${shopId}?pageNumber=${pageNumber}&pageSize=${pageSize}`
    );
    return response.data;
  }

  async getMySales(pageNumber: number = 1, pageSize: number = 10): Promise<ApiResponse<PagedResult<Sale>>> {
    const response = await apiClient.get<ApiResponse<PagedResult<Sale>>>(
      `/sales/my?pageNumber=${pageNumber}&pageSize=${pageSize}`
    );
    return response.data;
  }

  async getById(id: string): Promise<ApiResponse<Sale>> {
    const response = await apiClient.get<ApiResponse<Sale>>(`/sales/${id}`);
    return response.data;
  }
}

export default new SalesService();
