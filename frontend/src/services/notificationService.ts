import apiClient from './api';

export interface Notification {
  id: string;
  userId: string;
  type: string;
  title: string;
  message: string;
  isRead: boolean;
  readAt?: string;
  relatedEntityType?: string;
  relatedEntityId?: string;
  createdAt: string;
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

class NotificationService {
  async getAll(isRead?: boolean, type?: string, pageNumber: number = 1, pageSize: number = 20): Promise<ApiResponse<PagedResult<Notification>>> {
    const params = new URLSearchParams();
    if (isRead !== undefined) params.append('isRead', isRead.toString());
    if (type) params.append('type', type);
    params.append('pageNumber', pageNumber.toString());
    params.append('pageSize', pageSize.toString());

    const response = await apiClient.get<ApiResponse<PagedResult<Notification>>>(
      `/notifications?${params.toString()}`
    );
    return response.data;
  }

  async getUnreadCount(): Promise<ApiResponse<number>> {
    const response = await apiClient.get<ApiResponse<number>>('/notifications/unread-count');
    return response.data;
  }

  async markAsRead(id: string): Promise<ApiResponse<object>> {
    const response = await apiClient.put<ApiResponse<object>>(`/notifications/${id}/read`, {});
    return response.data;
  }

  async markAllAsRead(): Promise<ApiResponse<object>> {
    const response = await apiClient.put<ApiResponse<object>>('/notifications/read-all', {});
    return response.data;
  }
}

export default new NotificationService();
