import axiosClient from './axios/AxiosBase';
import type { ApiResponse, MyDues, UserDues, WeeklySummary } from '@/types';

export const getMyDues = async (): Promise<ApiResponse<MyDues>> => {
    const response = await axiosClient.get('/api/dues/my');
    return response.data;
};

export const getAllDues = async (): Promise<ApiResponse<UserDues[]>> => {
    const response = await axiosClient.get('/api/dues');
    return response.data;
};

export const getWeeklySummary = async (weekStart: string): Promise<ApiResponse<WeeklySummary>> => {
    const response = await axiosClient.get('/api/dues/weekly-summary', { params: { weekStart } });
    return response.data;
};
