import { httpClient } from './httpClient';
import { ENDPOINTS } from '../config/api';
import { authStore } from './localAuthStore';

export interface LoginDto { email: string; password: string; }
export interface RegisterDto { firstName: string; lastName: string; email: string; password: string; }
interface AuthUser { id: string; email: string; firstName?: string; lastName?: string; }
interface AuthResponse { token: string; refreshToken: string; user: AuthUser; }

export const authService = {
  login: async (data: LoginDto) => {
    const res = await httpClient.post<AuthResponse>(ENDPOINTS.AUTH.LOGIN, data);
    authStore.setTokens(res.token, res.refreshToken);
    authStore.setUser(res.user);
    return res;
  },
  register: async (data: RegisterDto) => {
    const res = await httpClient.post<AuthResponse>(ENDPOINTS.AUTH.REGISTER, data);
    authStore.setTokens(res.token, res.refreshToken);
    authStore.setUser(res.user);
    return res;
  },
  logout: () => authStore.clear()
};
