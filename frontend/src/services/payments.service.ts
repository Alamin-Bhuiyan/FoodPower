import axiosClient from './axios/AxiosBase';
import type { ApiResponse, Payment } from '@/types';

export interface SubmitPaymentPayload {
    screenshot: string; // base64 data URL
    note?: string;
    allocations: { beneficiary_user_id: number; days: number }[];
}

export const submitPayment = async (data: SubmitPaymentPayload): Promise<ApiResponse<Payment>> => {
    const response = await axiosClient.post('/api/payments', data);
    return response.data;
};

export const getMyPayments = async (): Promise<ApiResponse<Payment[]>> => {
    const response = await axiosClient.get('/api/payments/my');
    return response.data;
};

export const getPayments = async (status?: string): Promise<ApiResponse<Payment[]>> => {
    const response = await axiosClient.get('/api/payments', { params: status ? { status } : undefined });
    return response.data;
};

export const approvePayment = async (id: number): Promise<ApiResponse> => {
    const response = await axiosClient.post(`/api/payments/${id}/approve`);
    return response.data;
};

export const rejectPayment = async (id: number, reason?: string): Promise<ApiResponse> => {
    const response = await axiosClient.post(`/api/payments/${id}/reject`, { reason });
    return response.data;
};
