import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { auctionService } from "@/services/auction.service";
import { CreateAuctionDto, UpdateAuctionDto } from "@/types/auction";
import { QUERY_KEYS } from "@/constants/api";
import { toast } from "sonner";
import { AxiosError } from "axios";

export const useAuctions = (params?: {
  pageNumber?: number;
  pageSize?: number;
}) => {
  return useQuery({
    queryKey: [QUERY_KEYS.AUCTIONS, params],
    queryFn: () => auctionService.getAuctions(params)
  });
};

export const useAuction = (id: string) => {
  return useQuery({
    queryKey: [QUERY_KEYS.AUCTION, id],
    queryFn: () => auctionService.getAuctionById(id),
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
