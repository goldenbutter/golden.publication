import { describe, it, expect, vi, beforeEach } from 'vitest';
import * as authService from './authService';
import * as authClient from '../api/authClient';
import { getAccessToken, clearAccessToken } from './tokenManager';

vi.mock('../api/authClient', () => ({
  login: vi.fn(),
  register: vi.fn(),
  refresh: vi.fn(),
  logout: vi.fn(),
  me: vi.fn(),
}));

describe('authService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    clearAccessToken();
  });

  const mockResponse = {
    access_token: 'test-access-token',
    expires_at: '2026-01-01T00:00:00Z',
    user: { id: '1', username: 'testuser', role: 'user' as const }
  };

  it('loginWithCredentials should call authClient.login and set token', async () => {
    vi.mocked(authClient.login).mockResolvedValue(mockResponse);
    
    const result = await authService.loginWithCredentials({ username: 'user', password: 'pass' });
    
    expect(authClient.login).toHaveBeenCalledWith({ username: 'user', password: 'pass' });
    expect(getAccessToken()).toBe('test-access-token');
    expect(result).toEqual(mockResponse);
  });

  it('registerAndLogin should call authClient.register and set token', async () => {
    vi.mocked(authClient.register).mockResolvedValue(mockResponse);
    
    const result = await authService.registerAndLogin({ username: 'user', password: 'pass' });
    
    expect(authClient.register).toHaveBeenCalledWith({ username: 'user', password: 'pass' });
    expect(getAccessToken()).toBe('test-access-token');
    expect(result).toEqual(mockResponse);
  });

  it('refreshSession should call authClient.refresh and set token', async () => {
    vi.mocked(authClient.refresh).mockResolvedValue(mockResponse);
    
    const result = await authService.refreshSession();
    
    expect(authClient.refresh).toHaveBeenCalled();
    expect(getAccessToken()).toBe('test-access-token');
    expect(result).toEqual(mockResponse);
  });
});
