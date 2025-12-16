import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { auctionService, GetAuctionsParams, GetMyAuctionsParams, AuctionPagedResult } from "@/services/auction.service";
import { Auction, CreateAuctionDto, UpdateAuctionDto, Category, Brand, Tag } from "@/types/auction";
import { PaginatedResponse } from "@/types";
import { QUERY_KEYS, DEFAULT_STALE_TIME } from "@/constants/api";

export function useAuctionsQuery(params?: GetAuctionsParams, enabled = true) {
    return useQuery<AuctionPagedResult>({
        queryKey: [QUERY_KEYS.AUCTIONS, params],
        queryFn: () => auctionService.getAuctions(params),
        staleTime: DEFAULT_STALE_TIME,
        enabled,
    });
}

export function useMyAuctionsQuery(params?: GetMyAuctionsParams, enabled = true) {
    return useQuery<PaginatedResponse<Auction>>({
        queryKey: [QUERY_KEYS.MY_AUCTIONS, params],
        queryFn: () => auctionService.getMyAuctions(params),
        staleTime: DEFAULT_STALE_TIME,
        enabled,
    });
}

export function useAuctionQuery(id: string, enabled = true) {
    return useQuery<Auction>({
        queryKey: [QUERY_KEYS.AUCTION, id],
        queryFn: () => auctionService.getAuctionById(id),
        staleTime: DEFAULT_STALE_TIME,
        enabled: enabled && !!id,
    });
}

export function useRelatedAuctionsQuery(categoryId: string | undefined, excludeId: string, enabled = true) {
    return useQuery<Auction[]>({
        queryKey: ["related-auctions", categoryId, excludeId],
        queryFn: async () => {
            if (!categoryId) return [];
            const result = await auctionService.getAuctions({
                category: categoryId,
                pageSize: 5,
                status: "Live",
            });
            return result.items.filter(a => a.id !== excludeId).slice(0, 4);
        },
        staleTime: DEFAULT_STALE_TIME,
        enabled: enabled && !!categoryId,
    });
}

export function useFeaturedAuctionsQuery(limit = 8) {
    return useQuery<Auction[]>({
        queryKey: [QUERY_KEYS.FEATURED_AUCTIONS, limit],
        queryFn: () => auctionService.getFeaturedAuctions(limit),
        staleTime: DEFAULT_STALE_TIME,
    });
}

export function useCategoriesQuery() {
    return useQuery<Category[]>({
        queryKey: [QUERY_KEYS.CATEGORIES],
        queryFn: () => auctionService.getCategories(),
        staleTime: 10 * 60 * 1000,
    });
}

export function useBrandsQuery() {
    return useQuery<Brand[]>({
        queryKey: ["brands"],
        queryFn: () => auctionService.getBrands(),
        staleTime: 10 * 60 * 1000,
    });
}

export function useTagsQuery() {
    return useQuery<Tag[]>({
        queryKey: ["tags"],
        queryFn: () => auctionService.getTags(),
        staleTime: 10 * 60 * 1000,
    });
}

export function useCreateAuctionMutation() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (data: CreateAuctionDto) => auctionService.createAuction(data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.AUCTIONS] });
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.MY_AUCTIONS] });
        },
    });
}

export function useUpdateAuctionMutation() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({ id, data }: { id: string; data: UpdateAuctionDto }) =>
            auctionService.updateAuction(id, data),
        onSuccess: (_, { id }) => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.AUCTION, id] });
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.AUCTIONS] });
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.MY_AUCTIONS] });
        },
    });
}

export function useDeleteAuctionMutation() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (id: string) => auctionService.deleteAuction(id),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.AUCTIONS] });
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.MY_AUCTIONS] });
        },
    });
}

export function useActivateAuctionMutation() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (id: string) => auctionService.activateAuction(id),
        onSuccess: (_, id) => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.AUCTION, id] });
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.AUCTIONS] });
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.MY_AUCTIONS] });
        },
    });
}

export function useDeactivateAuctionMutation() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (id: string) => auctionService.deactivateAuction(id),
        onSuccess: (_, id) => {
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.AUCTION, id] });
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.AUCTIONS] });
            queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.MY_AUCTIONS] });
        },
    });
}
