import { describe, it, expect, vi, beforeEach } from 'vitest';
import { apiFetch } from './httpClient';
import * as authClient from './authClient';
import * as tokenManager from '../auth/tokenManager';

vi.mock('./authClient', () => ({
  refresh: vi.fn(),
}));

describe('httpClient apiFetch', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    tokenManager.clearAccessToken();
    // Mock global fetch
    vi.stubGlobal('fetch', vi.fn());
  });

  it('should add Authorization header if token exists', async () => {
    tokenManager.setAccessToken('test-token');
    vi.mocked(fetch).mockResolvedValue({ status: 200 } as Response);

    await apiFetch('/test');

    expect(fetch).toHaveBeenCalledWith('/test', expect.objectContaining({
      headers: expect.any(Headers)
    }));
    
    const callHeaders = (vi.mocked(fetch).mock.calls[0][1] as RequestInit).headers as Headers;
    expect(callHeaders.get('Authorization')).toBe('Bearer test-token');
  });

  it('should retry once on 401 and refresh success', async () => {
    tokenManager.setAccessToken('old-token');
    
    // First call returns 401, second call returns 200
    vi.mocked(fetch)
      .mockResolvedValueOnce({ status: 401 } as Response)
      .mockResolvedValueOnce({ status: 200 } as Response);
    
    vi.mocked(authClient.refresh).mockResolvedValue({
      access_token: 'new-token',
      expires_at: '',
      user: {} as any
    });

    const response = await apiFetch('/protected');

    expect(response.status).toBe(200);
    expect(authClient.refresh).toHaveBeenCalled();
    expect(tokenManager.getAccessToken()).toBe('new-token');
    
    // Check headers of second call
    expect(fetch).toHaveBeenCalledTimes(2);
    const retryHeaders = (vi.mocked(fetch).mock.calls[1][1] as RequestInit).headers as Headers;
    expect(retryHeaders.get('Authorization')).toBe('Bearer new-token');
  });

  it('should not retry if refresh fails', async () => {
    tokenManager.setAccessToken('old-token');
    vi.mocked(fetch).mockResolvedValue({ status: 401 } as Response);
    vi.mocked(authClient.refresh).mockRejectedValue(new Error('Refresh failed'));

    const response = await apiFetch('/protected');

    expect(response.status).toBe(401);
    expect(authClient.refresh).toHaveBeenCalled();
    expect(tokenManager.getAccessToken()).toBeNull();
    expect(fetch).toHaveBeenCalledTimes(1);
  });
});
