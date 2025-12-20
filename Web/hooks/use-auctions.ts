import { useEffect, useState, useMemo } from "react";
import { auctionService, AuctionPagedResult } from "@/services/auction.service";
import {
  useAuctionParams,
  useAuction as useAuctionContext
} from "@/context/auction.context";

export const useAuctions = () => {
  const params = useAuctionParams();
  const { setPaginationInfo } = useAuctionContext();
  const [data, setData] = useState<AuctionPagedResult | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  const searchParams = useMemo(
    () => ({
      searchTerm: params.searchTerm || undefined,
      pageNumber: params.page,
      pageSize: params.pageSize,
      status: params.status || undefined,
      sellerUsername: params.sellerUsername || undefined,
      winnerUsername: params.winnerUsername || undefined,
      brandId: params.brandId || undefined,
      condition: params.condition || undefined,
      orderBy: params.sortBy || undefined,
      descending: params.sortOrder === "desc"
    }),
    [
      params.searchTerm,
      params.page,
      params.pageSize,
      params.status,
      params.sellerUsername,
      params.winnerUsername,
      params.brandId,
      params.condition,
      params.sortBy,
      params.sortOrder
    ]
  );

  useEffect(() => {
    let isMounted = true;

    const fetchData = async () => {
      try {
        const result = await auctionService.getAuctions(searchParams);
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
