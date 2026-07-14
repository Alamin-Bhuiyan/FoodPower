import axiosClient from './axios/AxiosBase';
import { clearSession } from '@/lib/auth';
import type { ApiResponse, AuthUser, LoginResult } from '@/types';

export const register = async (data: { full_name: string; email: string; password: string }): Promise<ApiResponse> => {
    const response = await axiosClient.post('/api/auth/register', data);
    return response.data;
};

export const verifyOtp = async (data: { email: string; otp: string; purpose?: string }): Promise<ApiResponse> => {
    const response = await axiosClient.post('/api/auth/verify-otp', data);
    return response.data;
};

export const resendOtp = async (data: { email: string; purpose?: string }): Promise<ApiResponse> => {
    const response = await axiosClient.post('/api/auth/resend-otp', data);
    return response.data;
};

export const login = async (data: { email: string; password: string }): Promise<ApiResponse<LoginResult>> => {
    const response = await axiosClient.post('/api/auth/login', data);
    return response.data;
};

export const forgetPassword = async (data: { email: string }): Promise<ApiResponse> => {
    const response = await axiosClient.post('/api/auth/forget-password', data);
    return response.data;
};

export const resetPassword = async (data: { email: string; otp: string; new_password: string }): Promise<ApiResponse> => {
    const response = await axiosClient.post('/api/auth/reset-password', data);
    return response.data;
};

export const changePassword = async (data: { old_password: string; new_password: string }): Promise<ApiResponse> => {
    const response = await axiosClient.post('/api/auth/change-password', data);
    return response.data;
};

export const logout = () => {
    clearSession();
    delete axiosClient.defaults.headers.common.Authorization;
    window.location.href = '/login';
};

/** Normalize the login payload — supports {token,user} and {access_token,user}. */
export const extractLoginResult = (data: any): { token: string; user: AuthUser } => {
    const token = data?.token ?? data?.access_token;
    const rawUser = data?.user ?? {};
    const roles: string[] = rawUser.roles ?? (rawUser.role ? [rawUser.role] : []);
    return {
        token,
        user: {
            id: rawUser.id,
            full_name: rawUser.full_name ?? rawUser.fullName ?? '',
            email: rawUser.email ?? '',
            roles,
            profile_picture: rawUser.profile_picture ?? rawUser.profilePicture ?? null,
        },
    };
};
