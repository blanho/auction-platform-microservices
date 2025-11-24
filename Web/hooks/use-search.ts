import { useQuery } from "@tanstack/react-query";
import { searchService } from "@/services/search.service";
import { SearchRequestDto } from "@/types/search";
import { QUERY_KEYS } from "@/constants/api";

export const useSearch = (params: SearchRequestDto) => {
  return useQuery({
    queryKey: [QUERY_KEYS.SEARCH, params],
    queryFn: () => searchService.search(params),
    enabled: !!params.searchTerm || !!params.filterBy
  });
};
