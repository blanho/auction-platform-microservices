import { useEffect, useState, useMemo } from "react";
import { searchService } from "@/services/search.service";
import { SearchRequestDto, SearchResultDto } from "@/types/search";

export const useSearch = (params: SearchRequestDto) => {
  const [data, setData] = useState<SearchResultDto | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  const enabled = useMemo(
    () => !!params.query || !!params.status || !!params.category,
    [params.query, params.status, params.category]
  );

  useEffect(() => {
    if (!enabled) {
      return;
    }

    let isMounted = true;

    const fetchData = async () => {
      try {
        const result = await searchService.search(params);
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
