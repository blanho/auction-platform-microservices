"use client";

import React, {
    createContext,
    useContext,
    useState,
    useCallback,
    useEffect,
    useMemo,
} from "react";
import { useSession } from "next-auth/react";
import { bookmarkService } from "@/services/bookmark.service";
import { showSuccessToast, showErrorToast } from "@/utils";

interface WatchlistContextType {
    watchlistIds: Set<string>;
    isInWatchlist: (auctionId: string) => boolean;
    toggleWatchlist: (auctionId: string) => Promise<boolean>;
    addToWatchlist: (auctionId: string) => Promise<void>;
    removeFromWatchlist: (auctionId: string) => Promise<void>;
    refreshWatchlist: () => Promise<void>;
    isLoading: boolean;
    watchlistCount: number;
}

const WatchlistContext = createContext<WatchlistContextType | undefined>(undefined);

interface WatchlistProviderProps {
    children: React.ReactNode;
}

export function WatchlistProvider({ children }: WatchlistProviderProps) {
    const { data: session, status } = useSession();
    const [watchlistIds, setWatchlistIds] = useState<Set<string>>(new Set());
    const [isLoading, setIsLoading] = useState(false);
    const [initialLoadComplete, setInitialLoadComplete] = useState(false);
    const [pendingWatchlist, setPendingWatchlist] = useState<Set<string>>(new Set());

    const isAuthenticated = status === "authenticated" && !!session;
    const isSessionLoading = status === "loading";

    const refreshWatchlist = useCallback(async () => {
        if (!isAuthenticated || isSessionLoading) {
            setWatchlistIds(new Set());
            return;
        }

        try {
            const items = await bookmarkService.getWatchlist();
            setWatchlistIds(new Set(items.map((item) => item.auctionId)));
        } catch (error) {
            console.error("Failed to fetch watchlist:", error);
        }
    }, [isAuthenticated, isSessionLoading]);

    useEffect(() => {
        let isMounted = true;

        if (isSessionLoading) {
            return;
        }

        if (!isAuthenticated) {
            if (!initialLoadComplete) {
                setInitialLoadComplete(true);
            }
            return;
        }

        const fetchData = async () => {
            setIsLoading(true);
            try {
                const watchlistItems = await bookmarkService.getWatchlist();

                if (isMounted) {
                    setWatchlistIds(new Set(watchlistItems.map((item) => item.auctionId)));
                }
            } catch (error) {
                console.error("Failed to fetch watchlist:", error);
            } finally {
                if (isMounted) {
                    setIsLoading(false);
                    setInitialLoadComplete(true);
                }
            }
        };

        fetchData();

        return () => {
            isMounted = false;
        };
    }, [isAuthenticated, isSessionLoading, initialLoadComplete]);

    const isInWatchlist = useCallback(
        (auctionId: string) => watchlistIds.has(auctionId),
        [watchlistIds]
    );

    const addToWatchlist = useCallback(
        async (auctionId: string) => {
            if (!isAuthenticated) {
                showErrorToast("Please sign in to add items to your watchlist");
                return;
            }

            if (watchlistIds.has(auctionId) || pendingWatchlist.has(auctionId)) {
                return;
            }

            setPendingWatchlist((prev) => new Set([...prev, auctionId]));

            try {
                await bookmarkService.addToWatchlist({ auctionId });
                setWatchlistIds((prev) => new Set([...prev, auctionId]));
                showSuccessToast("Added to watchlist");
            } catch (error) {
                showErrorToast(error);
                throw error;
            } finally {
                setPendingWatchlist((prev) => {
                    const next = new Set(prev);
                    next.delete(auctionId);
                    return next;
                });
            }
        },
        [isAuthenticated, watchlistIds, pendingWatchlist]
    );

    const removeFromWatchlist = useCallback(
        async (auctionId: string) => {
            if (!isAuthenticated) return;

            if (pendingWatchlist.has(auctionId)) {
                return;
            }

            setPendingWatchlist((prev) => new Set([...prev, auctionId]));

            try {
                await bookmarkService.removeFromWatchlist(auctionId);
                setWatchlistIds((prev) => {
                    const next = new Set(prev);
                    next.delete(auctionId);
                    return next;
                });
                showSuccessToast("Removed from watchlist");
            } catch (error) {
                showErrorToast(error);
                throw error;
            } finally {
                setPendingWatchlist((prev) => {
                    const next = new Set(prev);
                    next.delete(auctionId);
                    return next;
                });
            }
        },
        [isAuthenticated, pendingWatchlist]
    );

    const toggleWatchlist = useCallback(
        async (auctionId: string): Promise<boolean> => {
            if (!isAuthenticated) {
                showErrorToast("Please sign in to manage your watchlist");
                return false;
            }

            if (pendingWatchlist.has(auctionId)) {
                return watchlistIds.has(auctionId);
            }

            const isCurrentlyInWatchlist = watchlistIds.has(auctionId);

            try {
                if (isCurrentlyInWatchlist) {
                    await removeFromWatchlist(auctionId);
                    return false;
                } else {
                    await addToWatchlist(auctionId);
                    return true;
                }
            } catch {
                return isCurrentlyInWatchlist;
            }
        },
        [isAuthenticated, watchlistIds, pendingWatchlist, addToWatchlist, removeFromWatchlist]
    );

    const value = useMemo<WatchlistContextType>(
        () => ({
            watchlistIds,
            isInWatchlist,
            toggleWatchlist,
            addToWatchlist,
            removeFromWatchlist,
            refreshWatchlist,
            isLoading,
            watchlistCount: watchlistIds.size,
        }),
        [
            watchlistIds,
            isInWatchlist,
            toggleWatchlist,
            addToWatchlist,
            removeFromWatchlist,
            refreshWatchlist,
            isLoading,
        ]
    );

    return (
        <WatchlistContext.Provider value={value}>
            {children}
        </WatchlistContext.Provider>
    );
}

export function useWatchlist() {
    const context = useContext(WatchlistContext);
    if (context === undefined) {
        throw new Error("useWatchlist must be used within a WatchlistProvider");
    }
    return context;
}

export function useWatchlistOptional() {
    return useContext(WatchlistContext);
}
