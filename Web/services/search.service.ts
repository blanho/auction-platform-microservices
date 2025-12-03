import apiClient from "@/lib/api/axios";
import { SearchRequestDto, SearchResultDto } from "@/types/search";
import { Auction } from "@/types/auction";
import { API_ENDPOINTS } from "@/constants/api";

export const searchService = {
  search: async (params: SearchRequestDto): Promise<SearchResultDto> => {
    const { data } = await apiClient.get<SearchResultDto>(
      API_ENDPOINTS.SEARCH,
      { params }
    );
    return data;
  },

  getById: async (id: string): Promise<Auction> => {
    const { data } = await apiClient.get<Auction>(
      API_ENDPOINTS.SEARCH_ITEM_BY_ID(id)
    );
    return data;
  }
};
