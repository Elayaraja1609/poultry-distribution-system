import apiClient from './api';

export interface LoginRequest {
  usernameOrEmail: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
  fullName: string;
  phone: string;
  role: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: {
    id: string;
    username: string;
    email: string;
    role: string;
    fullName?: string;
  };
}

export interface ApiResponse<T> {
  success: boolean;
  message?: string;
  data?: T;
  errors?: string[];
}

class AuthService {
  async login(credentials: LoginRequest): Promise<ApiResponse<LoginResponse>> {
    const response = await apiClient.post<ApiResponse<LoginResponse>>('/auth/login', credentials);
    return response.data;
  }

  async register(userData: RegisterRequest): Promise<ApiResponse<any>> {
    const response = await apiClient.post<ApiResponse<any>>('/auth/register', userData);
    return response.data;
  }

  async logout(): Promise<void> {
    await apiClient.post('/auth/logout');
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
  }

  getCurrentUser() {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  }

  isAuthenticated(): boolean {
    return !!localStorage.getItem('accessToken');
  }

  async getProfile(): Promise<ApiResponse<LoginResponse['user']>> {
    const response = await apiClient.get<ApiResponse<LoginResponse['user']>>('/auth/profile');
    return response.data;
  }

  async updateProfile(profileData: { fullName?: string; phone?: string; address?: string }): Promise<ApiResponse<LoginResponse['user']>> {
    const response = await apiClient.put<ApiResponse<LoginResponse['user']>>('/auth/profile', profileData);
    return response.data;
  }
}

export default new AuthService();
