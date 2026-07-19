export type AuthFlowType = 'register' | 'forgot' | 'login' | 'verify-user' | 'verify-email' | 'forgot-password' | 'login-unverified';

export interface AuthFlowState {
  email?: string;
  phone?: string;
  flow: AuthFlowType;
  method?: 'email' | 'phone';
}

const AUTH_FLOW_STORAGE_KEY = 'auth_flow_state';
const RESET_PASSWORD_TOKEN_KEY = 'reset_password_token';

export const setAuthFlow = (
  flow: AuthFlowType,
  email?: string,
  phone?: string,
  method?: 'email' | 'phone'
) => {
  try {
    sessionStorage.setItem(AUTH_FLOW_STORAGE_KEY, JSON.stringify({ email, phone, flow, method }));
  } catch {
    // ignore storage failures
  }
};

export const getAuthFlow = (): AuthFlowState | null => {
  try {
    const raw = sessionStorage.getItem(AUTH_FLOW_STORAGE_KEY);
    if (!raw) return null;
    return JSON.parse(raw) as AuthFlowState;
  } catch {
    return null;
  }
};

export const clearAuthFlow = () => {
  try {
    sessionStorage.removeItem(AUTH_FLOW_STORAGE_KEY);
  } catch {
    // ignore
  }
};

export const setResetPasswordToken = (token: string) => {
  try {
    sessionStorage.setItem(RESET_PASSWORD_TOKEN_KEY, token);
  } catch {
    // ignore storage failures
  }
};

export const getResetPasswordToken = (): string | null => {
  try {
    return sessionStorage.getItem(RESET_PASSWORD_TOKEN_KEY);
  } catch {
    return null;
  }
};

export const clearResetPasswordToken = () => {
  try {
    sessionStorage.removeItem(RESET_PASSWORD_TOKEN_KEY);
  } catch {
    // ignore
  }
};
