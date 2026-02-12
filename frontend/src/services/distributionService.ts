import apiClient from './api';

export interface Distribution {
  id: string;
  vehicleId: string;
  vehicleNumber: string;
  driverName: string;
  cleanerName: string;
  scheduledDate: string;
  status: string;
  totalItems: number;
  createdAt: string;
}

export interface CreateDistributionDto {
  vehicleId: string;
  scheduledDate: string;
  items: DistributionItemDto[];
}

export interface DistributionItemDto {
  chickenId: string;
  shopId: string;
  quantity: number;
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

class DistributionService {
  async getAll(pageNumber: number = 1, pageSize: number = 10): Promise<ApiResponse<PagedResult<Distribution>>> {
    const response = await apiClient.get<ApiResponse<PagedResult<Distribution>>>(
      `/distributions?pageNumber=${pageNumber}&pageSize=${pageSize}`
    );
    return response.data;
  }

  async getById(id: string): Promise<ApiResponse<Distribution>> {
    const response = await apiClient.get<ApiResponse<Distribution>>(`/distributions/${id}`);
    return response.data;
  }

  async create(distribution: CreateDistributionDto): Promise<ApiResponse<Distribution>> {
    const response = await apiClient.post<ApiResponse<Distribution>>('/distributions', distribution);
    return response.data;
  }

  async updateStatus(id: string, status: string): Promise<ApiResponse<Distribution>> {
    const response = await apiClient.put<ApiResponse<Distribution>>(`/distributions/${id}/status`, status);
    return response.data;
  }

  async delete(id: string): Promise<ApiResponse<void>> {
    const response = await apiClient.delete<ApiResponse<void>>(`/distributions/${id}`);
    return response.data;
  }
}

export default new DistributionService();
