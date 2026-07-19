import axiosClient from './axios/AxiosBase';
import type { ApiResponse } from '@/types';

export interface PushSubscribePayload {
    endpoint: string;
    p256dh: string;
    auth: string;
}

export const getVapidPublicKey = async (): Promise<ApiResponse<{ public_key: string }>> => {
    const response = await axiosClient.get('/api/push/vapid-public-key');
    return response.data;
};

export const subscribe = async (data: PushSubscribePayload): Promise<ApiResponse<{ message: string }>> => {
    const response = await axiosClient.post('/api/push/subscribe', data);
    return response.data;
};

export const unsubscribe = async (data: { endpoint: string }): Promise<ApiResponse<{ message: string }>> => {
    const response = await axiosClient.post('/api/push/unsubscribe', data);
    return response.data;
};

/** Admin: send a push reminder to everyone who has not voted on a poll. */
export const remindPoll = async (pollId: number): Promise<ApiResponse<{ message: string }>> => {
    const response = await axiosClient.post(`/api/polls/${pollId}/remind`);
    return response.data;
};

/** Admin: send a push reminder to everyone with an outstanding due. */
export const remindDue = async (): Promise<ApiResponse<{ message: string }>> => {
    const response = await axiosClient.post('/api/payments/remind-due');
    return response.data;
};
