'use client';

import React, { useState, useEffect, useCallback } from 'react';
import Sidebar from '@/components/Sidebar';
import { api } from '@/services/api';
import {
  BarChart3,
  Ticket,
  Clock,
  AlertCircle,
  ThumbsUp,
  ThumbsDown,
  Minus,
  Download,
  RefreshCw
} from 'lucide-react';

interface AnalyticsData {
  summary: {
    totalTickets: number;
    negativeSentimentRate: number;
    averageResolutionTimeHours: number;
    topIssueCategory: string;
    sentimentDistribution: Record<string, number>;
  };
  categoryDistribution: Array<{ category: string; count: number }>;
  sentimentTrend: Array<{ date: string; averageSentiment: number; totalTickets: number }>;
}

export default function AnalyticsPage() {
  const [data, setData] = useState<AnalyticsData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [dateRange, setDateRange] = useState('30');

  // Define fetchAnalytics with useCallback to prevent unnecessary re-renders
  const fetchAnalytics = useCallback(async () => {
    try {
      setLoading(true);
      setError('');
      const response = await api.getAnalytics();
      setData(response);
    } catch (err) {
      console.error('Error fetching analytics:', err);
      setError('Failed to load analytics data');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchAnalytics();
  }, [dateRange, fetchAnalytics]);

  const getSentimentIcon = (label: string) => {
    switch (label) {
      case 'Positive': return <ThumbsUp className="size-4 text-green-500" />;
      case 'Negative': return <ThumbsDown className="size-4 text-red-500" />;
      default: return <Minus className="size-4 text-gray-400" />;
    }
  };

  // Stats cards
  const stats = [
    {
      label: 'Total Tickets',
      value: data?.summary.totalTickets || 0,
      icon: <Ticket className="size-6" />,
      color: 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400'
    },
    {
      label: 'Negative Sentiment Rate',
      value: `${data?.summary.negativeSentimentRate || 0}%`,
      icon: <AlertCircle className="size-6" />,
      color: 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400'
    },
    {
      label: 'Avg Resolution Time',
      value: `${data?.summary.averageResolutionTimeHours || 0}h`,
      icon: <Clock className="size-6" />,
      color: 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400'
    },
    {
      label: 'Top Category',
      value: data?.summary.topIssueCategory || 'N/A',
      icon: <BarChart3 className="size-6" />,
      color: 'bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-400'
    },
  ];

  return (
    <div className="flex min-h-screen bg-gray-50 dark:bg-gray-900">
      <Sidebar />
      
      <main className="flex-1 ml-64 p-8">
        <div className="max-w-7xl mx-auto">
          {/* Header */}
          <header className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-8">
            <div>
              <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Analytics</h1>
              <p className="text-gray-600 dark:text-gray-400">Monitor your support performance metrics</p>
            </div>
            <div className="flex gap-3">
              <select
                value={dateRange}
                onChange={(e) => setDateRange(e.target.value)}
                className="px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
              >
                <option value="7">Last 7 days</option>
                <option value="30">Last 30 days</option>
                <option value="90">Last 90 days</option>
              </select>
              <button
                onClick={fetchAnalytics}
                className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors flex items-center gap-2"
              >
                <RefreshCw className="size-4" />
                Refresh
              </button>
              <button className="px-4 py-2 bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-200 dark:hover:bg-gray-600 transition-colors flex items-center gap-2">
                <Download className="size-4" />
                Export
              </button>
            </div>
          </header>

          {/* Error */}
          {error && (
            <div className="mb-6 p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg text-red-700 dark:text-red-400">
              {error}
            </div>
          )}

          {loading ? (
            <div className="p-8 text-center">
              <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
              <p className="mt-2 text-gray-500 dark:text-gray-400">Loading analytics...</p>
            </div>
          ) : data ? (
            <>
              {/* Stats Cards */}
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
                {stats.map((stat, index) => (
                  <div key={index} className="bg-white dark:bg-gray-800 rounded-xl shadow-sm p-6 border border-gray-200 dark:border-gray-700">
                    <div className="flex items-start justify-between">
                      <div>
                        <p className="text-sm text-gray-500 dark:text-gray-400">{stat.label}</p>
                        <p className="text-2xl font-bold text-gray-900 dark:text-white mt-1">{stat.value}</p>
                      </div>
                      <div className={`p-3 rounded-lg ${stat.color}`}>
                        {stat.icon}
                      </div>
                    </div>
                  </div>
                ))}
              </div>

              {/* Charts Grid */}
              <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                {/* Sentiment Distribution */}
                <div className="bg-white dark:bg-gray-800 rounded-xl shadow-sm p-6 border border-gray-200 dark:border-gray-700">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">Sentiment Distribution</h3>
                  <div className="space-y-4">
                    {Object.entries(data.summary.sentimentDistribution || {}).map(([label, count]) => (
                      <div key={label}>
                        <div className="flex justify-between text-sm mb-1">
                          <span className="flex items-center gap-2 text-gray-700 dark:text-gray-300">
                            {getSentimentIcon(label)}
                            {label}
                          </span>
                          <span className="font-medium text-gray-900 dark:text-white">{count}</span>
                        </div>
                        <div className="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-2">
                          <div
                            className={`h-2 rounded-full ${
                              label === 'Positive' ? 'bg-green-500' :
                              label === 'Negative' ? 'bg-red-500' :
                              'bg-gray-500'
                            }`}
                            style={{
                              width: `${(count / (data.summary.totalTickets || 1)) * 100}%`
                            }}
                          />
                        </div>
                      </div>
                    ))}
                  </div>
                </div>

                {/* Category Distribution */}
                <div className="bg-white dark:bg-gray-800 rounded-xl shadow-sm p-6 border border-gray-200 dark:border-gray-700">
                  <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">Category Distribution</h3>
                  <div className="space-y-4">
                    {data.categoryDistribution?.map((item) => (
                      <div key={item.category}>
                        <div className="flex justify-between text-sm mb-1">
                          <span className="text-gray-700 dark:text-gray-300">{item.category}</span>
                          <span className="font-medium text-gray-900 dark:text-white">{item.count}</span>
                        </div>
                        <div className="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-2">
                          <div
                            className="h-2 rounded-full bg-blue-500"
                            style={{
                              width: `${(item.count / (data.summary.totalTickets || 1)) * 100}%`
                            }}
                          />
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              </div>

              {/* Sentiment Trend */}
              <div className="mt-6 bg-white dark:bg-gray-800 rounded-xl shadow-sm p-6 border border-gray-200 dark:border-gray-700">
                <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">Sentiment Trend</h3>
                {data.sentimentTrend && data.sentimentTrend.length > 0 ? (
                  <div className="space-y-4">
                    {data.sentimentTrend.map((item, index) => (
                      <div key={index} className="flex items-center gap-4">
                        <div className="w-32 text-sm text-gray-600 dark:text-gray-400">
                          {new Date(item.date).toLocaleDateString()}
                        </div>
                        <div className="flex-1">
                          <div className="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-2">
                            <div
                              className="h-2 rounded-full"
                              style={{
                                width: `${((item.averageSentiment + 1) / 2) * 100}%`,
                                backgroundColor: item.averageSentiment > 0.3 ? '#22c55e' :
                                               item.averageSentiment < -0.3 ? '#ef4444' : '#6b7280'
                              }}
                            />
                          </div>
                        </div>
                        <div className="w-24 text-sm text-right text-gray-700 dark:text-gray-300">
                          {item.averageSentiment.toFixed(2)}
                        </div>
                      </div>
                    ))}
                  </div>
                ) : (
                  <p className="text-gray-500 dark:text-gray-400">No trend data available</p>
                )}
              </div>
            </>
          ) : (
            <div className="text-center text-gray-500 dark:text-gray-400">
              No data available
            </div>
          )}
        </div>
      </main>
    </div>
  );
}