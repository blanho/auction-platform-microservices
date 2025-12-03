import { create } from "zustand";
import { persist } from "zustand/middleware";
import { useShallow } from "zustand/react/shallow";
import { SearchOrderBy } from "@/types/search";

export interface PaginationParams {
  page: number;
  pageSize: number;
}

export interface FilterParams {
  searchTerm: string;
  status: string;
  category: string;
  seller: string;
  winner: string;
  minPrice: number | undefined;
  maxPrice: number | undefined;
}

export interface SortParams {
  sortBy: string;
  sortOrder: "asc" | "desc";
}

export interface AuctionParams
  extends PaginationParams,
    FilterParams,
    SortParams {}

export type PageButtonType = "first" | "prev" | "next" | "last" | number;

interface AuctionStore {
  params: AuctionParams;

  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;

  setPage: (page: number) => void;
  setPageSize: (pageSize: number) => void;
  goToPage: (type: PageButtonType) => void;

  setSearchTerm: (searchTerm: string) => void;
  setStatus: (status: string) => void;
  setCategory: (category: string) => void;
  setSeller: (seller: string) => void;
  setWinner: (winner: string) => void;
  setPriceRange: (
    minPrice: number | undefined,
    maxPrice: number | undefined
  ) => void;
  clearFilters: () => void;

  setSortBy: (sortBy: string) => void;
  setSortOrder: (sortOrder: "asc" | "desc") => void;
  toggleSortOrder: () => void;

  setParams: (params: Partial<AuctionParams>) => void;

  setPaginationInfo: (info: {
    totalCount: number;
    totalPages: number;
    hasNextPage: boolean;
    hasPreviousPage: boolean;
  }) => void;

  reset: () => void;
}

const DEFAULT_PARAMS: AuctionParams = {
  page: 1,
  pageSize: 12,

  searchTerm: "",
  status: "",
  category: "",
  seller: "",
  winner: "",
  minPrice: undefined,
  maxPrice: undefined,

  sortBy: SearchOrderBy.New,
  sortOrder: "desc"
};

export const useAuctionStore = create<AuctionStore>()(
  persist(
    (set, get) => ({
      params: { ...DEFAULT_PARAMS },
      totalCount: 0,
      totalPages: 0,
      hasNextPage: false,
      hasPreviousPage: false,

      setPage: (page) =>
        set((state) => ({
          params: { ...state.params, page: Math.max(1, page) }
        })),

      setPageSize: (pageSize) =>
        set((state) => ({
          params: { ...state.params, pageSize, page: 1 }
        })),

      goToPage: (type) => {
        const { params, totalPages } = get();
        let newPage = params.page;

        switch (type) {
          case "first":
            newPage = 1;
            break;
          case "prev":
            newPage = Math.max(1, params.page - 1);
            break;
          case "next":
            newPage = Math.min(totalPages, params.page + 1);
            break;
          case "last":
            newPage = totalPages;
            break;
          default:
            if (typeof type === "number") {
              newPage = Math.max(1, Math.min(totalPages, type));
            }
        }

        set((state) => ({
          params: { ...state.params, page: newPage }
        }));
      },

      setSearchTerm: (searchTerm) =>
        set((state) => ({
          params: { ...state.params, searchTerm, page: 1 }
        })),

      setStatus: (status) =>
        set((state) => ({
          params: { ...state.params, status, page: 1 }
        })),

      setCategory: (category) =>
        set((state) => ({
          params: { ...state.params, category, page: 1 }
        })),

      setSeller: (seller) =>
        set((state) => ({
          params: { ...state.params, seller, page: 1 }
        })),

      setWinner: (winner) =>
        set((state) => ({
          params: { ...state.params, winner, page: 1 }
        })),

      setPriceRange: (minPrice, maxPrice) =>
        set((state) => ({
          params: { ...state.params, minPrice, maxPrice, page: 1 }
        })),

      clearFilters: () =>
        set((state) => ({
          params: {
            ...state.params,
            searchTerm: "",
            status: "",
            category: "",
            seller: "",
            winner: "",
            minPrice: undefined,
            maxPrice: undefined,
            page: 1
          }
        })),

      setSortBy: (sortBy) =>
        set((state) => ({
          params: { ...state.params, sortBy, page: 1 }
        })),

      setSortOrder: (sortOrder) =>
        set((state) => ({
          params: { ...state.params, sortOrder, page: 1 }
        })),

      toggleSortOrder: () =>
        set((state) => ({
          params: {
            ...state.params,
            sortOrder: state.params.sortOrder === "asc" ? "desc" : "asc",
            page: 1
          }
        })),

      setParams: (newParams) =>
        set((state) => ({
          params: { ...state.params, ...newParams }
        })),

      setPaginationInfo: (info) =>
        set({
          totalCount: info.totalCount,
          totalPages: info.totalPages,
          hasNextPage: info.hasNextPage,
          hasPreviousPage: info.hasPreviousPage
        }),

      reset: () =>
        set({
          params: { ...DEFAULT_PARAMS },
          totalCount: 0,
          totalPages: 0,
          hasNextPage: false,
          hasPreviousPage: false
        })
    }),
    {
      name: "auction-store",
      partialize: (state) => ({
        params: {
          pageSize: state.params.pageSize,
          sortBy: state.params.sortBy,
          sortOrder: state.params.sortOrder
        }
      })
    }
  )
);

export const useAuctionParams = () =>
  useAuctionStore(useShallow((state) => state.params));
export const useAuctionPagination = () =>
  useAuctionStore(
    useShallow((state) => ({
      page: state.params.page,
      pageSize: state.params.pageSize,
      totalCount: state.totalCount,
      totalPages: state.totalPages,
      hasNextPage: state.hasNextPage,
      hasPreviousPage: state.hasPreviousPage
    }))
  );
export const useAuctionFilters = () =>
  useAuctionStore(
    useShallow((state) => ({
      searchTerm: state.params.searchTerm,
      status: state.params.status,
      category: state.params.category,
      seller: state.params.seller,
      winner: state.params.winner,
      minPrice: state.params.minPrice,
      maxPrice: state.params.maxPrice
    }))
  );
export const useAuctionSort = () =>
  useAuctionStore(
    useShallow((state) => ({
      sortBy: state.params.sortBy,
      sortOrder: state.params.sortOrder
    }))
  );
