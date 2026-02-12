import apiClient from './api';

export interface Shop {
  id: string;
  name: string;
  ownerName: string;
  phone: string;
  email: string;
  address: string;
  isActive: boolean;
  createdAt: string;
}

export interface CreateShopDto {
  name: string;
  ownerName: string;
  phone: string;
  email: string;
  address: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

export interface ApiResponse<T> {
  success: boolean;
  message?: string;
  data?: T;
}

class ShopService {
  async getAll(pageNumber: number = 1, pageSize: number = 10): Promise<ApiResponse<PagedResult<Shop>>> {
    const response = await apiClient.get<ApiResponse<PagedResult<Shop>>>(
      `/shops?pageNumber=${pageNumber}&pageSize=${pageSize}`
    );
    return response.data;
  }

  async getById(id: string): Promise<ApiResponse<Shop>> {
    const response = await apiClient.get<ApiResponse<Shop>>(`/shops/${id}`);
    return response.data;
  }
}

export default new ShopService();
