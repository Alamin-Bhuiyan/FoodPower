import axiosClient from './axios/AxiosBase';
import type { ApiResponse, UserSummary } from '@/types';

export const getUsers = async (): Promise<ApiResponse<UserSummary[]>> => {
    const response = await axiosClient.get('/api/users');
    return response.data;
};

export const getMe = async (): Promise<ApiResponse<UserSummary>> => {
    const response = await axiosClient.get('/api/users/me');
    return response.data;
};
