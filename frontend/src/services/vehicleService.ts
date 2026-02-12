import apiClient from './api';

export interface Vehicle {
  id: string;
  vehicleNumber: string;
  model: string;
  capacity: number;
  driverId: string;
  driverName: string;
  cleanerId: string;
  cleanerName: string;
  isActive: boolean;
  createdAt: string;
}

export interface CreateVehicleDto {
  vehicleNumber: string;
  model: string;
  capacity: number;
  driverId: string;
  cleanerId: string;
}

export interface UpdateVehicleDto {
  vehicleNumber: string;
  model: string;
  capacity: number;
  driverId: string;
  cleanerId: string;
  isActive: boolean;
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

class VehicleService {
  async getAll(pageNumber: number = 1, pageSize: number = 10): Promise<ApiResponse<PagedResult<Vehicle>>> {
    const response = await apiClient.get<ApiResponse<PagedResult<Vehicle>>>(
      `/vehicles?pageNumber=${pageNumber}&pageSize=${pageSize}`
    );
    return response.data;
  }

  async getById(id: string): Promise<ApiResponse<Vehicle>> {
    const response = await apiClient.get<ApiResponse<Vehicle>>(`/vehicles/${id}`);
    return response.data;
  }

  async create(vehicle: CreateVehicleDto): Promise<ApiResponse<Vehicle>> {
    const response = await apiClient.post<ApiResponse<Vehicle>>('/vehicles', vehicle);
    return response.data;
  }

  async update(id: string, vehicle: UpdateVehicleDto): Promise<ApiResponse<Vehicle>> {
    const response = await apiClient.put<ApiResponse<Vehicle>>(`/vehicles/${id}`, vehicle);
    return response.data;
  }

  async delete(id: string): Promise<ApiResponse<void>> {
    const response = await apiClient.delete<ApiResponse<void>>(`/vehicles/${id}`);
    return response.data;
  }
}

export default new VehicleService();
