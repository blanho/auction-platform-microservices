import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { bidService } from "@/services/bid.service";
import { Bid, PlaceBidDto } from "@/types/bid";
import { QUERY_KEYS, DEFAULT_STALE_TIME } from "@/constants/api";

export function useBidsForAuctionQuery(auctionId: string, enabled = true) {
    return useQuery<Bid[]>({
        queryKey: [QUERY_KEYS.BIDS_FOR_AUCTION, auctionId],
        queryFn: () => bidService.getBidsForAuction(auctionId),
        staleTime: 30 * 1000,
        enabled: enabled && !!auctionId,
    });
}

export function useMyBidsQuery(enabled = true) {
    return useQuery<Bid[]>({
        queryKey: [QUERY_KEYS.MY_BIDS],
        queryFn: () => bidService.getMyBids(),
        staleTime: DEFAULT_STALE_TIME,
        enabled,
    });
}

export function useBidsForBidderQuery(bidder: string, enabled = true) {
    return useQuery<Bid[]>({
        queryKey: [QUERY_KEYS.BIDS_FOR_BIDDER, bidder],
        queryFn: () => bidService.getBidsForBidder(bidder),
        staleTime: DEFAULT_STALE_TIME,
        enabled: enabled && !!bidder,
    });
}

export function usePlaceBidMutation() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (data: PlaceBidDto) => bidService.placeBid(data),
        onSuccess: (_, variables) => {
            queryClient.invalidateQueries({ 
                queryKey: [QUERY_KEYS.BIDS_FOR_AUCTION, variables.auctionId] 
            });
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.MY_BIDS] });
            queryClient.invalidateQueries({ 
                queryKey: [QUERY_KEYS.AUCTION, variables.auctionId] 
            });
        },
    });
}
