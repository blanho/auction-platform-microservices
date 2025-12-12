'use client';

import Link from 'next/link';
import {
    Gavel,
    Users,
    Flag,
    CreditCard,
    TrendingUp,
    TrendingDown,
    Eye,
    DollarSign,
    AlertTriangle,
    CheckCircle,
} from 'lucide-react';

import { AdminLayout } from '@/components/layout/admin-layout';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';

const stats = [
    {
        title: 'Total Revenue',
        value: '$2.4M',
        change: '+12.5%',
        trend: 'up',
        icon: DollarSign,
        color: 'text-green-500',
    },
    {
        title: 'Active Users',
        value: '12,543',
        change: '+8.2%',
        trend: 'up',
        icon: Users,
        color: 'text-blue-500',
    },
    {
        title: 'Live Auctions',
        value: '342',
        change: '+15.3%',
        trend: 'up',
        icon: Gavel,
        color: 'text-amber-500',
    },
    {
        title: 'Pending Reports',
        value: '8',
        change: '-25%',
        trend: 'down',
        icon: Flag,
        color: 'text-red-500',
    },
];

const recentActions = [
    {
        type: 'auction',
        message: 'New auction created: 2024 Ferrari SF90',
        time: '5 minutes ago',
        status: 'success',
    },
    {
        type: 'report',
        message: 'Fraud report filed for user seller_123',
        time: '15 minutes ago',
        status: 'warning',
    },
    {
        type: 'payment',
        message: 'Withdrawal request approved: $25,000',
        time: '1 hour ago',
        status: 'success',
    },
    {
        type: 'user',
        message: 'New user registration: john_buyer',
        time: '2 hours ago',
        status: 'info',
    },
];

const quickActions = [
    {
        title: 'Manage Auctions',
        description: 'Create, edit, and moderate listings',
        href: '/admin/auctions',
        icon: Gavel,
        color: 'bg-amber-500',
    },
    {
        title: 'User Management',
        description: 'View and manage user accounts',
        href: '/admin/users',
        icon: Users,
        color: 'bg-blue-500',
    },
    {
        title: 'Review Reports',
        description: 'Handle fraud and dispute reports',
        href: '/admin/reports',
        icon: Flag,
        color: 'bg-red-500',
    },
    {
        title: 'Payment Requests',
        description: 'Approve withdrawal requests',
        href: '/admin/payments',
        icon: CreditCard,
        color: 'bg-green-500',
    },
];

