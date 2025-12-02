import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { auctionService } from "@/services/auction.service";
import { searchService } from "@/services/search.service";
import { CreateAuctionDto, UpdateAuctionDto } from "@/types/auction";
import { QUERY_KEYS } from "@/constants/api";
import { toast } from "sonner";
import { AxiosError } from "axios";

export const useAuctions = (params?: {
  pageNumber?: number;
  pageSize?: number;
}) => {
  return useQuery({
    queryKey: [QUERY_KEYS.SEARCH, params],
    queryFn: () =>
      searchService.search({
        pageNumber: params?.pageNumber || 1,
        pageSize: params?.pageSize || 12
      })
  });
};

export const useAuction = (id: string) => {
  return useQuery({
    queryKey: [QUERY_KEYS.AUCTION, id],
    queryFn: async () => {
      const result = await searchService.search({
        searchTerm: id,
        pageSize: 1
      });
      return result.results[0];
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
