import apiClient from './api';

export interface Payment {
  id: string;
  saleId: string;
  amount: number;
  paymentDate: string;
  paymentMethod: string;
  paymentStatus: string;
  transactionId?: string;
  notes?: string;
  createdAt: string;
}

export interface CreatePaymentDto {
  saleId: string;
  amount: number;
  paymentMethod: string;
  transactionId?: string;
  notes?: string;
}

export interface PaymentIntent {
  clientSecret: string;
  paymentIntentId: string;
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

class PaymentService {
  async getMyPayments(pageNumber: number = 1, pageSize: number = 10): Promise<ApiResponse<PagedResult<Payment>>> {
    const response = await apiClient.get<ApiResponse<PagedResult<Payment>>>(
      `/payments/my?pageNumber=${pageNumber}&pageSize=${pageSize}`
    );
    return response.data;
  }

  async getBySale(saleId: string): Promise<ApiResponse<Payment[]>> {
    const response = await apiClient.get<ApiResponse<Payment[]>>(`/payments/sale/${saleId}`);
    return response.data;
  }

  async createPaymentIntent(amount: number, saleId: string): Promise<ApiResponse<PaymentIntent>> {
    const response = await apiClient.post<ApiResponse<PaymentIntent>>('/payments/create-intent', {
      amount,
      saleId,
    });
    return response.data;
  }

  async confirmPayment(paymentIntentId: string, saleId: string): Promise<ApiResponse<Payment>> {
    const response = await apiClient.post<ApiResponse<Payment>>('/payments/confirm', {
      paymentIntentId,
      saleId,
    });
    return response.data;
  }

  async create(payment: CreatePaymentDto): Promise<ApiResponse<Payment>> {
    const response = await apiClient.post<ApiResponse<Payment>>('/payments', payment);
    return response.data;
  }
}

export default new PaymentService();
