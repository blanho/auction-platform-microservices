import { apiClient } from "@/lib/api-client";
import { FlashSale, ActiveFlashSale } from "@/types/auction";

const BASE_URL = "/auction/api/v1/flashsales";

export const flashSaleService = {
  async getFlashSales(activeOnly: boolean = true): Promise<FlashSale[]> {
    const response = await apiClient.get<FlashSale[]>(BASE_URL, {
      params: { activeOnly },
    });
    return response.data;
  },

  async getActiveFlashSale(): Promise<ActiveFlashSale | null> {
    try {
      const response = await apiClient.get<ActiveFlashSale>(`${BASE_URL}/active`);
      return response.data;
    } catch (error: unknown) {
      const axiosError = error as { response?: { status: number } };
      if (axiosError.response?.status === 204) {
        return null;
      }
      throw error;
    }
  },

  async getFlashSaleById(id: string): Promise<FlashSale> {
    const response = await apiClient.get<FlashSale>(`${BASE_URL}/${id}`);
    return response.data;
  },
};
