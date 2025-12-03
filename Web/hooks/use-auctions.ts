import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";
import { auctionService } from "@/services/auction.service";
import { searchService } from "@/services/search.service";
import { CreateAuctionDto, UpdateAuctionDto } from "@/types/auction";
import { SearchRequestDto } from "@/types/search";
import { QUERY_KEYS } from "@/constants/api";
import { toast } from "sonner";
import { AxiosError } from "axios";
import {
  useAuctionParams,
  useAuction as useAuctionContext
} from "@/context/auction.context";

export const useAuctions = () => {
  const params = useAuctionParams();
  const { setPaginationInfo } = useAuctionContext();

  const searchParams: SearchRequestDto = {
    query: params.searchTerm || undefined,
    page: params.page,
    pageSize: params.pageSize,
    status: params.status || undefined,
    category: params.category || undefined,
    seller: params.seller || undefined,
    winner: params.winner || undefined,
    minPrice: params.minPrice,
    maxPrice: params.maxPrice,
    sortBy: params.sortBy || undefined,
    sortOrder: params.sortOrder || undefined
  };

  const query = useQuery({
    queryKey: [QUERY_KEYS.SEARCH, searchParams],
    queryFn: () => searchService.search(searchParams)
  });

  useEffect(() => {
    if (query.data) {
      setPaginationInfo({
        totalCount: query.data.totalCount,
        totalPages: query.data.totalPages,
        hasNextPage: query.data.hasNextPage,
        hasPreviousPage: query.data.hasPreviousPage
      });
    }
  }, [query.data, setPaginationInfo]);

  return query;
};

export const useAuctionsWithParams = (params?: {
  pageNumber?: number;
  pageSize?: number;
}) => {
  return useQuery({
    queryKey: [QUERY_KEYS.SEARCH, params],
    queryFn: () =>
      searchService.search({
        page: params?.pageNumber || 1,
        pageSize: params?.pageSize || 12
      })
  });
};

export const useAuction = (id: string) => {
  return useQuery({
    queryKey: [QUERY_KEYS.AUCTION, id],
    queryFn: async () => {
      const result = await searchService.search({
        query: id,
        pageSize: 1
      });
      return result.items[0];
    },
    enabled: !!id
  });
};

export const useCreateAuction = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateAuctionDto) => auctionService.createAuction(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.AUCTIONS] });
      toast.success("Auction created successfully");
    },
    onError: (error: AxiosError<{ message?: string }>) => {
      toast.error(error.response?.data?.message || "Failed to create auction");
    }
  });
};

export const useUpdateAuction = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateAuctionDto }) =>
      auctionService.updateAuction(id, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.AUCTIONS] });
      queryClient.invalidateQueries({
        queryKey: [QUERY_KEYS.AUCTION, variables.id]
      });
      toast.success("Auction updated successfully");
    },
    onError: (error: AxiosError<{ message?: string }>) => {
      toast.error(error.response?.data?.message || "Failed to update auction");
    }
  });
};

export const useDeleteAuction = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => auctionService.deleteAuction(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [QUERY_KEYS.AUCTIONS] });
      toast.success("Auction deleted successfully");
    },
    onError: (error: AxiosError<{ message?: string }>) => {
      toast.error(error.response?.data?.message || "Failed to delete auction");
    }
  });
};
