import axiosClient from './axios/AxiosBase';
import type { ApiResponse, AppSettings } from '@/types';

export const getSettings = async (): Promise<ApiResponse<AppSettings>> => {
    const response = await axiosClient.get('/api/settings');
    return response.data;
};

export const updateSettings = async (data: Partial<AppSettings>): Promise<ApiResponse<AppSettings>> => {
    const response = await axiosClient.put('/api/settings', data);
    return response.data;
};
