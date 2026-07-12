import axiosClient from './axios/AxiosBase';
import type { ApiResponse, AppNotification } from '@/types';

export const getNotifications = async (): Promise<ApiResponse<AppNotification[]>> => {
    const response = await axiosClient.get('/api/notifications');
    return response.data;
};

export const markRead = async (id: number): Promise<ApiResponse> => {
    const response = await axiosClient.post(`/api/notifications/${id}/read`);
    return response.data;
};

export const markAllRead = async (): Promise<ApiResponse> => {
    const response = await axiosClient.post('/api/notifications/read-all');
    return response.data;
};
