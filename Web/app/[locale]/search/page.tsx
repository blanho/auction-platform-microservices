'use client';

import { useState, useEffect, useCallback, Suspense } from 'react';
import { useSearchParams, useRouter } from 'next/navigation';
import { MainLayout } from "@/components/layout/main-layout";
import { SearchBar } from "@/features/search/search-bar";
import { AuctionCard } from "@/features/auction/auction-card";
import { LoadingSpinner } from "@/components/common/loading-spinner";
import { EmptyState } from "@/components/common/empty-state";
import { Pagination } from "@/components/common/pagination";
import { auctionService, GetAuctionsParams, AuctionPagedResult } from "@/services/auction.service";
import { Category } from "@/types/auction";
import { Search } from 'lucide-react';

function SearchContent() {
    const router = useRouter();
    const searchParams = useSearchParams();
    
    const [filters, setFilters] = useState<GetAuctionsParams>({
        searchTerm: searchParams.get('q') || '',
        status: searchParams.get('status') || undefined,
        pageNumber: parseInt(searchParams.get('page') || '1'),
        pageSize: 12,
    });

    const [data, setData] = useState<AuctionPagedResult | null>(null);
    const [categories, setCategories] = useState<Category[]>([]);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<Error | null>(null);

    useEffect(() => {
        auctionService.getCategories().then(setCategories).catch(() => {});
    }, []);

    const fetchResults = useCallback(async () => {
        const hasSearchCriteria = filters.searchTerm || filters.status || filters.category;
        if (!hasSearchCriteria) {
            setData(null);
            return;
        }

        setIsLoading(true);
        setError(null);

        try {
            const result = await auctionService.getAuctions(filters);
            setData(result);
        } catch (err) {
            setError(err as Error);
        } finally {
            setIsLoading(false);
        }
    }, [filters]);

    useEffect(() => {
        fetchResults();
    }, [fetchResults]);

    const handleSearch = useCallback((params: {
        query?: string;
        sortBy?: string;
        status?: string;
        category?: string;
        minPrice?: number;
        maxPrice?: number;
        condition?: string;
    }) => {
        const newFilters: GetAuctionsParams = {
            ...filters,
            searchTerm: params.query,
            status: params.status === 'all' ? undefined : params.status,
            orderBy: params.sortBy,
            pageNumber: 1,
        };

        setFilters(newFilters);

        const queryParams = new URLSearchParams();
        if (params.query) queryParams.set('q', params.query);
        if (params.status && params.status !== 'all') queryParams.set('status', params.status);
        if (params.sortBy) queryParams.set('sort', params.sortBy);
        
        router.push(`/search?${queryParams.toString()}`, { scroll: false });
    }, [filters, router]);

    const handlePageChange = (page: number) => {
        setFilters(prev => ({ ...prev, pageNumber: page }));
        window.scrollTo({ top: 0, behavior: 'smooth' });
    };

    const hasSearchCriteria = filters.searchTerm || filters.status || filters.category;

    return (
        <MainLayout>
            <div className="space-y-6">
                <div>
                    <h1 className="text-3xl font-bold flex items-center gap-2">
                        <Search className="h-8 w-8" />
                        Search Auctions
                    </h1>
                    <p className="text-muted-foreground mt-1">
                        Find the perfect item from thousands of auctions
                    </p>
                </div>

                <SearchBar 
                    onSearch={handleSearch} 
                    categories={categories}
                    showFilters={true}
                />

                {isLoading ? (
                    <div className="flex items-center justify-center py-12">
                        <LoadingSpinner size="lg" />
                    </div>
                ) : error ? (
                    <EmptyState
                        title="Search failed"
                        description="An error occurred while searching. Please try again."
                    />
                ) : data?.items && data.items.length > 0 ? (
                    <>
                        <div className="flex items-center justify-between">
                            <p className="text-sm text-muted-foreground">
                                Showing {((filters.pageNumber || 1) - 1) * (filters.pageSize || 12) + 1}-
                                {Math.min((filters.pageNumber || 1) * (filters.pageSize || 12), data.totalCount)} of {data.totalCount} results
                            </p>
                        </div>
                        
                        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
                            {data.items.map((item) => (
                                <AuctionCard key={item.id} auction={item} />
                            ))}
                        </div>

                        {data.totalPages > 1 && (
                            <div className="flex justify-center pt-6">
                                <Pagination
                                    currentPage={filters.pageNumber || 1}
                                    totalPages={data.totalPages}
                                    onPageChange={handlePageChange}
                                />
                            </div>
                        )}
                    </>
                ) : hasSearchCriteria ? (
                    <EmptyState
                        title="No results found"
                        description="Try adjusting your search terms or filters to find what you're looking for."
                    />
                ) : (
                    <div className="text-center py-12">
                        <Search className="h-16 w-16 mx-auto text-muted-foreground/50 mb-4" />
                        <h2 className="text-xl font-semibold mb-2">Start Your Search</h2>
                        <p className="text-muted-foreground max-w-md mx-auto">
                            Enter a search term or apply filters to discover auctions. 
                            You can search by title, make, model, and more.
                        </p>
                    </div>
                )}
            </div>
        </MainLayout>
    );
}

export default function SearchPage() {
    return (
        <Suspense fallback={
            <MainLayout>
                <div className="flex items-center justify-center py-20">
                    <LoadingSpinner size="lg" />
                </div>
            </MainLayout>
        }>
            <SearchContent />
        </Suspense>
    );
}
