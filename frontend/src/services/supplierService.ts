import apiClient from './api';

export interface Supplier {
  id: string;
  name: string;
  contactPerson: string;
  phone: string;
  email: string;
  address: string;
  isActive: boolean;
  createdAt: string;
}

export interface CreateSupplierDto {
  name: string;
  contactPerson: string;
  phone: string;
  email: string;
  address: string;
}

export interface UpdateSupplierDto {
  name: string;
  contactPerson: string;
  phone: string;
  email: string;
  address: string;
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

class SupplierService {
  async getAll(pageNumber: number = 1, pageSize: number = 10): Promise<ApiResponse<PagedResult<Supplier>>> {
    const response = await apiClient.get<ApiResponse<PagedResult<Supplier>>>(
      `/suppliers?pageNumber=${pageNumber}&pageSize=${pageSize}`
    );
    return response.data;
  }

  async getById(id: string): Promise<ApiResponse<Supplier>> {
    const response = await apiClient.get<ApiResponse<Supplier>>(`/suppliers/${id}`);
    return response.data;
  }

  async create(supplier: CreateSupplierDto): Promise<ApiResponse<Supplier>> {
    const response = await apiClient.post<ApiResponse<Supplier>>('/suppliers', supplier);
    return response.data;
  }

  async update(id: string, supplier: UpdateSupplierDto): Promise<ApiResponse<Supplier>> {
    const response = await apiClient.put<ApiResponse<Supplier>>(`/suppliers/${id}`, supplier);
    return response.data;
  }

  async delete(id: string): Promise<ApiResponse<void>> {
    const response = await apiClient.delete<ApiResponse<void>>(`/suppliers/${id}`);
    return response.data;
  }
}

export default new SupplierService();
