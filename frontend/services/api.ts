import axios from 'axios';
import { UploadTicketRequest } from '@/types';

// Use environment variable or fallback
const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

console.log('🌐 API Base URL:', API_BASE_URL);

// API client for all endpoints
const apiClient = axios.create({
  baseURL: `${API_BASE_URL}/api`,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 10000,
});

// Health client (no /api prefix)
const healthClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 5000,
});

// Request interceptor for logging
apiClient.interceptors.request.use(
  (config) => {
    console.log(`🚀 ${config.method?.toUpperCase()} ${config.url}`);
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor for error handling
apiClient.interceptors.response.use(
  (response) => {
    console.log(`✅ ${response.status} ${response.config.url}`);
    return response;
  },
  (error) => {
    if (error.code === 'ECONNREFUSED') {
      console.error('🚫 Backend connection refused - is it running on port 5000?');
    } else if (error.response) {
      console.error(`❌ API Error ${error.response.status}:`, error.response.data);
    } else if (error.request) {
      console.error('❌ No response from server:', error.message);
    } else {
      console.error('❌ API Error:', error.message);
    }
    return Promise.reject(error);
  }
);

export const api = {
  // Health check
  healthCheck: async () => {
    try {
      console.log('🔍 Checking health at:', `${API_BASE_URL}/health`);
      const response = await healthClient.get('/health');
      console.log('✅ Health check successful:', response.data);
      return response.data;
    } catch (error: any) {
      console.error('❌ Health check failed:', error.message);
      if (error.code === 'ECONNREFUSED') {
        console.error('   Backend is not running on http://localhost:5000');
        console.error('   Please start the backend with: dotnet run --urls "http://localhost:5000"');
      }
      throw error;
    }
  },

  // Auth endpoints
  register: async (data: { name: string; email: string; password: string }) => {
    try {
      console.log('📝 Registering user:', data.email);
      const response = await apiClient.post('/auth/register', data);
      console.log('✅ Registration successful');
      return response.data;
    } catch (error: any) {
      console.error('❌ Registration failed:', error.response?.data || error.message);
      throw error;
    }
  },

  login: async (data: { email: string; password: string }) => {
    try {
      console.log('🔑 Logging in:', data.email);
      const response = await apiClient.post('/auth/login', data);
      console.log('✅ Login successful');
      return response.data;
    } catch (error: any) {
      console.error('❌ Login failed:', error.response?.data || error.message);
      throw error;
    }
  },

  getCurrentUser: async () => {
    try {
      const response = await apiClient.get('/auth/me');
      return response.data;
    } catch (error: any) {
      console.error('❌ Failed to get user:', error.response?.data || error.message);
      throw error;
    }
  },

  // Ticket endpoints
  getTickets: async (page = 1, pageSize = 20) => {
    try {
      const response = await apiClient.get(`/tickets?page=${page}&pageSize=${pageSize}`);
      return response.data;
    } catch (error) {
      console.error('Error fetching tickets:', error);
      return { data: [], pagination: { totalCount: 0, currentPage: 1, pageSize: 20, totalPages: 0 } };
    }
  },

  getTicket: async (id: number) => {
    const response = await apiClient.get(`/tickets/${id}`);
    return response.data;
  },

  uploadTicket: async (data: UploadTicketRequest) => {
    const response = await apiClient.post('/tickets/upload', data);
    return response.data;
  },

  getAnalytics: async () => {
    try {
      const response = await apiClient.get('/tickets/analytics');
      return response.data;
    } catch (error) {
      console.error('Error fetching analytics:', error);
      return {
        summary: {
          totalTickets: 0,
          negativeSentimentRate: 0,
          averageResolutionTimeHours: 0,
          topIssueCategory: 'N/A',
          sentimentDistribution: {}
        },
        categoryDistribution: [],
        sentimentTrend: []
      };
    }
  },

  updateTicket: async (id: number, data: { status?: string; priority?: string }) => {
    const response = await apiClient.put(`/tickets/${id}`, data);
    return response.data;
  },

  // Customer endpoints
  getCustomers: async (page = 1, pageSize = 20) => {
    try {
      const response = await apiClient.get(`/customers?page=${page}&pageSize=${pageSize}`);
      return response.data;
    } catch (error) {
      console.error('Error fetching customers:', error);
      return { data: [], pagination: { totalCount: 0, currentPage: 1, pageSize: 20, totalPages: 0 } };
    }
  },

  getCustomer: async (id: number) => {
    try {
      const response = await apiClient.get(`/customers/${id}`);
      return response.data;
    } catch (error) {
      console.error('Error fetching customer:', error);
      throw error;
    }
  },

  getCustomerTickets: async (id: number) => {
    try {
      const response = await apiClient.get(`/customers/${id}/tickets`);
      return response.data;
    } catch (error) {
      console.error('Error fetching customer tickets:', error);
      throw error;
    }
  },

  // Logout helper
  logout: () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    window.location.href = '/login';
  }
};