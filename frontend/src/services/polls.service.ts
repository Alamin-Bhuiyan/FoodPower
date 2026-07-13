import axiosClient from './axios/AxiosBase';
import type { ApiResponse, Poll } from '@/types';

export interface CreatePollOption {
    menu_item_id?: number | null;
    custom_name?: string | null;
}

export interface CreatePollPayload {
    lunch_date: string;          // yyyy-MM-dd
    caterer_id?: number | null;
    options: CreatePollOption[];
    cutoff_at?: string | null;   // ISO datetime, optional custom cutoff
    /** "Lunch" (default) | "General". General polls never affect dues. */
    poll_type?: string;
    /** Required when poll_type is "General". */
    question?: string;
}

export const createPoll = async (data: CreatePollPayload): Promise<ApiResponse<Poll>> => {
    const response = await axiosClient.post('/api/polls', data);
    return response.data;
};

export const getActivePoll = async (): Promise<ApiResponse<Poll | null>> => {
    const response = await axiosClient.get('/api/polls/active');
    return response.data;
};

/** All currently open General polls (never affect dues). */
export const getActiveGeneralPolls = async (): Promise<ApiResponse<Poll[]>> => {
    const response = await axiosClient.get('/api/polls/general-active');
    return response.data;
};

/** The most recent 10 Lunch polls, newest first — each a full poll (same shape as getActivePoll's data). */
export const getRecentLunchPolls = async (): Promise<ApiResponse<Poll[]>> => {
    const response = await axiosClient.get('/api/polls/lunch-recent');
    return response.data;
};

export const getSharedPoll = async (shareToken: string): Promise<ApiResponse<Poll>> => {
    const response = await axiosClient.get(`/api/polls/shared/${shareToken}`);
    return response.data;
};

export const getPolls = async (params?: { page?: number; pageSize?: number }): Promise<ApiResponse<Poll[]>> => {
    const response = await axiosClient.get('/api/polls', { params });
    return response.data;
};

export const getPollResults = async (pollId: number): Promise<ApiResponse<Poll>> => {
    const response = await axiosClient.get(`/api/polls/${pollId}/results`);
    return response.data;
};

export const vote = async (pollId: number, pollOptionId: number): Promise<ApiResponse> => {
    const response = await axiosClient.post(`/api/polls/${pollId}/votes`, { poll_option_id: pollOptionId });
    return response.data;
};

export const removeVote = async (pollId: number): Promise<ApiResponse> => {
    const response = await axiosClient.delete(`/api/polls/${pollId}/votes`);
    return response.data;
};

export const addManualVote = async (pollId: number, data: { user_id: number; poll_option_id: number }): Promise<ApiResponse> => {
    const response = await axiosClient.post(`/api/polls/${pollId}/manual-votes`, data);
    return response.data;
};

export const closePoll = async (pollId: number): Promise<ApiResponse> => {
    const response = await axiosClient.post(`/api/polls/${pollId}/close`);
    return response.data;
};

export const sendPollEmails = async (pollId: number): Promise<ApiResponse<{ message: string }>> => {
    const response = await axiosClient.post(`/api/polls/${pollId}/send-emails`);
    return response.data;
};
