import apiClient from './api';

export interface Chicken {
  id: string;
  batchNumber: string;
  supplierId: string;
  supplierName: string;
  farmId?: string;
  farmName?: string;
  purchaseDate: string;
  quantity: number;
  ageDays: number;
  weightKg: number;
  status: string;
  healthStatus: string;
  createdAt: string;
}

export interface CreateChickenDto {
  batchNumber: string;
  supplierId: string;
  farmId?: string;
  purchaseDate: string;
  quantity: number;
  weightKg: number;
}

export interface UpdateChickenDto {
  farmId?: string;
  ageDays: number;
  weightKg: number;
  status: string;
  healthStatus: string;
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

class ChickenService {
  async getAll(pageNumber: number = 1, pageSize: number = 10): Promise<ApiResponse<PagedResult<Chicken>>> {
    const response = await apiClient.get<ApiResponse<PagedResult<Chicken>>>(
      `/chickens?pageNumber=${pageNumber}&pageSize=${pageSize}`
    );
    return response.data;
  }

  async getById(id: string): Promise<ApiResponse<Chicken>> {
    const response = await apiClient.get<ApiResponse<Chicken>>(`/chickens/${id}`);
    return response.data;
  }

  async getByFarm(farmId: string, pageNumber: number = 1, pageSize: number = 10): Promise<ApiResponse<PagedResult<Chicken>>> {
    const response = await apiClient.get<ApiResponse<PagedResult<Chicken>>>(
      `/chickens/farm/${farmId}?pageNumber=${pageNumber}&pageSize=${pageSize}`
    );
    return response.data;
  }

  async create(chicken: CreateChickenDto): Promise<ApiResponse<Chicken>> {
    const response = await apiClient.post<ApiResponse<Chicken>>('/chickens', chicken);
    return response.data;
  }

  async update(id: string, chicken: UpdateChickenDto): Promise<ApiResponse<Chicken>> {
    const response = await apiClient.put<ApiResponse<Chicken>>(`/chickens/${id}`, chicken);
    return response.data;
  }

  async delete(id: string): Promise<ApiResponse<void>> {
    const response = await apiClient.delete<ApiResponse<void>>(`/chickens/${id}`);
    return response.data;
  }
}

export default new ChickenService();
