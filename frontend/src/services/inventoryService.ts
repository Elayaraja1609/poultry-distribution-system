import apiClient from './api';

export interface StockMovement {
  id: string;
  farmId: string;
  farmName: string;
  chickenId: string;
  batchNumber: string;
  movementType: string;
  quantity: number;
  previousQuantity: number;
  newQuantity: number;
  reason?: string;
  movementDate: string;
  createdAt: string;
}

export interface CreateStockMovementDto {
  farmId: string;
  chickenId: string;
  movementType: string;
  quantity: number;
  reason?: string;
  movementDate?: string;
}

export interface FarmInventory {
  farmId: string;
  farmName: string;
  capacity: number;
  currentStock: number;
  availableStock: number;
  stockIn: number;
  stockOut: number;
  stockLoss: number;
  chickenStocks: ChickenStock[];
}

export interface ChickenStock {
  chickenId: string;
  batchNumber: string;
  quantity: number;
  availableQuantity: number;
  status: string;
}

export interface StockSummary {
  totalIn: number;
  totalOut: number;
  totalLoss: number;
  totalAdjustments: number;
  netChange: number;
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

class InventoryService {
  async getFarmInventory(farmId: string): Promise<ApiResponse<FarmInventory>> {
    const response = await apiClient.get<ApiResponse<FarmInventory>>(`/inventory/farms/${farmId}`);
    return response.data;
  }

  async getStockMovements(
    farmId?: string,
    chickenId?: string,
    startDate?: string,
    endDate?: string,
    pageNumber: number = 1,
    pageSize: number = 10
  ): Promise<ApiResponse<PagedResult<StockMovement>>> {
    const params = new URLSearchParams();
    if (farmId) params.append('farmId', farmId);
    if (chickenId) params.append('chickenId', chickenId);
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);
    params.append('pageNumber', pageNumber.toString());
    params.append('pageSize', pageSize.toString());

    const response = await apiClient.get<ApiResponse<PagedResult<StockMovement>>>(
      `/inventory/movements?${params.toString()}`
    );
    return response.data;
  }

  async recordStockMovement(movement: CreateStockMovementDto): Promise<ApiResponse<StockMovement>> {
    const response = await apiClient.post<ApiResponse<StockMovement>>('/inventory/movements', movement);
    return response.data;
  }

  async getStockSummary(farmId: string, startDate?: string, endDate?: string): Promise<ApiResponse<StockSummary>> {
    const params = new URLSearchParams();
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);

    const response = await apiClient.get<ApiResponse<StockSummary>>(
      `/inventory/farms/${farmId}/stock-summary?${params.toString()}`
    );
    return response.data;
  }
}

export default new InventoryService();
