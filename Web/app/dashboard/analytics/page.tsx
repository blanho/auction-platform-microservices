"use client";

import { useEffect, useState } from "react";
import { useSession } from "next-auth/react";
import { useRouter } from "next/navigation";
import {
    Loader2,
    DollarSign,
    Package,
    Eye,
    TrendingUp,
    TrendingDown,
    BarChart3,
} from "lucide-react";

import { formatCurrency, formatNumber } from "@/utils";
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/components/ui/card";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";
import { DashboardLayout } from "@/components/layout/dashboard-layout";

interface AnalyticsData {
    totalRevenue: number;
    revenueChange: number;
    itemsSold: number;
    itemsChange: number;
    activeListings: number;
    viewsToday: number;
    viewsChange: number;
    topListings: {
        id: string;
        title: string;
        currentBid: number;
        views: number;
        bids: number;
    }[];
    chartData: {
        date: string;
        revenue: number;
        bids?: number;
    }[];
}

function SimpleBarChart({ data }: { data: { date: string; revenue: number }[] }) {
    const maxValue = Math.max(...data.map((d) => d.revenue));

    return (
        <div className="flex items-end justify-between gap-2 h-48 pt-8">
            {data.map((item, index) => (
                <div
                    key={index}
                    className="flex-1 flex flex-col items-center gap-2"
                >
                    <div
                        className="w-full bg-amber-500 rounded-t-sm transition-all hover:bg-amber-600"
                        style={{
                            height: `${(item.revenue / maxValue) * 100}%`,
                            minHeight: "4px",
                        }}
                    />
                    <span className="text-xs text-zinc-500">{item.date}</span>
                </div>
            ))}
        </div>
    );
}

