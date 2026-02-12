import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import authService, { LoginRequest, RegisterRequest } from '../services/authService';

interface User {
  id: string;
  username: string;
  email: string;
  role: string;
  fullName?: string;
}

interface AuthContextType {
  user: User | null;
  loading: boolean;
  login: (credentials: LoginRequest) => Promise<void>;
  register: (userData: RegisterRequest) => Promise<void>;
  logout: () => Promise<void>;
  isAuthenticated: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const currentUser = authService.getCurrentUser();
    setUser(currentUser);
    setLoading(false);
  }, []);

  const login = async (credentials: LoginRequest) => {
    const response = await authService.login(credentials);
    // Support both camelCase (from API with camelCase JSON) and PascalCase (legacy)
    const success = response.success ?? (response as { Success?: boolean }).Success;
    const payload = response.data ?? (response as { Data?: typeof response.data }).Data;
    console.log('login response', JSON.stringify(response));
    if (success && payload) {
      const token = payload.accessToken ?? (payload as { AccessToken?: string }).AccessToken;
      const refresh = payload.refreshToken ?? (payload as { RefreshToken?: string }).RefreshToken;
      const rawUser = payload.user ?? (payload as { User?: User }).User;
      if (!token || !rawUser) {
        throw new Error('Invalid login response: missing token or user');
      }
      const userData: User = {
        id: String((rawUser as User).id ?? (rawUser as { Id?: string }).Id ?? ''),
        username: (rawUser as User).username ?? (rawUser as { Username?: string }).Username ?? '',
        email: (rawUser as User).email ?? (rawUser as { Email?: string }).Email ?? '',
        role: (rawUser as User).role ?? (rawUser as { Role?: string }).Role ?? '',
        fullName: (rawUser as User).fullName ?? (rawUser as { FullName?: string }).FullName,
      };
      localStorage.setItem('accessToken', token);
      localStorage.setItem('refreshToken', refresh);
      localStorage.setItem('user', JSON.stringify(userData));
      setUser(userData);
    } else {
      throw new Error((response.message ?? (response as { Message?: string }).Message) || 'Login failed');
    }
  };

  const register = async (userData: RegisterRequest) => {
    const response = await authService.register(userData);
    if (!response.success) {
      throw new Error(response.message || 'Registration failed');
    }
  };

  const logout = async () => {
    await authService.logout();
    setUser(null);
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        loading,
        login,
        register,
        logout,
        isAuthenticated: !!user,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};
