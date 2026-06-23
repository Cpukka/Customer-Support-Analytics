"use client"

import React from 'react';
import Link from 'next/link';
import Navigation from '@/components/Navigation';
import Footer from '@/components/Footer';
import { ArrowRight, Sparkles, Shield, Zap, BarChart3, Users, MessageSquare, Clock, Brain, TrendingUp, Award } from 'lucide-react';

export default function HomePage() {
  const features = [
    {
      icon: <Brain className="size-8 text-blue-600" />,
      title: 'AI Sentiment Analysis',
      description: 'Automatically analyze customer sentiment with advanced AI to understand emotions and urgency.'
    },
    {
      icon: <BarChart3 className="size-8 text-purple-600" />,
      title: 'Real-time Analytics',
      description: 'Get instant insights into customer support performance with live dashboards and KPIs.'
    },
    {
      icon: <TrendingUp className="size-8 text-green-600" />,
      title: 'Smart Categorization',
      description: 'AI automatically categorizes tickets into Billing, Technical, Account Recovery, and more.'
    },
    {
      icon: <Users className="size-8 text-orange-600" />,
      title: 'Team Collaboration',
      description: 'Empower your support team with tools to collaborate and resolve issues faster.'
    },
    {
      icon: <MessageSquare className="size-8 text-red-600" />,
      title: 'Multi-channel Support',
      description: 'Support your customers across email, chat, and social media from one platform.'
    },
    {
      icon: <Award className="size-8 text-indigo-600" />,
      title: 'Better Customer Experience',
      description: 'Deliver personalized support that improves customer satisfaction and loyalty.'
    },
  ];

  const stats = [
    { value: '98%', label: 'Customer Satisfaction' },
    { value: '50%', label: 'Faster Resolution Time' },
    { value: '10K+', label: 'Tickets Analyzed Daily' },
    { value: '4.9', label: 'Average Rating' },
  ];

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
      <Navigation />
      
      {/* Hero Section */}
      <section className="pt-32 pb-20 px-4 bg-gradient-to-br from-blue-50 via-white to-purple-50 dark:from-gray-900 dark:via-gray-900 dark:to-gray-800">
        <div className="max-w-7xl mx-auto">
          <div className="text-center">
            <div className="inline-flex items-center gap-2 px-4 py-2 bg-blue-100 dark:bg-blue-900/30 rounded-full text-blue-700 dark:text-blue-300 text-sm font-medium mb-6">
              <Sparkles className="size-4" />
              AI-Powered Support Analytics Platform
            </div>
            <h1 className="text-4xl md:text-6xl font-bold text-gray-900 dark:text-white mb-6">
              Transform Your
              <span className="bg-gradient-to-r from-blue-600 to-purple-600 bg-clip-text text-transparent"> Customer Support</span>
              <br />
              with AI Intelligence
            </h1>
            <p className="text-xl text-gray-600 dark:text-gray-300 max-w-2xl mx-auto mb-10">
              Automatically analyze customer sentiment, categorize tickets, and gain real-time insights
              to deliver exceptional support experiences.
            </p>
            <div className="flex flex-col sm:flex-row gap-4 justify-center">
              <Link
                href="/signup"
                className="inline-flex items-center gap-2 px-8 py-4 bg-blue-600 text-white rounded-xl hover:bg-blue-700 transition-all shadow-lg hover:shadow-xl text-lg font-medium"
              >
                Get Started Free
                <ArrowRight className="size-5" />
              </Link>
              <Link
                href="/demo"
                className="inline-flex items-center gap-2 px-8 py-4 bg-white dark:bg-gray-800 text-gray-900 dark:text-white rounded-xl hover:bg-gray-50 dark:hover:bg-gray-700 transition-all shadow-lg hover:shadow-xl text-lg font-medium border border-gray-200 dark:border-gray-700"
              >
                Watch Demo
              </Link>
            </div>
          </div>

          {/* Stats */}
          <div className="grid grid-cols-2 md:grid-cols-4 gap-6 mt-16">
            {stats.map((stat, index) => (
              <div key={index} className="text-center bg-white/80 dark:bg-gray-800/80 backdrop-blur-sm rounded-xl p-6 shadow-sm border border-gray-100 dark:border-gray-700">
                <div className="text-3xl font-bold text-gray-900 dark:text-white">{stat.value}</div>
                <div className="text-sm text-gray-600 dark:text-gray-400">{stat.label}</div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Features Section */}
      <section className="py-20 px-4 bg-white dark:bg-gray-800">
        <div className="max-w-7xl mx-auto">
          <div className="text-center mb-16">
            <h2 className="text-3xl md:text-4xl font-bold text-gray-900 dark:text-white mb-4">
              Everything You Need for
              <span className="bg-gradient-to-r from-blue-600 to-purple-600 bg-clip-text text-transparent"> Smart Support</span>
            </h2>
            <p className="text-xl text-gray-600 dark:text-gray-400 max-w-2xl mx-auto">
              Powerful features that help you understand and improve your customer support operations.
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
            {features.map((feature, index) => (
              <div
                key={index}
                className="group p-6 bg-gray-50 dark:bg-gray-700/50 rounded-2xl hover:shadow-xl transition-all hover:-translate-y-1 border border-gray-100 dark:border-gray-600"
              >
                <div className="mb-4">{feature.icon}</div>
                <h3 className="text-xl font-semibold text-gray-900 dark:text-white mb-2">
                  {feature.title}
                </h3>
                <p className="text-gray-600 dark:text-gray-400">{feature.description}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* How It Works */}
      <section className="py-20 px-4 bg-gray-50 dark:bg-gray-900">
        <div className="max-w-7xl mx-auto">
          <div className="text-center mb-16">
            <h2 className="text-3xl md:text-4xl font-bold text-gray-900 dark:text-white mb-4">
              How It Works
            </h2>
            <p className="text-xl text-gray-600 dark:text-gray-400 max-w-2xl mx-auto">
              Three simple steps to start analyzing your customer support
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            <div className="text-center">
              <div className="size-20 mx-auto bg-blue-100 dark:bg-blue-900/30 rounded-full flex items-center justify-center text-3xl font-bold text-blue-600 dark:text-blue-400 mb-4">
                1
              </div>
              <h3 className="text-xl font-semibold text-gray-900 dark:text-white mb-2">Connect</h3>
              <p className="text-gray-600 dark:text-gray-400">Connect your support channels to start receiving tickets</p>
            </div>
            <div className="text-center">
              <div className="size-20 mx-auto bg-purple-100 dark:bg-purple-900/30 rounded-full flex items-center justify-center text-3xl font-bold text-purple-600 dark:text-purple-400 mb-4">
                2
              </div>
              <h3 className="text-xl font-semibold text-gray-900 dark:text-white mb-2">Analyze</h3>
              <p className="text-gray-600 dark:text-gray-400">AI automatically analyzes sentiment, category, and urgency</p>
            </div>
            <div className="text-center">
              <div className="size-20 mx-auto bg-green-100 dark:bg-green-900/30 rounded-full flex items-center justify-center text-3xl font-bold text-green-600 dark:text-green-400 mb-4">
                3
              </div>
              <h3 className="text-xl font-semibold text-gray-900 dark:text-white mb-2">Optimize</h3>
              <p className="text-gray-600 dark:text-gray-400">Use insights to improve support quality and customer satisfaction</p>
            </div>
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="py-20 px-4 bg-gradient-to-r from-blue-600 to-purple-600">
        <div className="max-w-4xl mx-auto text-center">
          <h2 className="text-3xl md:text-4xl font-bold text-white mb-4">
            Ready to Transform Your Support?
          </h2>
          <p className="text-xl text-blue-100 mb-8">
            Join thousands of companies using AI to deliver exceptional customer support.
          </p>
          <Link
            href="/signup"
            className="inline-flex items-center gap-2 px-8 py-4 bg-white text-blue-600 rounded-xl hover:bg-gray-100 transition-all shadow-lg hover:shadow-xl text-lg font-medium"
          >
            Get Started Now
            <ArrowRight className="size-5" />
          </Link>
        </div>
      </section>

      <Footer />
    </div>
  );
}