export default function AnalyticsPage() {
    const { status } = useSession();
    const router = useRouter();
    const [isLoading, setIsLoading] = useState(true);
    const [timeRange, setTimeRange] = useState("30d");
    const [analytics, setAnalytics] = useState<AnalyticsData | null>(null);

    useEffect(() => {
        if (status === "unauthenticated") {
            router.push("/auth/signin?callbackUrl=/dashboard/analytics");
            return;
        }

        // Simulate API call - replace with actual analytics service
        setTimeout(() => {
            setAnalytics({
                totalRevenue: 125000,
                revenueChange: 12.5,
                itemsSold: 23,
                itemsChange: 8,
                activeListings: 5,
                viewsToday: 342,
                viewsChange: -3.2,
                topListings: [
                    {
                        id: "1",
                        title: "2024 Porsche 911 GT3",
                        currentBid: 195000,
                        views: 1250,
                        bids: 24,
                    },
                    {
                        id: "2",
                        title: "Vintage Rolex Daytona",
                        currentBid: 45000,
                        views: 890,
                        bids: 18,
                    },
                    {
                        id: "3",
                        title: "2023 BMW M4 Competition",
                        currentBid: 78000,
                        views: 654,
                        bids: 12,
                    },
                ],
                chartData: [
                    { date: "Mon", revenue: 12000 },
                    { date: "Tue", revenue: 8500 },
                    { date: "Wed", revenue: 15000 },
                    { date: "Thu", revenue: 22000 },
                    { date: "Fri", revenue: 18500 },
                    { date: "Sat", revenue: 28000 },
                    { date: "Sun", revenue: 21000 },
                ],
            });
            setIsLoading(false);
        }, 500);
    }, [status, router, timeRange]);

    if (status === "loading" || isLoading) {
        return (
            <DashboardLayout
                title="Analytics"
                description="Track your sales performance"
            >
                <div className="flex justify-center py-12">
                    <Loader2 className="h-8 w-8 animate-spin text-amber-500" />
                </div>
            </DashboardLayout>
        );
    }

    return (
        <DashboardLayout
            title="Analytics"
            description="Track your sales performance"
        >
            {/* Time Range Selector */}
            <div className="flex justify-end mb-6">
                <Select value={timeRange} onValueChange={setTimeRange}>
                    <SelectTrigger className="w-[180px]">
                        <SelectValue placeholder="Select range" />
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem value="7d">Last 7 days</SelectItem>
                        <SelectItem value="30d">Last 30 days</SelectItem>
                        <SelectItem value="90d">Last 90 days</SelectItem>
                        <SelectItem value="1y">Last year</SelectItem>
                    </SelectContent>
                </Select>
            </div>

            {/* Stats Cards */}
            <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4 mb-8">
                <Card>
                    <CardHeader className="flex flex-row items-center justify-between pb-2">
                        <CardTitle className="text-sm font-medium text-zinc-600 dark:text-zinc-400">
                            Total Revenue
                        </CardTitle>
                        <DollarSign className="h-5 w-5 text-green-500" />
                    </CardHeader>
                    <CardContent>
                        <div className="text-3xl font-bold">
                            {formatCurrency(analytics?.totalRevenue ?? 0)}
                        </div>
                        <div className="flex items-center text-sm mt-1">
                            {(analytics?.revenueChange || 0) > 0 ? (
                                <TrendingUp className="h-4 w-4 text-green-500 mr-1" />
                            ) : (
                                <TrendingDown className="h-4 w-4 text-red-500 mr-1" />
                            )}
                            <span
                                className={
                                    (analytics?.revenueChange || 0) > 0
                                        ? "text-green-500"
                                        : "text-red-500"
                                }
                            >
                                {analytics?.revenueChange}%
                            </span>
                            <span className="text-zinc-500 ml-1">
                                from last period
                            </span>
                        </div>
                    </CardContent>
                </Card>

                <Card>
                    <CardHeader className="flex flex-row items-center justify-between pb-2">
                        <CardTitle className="text-sm font-medium text-zinc-600 dark:text-zinc-400">
                            Items Sold
                        </CardTitle>
                        <Package className="h-5 w-5 text-blue-500" />
                    </CardHeader>
                    <CardContent>
                        <div className="text-3xl font-bold">
                            {analytics?.itemsSold}
                        </div>
                        <div className="flex items-center text-sm mt-1">
                            {(analytics?.itemsChange || 0) > 0 ? (
                                <TrendingUp className="h-4 w-4 text-green-500 mr-1" />
                            ) : (
                                <TrendingDown className="h-4 w-4 text-red-500 mr-1" />
                            )}
                            <span
                                className={
                                    (analytics?.itemsChange || 0) > 0
                                        ? "text-green-500"
                                        : "text-red-500"
                                }
                            >
                                {analytics?.itemsChange}%
                            </span>
                            <span className="text-zinc-500 ml-1">
                                from last period
                            </span>
                        </div>
                    </CardContent>
                </Card>

                <Card>
                    <CardHeader className="flex flex-row items-center justify-between pb-2">
                        <CardTitle className="text-sm font-medium text-zinc-600 dark:text-zinc-400">
                            Active Listings
                        </CardTitle>
                        <BarChart3 className="h-5 w-5 text-amber-500" />
                    </CardHeader>
                    <CardContent>
                        <div className="text-3xl font-bold">
                            {analytics?.activeListings}
                        </div>
                        <p className="text-sm text-zinc-500 mt-1">
                            Currently selling
                        </p>
                    </CardContent>
                </Card>

                <Card>
                    <CardHeader className="flex flex-row items-center justify-between pb-2">
                        <CardTitle className="text-sm font-medium text-zinc-600 dark:text-zinc-400">
                            Views Today
                        </CardTitle>
                        <Eye className="h-5 w-5 text-purple-500" />
                    </CardHeader>
                    <CardContent>
                        <div className="text-3xl font-bold">
                            {analytics?.viewsToday}
                        </div>
                        <div className="flex items-center text-sm mt-1">
                            {(analytics?.viewsChange || 0) > 0 ? (
                                <TrendingUp className="h-4 w-4 text-green-500 mr-1" />
                            ) : (
                                <TrendingDown className="h-4 w-4 text-red-500 mr-1" />
                            )}
                            <span
                                className={
                                    (analytics?.viewsChange || 0) > 0
                                        ? "text-green-500"
                                        : "text-red-500"
                                }
                            >
                                {Math.abs(analytics?.viewsChange || 0)}%
                            </span>
                            <span className="text-zinc-500 ml-1">
                                from yesterday
                            </span>
                        </div>
                    </CardContent>
                </Card>
            </div>

            {/* Charts Row */}
            <div className="grid gap-6 lg:grid-cols-2 mb-8">
                <Card>
                    <CardHeader>
                        <CardTitle>Revenue Over Time</CardTitle>
                        <CardDescription>
                            Daily revenue for the selected period
                        </CardDescription>
                    </CardHeader>
                    <CardContent>
                        {analytics?.chartData && (
                            <SimpleBarChart data={analytics.chartData} />
                        )}
                    </CardContent>
                </Card>

                <Card>
                    <CardHeader>
                        <CardTitle>Top Performing Listings</CardTitle>
                        <CardDescription>
                            Your best performing auctions
                        </CardDescription>
                    </CardHeader>
                    <CardContent>
                        <div className="space-y-4">
                            {analytics?.topListings.map((listing, index) => (
                                <div
                                    key={listing.id}
                                    className="flex items-center gap-4 p-3 bg-zinc-50 dark:bg-zinc-800/50 rounded-lg"
                                >
                                    <div className="w-8 h-8 rounded-full bg-amber-500/10 flex items-center justify-center text-amber-500 font-bold">
                                        {index + 1}
                                    </div>
                                    <div className="flex-1 min-w-0">
                                        <p className="font-medium truncate">
                                            {listing.title}
                                        </p>
                                        <div className="flex items-center gap-4 text-sm text-zinc-500">
                                            <span>
                                                {formatCurrency(listing.currentBid)}
                                            </span>
                                            <span className="flex items-center gap-1">
                                                <Eye className="h-3 w-3" />
                                                {formatNumber(listing.views)}
                                            </span>
                                            <span>{listing.bids} bids</span>
                                        </div>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </CardContent>
                </Card>
            </div>

            {/* Performance Summary */}
            <Card>
                <CardHeader>
                    <CardTitle>Performance Summary</CardTitle>
                    <CardDescription>
                        Key metrics for your selling activity
                    </CardDescription>
                </CardHeader>
                <CardContent>
                    <div className="grid gap-4 md:grid-cols-3">
                        <div className="p-4 bg-zinc-50 dark:bg-zinc-800/50 rounded-lg">
                            <p className="text-sm text-zinc-500 mb-1">
                                Average Sale Price
                            </p>
                            <p className="text-2xl font-bold">
                                {formatCurrency(
                                    analytics && analytics.itemsSold > 0
                                        ? Math.round(analytics.totalRevenue / analytics.itemsSold)
                                        : 0
                                )}
                            </p>
                        </div>
                        <div className="p-4 bg-zinc-50 dark:bg-zinc-800/50 rounded-lg">
                            <p className="text-sm text-zinc-500 mb-1">
                                Conversion Rate
                            </p>
                            <p className="text-2xl font-bold">12.5%</p>
                        </div>
                        <div className="p-4 bg-zinc-50 dark:bg-zinc-800/50 rounded-lg">
                            <p className="text-sm text-zinc-500 mb-1">
                                Avg. Bids per Listing
                            </p>
                            <p className="text-2xl font-bold">18</p>
                        </div>
                    </div>
                </CardContent>
            </Card>
        </DashboardLayout>
    );
}
