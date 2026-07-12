import axios from 'axios';
import { BASE_URL } from '@/lib/config';
import { STORAGE_KEYS } from '@/lib/constants';
import { clearSession } from '@/lib/auth';

const axiosClient = axios.create({
    baseURL: BASE_URL,
    headers: {
        'Content-Type': 'application/json',
    },
});

// Request interceptor to add the access token to every request
axiosClient.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN);
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

// Response interceptor for error handling
axiosClient.interceptors.response.use(
    (response) => {
        return response;
    },
    async (error) => {
        const originalRequest = error.config;

        // Skip logout handling for public auth endpoints
        const publicAuthEndpoints = [
            '/login',
            '/register',
            '/forget-password',
            '/verify-otp',
            '/resend-otp',
            '/reset-password',
            '/shared/'
        ];

        const isPublicEndpoint = originalRequest?.url &&
            publicAuthEndpoints.some(endpoint => originalRequest.url.includes(endpoint));

        if (isPublicEndpoint) {
            return Promise.reject(error);
        }

        const status = error.response?.status;
        if (status === 401) {
            console.warn('[Axios Interceptor] 401 received, logging out');
            logoutUser();
        }

        return Promise.reject(error);
    }
);

// Helper function to clear auth and redirect
const logoutUser = () => {
    clearSession();
    delete axiosClient.defaults.headers.common.Authorization;
    window.location.href = '/login';
};

/** Extract a human-readable message from an API error. */
export const getErrorMessage = (error: any, fallback = 'Something went wrong'): string => {
    return (
        error?.response?.data?.status?.message ||
        error?.response?.data?.message ||
        error?.message ||
        fallback
    );
};

export default axiosClient;
