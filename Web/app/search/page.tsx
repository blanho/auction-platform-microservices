// Search page
'use client';

import { useState } from 'react';
import { MainLayout } from "@/components/layout/main-layout";
import { SearchBar } from "@/features/search/search-bar";
import { useSearch } from "@/hooks/use-search";
import { SearchRequestDto } from "@/types/search";
import { Auction } from "@/types/auction";
import { AuctionCard } from "@/features/auction/auction-card";
import { LoadingSpinner } from "@/components/common/loading-spinner";
import { EmptyState } from "@/components/common/empty-state";

export default function SearchPage() {
    const [searchParams, setSearchParams] = useState<SearchRequestDto>({
        searchTerm: '',
        pageNumber: 1,
        pageSize: 12,
    });

    const { data, isLoading } = useSearch(searchParams);

    const handleSearch = (params: Partial<SearchRequestDto>) => {
        setSearchParams((prev) => ({ ...prev, ...params, pageNumber: 1 }));
    };

    return (
        <MainLayout>
            <div className="space-y-6">
                <div>
                    <h1 className="text-3xl font-bold">Search Auctions</h1>
                    <p className="text-muted-foreground">
                        Find the perfect auction for you
                    </p>
                </div>

                <SearchBar onSearch={handleSearch} />

                {isLoading ? (
                    <div className="flex items-center justify-center py-12">
                        <LoadingSpinner size="lg" />
                    </div>
                ) : data?.results && data.results.length > 0 ? (
                    <>
                        <p className="text-sm text-muted-foreground">
                            Found {data.totalCount} results
                        </p>
                        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
                            {data.results.map((item) => (
                                <AuctionCard key={item.id} auction={item as Auction} />
                            ))}
                        </div>
                    </>
                ) : searchParams.searchTerm || searchParams.filterBy ? (
                    <EmptyState
                        title="No results found"
                        description="Try adjusting your search criteria"
                    />
                ) : (
                    <EmptyState
                        title="Start searching"
                        description="Enter a search term or apply filters to find auctions"
                    />
                )}
            </div>
        </MainLayout>
    );
}
