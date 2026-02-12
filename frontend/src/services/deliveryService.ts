import apiClient from './api';

export interface Delivery {
  id: string;
  distributionId: string;
  shopId: string;
  shopName: string;
  deliveryDate: string;
  totalQuantity: number;
  verifiedQuantity: number;
  deliveryStatus: string;
  notes?: string;
  createdAt: string;
}

export interface UpdateDeliveryDto {
  verifiedQuantity: number;
  deliveryStatus: string;
  notes?: string;
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

class DeliveryService {
  async getAll(pageNumber: number = 1, pageSize: number = 10): Promise<ApiResponse<PagedResult<Delivery>>> {
    const response = await apiClient.get<ApiResponse<PagedResult<Delivery>>>(
      `/deliveries?pageNumber=${pageNumber}&pageSize=${pageSize}`
    );
    return response.data;
  }

  async getByShop(shopId: string, pageNumber: number = 1, pageSize: number = 10): Promise<ApiResponse<PagedResult<Delivery>>> {
    const response = await apiClient.get<ApiResponse<PagedResult<Delivery>>>(
      `/deliveries/shop/${shopId}?pageNumber=${pageNumber}&pageSize=${pageSize}`
    );
    return response.data;
  }

  async update(id: string, delivery: UpdateDeliveryDto): Promise<ApiResponse<Delivery>> {
    const response = await apiClient.put<ApiResponse<Delivery>>(`/deliveries/${id}`, delivery);
    return response.data;
  }
}

export default new DeliveryService();
