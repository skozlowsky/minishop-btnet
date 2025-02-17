import axios from 'axios';

export const API_URL = import.meta.env.services__apigateway__http__0;

export const apiClient = axios.create({
    baseURL: API_URL,
    headers: {
        'Content-Type': 'application/json',
    },
});

// Add interceptor to include user email
apiClient.interceptors.request.use((config) => {
    const userEmail = localStorage.getItem('userEmail') || 'default@example.com';
    config.headers['X-User-Email'] = userEmail;
    return config;
});