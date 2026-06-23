'use client';

import React, { useState, useEffect } from 'react';
import Sidebar from '@/components/Sidebar';
import KPISummary from '@/components/KPISummary';
import TicketUpload from '@/components/TicketUpload';
import SentimentChart from '@/components/SentimentChart';
import CategoryChart from '@/components/CategoryChart';
import TicketTrendChart from '@/components/TicketTrendChart';
import { api } from '@/services/api';
import { PlusIcon, RefreshCw } from 'lucide-react';

export default function DashboardPage() {
  const [isUploadModalOpen, setIsUploadModalOpen] = useState(false);
  const [isConnected, setIsConnected] = useState(false);
  const [loading, setLoading] = useState(true);
  const [analyticsData, setAnalyticsData] = useState<any>(null);
  const [refreshing, setRefreshing] = useState(false);

  useEffect(() => {
    checkConnection();
    fetchAnalytics();
  }, []);

  const checkConnection = async () => {
    try {
      await api.healthCheck();
      setIsConnected(true);
      console.log('✅ Connected to backend');
    } catch (error) {
      console.error('❌ Backend not connected:', error);
      setIsConnected(false);
    }
  };

  const fetchAnalytics = async () => {
    try {
      setLoading(true);
      const data = await api.getAnalytics();
      setAnalyticsData(data);
    } catch (error) {
      console.error('Error fetching analytics:', error);
      // Set default empty data to avoid errors
      setAnalyticsData({
        summary: {
          totalTickets: 0,
          negativeSentimentRate: 0,
          averageResolutionTimeHours: 0,
          topIssueCategory: 'N/A',
          sentimentDistribution: {}
        },
        categoryDistribution: [],
        sentimentTrend: []
      });
    } finally {
      setLoading(false);
    }
  };

  const handleRefresh = async () => {
    setRefreshing(true);
    await fetchAnalytics();
    setRefreshing(false);
  };

  return (
    <div className="flex min-h-screen bg-gray-50 dark:bg-gray-900">
      <Sidebar />
      
      <main className="flex-1 ml-64 p-8 transition-all duration-300">
        <div className="max-w-7xl mx-auto">
          {/* Header */}
          <header className="flex justify-between items-center mb-8">
            <div>
              <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Dashboard</h1>
              <p className="text-gray-600 dark:text-gray-400 flex items-center gap-2">
                AI-powered customer support analytics overview
                <span className={`inline-flex items-center gap-1 text-xs font-medium px-2 py-0.5 rounded-full ${
                  isConnected ? 'bg-green-100 dark:bg-green-900/30 text-green-700 dark:text-green-400' : 'bg-red-100 dark:bg-red-900/30 text-red-700 dark:text-red-400'
                }`}>
                  <span className={`size-1.5 rounded-full ${isConnected ? 'bg-green-500' : 'bg-red-500'}`}></span>
                  {isConnected ? 'Connected' : 'Disconnected'}
                </span>
              </p>
            </div>
            <div className="flex gap-3">
              <button
                onClick={handleRefresh}
                disabled={refreshing}
                className="px-4 py-2 bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-200 dark:hover:bg-gray-600 transition-colors flex items-center gap-2"
              >
                <RefreshCw className={`size-4 ${refreshing ? 'animate-spin' : ''}`} />
                Refresh
              </button>
              <button
                onClick={() => setIsUploadModalOpen(true)}
                disabled={!isConnected}
                className={`px-4 py-2 rounded-lg transition-colors flex items-center gap-2 font-medium ${
                  isConnected 
                    ? 'bg-blue-600 text-white hover:bg-blue-700' 
                    : 'bg-gray-300 dark:bg-gray-700 text-gray-500 dark:text-gray-400 cursor-not-allowed'
                }`}
              >
                <PlusIcon className="size-5" />
                New Ticket
              </button>
            </div>
          </header>

          {/* Connection Warning */}
          {!isConnected && (
            <div className="mb-6 p-4 bg-yellow-50 dark:bg-yellow-900/20 border border-yellow-200 dark:border-yellow-800 rounded-lg text-yellow-800 dark:text-yellow-200">
              ⚠️ Backend service is not connected. Please make sure the backend is running on http://localhost:5000
            </div>
          )}

          {/* KPIs */}
          <KPISummary />

          {/* Charts Grid */}
          {loading ? (
            <div className="p-8 text-center">
              <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
              <p className="mt-2 text-gray-500 dark:text-gray-400">Loading analytics...</p>
            </div>
          ) : analyticsData ? (
            <div className="mt-6 grid grid-cols-1 lg:grid-cols-2 gap-6">
              {/* Sentiment Chart */}
              <div className="bg-white dark:bg-gray-800 rounded-xl shadow-sm p-6 border border-gray-200 dark:border-gray-700">
                <h3 className="text-lg font-semibold text-gray-800 dark:text-white mb-2">Sentiment Distribution</h3>
                <p className="text-sm text-gray-500 dark:text-gray-400 mb-4">Breakdown of ticket sentiment</p>
                <SentimentChart data={analyticsData.summary?.sentimentDistribution || {}} />
              </div>
              
              {/* Category Chart */}
              <div className="bg-white dark:bg-gray-800 rounded-xl shadow-sm p-6 border border-gray-200 dark:border-gray-700">
                <h3 className="text-lg font-semibold text-gray-800 dark:text-white mb-2">Category Distribution</h3>
                <p className="text-sm text-gray-500 dark:text-gray-400 mb-4">Tickets by issue category</p>
                <CategoryChart data={analyticsData.categoryDistribution || []} />
              </div>
            </div>
          ) : null}

          {/* Trend Chart */}
          {analyticsData?.sentimentTrend && analyticsData.sentimentTrend.length > 0 && (
            <div className="mt-6 bg-white dark:bg-gray-800 rounded-xl shadow-sm p-6 border border-gray-200 dark:border-gray-700">
              <h3 className="text-lg font-semibold text-gray-800 dark:text-white mb-2">Sentiment Trend</h3>
              <p className="text-sm text-gray-500 dark:text-gray-400 mb-4">Average sentiment score over time</p>
              <TicketTrendChart data={analyticsData.sentimentTrend} />
            </div>
          )}
        </div>
      </main>

      {/* Upload Modal */}
      {isUploadModalOpen && (
        <TicketUpload 
          onClose={() => setIsUploadModalOpen(false)}
          onSuccess={() => {
            fetchAnalytics();
          }}
        />
      )}
    </div>
  );
}