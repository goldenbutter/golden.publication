import { describe, it, expect, beforeEach } from 'vitest';
import { setAccessToken, getAccessToken, clearAccessToken } from './tokenManager';

describe('tokenManager', () => {
  beforeEach(() => {
    clearAccessToken();
  });

  it('should set and get access token', () => {
    setAccessToken('test-token');
    expect(getAccessToken()).toBe('test-token');
  });

  it('should clear access token', () => {
    setAccessToken('test-token');
    clearAccessToken();
    expect(getAccessToken()).toBeNull();
  });
});
