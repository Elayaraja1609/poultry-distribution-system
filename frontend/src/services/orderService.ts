import apiClient from './api';

export interface Order {
  id: string;
  shopId: string;
  shopName: string;
  orderDate: string;
  requestedDeliveryDate: string;
  status: string;
  totalAmount: number;
  approvedBy?: string;
  approvedAt?: string;
  rejectedReason?: string;
  fulfillmentStatus: string;
  items: OrderItem[];
  createdAt: string;
}

export interface OrderItem {
  id: string;
  orderId: string;
  chickenId: string;
  batchNumber: string;
  requestedQuantity: number;
  fulfilledQuantity: number;
  unitPrice: number;
  totalPrice: number;
}

export interface CreateOrderDto {
  requestedDeliveryDate: string;
  items: {
    chickenId: string;
    quantity: number;
  }[];
}

export interface UpdateFulfillmentDto {
  items: {
    orderItemId: string;
    fulfilledQuantity: number;
  }[];
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

class OrderService {
  async getAll(shopId?: string, status?: string, pageNumber: number = 1, pageSize: number = 10): Promise<ApiResponse<PagedResult<Order>>> {
    const params = new URLSearchParams();
    if (shopId) params.append('shopId', shopId);
    if (status) params.append('status', status);
    params.append('pageNumber', pageNumber.toString());
    params.append('pageSize', pageSize.toString());

    const response = await apiClient.get<ApiResponse<PagedResult<Order>>>(
      `/orders?${params.toString()}`
    );
    return response.data;
  }

  async getById(id: string): Promise<ApiResponse<Order>> {
    const response = await apiClient.get<ApiResponse<Order>>(`/orders/${id}`);
    return response.data;
  }

  async create(order: CreateOrderDto): Promise<ApiResponse<Order>> {
    const response = await apiClient.post<ApiResponse<Order>>('/orders', order);
    return response.data;
  }

  async approve(id: string): Promise<ApiResponse<Order>> {
    const response = await apiClient.put<ApiResponse<Order>>(`/orders/${id}/approve`, {});
    return response.data;
  }

  async reject(id: string, reason: string): Promise<ApiResponse<Order>> {
    const response = await apiClient.put<ApiResponse<Order>>(`/orders/${id}/reject`, { reason });
    return response.data;
  }

  async updateFulfillment(id: string, fulfillment: UpdateFulfillmentDto): Promise<ApiResponse<Order>> {
    const response = await apiClient.put<ApiResponse<Order>>(`/orders/${id}/fulfillment`, fulfillment);
    return response.data;
  }

  async cancel(id: string): Promise<ApiResponse<Order>> {
    const response = await apiClient.put<ApiResponse<Order>>(`/orders/${id}/cancel`, {});
    return response.data;
  }
}

export default new OrderService();
