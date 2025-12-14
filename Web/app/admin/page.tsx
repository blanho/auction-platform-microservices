'use client';

import { useState, useEffect, useCallback } from 'react';
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
    RefreshCw,
    FolderTree,
} from 'lucide-react';

import { formatCurrency, formatNumber, formatRelativeTime } from '@/utils';
import { MESSAGES } from '@/constants';

import { AdminLayout } from '@/components/layout/admin-layout';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { toast } from 'sonner';

import {
    adminDashboardService,
    AdminDashboardStats,
    AdminRecentActivity,
    PlatformHealth,
} from '@/services/admin-dashboard.service';

const quickActions = [
    {
        title: 'Manage Auctions',
        description: 'Create, edit, and moderate listings',
        href: '/admin/auctions',
        icon: Gavel,
        color: 'bg-amber-500',
    },
    {
        title: 'Categories',
        description: 'Manage auction categories',
        href: '/admin/categories',
        icon: FolderTree,
        color: 'bg-purple-500',
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

function StatCardSkeleton() {
    return (
        <Card>
            <CardHeader className="flex flex-row items-center justify-between pb-2">
                <Skeleton className="h-4 w-24" />
                <Skeleton className="h-5 w-5 rounded" />
            </CardHeader>
            <CardContent>
                <Skeleton className="h-8 w-20 mb-2" />
                <Skeleton className="h-4 w-32" />
            </CardContent>
        </Card>
    );
}

function ActivitySkeleton() {
    return (
        <div className="flex items-start gap-3 p-3 bg-zinc-50 dark:bg-zinc-800/50 rounded-lg">
            <Skeleton className="w-8 h-8 rounded-full" />
            <div className="flex-1">
                <Skeleton className="h-4 w-3/4 mb-2" />
                <Skeleton className="h-3 w-20" />
            </div>
        </div>
    );
}

function getStatusIcon(status: AdminRecentActivity['status']) {
    switch (status) {
        case 'success':
            return <CheckCircle className="h-4 w-4 text-green-500" />;
        case 'warning':
            return <AlertTriangle className="h-4 w-4 text-amber-500" />;
        case 'error':
            return <AlertTriangle className="h-4 w-4 text-red-500" />;
        default:
            return <Eye className="h-4 w-4 text-blue-500" />;
    }
}

function getStatusBgColor(status: AdminRecentActivity['status']) {
    switch (status) {
        case 'success':
            return 'bg-green-500/10';
        case 'warning':
            return 'bg-amber-500/10';
        case 'error':
            return 'bg-red-500/10';
        default:
            return 'bg-blue-500/10';
    }
}

function getHealthStatusColor(status: string) {
    switch (status) {
        case 'healthy':
        case 'connected':
        case 'active':
            return { bg: 'bg-green-500/10', text: 'text-green-600', dot: 'bg-green-500' };
        case 'warning':
        case 'degraded':
            return { bg: 'bg-amber-500/10', text: 'text-amber-600', dot: 'bg-amber-500' };
        default:
            return { bg: 'bg-red-500/10', text: 'text-red-600', dot: 'bg-red-500' };
    }
}

export default function AdminPage() {
    const [stats, setStats] = useState<AdminDashboardStats | null>(null);
    const [recentActivity, setRecentActivity] = useState<AdminRecentActivity[]>([]);
    const [platformHealth, setPlatformHealth] = useState<PlatformHealth | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [isRefreshing, setIsRefreshing] = useState(false);

    const fetchData = useCallback(async (showRefresh = false) => {
        if (showRefresh) {
            setIsRefreshing(true);
        } else {
            setIsLoading(true);
        }

        try {
            const [statsResult, activityResult, healthResult] = await Promise.allSettled([
                adminDashboardService.getStats(),
                adminDashboardService.getRecentActivity(5),
                adminDashboardService.getPlatformHealth(),
            ]);

            if (statsResult.status === 'fulfilled') {
                setStats(statsResult.value);
            }
            if (activityResult.status === 'fulfilled') {
                setRecentActivity(activityResult.value ?? []);
            }
            if (healthResult.status === 'fulfilled') {
                setPlatformHealth(healthResult.value);
            }

            const hasErrors = [statsResult, activityResult, healthResult].some(
                r => r.status === 'rejected'
            );
            if (hasErrors) {
                toast.error(MESSAGES.ERROR.GENERIC);
            }
        } catch {
            toast.error(MESSAGES.ERROR.GENERIC);
        } finally {
            setIsLoading(false);
            setIsRefreshing(false);
        }
    }, []);

    useEffect(() => {
        fetchData();
    }, [fetchData]);

    const statsConfig = stats
        ? [
            {
                title: 'Total Revenue',
                value: formatCurrency(stats.totalRevenue),
                change: `${stats.revenueChange >= 0 ? '+' : ''}${stats.revenueChange.toFixed(1)}%`,
                trend: stats.revenueChange >= 0 ? 'up' : 'down',
                icon: DollarSign,
                color: 'text-green-500',
            },
            {
                title: 'Active Users',
                value: formatNumber(stats.activeUsers),
                change: `${stats.activeUsersChange >= 0 ? '+' : ''}${stats.activeUsersChange.toFixed(1)}%`,
                trend: stats.activeUsersChange >= 0 ? 'up' : 'down',
                icon: Users,
                color: 'text-blue-500',
            },
            {
                title: 'Live Auctions',
                value: formatNumber(stats.liveAuctions),
                change: `${stats.liveAuctionsChange >= 0 ? '+' : ''}${stats.liveAuctionsChange.toFixed(1)}%`,
                trend: stats.liveAuctionsChange >= 0 ? 'up' : 'down',
                icon: Gavel,
                color: 'text-amber-500',
            },
            {
                title: 'Pending Reports',
                value: formatNumber(stats.pendingReports),
                change: `${stats.pendingReportsChange >= 0 ? '+' : ''}${stats.pendingReportsChange.toFixed(1)}%`,
                trend: stats.pendingReportsChange <= 0 ? 'up' : 'down',
                icon: Flag,
                color: 'text-red-500',
            },
        ]
        : [];

    return (
        <AdminLayout>
            <div className="p-6 lg:p-8 space-y-6">
                <div className="flex items-center justify-between">
                    <div>
                        <h1 className="text-3xl font-bold text-zinc-900 dark:text-white">Admin Dashboard</h1>
                        <p className="text-zinc-500 mt-1">Overview of platform activity and management</p>
                    </div>
                    <Button
                        variant="outline"
                        size="sm"
                        onClick={() => fetchData(true)}
                        disabled={isRefreshing}
                    >
                        <RefreshCw className={`h-4 w-4 mr-2 ${isRefreshing ? 'animate-spin' : ''}`} />
                        Refresh
                    </Button>
                </div>

                <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
                {isLoading ? (
                    <>
                        <StatCardSkeleton />
                        <StatCardSkeleton />
                        <StatCardSkeleton />
                        <StatCardSkeleton />
                    </>
                ) : (
                    statsConfig.map((stat) => (
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
                    ))
                )}
            </div>

            <div className="grid gap-6 lg:grid-cols-2 mb-8">
                <Card>
                    <CardHeader>
                        <CardTitle>Quick Actions</CardTitle>
                        <CardDescription>Common administrative tasks</CardDescription>
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
                            {isLoading ? (
                                <>
                                    <ActivitySkeleton />
                                    <ActivitySkeleton />
                                    <ActivitySkeleton />
                                    <ActivitySkeleton />
                                </>
                            ) : !recentActivity || recentActivity.length === 0 ? (
                                <p className="text-sm text-zinc-500 text-center py-4">
                                    No recent activity
                                </p>
                            ) : (
                                recentActivity.map((action) => (
                                    <div
                                        key={action.id}
                                        className="flex items-start gap-3 p-3 bg-zinc-50 dark:bg-zinc-800/50 rounded-lg"
                                    >
                                        <div
                                            className={`w-8 h-8 rounded-full flex items-center justify-center ${getStatusBgColor(action.status)}`}
                                        >
                                            {getStatusIcon(action.status)}
                                        </div>
                                        <div className="flex-1 min-w-0">
                                            <p className="text-sm font-medium truncate">
                                                {action.message}
                                            </p>
                                            <p className="text-xs text-zinc-500">
                                                {formatRelativeTime(action.timestamp)}
                                            </p>
                                        </div>
                                    </div>
                                ))
                            )}
                        </div>
                        <Button variant="outline" className="w-full mt-4">
                            View All Activity
                        </Button>
                    </CardContent>
                </Card>
            </div>

            <Card>
                <CardHeader>
                    <CardTitle>Platform Health</CardTitle>
                    <CardDescription>System status and metrics</CardDescription>
                </CardHeader>
                <CardContent>
                    {isLoading ? (
                        <div className="grid gap-4 md:grid-cols-4">
                            {[1, 2, 3, 4].map((i) => (
                                <Skeleton key={i} className="h-24 rounded-lg" />
                            ))}
                        </div>
                    ) : platformHealth ? (
                        <div className="grid gap-4 md:grid-cols-4">
                            <div className={`p-4 ${getHealthStatusColor(platformHealth.apiStatus).bg} rounded-lg`}>
                                <div className="flex items-center gap-2 mb-2">
                                    <div className={`w-2 h-2 ${getHealthStatusColor(platformHealth.apiStatus).dot} rounded-full animate-pulse`} />
                                    <span className="text-sm font-medium">API Status</span>
                                </div>
                                <p className={`text-2xl font-bold ${getHealthStatusColor(platformHealth.apiStatus).text}`}>
                                    {platformHealth.apiStatus === 'healthy' ? 'Healthy' : platformHealth.apiStatus}
                                </p>
                            </div>
                            <div className={`p-4 ${getHealthStatusColor(platformHealth.databaseStatus).bg} rounded-lg`}>
                                <div className="flex items-center gap-2 mb-2">
                                    <div className={`w-2 h-2 ${getHealthStatusColor(platformHealth.databaseStatus).dot} rounded-full animate-pulse`} />
                                    <span className="text-sm font-medium">Database</span>
                                </div>
                                <p className={`text-2xl font-bold ${getHealthStatusColor(platformHealth.databaseStatus).text}`}>
                                    {platformHealth.databaseStatus === 'connected' ? 'Connected' : platformHealth.databaseStatus}
                                </p>
                            </div>
                            <div className={`p-4 ${getHealthStatusColor(platformHealth.cacheStatus).bg} rounded-lg`}>
                                <div className="flex items-center gap-2 mb-2">
                                    <div className={`w-2 h-2 ${getHealthStatusColor(platformHealth.cacheStatus).dot} rounded-full animate-pulse`} />
                                    <span className="text-sm font-medium">Cache</span>
                                </div>
                                <p className={`text-2xl font-bold ${getHealthStatusColor(platformHealth.cacheStatus).text}`}>
                                    {platformHealth.cacheStatus === 'active' ? 'Active' : platformHealth.cacheStatus}
                                </p>
                            </div>
                            <div className={`p-4 ${getHealthStatusColor(platformHealth.queueStatus).bg} rounded-lg`}>
                                <div className="flex items-center gap-2 mb-2">
                                    <div className={`w-2 h-2 ${getHealthStatusColor(platformHealth.queueStatus).dot} rounded-full animate-pulse`} />
                                    <span className="text-sm font-medium">Queue</span>
                                </div>
                                <p className={`text-2xl font-bold ${getHealthStatusColor(platformHealth.queueStatus).text}`}>
                                    {platformHealth.queueJobCount} Jobs
                                </p>
                            </div>
                        </div>
                    ) : (
                        <p className="text-sm text-zinc-500 text-center py-4">
                            Unable to load platform health
                        </p>
                    )}
                </CardContent>
            </Card>
            </div>
        </AdminLayout>
    );
}
