'use client';

import React from 'react';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend } from 'recharts';

interface TicketTrendChartProps {
  data: Array<{ date: string; averageSentiment: number; totalTickets: number }>;
}

export default function TicketTrendChart({ data }: TicketTrendChartProps) {
  if (!data || !data.length) {
    return (
      <div className="h-64 flex items-center justify-center text-gray-500 dark:text-gray-400">
        No trend data available
      </div>
    );
  }

  const formattedData = data.map(item => ({
    ...item,
    date: new Date(item.date).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })
  }));

  return (
    <ResponsiveContainer width="100%" height={260}>
      <LineChart data={formattedData}>
        <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
        <XAxis dataKey="date" stroke="#9ca3af" />
        <YAxis 
          domain={[-1, 1]} 
          stroke="#9ca3af"
          tickFormatter={(value) => value.toFixed(2)}
        />
        <Tooltip 
          formatter={(value: any) => {
            if (typeof value === 'number') {
              return [value.toFixed(3), 'Sentiment Score'];
            }
            return [String(value), ''];
          }}
          labelFormatter={(label) => `Date: ${label}`}
        />
        <Legend />
        <Line 
          type="monotone" 
          dataKey="averageSentiment" 
          stroke="#3b82f6" 
          strokeWidth={2}
          dot={{ r: 4 }}
          activeDot={{ r: 8 }}
          name="Average Sentiment"
        />
      </LineChart>
    </ResponsiveContainer>
  );
}