export default function AdminPage() {
    return (
        <AdminLayout
            title="Admin Dashboard"
            description="Overview of platform activity and management"
        >
            {/* Stats Grid */}
            <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4 mb-8">
                {stats.map((stat) => (
                    <Card key={stat.title}>
                        <CardHeader className="flex flex-row items-center justify-between pb-2">
                            <CardTitle className="text-sm font-medium text-zinc-600 dark:text-zinc-400">
                                {stat.title}
                            </CardTitle>
                            <stat.icon className={`h-5 w-5 ${stat.color}`} />
                        </CardHeader>
                        <CardContent>
                            <div className="text-3xl font-bold">{stat.value}</div>
                            <div className="flex items-center text-sm mt-1">
                                {stat.trend === 'up' ? (
                                    <TrendingUp className="h-4 w-4 text-green-500 mr-1" />
                                ) : (
                                    <TrendingDown className="h-4 w-4 text-red-500 mr-1" />
                                )}
                                <span
                                    className={
                                        stat.trend === 'up'
                                            ? 'text-green-500'
                                            : 'text-red-500'
                                    }
                                >
                                    {stat.change}
                                </span>
                                <span className="text-zinc-500 ml-1">from last month</span>
                            </div>
                        </CardContent>
                    </Card>
                ))}
            </div>

            {/* Quick Actions & Recent Activity */}
            <div className="grid gap-6 lg:grid-cols-2 mb-8">
                <Card>
                    <CardHeader>
                        <CardTitle>Quick Actions</CardTitle>
                        <CardDescription>
                            Common administrative tasks
                        </CardDescription>
                    </CardHeader>
                    <CardContent>
                        <div className="grid grid-cols-2 gap-4">
                            {quickActions.map((action) => (
                                <Link
                                    key={action.href}
                                    href={action.href}
                                    className="group"
                                >
                                    <div className="p-4 rounded-lg border border-zinc-200 dark:border-zinc-700 hover:border-zinc-300 dark:hover:border-zinc-600 transition-colors">
                                        <div
                                            className={`w-10 h-10 rounded-lg ${action.color} flex items-center justify-center mb-3`}
                                        >
                                            <action.icon className="h-5 w-5 text-white" />
                                        </div>
                                        <h4 className="font-semibold text-sm group-hover:text-amber-500 transition-colors">
                                            {action.title}
                                        </h4>
                                        <p className="text-xs text-zinc-500 mt-1">
                                            {action.description}
                                        </p>
                                    </div>
                                </Link>
                            ))}
                        </div>
                    </CardContent>
                </Card>

                <Card>
                    <CardHeader>
                        <CardTitle>Recent Activity</CardTitle>
                        <CardDescription>Latest platform events</CardDescription>
                    </CardHeader>
                    <CardContent>
                        <div className="space-y-4">
                            {recentActions.map((action, index) => (
                                <div
                                    key={index}
                                    className="flex items-start gap-3 p-3 bg-zinc-50 dark:bg-zinc-800/50 rounded-lg"
                                >
                                    <div
                                        className={`w-8 h-8 rounded-full flex items-center justify-center ${
                                            action.status === 'success'
                                                ? 'bg-green-500/10'
                                                : action.status === 'warning'
                                                ? 'bg-amber-500/10'
                                                : 'bg-blue-500/10'
                                        }`}
                                    >
                                        {action.status === 'success' ? (
                                            <CheckCircle className="h-4 w-4 text-green-500" />
                                        ) : action.status === 'warning' ? (
                                            <AlertTriangle className="h-4 w-4 text-amber-500" />
                                        ) : (
                                            <Eye className="h-4 w-4 text-blue-500" />
                                        )}
                                    </div>
                                    <div className="flex-1 min-w-0">
                                        <p className="text-sm font-medium truncate">
                                            {action.message}
                                        </p>
                                        <p className="text-xs text-zinc-500">
                                            {action.time}
                                        </p>
                                    </div>
                                </div>
                            ))}
                        </div>
                        <Button variant="outline" className="w-full mt-4">
                            View All Activity
                        </Button>
                    </CardContent>
                </Card>
            </div>

            {/* Platform Health */}
            <Card>
                <CardHeader>
                    <CardTitle>Platform Health</CardTitle>
                    <CardDescription>System status and metrics</CardDescription>
                </CardHeader>
                <CardContent>
                    <div className="grid gap-4 md:grid-cols-4">
                        <div className="p-4 bg-green-500/10 rounded-lg">
                            <div className="flex items-center gap-2 mb-2">
                                <div className="w-2 h-2 bg-green-500 rounded-full animate-pulse" />
                                <span className="text-sm font-medium">API Status</span>
                            </div>
                            <p className="text-2xl font-bold text-green-600">Healthy</p>
                        </div>
                        <div className="p-4 bg-green-500/10 rounded-lg">
                            <div className="flex items-center gap-2 mb-2">
                                <div className="w-2 h-2 bg-green-500 rounded-full animate-pulse" />
                                <span className="text-sm font-medium">Database</span>
                            </div>
                            <p className="text-2xl font-bold text-green-600">Connected</p>
                        </div>
                        <div className="p-4 bg-green-500/10 rounded-lg">
                            <div className="flex items-center gap-2 mb-2">
                                <div className="w-2 h-2 bg-green-500 rounded-full animate-pulse" />
                                <span className="text-sm font-medium">Cache</span>
                            </div>
                            <p className="text-2xl font-bold text-green-600">Active</p>
                        </div>
                        <div className="p-4 bg-amber-500/10 rounded-lg">
                            <div className="flex items-center gap-2 mb-2">
                                <div className="w-2 h-2 bg-amber-500 rounded-full animate-pulse" />
                                <span className="text-sm font-medium">Queue</span>
                            </div>
                            <p className="text-2xl font-bold text-amber-600">12 Jobs</p>
                        </div>
                    </div>
                </CardContent>
            </Card>
        </AdminLayout>
    );
}
