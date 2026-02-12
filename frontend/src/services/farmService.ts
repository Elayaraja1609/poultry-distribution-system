import apiClient from './api';

export interface Farm {
  id: string;
  name: string;
  location: string;
  capacity: number;
  currentCount: number;
  managerName: string;
  phone: string;
  isActive: boolean;
  createdAt: string;
}

export interface CreateFarmDto {
  name: string;
  location: string;
  capacity: number;
  managerName: string;
  phone: string;
}

export interface UpdateFarmDto {
  name: string;
  location: string;
  capacity: number;
  managerName: string;
  phone: string;
  isActive: boolean;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface ApiResponse<T> {
  success: boolean;
  message?: string;
  data?: T;
  errors?: string[];
}

class FarmService {
  async getAll(pageNumber: number = 1, pageSize: number = 10): Promise<ApiResponse<PagedResult<Farm>>> {
    const response = await apiClient.get<ApiResponse<PagedResult<Farm>>>(
      `/farms?pageNumber=${pageNumber}&pageSize=${pageSize}`
    );
    return response.data;
  }

  async getById(id: string): Promise<ApiResponse<Farm>> {
    const response = await apiClient.get<ApiResponse<Farm>>(`/farms/${id}`);
    return response.data;
  }

  async create(farm: CreateFarmDto): Promise<ApiResponse<Farm>> {
    const response = await apiClient.post<ApiResponse<Farm>>('/farms', farm);
    return response.data;
  }

  async update(id: string, farm: UpdateFarmDto): Promise<ApiResponse<Farm>> {
    const response = await apiClient.put<ApiResponse<Farm>>(`/farms/${id}`, farm);
    return response.data;
  }

  async delete(id: string): Promise<ApiResponse<void>> {
    const response = await apiClient.delete<ApiResponse<void>>(`/farms/${id}`);
    return response.data;
  }

  async updateCapacity(id: string, currentCount: number): Promise<ApiResponse<void>> {
    const response = await apiClient.put<ApiResponse<void>>(`/farms/${id}/capacity`, currentCount);
    return response.data;
  }
}

export default new FarmService();
