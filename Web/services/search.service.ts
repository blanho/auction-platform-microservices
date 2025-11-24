import apiClient from "@/lib/api/axios";
import { SearchRequestDto, SearchResultDto } from "@/types/search";
import { API_ENDPOINTS } from "@/constants/api";

export const searchService = {
  search: async (params: SearchRequestDto): Promise<SearchResultDto> => {
    const { data } = await apiClient.get<SearchResultDto>(
      API_ENDPOINTS.SEARCH,
      { params }
    );
    return data;
  }
};
