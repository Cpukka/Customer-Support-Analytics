'use client';

import React, { useState } from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { useTheme } from 'next-themes';
import {
  Squares2X2Icon,
  TicketIcon,
  ChartBarIcon,
  UsersIcon,
  Cog6ToothIcon,
  ArrowRightOnRectangleIcon,
  SunIcon,
  MoonIcon,
  ComputerDesktopIcon,
  ChevronDownIcon,
  ChevronRightIcon
} from '@heroicons/react/24/outline';
import { useRouter } from 'next/navigation';

const Sidebar = () => {
  const pathname = usePathname();
  const router = useRouter();
  const { theme, setTheme } = useTheme();
  const [mounted, setMounted] = React.useState(false);
  const [collapsed, setCollapsed] = useState(false);
  const [expandedSections, setExpandedSections] = useState<string[]>(['main']);

  React.useEffect(() => {
    setMounted(true);
  }, []);

  const isActive = (path: string) => pathname === path;

  const toggleSection = (section: string) => {
    setExpandedSections(prev =>
      prev.includes(section)
        ? prev.filter(s => s !== section)
        : [...prev, section]
    );
  };

  const navSections = [
    {
      id: 'main',
      label: 'Main',
      items: [
        { name: 'Dashboard', icon: <Squares2X2Icon className="size-5" />, href: '/dashboard' },
        { name: 'Tickets', icon: <TicketIcon className="size-5" />, href: '/tickets' },
        { name: 'Analytics', icon: <ChartBarIcon className="size-5" />, href: '/analytics' },
      ]
    },
    {
      id: 'management',
      label: 'Management',
      items: [
        { name: 'Customers', icon: <UsersIcon className="size-5" />, href: '/customers' },
        { name: 'Settings', icon: <Cog6ToothIcon className="size-5" />, href: '/settings' },
      ]
    }
  ];

  const handleLogout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    router.push('/login');
  };

  if (!mounted) return null;

  return (
    <aside className={`bg-white dark:bg-gray-800 border-r border-gray-200 dark:border-gray-700 flex flex-col h-screen fixed left-0 top-0 transition-all duration-300 ${collapsed ? 'w-20' : 'w-64'}`}>
      {/* Logo */}
      <div className={`p-6 border-b border-gray-100 dark:border-gray-700 flex items-center ${collapsed ? 'justify-center' : 'justify-between'}`}>
        <Link href="/dashboard" className="flex items-center gap-2">
          <div className="size-10 rounded-xl bg-gradient-to-r from-blue-600 to-purple-600 flex items-center justify-center flex-shrink-0">
            <span className="text-white font-bold text-xl">AI</span>
          </div>
          {!collapsed && (
            <span className="text-xl font-bold bg-gradient-to-r from-blue-600 to-purple-600 bg-clip-text text-transparent">
              SupportAI
            </span>
          )}
        </Link>
        <button
          onClick={() => setCollapsed(!collapsed)}
          className={`p-1.5 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors ${collapsed ? 'hidden' : 'block'}`}
        >
          <ChevronLeftIcon className="size-4 text-gray-500" />
        </button>
      </div>

      {/* Navigation */}
      <nav className="flex-1 overflow-y-auto p-4 space-y-4">
        {navSections.map((section) => (
          <div key={section.id}>
            {!collapsed && (
              <button
                onClick={() => toggleSection(section.id)}
                className="flex items-center justify-between w-full text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider hover:text-gray-700 dark:hover:text-gray-300 transition-colors mb-2"
              >
                <span>{section.label}</span>
                {expandedSections.includes(section.id) ? (
                  <ChevronDownIcon className="size-3" />
                ) : (
                  <ChevronRightIcon className="size-3" />
                )}
              </button>
            )}
            <div className="space-y-1">
              {section.items.map((item) => (
                <Link
                  key={item.name}
                  href={item.href}
                  className={`flex items-center gap-3 px-4 py-3 text-sm font-medium rounded-lg transition-all duration-200 group ${
                    isActive(item.href)
                      ? 'bg-blue-50 dark:bg-blue-900/20 text-blue-700 dark:text-blue-400 shadow-sm'
                      : 'text-gray-600 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700 hover:text-gray-900 dark:hover:text-white'
                  } ${collapsed ? 'justify-center px-2' : ''}`}
                >
                  <span className={`${isActive(item.href) ? 'text-blue-700 dark:text-blue-400' : 'text-gray-500 dark:text-gray-400 group-hover:text-gray-700 dark:group-hover:text-gray-300'}`}>
                    {item.icon}
                  </span>
                  {!collapsed && <span>{item.name}</span>}
                  {collapsed && (
                    <div className="absolute left-20 hidden group-hover:block bg-gray-900 dark:bg-gray-700 text-white text-sm px-2 py-1 rounded whitespace-nowrap z-50">
                      {item.name}
                    </div>
                  )}
                </Link>
              ))}
            </div>
          </div>
        ))}
      </nav>

      {/* Bottom Section */}
      <div className="p-4 border-t border-gray-200 dark:border-gray-700 space-y-2">
        {/* Theme Toggle */}
        <div className={`flex ${collapsed ? 'justify-center' : 'gap-2'} p-2 bg-gray-50 dark:bg-gray-700/50 rounded-lg`}>
          <button
            onClick={() => setTheme('light')}
            className={`p-1.5 rounded-lg transition-all ${theme === 'light' ? 'bg-white dark:bg-gray-600 shadow-sm' : 'hover:bg-white/50 dark:hover:bg-gray-600/50'}`}
            title="Light mode"
          >
            <SunIcon className="size-4 text-yellow-500" />
          </button>
          <button
            onClick={() => setTheme('dark')}
            className={`p-1.5 rounded-lg transition-all ${theme === 'dark' ? 'bg-white dark:bg-gray-600 shadow-sm' : 'hover:bg-white/50 dark:hover:bg-gray-600/50'}`}
            title="Dark mode"
          >
            <MoonIcon className="size-4 text-indigo-400" />
          </button>
          <button
            onClick={() => setTheme('system')}
            className={`p-1.5 rounded-lg transition-all ${theme === 'system' ? 'bg-white dark:bg-gray-600 shadow-sm' : 'hover:bg-white/50 dark:hover:bg-gray-600/50'}`}
            title="System preference"
          >
            <ComputerDesktopIcon className="size-4 text-gray-500" />
          </button>
        </div>

        {/* User Profile */}
        <div className={`flex items-center ${collapsed ? 'justify-center' : 'gap-3'} p-2 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors cursor-pointer`}>
          <div className="size-8 rounded-full bg-gradient-to-r from-blue-500 to-purple-500 flex items-center justify-center text-white font-semibold text-sm flex-shrink-0">
            JD
          </div>
          {!collapsed && (
            <div className="flex-1 min-w-0">
              <p className="text-sm font-medium text-gray-800 dark:text-white truncate">John Doe</p>
              <p className="text-xs text-gray-500 dark:text-gray-400 truncate">Admin</p>
            </div>
          )}
          {!collapsed && (
            <button
              onClick={handleLogout}
              className="p-1.5 hover:bg-gray-100 dark:hover:bg-gray-600 rounded-lg transition-colors"
            >
              <ArrowRightOnRectangleIcon className="size-4 text-gray-500" />
            </button>
          )}
        </div>
      </div>
    </aside>
  );
};

// Need to import ChevronLeftIcon
import { ChevronLeftIcon } from '@heroicons/react/24/outline';

export default Sidebar;