"use client";

import { useEffect, useCallback, useRef } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { QUERY_KEYS } from "@/constants/api";
import { Notification, NotificationType } from "@/types/notification";
import { useNotifications } from "@/context/notification.context";

interface BidUpdate {
    auctionId: string;
    bidId: string;
    amount: number;
    bidderUsername: string;
    bidTime: string;
}

interface UseAuctionRealtimeOptions {
    auctionId: string;
    enabled?: boolean;
    onBidUpdate?: (update: BidUpdate) => void;
    onOutbid?: (notification: Notification) => void;
    onAuctionEnded?: () => void;
    onAuctionEndingSoon?: () => void;
}

interface UseAuctionRealtimeReturn {
    isConnected: boolean;
}

export function useAuctionRealtime({
    auctionId,
    enabled = true,
    onBidUpdate,
    onOutbid,
    onAuctionEnded,
    onAuctionEndingSoon,
}: UseAuctionRealtimeOptions): UseAuctionRealtimeReturn {
    const queryClient = useQueryClient();
    const { notifications, isConnected } = useNotifications();
    const processedNotifications = useRef<Set<string>>(new Set());
    const callbacksRef = useRef({ onBidUpdate, onOutbid, onAuctionEnded, onAuctionEndingSoon });

    useEffect(() => {
        callbacksRef.current = { onBidUpdate, onOutbid, onAuctionEnded, onAuctionEndingSoon };
    }, [onBidUpdate, onOutbid, onAuctionEnded, onAuctionEndingSoon]);

    const handleNotification = useCallback((notification: Notification) => {
        if (processedNotifications.current.has(notification.id)) {
            return;
        }
        processedNotifications.current.add(notification.id);

        const notificationAuctionId = notification.auctionId;
        if (notificationAuctionId !== auctionId) {
            return;
        }

        switch (notification.type) {
            case NotificationType.BidPlaced:
            case NotificationType.BidAccepted: {
                queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.BIDS_FOR_AUCTION, auctionId] });
                queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.AUCTION, auctionId] });

                try {
                    const data = JSON.parse(notification.data || "{}");
                    const bidUpdate: BidUpdate = {
                        auctionId,
                        bidId: notification.bidId || data.bidId || "",
                        amount: data.amount || data.bidAmount || 0,
                        bidderUsername: data.bidderUsername || data.bidder || "",
                        bidTime: data.bidTime || notification.createdAt,
                    };
                    callbacksRef.current.onBidUpdate?.(bidUpdate);
                } catch {
                }
                break;
            }

            case NotificationType.Outbid:
            case NotificationType.OutBid: {
                queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.BIDS_FOR_AUCTION, auctionId] });
                queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.AUCTION, auctionId] });
                callbacksRef.current.onOutbid?.(notification);
                break;
            }

            case NotificationType.AuctionFinished:
            case NotificationType.AuctionWon:
            case NotificationType.AuctionLost: {
                queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.AUCTION, auctionId] });
                callbacksRef.current.onAuctionEnded?.();
                break;
            }

            case NotificationType.AuctionEndingSoon:
            case NotificationType.AuctionEnding: {
                callbacksRef.current.onAuctionEndingSoon?.();
                break;
            }

            default:
                break;
        }
    }, [auctionId, queryClient]);

    useEffect(() => {
        if (!enabled || !auctionId) {
            return;
        }

        notifications.forEach(handleNotification);
    }, [enabled, auctionId, notifications, handleNotification]);

    useEffect(() => {
        const processedSet = processedNotifications.current;
        return () => {
            processedSet.clear();
        };
    }, [auctionId]);

    return {
        isConnected,
    };
}

export function useBidRealtimeUpdates(auctionId: string, onUpdate?: () => void) {
    const queryClient = useQueryClient();

    const handleBidUpdate = useCallback(() => {
        queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.BIDS_FOR_AUCTION, auctionId] });
        queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.AUCTION, auctionId] });
        onUpdate?.();
    }, [queryClient, auctionId, onUpdate]);

    return useAuctionRealtime({
        auctionId,
        enabled: !!auctionId,
        onBidUpdate: handleBidUpdate,
        onOutbid: handleBidUpdate,
    });
}
