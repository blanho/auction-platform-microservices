import { QueryClient } from "@tanstack/react-query";
import { DEFAULT_STALE_TIME } from "@/constants/api";

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: DEFAULT_STALE_TIME,
      refetchOnWindowFocus: false,
      retry: 1
    },
    mutations: {
      retry: false
    }
  }
});
