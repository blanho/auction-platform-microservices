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

interface WishlistContextType {
    wishlistIds: Set<string>;
    watchlistIds: Set<string>;
    isInWishlist: (auctionId: string) => boolean;
    isInWatchlist: (auctionId: string) => boolean;
    toggleWishlist: (auctionId: string) => Promise<boolean>;
    toggleWatchlist: (auctionId: string) => Promise<boolean>;
    addToWishlist: (auctionId: string) => Promise<void>;
    addToWatchlist: (auctionId: string) => Promise<void>;
    removeFromWishlist: (auctionId: string) => Promise<void>;
    removeFromWatchlist: (auctionId: string) => Promise<void>;
    refreshWishlist: () => Promise<void>;
    refreshWatchlist: () => Promise<void>;
    isLoading: boolean;
    wishlistCount: number;
    watchlistCount: number;
}

const WishlistContext = createContext<WishlistContextType | undefined>(undefined);

interface WishlistProviderProps {
    children: React.ReactNode;
}

export function WishlistProvider({ children }: WishlistProviderProps) {
    const { data: session, status } = useSession();
    const [wishlistIds, setWishlistIds] = useState<Set<string>>(new Set());
    const [watchlistIds, setWatchlistIds] = useState<Set<string>>(new Set());
    const [isLoading, setIsLoading] = useState(false);
    const [initialLoadComplete, setInitialLoadComplete] = useState(false);

    const isAuthenticated = status === "authenticated" && !!session;
    const isSessionLoading = status === "loading";

    const refreshWishlist = useCallback(async () => {
        if (!isAuthenticated || isSessionLoading) {
            setWishlistIds(new Set());
            return;
        }

        try {
            const items = await bookmarkService.getWishlist();
            setWishlistIds(new Set(items.map((item) => item.auctionId)));
        } catch (error) {
            console.error("Failed to fetch wishlist:", error);
        }
    }, [isAuthenticated, isSessionLoading]);

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
                const [wishlistItems, watchlistItems] = await Promise.all([
                    bookmarkService.getWishlist(),
                    bookmarkService.getWatchlist(),
                ]);

                if (isMounted) {
                    setWishlistIds(new Set(wishlistItems.map((item) => item.auctionId)));
                    setWatchlistIds(new Set(watchlistItems.map((item) => item.auctionId)));
                }
            } catch (error) {
                console.error("Failed to fetch bookmarks:", error);
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
    }, [isAuthenticated, initialLoadComplete]);

    const isInWishlist = useCallback(
        (auctionId: string) => wishlistIds.has(auctionId),
        [wishlistIds]
    );

    const isInWatchlist = useCallback(
        (auctionId: string) => watchlistIds.has(auctionId),
        [watchlistIds]
    );

    const addToWishlist = useCallback(
        async (auctionId: string) => {
            if (!isAuthenticated) {
                showErrorToast("Please sign in to add items to your wishlist");
                return;
            }

            try {
                await bookmarkService.addToWishlist(auctionId);
                setWishlistIds((prev) => new Set([...prev, auctionId]));
                showSuccessToast("Added to wishlist");
            } catch (error) {
                showErrorToast(error);
                throw error;
            }
        },
        [isAuthenticated]
    );

    const addToWatchlist = useCallback(
        async (auctionId: string) => {
            if (!isAuthenticated) {
                showErrorToast("Please sign in to add items to your watchlist");
                return;
            }

            try {
                await bookmarkService.addToWatchlist({ auctionId });
                setWatchlistIds((prev) => new Set([...prev, auctionId]));
                showSuccessToast("Added to watchlist");
            } catch (error) {
                showErrorToast(error);
                throw error;
            }
        },
        [isAuthenticated]
    );

    const removeFromWishlist = useCallback(
        async (auctionId: string) => {
            if (!isAuthenticated) return;

            try {
                await bookmarkService.removeFromWishlist(auctionId);
                setWishlistIds((prev) => {
                    const next = new Set(prev);
                    next.delete(auctionId);
                    return next;
                });
                showSuccessToast("Removed from wishlist");
            } catch (error) {
                showErrorToast(error);
                throw error;
            }
        },
        [isAuthenticated]
    );

    const removeFromWatchlist = useCallback(
        async (auctionId: string) => {
            if (!isAuthenticated) return;

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
            }
        },
        [isAuthenticated]
    );

    const toggleWishlist = useCallback(
        async (auctionId: string): Promise<boolean> => {
            if (!isAuthenticated) {
                showErrorToast("Please sign in to manage your wishlist");
                return false;
            }

            const isCurrentlyInWishlist = wishlistIds.has(auctionId);

            try {
                if (isCurrentlyInWishlist) {
                    await removeFromWishlist(auctionId);
                    return false;
                } else {
                    await addToWishlist(auctionId);
                    return true;
                }
            } catch {
                return isCurrentlyInWishlist;
            }
        },
        [isAuthenticated, wishlistIds, addToWishlist, removeFromWishlist]
    );

    const toggleWatchlist = useCallback(
        async (auctionId: string): Promise<boolean> => {
            if (!isAuthenticated) {
                showErrorToast("Please sign in to manage your watchlist");
                return false;
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
        [isAuthenticated, watchlistIds, addToWatchlist, removeFromWatchlist]
    );

    const value = useMemo<WishlistContextType>(
        () => ({
            wishlistIds,
            watchlistIds,
            isInWishlist,
            isInWatchlist,
            toggleWishlist,
            toggleWatchlist,
            addToWishlist,
            addToWatchlist,
            removeFromWishlist,
            removeFromWatchlist,
            refreshWishlist,
            refreshWatchlist,
            isLoading,
            wishlistCount: wishlistIds.size,
            watchlistCount: watchlistIds.size,
        }),
        [
            wishlistIds,
            watchlistIds,
            isInWishlist,
            isInWatchlist,
            toggleWishlist,
            toggleWatchlist,
            addToWishlist,
            addToWatchlist,
            removeFromWishlist,
            removeFromWatchlist,
            refreshWishlist,
            refreshWatchlist,
            isLoading,
        ]
    );

    return (
        <WishlistContext.Provider value={value}>{children}</WishlistContext.Provider>
    );
}

export function useWishlist() {
    const context = useContext(WishlistContext);
    if (context === undefined) {
        throw new Error("useWishlist must be used within a WishlistProvider");
    }
    return context;
}

export function useWishlistOptional() {
    return useContext(WishlistContext);
}
