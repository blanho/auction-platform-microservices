"use client";

import { useState, useCallback } from "react";
import { bidService } from "@/services/bid.service";
import { Bid, PlaceBidDto, BidStatus } from "@/types/bid";

interface UseBidsReturn {
  bids: Bid[];
  isLoading: boolean;
  error: string | null;
  fetchBidsForAuction: (id: string) => Promise<Bid[]>;
  fetchMyBids: (bidder: string) => Promise<Bid[]>;
  placeBid: (dto: PlaceBidDto) => Promise<Bid>;
  getHighestBid: () => Bid | null;
  clearError: () => void;
}

export function useBids(): UseBidsReturn {
  const [bids, setBids] = useState<Bid[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchBidsForAuction = useCallback(async (id: string) => {
    setIsLoading(true);
    setError(null);
    try {
      const data = await bidService.getBidsForAuction(id);
      setBids(data);
      return data;
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Failed to fetch bids";
      setError(message);
      throw err;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const fetchMyBids = useCallback(async (bidder: string) => {
    setIsLoading(true);
    setError(null);
    try {
      const data = await bidService.getBidsForBidder(bidder);
      setBids(data);
      return data;
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Failed to fetch bids";
      setError(message);
      throw err;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const placeBid = useCallback(async (dto: PlaceBidDto) => {
    setIsLoading(true);
    setError(null);
    try {
      const bid = await bidService.placeBid(dto);
      setBids((prev) => [bid, ...prev]);
      return bid;
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Failed to place bid";
      setError(message);
      throw err;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const getHighestBid = useCallback((): Bid | null => {
    const acceptedBids = bids.filter((b) => b.status === BidStatus.Accepted);
    if (acceptedBids.length === 0) return null;
    return acceptedBids.reduce((highest, current) =>
      current.amount > highest.amount ? current : highest
    );
  }, [bids]);

  const clearError = useCallback(() => setError(null), []);

  return {
    bids,
    isLoading,
    error,
    fetchBidsForAuction,
    fetchMyBids,
    placeBid,
    getHighestBid,
    clearError
  };
}
