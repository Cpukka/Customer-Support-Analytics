'use client';

import React, { useEffect, useState } from 'react';
import { api } from '@/services/api';

interface KPIData {
  totalTickets: number;
  negativeSentimentRate: number;
  averageResolutionTimeHours: number;
  topIssueCategory: string;
}

const KPISummary = () => {
  const [kpiData, setKpiData] = useState<KPIData>({
    totalTickets: 0,
    negativeSentimentRate: 0,
    averageResolutionTimeHours: 0,
    topIssueCategory: 'Loading...',
  });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const loadData = async () => {
      try {
        const data = await api.getAnalytics();
        setKpiData({
          totalTickets: data.summary.totalTickets || 0,
          negativeSentimentRate: data.summary.negativeSentimentRate || 0,
          averageResolutionTimeHours: data.summary.averageResolutionTimeHours || 0,
          topIssueCategory: data.summary.topIssueCategory || 'N/A',
        });
      } catch (error) {
        console.error('Error fetching KPI data:', error);
      } finally {
        setLoading(false);
      }
    };

    loadData();
  }, []);

  const kpiCards = [
    {
      title: 'Total Tickets',
      value: loading ? '...' : kpiData.totalTickets.toLocaleString(),
      change: '+12%',
      icon: '🎫',
      color: 'bg-blue-100 text-blue-700',
    },
    {
      title: 'Negative Sentiment Rate',
      value: loading ? '...' : `${kpiData.negativeSentimentRate}%`,
      change: '-3%',
      icon: '😡',
      color: 'bg-red-100 text-red-700',
    },
    {
      title: 'Avg. Resolution Time',
      value: loading ? '...' : `${kpiData.averageResolutionTimeHours}h`,
      change: '-8%',
      icon: '⏱️',
      color: 'bg-green-100 text-green-700',
    },
    {
      title: 'Top Issue Category',
      value: loading ? '...' : kpiData.topIssueCategory,
      change: '+5%',
      icon: '🏷️',
      color: 'bg-purple-100 text-purple-700',
    },
  ];

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
      {kpiCards.map((card, index) => (
        <div key={index} className="bg-white rounded-xl shadow-sm p-6 border border-gray-100 hover:shadow-md transition-shadow">
          <div className="flex items-start justify-between">
            <div>
              <p className="text-sm text-gray-500">{card.title}</p>
              {loading ? (
                <div className="h-8 w-20 bg-gray-200 rounded animate-pulse mt-1"></div>
              ) : (
                <p className="text-2xl font-bold text-gray-900 mt-1">{card.value}</p>
              )}
              <div className="flex items-center gap-1 mt-2">
                <span className={`text-xs font-medium ${
                  card.change?.startsWith('+') ? 'text-green-600' : 'text-red-600'
                }`}>
                  {card.change}
                </span>
                <span className="text-xs text-gray-500">from last month</span>
              </div>
            </div>
            <div className={`p-3 rounded-lg ${card.color}`}>
              <span className="text-xl">{card.icon}</span>
            </div>
          </div>
        </div>
      ))}
    </div>
  );
};

export default KPISummary;