import apiClient from './api';

export interface Driver {
  id: string;
  name: string;
  phone: string;
  licenseNumber: string;
  isActive: boolean;
  createdAt: string;
}

export interface CreateDriverDto {
  name: string;
  phone: string;
  licenseNumber: string;
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

class DriverService {
  async getAll(pageNumber: number = 1, pageSize: number = 10): Promise<ApiResponse<PagedResult<Driver>>> {
    const response = await apiClient.get<ApiResponse<PagedResult<Driver>>>(
      `/drivers?pageNumber=${pageNumber}&pageSize=${pageSize}`
    );
    return response.data;
  }

  async create(driver: CreateDriverDto): Promise<ApiResponse<Driver>> {
    const response = await apiClient.post<ApiResponse<Driver>>('/drivers', driver);
    return response.data;
  }

  async update(id: string, driver: CreateDriverDto): Promise<ApiResponse<Driver>> {
    const response = await apiClient.put<ApiResponse<Driver>>(`/drivers/${id}`, driver);
    return response.data;
  }

  async delete(id: string): Promise<ApiResponse<void>> {
    const response = await apiClient.delete<ApiResponse<void>>(`/drivers/${id}`);
    return response.data;
  }
}

export default new DriverService();
