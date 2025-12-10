import { useEffect, useState, useMemo } from "react";
import {
  auctionService,
  AuctionPagedResult,
  GetAuctionsParams
} from "@/services/auction.service";

export const useSearch = (params: GetAuctionsParams) => {
  const [data, setData] = useState<AuctionPagedResult | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  const enabled = useMemo(
    () => !!params.searchTerm || !!params.status,
    [params.searchTerm, params.status]
  );

  useEffect(() => {
    if (!enabled) {
      return;
    }

    let isMounted = true;

    const fetchData = async () => {
      try {
        const result = await auctionService.getAuctions(params);
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
  }, [enabled, params]);

  return { data, isLoading, error };
};
