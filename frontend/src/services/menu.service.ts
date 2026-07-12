import axiosClient from './axios/AxiosBase';
import type { ApiResponse, Caterer, MenuItem } from '@/types';

/* ── Caterers ── */
export const getCaterers = async (): Promise<ApiResponse<Caterer[]>> => {
    const response = await axiosClient.get('/api/caterers');
    return response.data;
};

export const createCaterer = async (data: { name: string; phone?: string; price_per_lunch: number }): Promise<ApiResponse<Caterer>> => {
    const response = await axiosClient.post('/api/caterers', data);
    return response.data;
};

export const updateCaterer = async (id: number, data: { name: string; phone?: string; price_per_lunch: number; is_active?: boolean }): Promise<ApiResponse<Caterer>> => {
    const response = await axiosClient.put(`/api/caterers/${id}`, data);
    return response.data;
};

export const deleteCaterer = async (id: number): Promise<ApiResponse> => {
    const response = await axiosClient.delete(`/api/caterers/${id}`);
    return response.data;
};

/* ── Menu items ── */
export const getMenuItems = async (params?: { catererId?: number; day?: number }): Promise<ApiResponse<MenuItem[]>> => {
    const response = await axiosClient.get('/api/menu-items', {
        params: {
            catererId: params?.catererId,
            day: params?.day,
        },
    });
    return response.data;
};

/** Bulk create set menus for a day. */
export const createMenuItems = async (data: {
    caterer_id: number;
    day_of_week: number;
    items: { name: string; description?: string }[];
}): Promise<ApiResponse<MenuItem[]>> => {
    const response = await axiosClient.post('/api/menu-items', data);
    return response.data;
};

export const updateMenuItem = async (id: number, data: { name: string; description?: string; is_active?: boolean }): Promise<ApiResponse<MenuItem>> => {
    const response = await axiosClient.put(`/api/menu-items/${id}`, data);
    return response.data;
};

export const deleteMenuItem = async (id: number): Promise<ApiResponse> => {
    const response = await axiosClient.delete(`/api/menu-items/${id}`);
    return response.data;
};
