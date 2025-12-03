import { useEffect, useState, useCallback, useMemo } from "react";
import { auctionService } from "@/services/auction.service";
import { searchService } from "@/services/search.service";
import { CreateAuctionDto, UpdateAuctionDto, Auction } from "@/types/auction";
import { SearchRequestDto, SearchResultDto, SearchItem } from "@/types/search";
import { toast } from "sonner";
import { AxiosError } from "axios";
import {
  useAuctionParams,
  useAuction as useAuctionContext
} from "@/context/auction.context";

export const useAuctions = () => {
  const params = useAuctionParams();
  const { setPaginationInfo } = useAuctionContext();
  const [data, setData] = useState<SearchResultDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  const searchParams: SearchRequestDto = useMemo(
    () => ({
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
    }),
    [
      params.searchTerm,
      params.page,
      params.pageSize,
      params.status,
      params.category,
      params.seller,
      params.winner,
      params.minPrice,
      params.maxPrice,
      params.sortBy,
      params.sortOrder
    ]
  );

  useEffect(() => {
    let isMounted = true;

    const fetchData = async () => {
      try {
        const result = await searchService.search(searchParams);
        if (isMounted) {
          setData(result);
          setError(null);
          setPaginationInfo({
            totalCount: result.totalCount,
            totalPages: result.totalPages,
            hasNextPage: result.hasNextPage,
            hasPreviousPage: result.hasPreviousPage
          });
        }
      } catch (err) {
        if (isMounted) {
          setError(err as Error);
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    };

    setIsLoading(true);
    fetchData();

    return () => {
      isMounted = false;
    };
  }, [searchParams, setPaginationInfo]);

  return { data, isLoading, error };
};

export const useAuctionsWithParams = (params?: {
  pageNumber?: number;
  pageSize?: number;
}) => {
  const [data, setData] = useState<SearchResultDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    let isMounted = true;

    const fetchData = async () => {
      try {
        const result = await searchService.search({
          page: params?.pageNumber || 1,
          pageSize: params?.pageSize || 12
        });
        if (isMounted) {
          setData(result);
          setError(null);
        }
      } catch (err) {
        if (isMounted) {
          setError(err as Error);
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    };

    setIsLoading(true);
    fetchData();

    return () => {
      isMounted = false;
    };
  }, [params?.pageNumber, params?.pageSize]);

  return { data, isLoading, error };
};

export const useAuction = (id: string) => {
  const [data, setData] = useState<SearchItem | Auction | null>(null);
  const [isLoading, setIsLoading] = useState(!!id);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    if (!id) {
      return;
    }

    let isMounted = true;

    const fetchData = async () => {
      try {
        const result = await searchService.getById(id);
        if (isMounted) {
          setData(result);
          setError(null);
        }
      } catch (err) {
        if (isMounted) {
          setError(err as Error);
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    };

    setIsLoading(true);
    fetchData();

    return () => {
      isMounted = false;
    };
  }, [id]);

  return { data, isLoading, error };
};

export const useCreateAuction = () => {
  const [isPending, setIsPending] = useState(false);

  const mutate = useCallback(
    async (
      data: CreateAuctionDto,
      options?: {
        onSuccess?: (data: Auction) => void;
        onError?: (error: Error) => void;
      }
    ) => {
      setIsPending(true);
      try {
        const result = await auctionService.createAuction(data);
        toast.success("Auction created successfully");
        options?.onSuccess?.(result);
      } catch (error) {
        const err = error as AxiosError<{ message?: string }>;
        toast.error(err.response?.data?.message || "Failed to create auction");
        options?.onError?.(err);
      } finally {
        setIsPending(false);
      }
    },
    []
  );

  return { mutate, isPending };
};

export const useUpdateAuction = () => {
  const [isPending, setIsPending] = useState(false);

  const mutate = useCallback(
    async (
      { id, data }: { id: string; data: UpdateAuctionDto },
      options?: {
        onSuccess?: () => void;
        onError?: (error: Error) => void;
      }
    ) => {
      setIsPending(true);
      try {
        await auctionService.updateAuction(id, data);
        toast.success("Auction updated successfully");
        options?.onSuccess?.();
      } catch (error) {
        const err = error as AxiosError<{ message?: string }>;
        toast.error(err.response?.data?.message || "Failed to update auction");
        options?.onError?.(err);
      } finally {
        setIsPending(false);
      }
    },
    []
  );

  return { mutate, isPending };
};

export const useDeleteAuction = () => {
  const [isPending, setIsPending] = useState(false);

  const mutate = useCallback(
    async (
      id: string,
      options?: {
        onSuccess?: () => void;
        onError?: (error: Error) => void;
      }
    ) => {
      setIsPending(true);
      try {
        await auctionService.deleteAuction(id);
        toast.success("Auction deleted successfully");
        options?.onSuccess?.();
      } catch (error) {
        const err = error as AxiosError<{ message?: string }>;
        toast.error(err.response?.data?.message || "Failed to delete auction");
        options?.onError?.(err);
      } finally {
        setIsPending(false);
      }
    },
    []
  );

  return { mutate, isPending };
};
