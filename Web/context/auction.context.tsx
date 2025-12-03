"use client";

import React, { createContext, useContext, useCallback, useMemo } from "react";
import { useSearchParams, useRouter } from "next/navigation";
import { SearchOrderBy } from "@/types/search";

export interface AuctionParams {
    page: number;
    pageSize: number;
    searchTerm: string;
    status: string;
    category: string;
    seller: string;
    winner: string;
    minPrice: number | undefined;
    maxPrice: number | undefined;
    sortBy: string;
    sortOrder: "asc" | "desc";
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

interface PaginationInfo {
    totalCount: number;
    totalPages: number;
    hasNextPage: boolean;
    hasPreviousPage: boolean;
}

interface AuctionContextType {
    params: AuctionParams;
    paginationInfo: PaginationInfo;
    filters: FilterParams;
    sort: SortParams;
    pagination: {
        page: number;
        pageSize: number;
        totalCount: number;
        totalPages: number;
        hasNextPage: boolean;
        hasPreviousPage: boolean;
    };
    setPage: (page: number) => void;
    setPageSize: (pageSize: number) => void;
    goToPage: (type: "first" | "prev" | "next" | "last" | number) => void;
    setSearchTerm: (searchTerm: string) => void;
    setStatus: (status: string) => void;
    setCategory: (category: string) => void;
    setSeller: (seller: string) => void;
    setWinner: (winner: string) => void;
    setPriceRange: (minPrice: number | undefined, maxPrice: number | undefined) => void;
    clearFilters: () => void;
    setSortBy: (sortBy: string) => void;
    setSortOrder: (sortOrder: "asc" | "desc") => void;
    toggleSortOrder: () => void;
    setPaginationInfo: (info: PaginationInfo) => void;
}

const AuctionContext = createContext<AuctionContextType | undefined>(undefined);

const DEFAULT_PAGE_SIZE = 12;
const DEFAULT_SORT_BY = SearchOrderBy.New;

function parseParams(searchParams: URLSearchParams): AuctionParams {
    const page = Math.max(1, Number(searchParams.get("page")) || 1);
    const pageSize = Number(searchParams.get("pageSize")) || DEFAULT_PAGE_SIZE;
    const searchTerm = searchParams.get("searchTerm") || "";
    const status = searchParams.get("status") || "";
    const category = searchParams.get("category") || "";
    const seller = searchParams.get("seller") || "";
    const winner = searchParams.get("winner") || "";
    const minPriceStr = searchParams.get("minPrice");
    const maxPriceStr = searchParams.get("maxPrice");
    const sortBy = searchParams.get("sortBy") || DEFAULT_SORT_BY;
    const sortOrder = (searchParams.get("sortOrder") || "desc") as "asc" | "desc";

    return {
        page,
        pageSize,
        searchTerm,
        status,
        category,
        seller,
        winner,
        minPrice: minPriceStr ? Number(minPriceStr) : undefined,
        maxPrice: maxPriceStr ? Number(maxPriceStr) : undefined,
        sortBy,
        sortOrder
    };
}

function paramsToQueryString(params: AuctionParams): string {
    const query = new URLSearchParams();

    if (params.page !== 1) query.set("page", String(params.page));
    if (params.pageSize !== DEFAULT_PAGE_SIZE) query.set("pageSize", String(params.pageSize));
    if (params.searchTerm) query.set("searchTerm", params.searchTerm);
    if (params.status) query.set("status", params.status);
    if (params.category) query.set("category", params.category);
    if (params.seller) query.set("seller", params.seller);
    if (params.winner) query.set("winner", params.winner);
    if (params.minPrice !== undefined) query.set("minPrice", String(params.minPrice));
    if (params.maxPrice !== undefined) query.set("maxPrice", String(params.maxPrice));
    if (params.sortBy !== DEFAULT_SORT_BY) query.set("sortBy", params.sortBy);
    if (params.sortOrder !== "desc") query.set("sortOrder", params.sortOrder);

    const queryString = query.toString();
    return queryString ? `?${queryString}` : "";
}

export function AuctionProvider({ children }: { children: React.ReactNode }) {
    const searchParams = useSearchParams();
    const router = useRouter();

    const params = useMemo(() => parseParams(searchParams), [searchParams]);

    const [paginationInfo, setPaginationInfoState] = React.useState<PaginationInfo>({
        totalCount: 0,
        totalPages: 0,
        hasNextPage: false,
        hasPreviousPage: false
    });

    const updateParams = useCallback(
        (newParams: Partial<AuctionParams>) => {
            const updated = { ...params, ...newParams };
            const queryString = paramsToQueryString(updated);
            router.push(`/auctions${queryString}`);
        },
        [params, router]
    );

    const filters = useMemo(
        () => ({
            searchTerm: params.searchTerm,
            status: params.status,
            category: params.category,
            seller: params.seller,
            winner: params.winner,
            minPrice: params.minPrice,
            maxPrice: params.maxPrice
        }),
        [params]
    );

    const sort = useMemo(
        () => ({
            sortBy: params.sortBy,
            sortOrder: params.sortOrder
        }),
        [params.sortBy, params.sortOrder]
    );

    const pagination = useMemo(
        () => ({
            page: params.page,
            pageSize: params.pageSize,
            totalCount: paginationInfo.totalCount,
            totalPages: paginationInfo.totalPages,
            hasNextPage: paginationInfo.hasNextPage,
            hasPreviousPage: paginationInfo.hasPreviousPage
        }),
        [params.page, params.pageSize, paginationInfo]
    );

    const value: AuctionContextType = {
        params,
        paginationInfo,
        filters,
        sort,
        pagination,
        setPage: (page) => updateParams({ page }),
        setPageSize: (pageSize) => updateParams({ pageSize, page: 1 }),
        goToPage: (type) => {
            let newPage = params.page;
            switch (type) {
                case "first":
                    newPage = 1;
                    break;
                case "prev":
                    newPage = Math.max(1, params.page - 1);
                    break;
                case "next":
                    newPage = Math.min(paginationInfo.totalPages, params.page + 1);
                    break;
                case "last":
                    newPage = paginationInfo.totalPages;
                    break;
                default:
                    if (typeof type === "number") {
                        newPage = Math.max(1, Math.min(paginationInfo.totalPages, type));
                    }
            }
            updateParams({ page: newPage });
        },
        setSearchTerm: (searchTerm) => updateParams({ searchTerm, page: 1 }),
        setStatus: (status) => updateParams({ status, page: 1 }),
        setCategory: (category) => updateParams({ category, page: 1 }),
        setSeller: (seller) => updateParams({ seller, page: 1 }),
        setWinner: (winner) => updateParams({ winner, page: 1 }),
        setPriceRange: (minPrice, maxPrice) =>
            updateParams({ minPrice, maxPrice, page: 1 }),
        clearFilters: () =>
            updateParams({
                searchTerm: "",
                status: "",
                category: "",
                seller: "",
                winner: "",
                minPrice: undefined,
                maxPrice: undefined,
                page: 1
            }),
        setSortBy: (sortBy) => updateParams({ sortBy, page: 1 }),
        setSortOrder: (sortOrder) => updateParams({ sortOrder, page: 1 }),
        toggleSortOrder: () =>
            updateParams({
                sortOrder: params.sortOrder === "asc" ? "desc" : "asc",
                page: 1
            }),
        setPaginationInfo: setPaginationInfoState
    };

    return (
        <AuctionContext.Provider value={value}>
            {children}
        </AuctionContext.Provider>
    );
}

export function useAuction() {
    const context = useContext(AuctionContext);
    if (!context) {
        throw new Error("useAuction must be used within AuctionProvider");
    }
    return context;
}

export function useAuctionFilters() {
    const context = useContext(AuctionContext);
    if (!context) {
        throw new Error("useAuctionFilters must be used within AuctionProvider");
    }
    return context.filters;
}

export function useAuctionSort() {
    const context = useContext(AuctionContext);
    if (!context) {
        throw new Error("useAuctionSort must be used within AuctionProvider");
    }
    return context.sort;
}

export function useAuctionPagination() {
    const context = useContext(AuctionContext);
    if (!context) {
        throw new Error("useAuctionPagination must be used within AuctionProvider");
    }
    return context.pagination;
}

export function useAuctionParams() {
    const context = useContext(AuctionContext);
    if (!context) {
        throw new Error("useAuctionParams must be used within AuctionProvider");
    }
    return context.params;
}
