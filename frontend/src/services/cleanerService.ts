import apiClient from './api';

export interface Cleaner {
  id: string;
  name: string;
  phone: string;
  isActive: boolean;
  createdAt: string;
}

export interface CreateCleanerDto {
  name: string;
  phone: string;
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

class CleanerService {
  async getAll(pageNumber: number = 1, pageSize: number = 10): Promise<ApiResponse<PagedResult<Cleaner>>> {
    const response = await apiClient.get<ApiResponse<PagedResult<Cleaner>>>(
      `/cleaners?pageNumber=${pageNumber}&pageSize=${pageSize}`
    );
    return response.data;
  }

  async create(cleaner: CreateCleanerDto): Promise<ApiResponse<Cleaner>> {
    const response = await apiClient.post<ApiResponse<Cleaner>>('/cleaners', cleaner);
    return response.data;
  }

  async update(id: string, cleaner: CreateCleanerDto): Promise<ApiResponse<Cleaner>> {
    const response = await apiClient.put<ApiResponse<Cleaner>>(`/cleaners/${id}`, cleaner);
    return response.data;
  }

  async delete(id: string): Promise<ApiResponse<void>> {
    const response = await apiClient.delete<ApiResponse<void>>(`/cleaners/${id}`);
    return response.data;
  }
}

export default new CleanerService();
