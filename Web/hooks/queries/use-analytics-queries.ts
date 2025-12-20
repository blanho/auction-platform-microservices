import { useQuery } from "@tanstack/react-query";
import { analyticsService, QuickStats, TrendingSearch, TopListing } from "@/services/analytics.service";

const ANALYTICS_QUERY_KEYS = {
    QUICK_STATS: "quick-stats",
    TRENDING_SEARCHES: "trending-searches",
    TOP_LISTINGS: "top-listings",
} as const;

const ANALYTICS_STALE_TIME = 2 * 60 * 1000;

export function useQuickStatsQuery(enabled = true) {
    return useQuery<QuickStats>({
        queryKey: [ANALYTICS_QUERY_KEYS.QUICK_STATS],
        queryFn: () => analyticsService.getQuickStats(),
        staleTime: ANALYTICS_STALE_TIME,
        enabled,
    });
}

export function useTrendingSearchesQuery(limit = 6, enabled = true) {
    return useQuery<TrendingSearch[]>({
        queryKey: [ANALYTICS_QUERY_KEYS.TRENDING_SEARCHES, limit],
        queryFn: () => analyticsService.getTrendingSearches(limit),
        staleTime: ANALYTICS_STALE_TIME,
        enabled,
    });
}

export function useTopListingsQuery(limit = 5, enabled = true) {
    return useQuery<TopListing[]>({
        queryKey: [ANALYTICS_QUERY_KEYS.TOP_LISTINGS, limit],
        queryFn: () => analyticsService.getTopListings(limit),
        staleTime: ANALYTICS_STALE_TIME,
        enabled,
    });
}
