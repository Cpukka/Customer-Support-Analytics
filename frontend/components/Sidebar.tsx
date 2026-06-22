'use client';

import React from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { 
  HomeIcon, 
  TicketIcon, 
  ChartBarIcon, 
  UsersIcon,
  Cog6ToothIcon,
  ArrowRightOnRectangleIcon,
  Squares2X2Icon
} from '@heroicons/react/24/outline';

const Sidebar = () => {
  const pathname = usePathname();

  const isActive = (path: string) => pathname === path;

  const navItems = [
    { name: 'Dashboard', icon: <Squares2X2Icon className="size-5" />, href: '/dashboard' },
    { name: 'Tickets', icon: <TicketIcon className="size-5" />, href: '/tickets' },
    { name: 'Analytics', icon: <ChartBarIcon className="size-5" />, href: '/analytics' },
    { name: 'Customers', icon: <UsersIcon className="size-5" />, href: '/customers' },
    { name: 'Settings', icon: <Cog6ToothIcon className="size-5" />, href: '/settings' },
  ];

  return (
    <aside className="w-64 bg-white dark:bg-gray-800 border-r border-gray-200 dark:border-gray-700 flex flex-col h-screen fixed left-0 top-0">
      {/* Logo */}
      <Link href="/" className="p-6 border-b border-gray-100 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors">
        <h1 className="text-xl font-bold text-gray-800 dark:text-white">SupportAI</h1>
        <p className="text-xs text-gray-500 dark:text-gray-400">Analytics Platform</p>
      </Link>

      {/* Navigation */}
      <nav className="flex-1 p-4 space-y-1">
        <Link
          href="/"
          className={`flex items-center gap-3 px-4 py-3 text-sm font-medium rounded-lg transition-colors ${
            isActive('/')
              ? 'bg-blue-50 dark:bg-blue-900/20 text-blue-700 dark:text-blue-400'
              : 'text-gray-600 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700'
          }`}
        >
          <HomeIcon className="size-5" />
          Home
        </Link>
        {navItems.map((item) => (
          <Link
            key={item.name}
            href={item.href}
            className={`flex items-center gap-3 px-4 py-3 text-sm font-medium rounded-lg transition-colors ${
              isActive(item.href)
                ? 'bg-blue-50 dark:bg-blue-900/20 text-blue-700 dark:text-blue-400'
                : 'text-gray-600 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700'
            }`}
          >
            {item.icon}
            {item.name}
          </Link>
        ))}
      </nav>

      {/* User Profile */}
      <div className="p-4 border-t border-gray-200 dark:border-gray-700">
        <div className="flex items-center gap-3">
          <div className="size-10 rounded-full bg-gradient-to-r from-blue-500 to-purple-500 flex items-center justify-center text-white font-semibold">
            JD
          </div>
          <div className="flex-1">
            <p className="text-sm font-medium text-gray-800 dark:text-white">John Doe</p>
            <p className="text-xs text-gray-500 dark:text-gray-400">Admin</p>
          </div>
          <button className="p-2 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-lg transition-colors">
            <ArrowRightOnRectangleIcon className="size-5 text-gray-500 dark:text-gray-400" />
          </button>
        </div>
      </div>
    </aside>
  );
};

export default Sidebar;