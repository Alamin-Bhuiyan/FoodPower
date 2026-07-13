import axiosClient from './axios/AxiosBase';
import type { ApiResponse, AuthUser, UserSummary } from '@/types';

export const getUsers = async (): Promise<ApiResponse<UserSummary[]>> => {
    const response = await axiosClient.get('/api/users');
    return response.data;
};

export const getMe = async (): Promise<ApiResponse<UserSummary>> => {
    const response = await axiosClient.get('/api/users/me');
    return response.data;
};

/** Upload the current user's profile photo. `imageBase64` is a base64 data URL. */
export const uploadMyPhoto = async (imageBase64: string): Promise<ApiResponse<AuthUser>> => {
    const response = await axiosClient.post('/api/users/me/photo', { image: imageBase64 });
    return response.data;
};

/** Remove the current user's profile photo. */
export const removeMyPhoto = async (): Promise<ApiResponse<AuthUser>> => {
    const response = await axiosClient.delete('/api/users/me/photo');
    return response.data;
};
