"use client";

import { useSession } from "next-auth/react";
import { useRouter } from "next/navigation";
import { useEffect, useState, useCallback } from "react";
import {
    Gavel,
    Trophy,
    Heart,
    Wallet,
    Star,
    TrendingUp,
    Package,
    Clock,
    RefreshCw,
} from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { DashboardLayout } from "@/components/layout/dashboard-layout";
import { cn } from "@/lib/utils";
import { conditionalStyles } from "@/lib/styles";
import Link from "next/link";

import { ROUTES, MESSAGES } from "@/constants";
import { UI } from "@/constants/config";
import { formatCurrency, formatRelativeTime } from "@/utils";
import { dashboardService, UserDashboardStats, RecentActivity } from "@/services/dashboard.service";
import { toast } from "sonner";

export default function DashboardPage() {
    const { data: session, status } = useSession();
    const router = useRouter();
    const [stats, setStats] = useState<UserDashboardStats | null>(null);
    const [isLoading, setIsLoading] = useState(true);

    const fetchStats = useCallback(async () => {
        try {
            const data = await dashboardService.getStats();
            setStats(data);
        } catch (error) {
            console.error("Failed to fetch dashboard stats:", error);
            toast.error(MESSAGES.ERROR.GENERIC);
        } finally {
            setIsLoading(false);
        }
    }, []);

    useEffect(() => {
        if (status === "unauthenticated") {
            router.push(`${ROUTES.AUTH.LOGIN}?callbackUrl=${ROUTES.DASHBOARD.HOME}`);
            return;
        }

        if (status === "authenticated") {
            fetchStats();
        }
    }, [status, router, fetchStats]);

    if (status === "loading" || isLoading) {
        return (
            <DashboardLayout title="Dashboard" description="Welcome back!">
                <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
                    {[...Array(UI.SKELETON.DASHBOARD_CARDS)].map((_, i) => (
                        <Card key={i}>
                            <CardHeader className="pb-2">
                                <Skeleton className="h-4 w-24" />
                            </CardHeader>
                            <CardContent>
                                <Skeleton className="h-8 w-16" />
                            </CardContent>
                        </Card>
                    ))}
                </div>
            </DashboardLayout>
        );
    }

    return (
        <DashboardLayout
            title="Dashboard"
            description={`Welcome back, ${session?.user?.name || "User"}!`}
        >
            {/* Stats Grid */}
            <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3 mb-8">
                <Card className="border-l-4 border-l-blue-500">
                    <CardHeader className="flex flex-row items-center justify-between pb-2">
                        <CardTitle className="text-sm font-medium text-zinc-600 dark:text-zinc-400">
                            Total Bids
                        </CardTitle>
                        <Gavel className="h-5 w-5 text-blue-500" />
                    </CardHeader>
                    <CardContent>
                        <div className="text-3xl font-bold text-zinc-900 dark:text-white">
                            {stats?.totalBids || 0}
                        </div>
                        <p className="text-xs text-zinc-500 mt-1">
                            <TrendingUp className="inline h-3 w-3 mr-1 text-green-500" />
                            +12% from last month
                        </p>
                    </CardContent>
                </Card>

                <Card className="border-l-4 border-l-green-500">
                    <CardHeader className="flex flex-row items-center justify-between pb-2">
                        <CardTitle className="text-sm font-medium text-zinc-600 dark:text-zinc-400">
                            Items Won
                        </CardTitle>
                        <Trophy className="h-5 w-5 text-green-500" />
                    </CardHeader>
                    <CardContent>
                        <div className="text-3xl font-bold text-zinc-900 dark:text-white">
                            {stats?.itemsWon ?? 0}
                        </div>
                        <p className="text-xs text-zinc-500 mt-1">
                            Lifetime wins
                        </p>
                    </CardContent>
                </Card>

                <Card className="border-l-4 border-l-red-500">
                    <CardHeader className="flex flex-row items-center justify-between pb-2">
                        <CardTitle className="text-sm font-medium text-zinc-600 dark:text-zinc-400">
                            Watchlist
                        </CardTitle>
                        <Heart className="h-5 w-5 text-red-500" />
                    </CardHeader>
                    <CardContent>
                        <div className="text-3xl font-bold text-zinc-900 dark:text-white">
                            {stats?.watchlistCount ?? 0}
                        </div>
                        <p className="text-xs text-zinc-500 mt-1">
                            Items tracked
                        </p>
                    </CardContent>
                </Card>

                <Card className="border-l-4 border-l-amber-500">
                    <CardHeader className="flex flex-row items-center justify-between pb-2">
                        <CardTitle className="text-sm font-medium text-zinc-600 dark:text-zinc-400">
                            Balance
                        </CardTitle>
                        <Wallet className="h-5 w-5 text-amber-500" />
                    </CardHeader>
                    <CardContent>
                        <div className="text-3xl font-bold text-zinc-900 dark:text-white">
                            {formatCurrency(stats?.balance ?? 0)}
                        </div>
                        <p className="text-xs text-zinc-500 mt-1">
                            Available funds
                        </p>
                    </CardContent>
                </Card>

                <Card className="border-l-4 border-l-purple-500">
                    <CardHeader className="flex flex-row items-center justify-between pb-2">
                        <CardTitle className="text-sm font-medium text-zinc-600 dark:text-zinc-400">
                            Rating
                        </CardTitle>
                        <Star className="h-5 w-5 text-purple-500" />
                    </CardHeader>
                    <CardContent>
                        <div className="text-3xl font-bold text-zinc-900 dark:text-white">
                            {stats?.sellerRating ?? 0}
                            <span className="text-lg text-zinc-400">/5</span>
                        </div>
                        <p className="text-xs text-zinc-500 mt-1">
                            Based on {stats?.reviewCount ?? 0} reviews
                        </p>
                    </CardContent>
                </Card>

                <Card className="border-l-4 border-l-cyan-500">
                    <CardHeader className="flex flex-row items-center justify-between pb-2">
                        <CardTitle className="text-sm font-medium text-zinc-600 dark:text-zinc-400">
                            Active Listings
                        </CardTitle>
                        <Package className="h-5 w-5 text-cyan-500" />
                    </CardHeader>
                    <CardContent>
                        <div className="text-3xl font-bold text-zinc-900 dark:text-white">
                            {stats?.activeListings ?? 0}
                        </div>
                        <p className="text-xs text-zinc-500 mt-1">
                            Currently selling
                        </p>
                    </CardContent>
                </Card>
            </div>

            {/* Quick Actions & Recent Activity */}
            <div className="grid gap-6 md:grid-cols-2">
                <Card>
                    <CardHeader className="flex flex-row items-center justify-between">
                        <CardTitle className="flex items-center gap-2">
                            <Clock className="h-5 w-5 text-amber-500" />
                            Recent Activity
                        </CardTitle>
                        <Button 
                            variant="ghost" 
                            size="sm" 
                            onClick={fetchStats}
                            disabled={isLoading}
                        >
                            <RefreshCw className={cn("h-4 w-4", conditionalStyles.loading(isLoading))} />
                        </Button>
                    </CardHeader>
                    <CardContent>
                        {stats?.recentActivity && stats.recentActivity.length > 0 ? (
                            <div className="space-y-4">
                                {stats.recentActivity.slice(0, 5).map((activity, index) => (
                                    <ActivityItem key={index} activity={activity} />
                                ))}
                            </div>
                        ) : (
                            <div className="text-center py-8 text-zinc-500">
                                <Clock className="h-8 w-8 mx-auto mb-2 opacity-50" />
                                <p>No recent activity</p>
                            </div>
                        )}
                        <Button variant="outline" className="w-full mt-4" asChild>
                            <Link href={ROUTES.DASHBOARD.BIDS}>View All Activity</Link>
                        </Button>
                    </CardContent>
                </Card>

                <Card>
                    <CardHeader>
                        <CardTitle className="flex items-center gap-2">
                            <Gavel className="h-5 w-5 text-amber-500" />
                            Quick Actions
                        </CardTitle>
                    </CardHeader>
                    <CardContent>
                        <div className="space-y-3">
                            <Button className="w-full justify-start" variant="outline" asChild>
                                <Link href={ROUTES.AUCTIONS.CREATE}>
                                    <Package className="h-4 w-4 mr-2" />
                                    Create New Listing
                                </Link>
                            </Button>
                            <Button className="w-full justify-start" variant="outline" asChild>
                                <Link href={ROUTES.DASHBOARD.BIDS}>
                                    <Gavel className="h-4 w-4 mr-2" />
                                    View My Bids
                                </Link>
                            </Button>
                            <Button className="w-full justify-start" variant="outline" asChild>
                                <Link href={ROUTES.DASHBOARD.LISTINGS}>
                                    <TrendingUp className="h-4 w-4 mr-2" />
                                    My Listings
                                </Link>
                            </Button>
                            <Button className="w-full justify-start" variant="outline" asChild>
                                <Link href={ROUTES.DASHBOARD.WALLET}>
                                    <Wallet className="h-4 w-4 mr-2" />
                                    Manage Wallet
                                </Link>
                            </Button>
                            <Button className="w-full justify-start" variant="outline" asChild>
                                <Link href={ROUTES.DASHBOARD.WATCHLIST}>
                                    <Heart className="h-4 w-4 mr-2" />
                                    My Watchlist
                                </Link>
                            </Button>
                        </div>
                    </CardContent>
                </Card>
            </div>
        </DashboardLayout>
    );
}

function ActivityItem({ activity }: { activity: RecentActivity }) {
    const getActivityColor = (type: string) => {
        switch (type.toLowerCase()) {
            case "bid":
            case "bid_placed":
                return "bg-green-500";
            case "outbid":
                return "bg-amber-500";
            case "won":
            case "auction_won":
                return "bg-blue-500";
            case "listing":
            case "listing_created":
                return "bg-purple-500";
            case "watchlist":
                return "bg-red-500";
            default:
                return "bg-zinc-500";
        }
    };

    return (
        <div className="flex items-center gap-4 p-3 bg-zinc-50 dark:bg-zinc-800/50 rounded-lg">
            <div className={`w-2 h-2 ${getActivityColor(activity.type)} rounded-full`} />
            <div className="flex-1">
                <p className="text-sm font-medium">{activity.description}</p>
                <p className="text-xs text-zinc-500">
                    {formatRelativeTime(activity.timestamp)}
                </p>
            </div>
        </div>
    );
}
