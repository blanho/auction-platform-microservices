'use client';

import { useState, useEffect } from 'react';
import { Search, SlidersHorizontal, X } from 'lucide-react';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from '@/components/ui/select';
import {
    Sheet,
    SheetContent,
    SheetDescription,
    SheetHeader,
    SheetTitle,
    SheetTrigger,
} from '@/components/ui/sheet';
import { Label } from '@/components/ui/label';
import { Slider } from '@/components/ui/slider';
import { Badge } from '@/components/ui/badge';
import { useDebounce } from '@/hooks';
import { formatCurrency } from '@/utils';

interface SearchFilters {
    query?: string;
    sortBy?: string;
    status?: string;
    category?: string;
    minPrice?: number;
    maxPrice?: number;
    condition?: string;
}

interface SearchBarProps {
    onSearch: (params: SearchFilters) => void;
    categories?: Array<{ id: string; name: string }>;
    showFilters?: boolean;
}

export function SearchBar({ onSearch, categories = [], showFilters = true }: SearchBarProps) {
    const [query, setQuery] = useState('');
    const [sortBy, setSortBy] = useState<string>();
    const [status, setStatus] = useState<string>();
    const [category, setCategory] = useState<string>();
    const [priceRange, setPriceRange] = useState<[number, number]>([0, 100000]);
    const [condition, setCondition] = useState<string>();
    const [isFiltersOpen, setIsFiltersOpen] = useState(false);
    
    const debouncedQuery = useDebounce(query, 300);

    useEffect(() => {
        if (debouncedQuery !== undefined) {
            onSearch({
                query: debouncedQuery,
                sortBy,
                status,
                category,
                minPrice: priceRange[0] > 0 ? priceRange[0] : undefined,
                maxPrice: priceRange[1] < 100000 ? priceRange[1] : undefined,
                condition,
            });
        }
    }, [debouncedQuery, sortBy, status, category, priceRange, condition, onSearch]);

    const activeFiltersCount = [
        status,
        category,
        condition,
        priceRange[0] > 0 || priceRange[1] < 100000,
    ].filter(Boolean).length;

    const clearFilters = () => {
        setStatus(undefined);
        setCategory(undefined);
        setCondition(undefined);
        setPriceRange([0, 100000]);
    };

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        onSearch({
            query,
            sortBy,
            status,
            category,
            minPrice: priceRange[0] > 0 ? priceRange[0] : undefined,
            maxPrice: priceRange[1] < 100000 ? priceRange[1] : undefined,
            condition,
        });
    };

    return (
        <form onSubmit={handleSubmit} className="space-y-4">
            <div className="flex gap-2">
                <div className="relative flex-1">
                    <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                    <Input
                        type="text"
                        placeholder="Search auctions by title, make, model..."
                        value={query}
                        onChange={(e) => setQuery(e.target.value)}
                        className="pl-10"
                    />
                    {query && (
                        <button
                            type="button"
                            onClick={() => setQuery('')}
                            className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground"
                        >
                            <X className="h-4 w-4" />
                        </button>
                    )}
                </div>
                <Button type="submit">Search</Button>
                
                {showFilters && (
                    <Sheet open={isFiltersOpen} onOpenChange={setIsFiltersOpen}>
                        <SheetTrigger asChild>
                            <Button variant="outline" className="relative">
                                <SlidersHorizontal className="h-4 w-4 mr-2" />
                                Filters
                                {activeFiltersCount > 0 && (
                                    <Badge 
                                        variant="secondary" 
                                        className="absolute -top-2 -right-2 h-5 w-5 rounded-full p-0 flex items-center justify-center text-xs"
                                    >
                                        {activeFiltersCount}
                                    </Badge>
                                )}
                            </Button>
                        </SheetTrigger>
                        <SheetContent>
                            <SheetHeader>
                                <SheetTitle>Filter Auctions</SheetTitle>
                                <SheetDescription>
                                    Narrow down your search results
                                </SheetDescription>
                            </SheetHeader>
                            
                            <div className="space-y-6 py-6">
                                <div className="space-y-2">
                                    <Label>Status</Label>
                                    <Select value={status} onValueChange={setStatus}>
                                        <SelectTrigger>
                                            <SelectValue placeholder="All statuses" />
                                        </SelectTrigger>
                                        <SelectContent>
                                            <SelectItem value="all">All Statuses</SelectItem>
                                            <SelectItem value="live">Live</SelectItem>
                                            <SelectItem value="endingSoon">Ending Soon</SelectItem>
                                            <SelectItem value="finished">Finished</SelectItem>
                                        </SelectContent>
                                    </Select>
                                </div>

                                {categories.length > 0 && (
                                    <div className="space-y-2">
                                        <Label>Category</Label>
                                        <Select value={category} onValueChange={setCategory}>
                                            <SelectTrigger>
                                                <SelectValue placeholder="All categories" />
                                            </SelectTrigger>
                                            <SelectContent>
                                                <SelectItem value="all">All Categories</SelectItem>
                                                {categories.map((cat) => (
                                                    <SelectItem key={cat.id} value={cat.id}>
                                                        {cat.name}
                                                    </SelectItem>
                                                ))}
                                            </SelectContent>
                                        </Select>
                                    </div>
                                )}

                                <div className="space-y-2">
                                    <Label>Condition</Label>
                                    <Select value={condition} onValueChange={setCondition}>
                                        <SelectTrigger>
                                            <SelectValue placeholder="Any condition" />
                                        </SelectTrigger>
                                        <SelectContent>
                                            <SelectItem value="all">Any Condition</SelectItem>
                                            <SelectItem value="New">Brand New</SelectItem>
                                            <SelectItem value="LikeNew">Like New</SelectItem>
                                            <SelectItem value="Excellent">Excellent</SelectItem>
                                            <SelectItem value="Good">Good</SelectItem>
                                            <SelectItem value="Fair">Fair</SelectItem>
                                        </SelectContent>
                                    </Select>
                                </div>

                                <div className="space-y-4">
                                    <Label>Price Range</Label>
                                    <Slider
                                        value={priceRange}
                                        onValueChange={(value) => setPriceRange(value as [number, number])}
                                        max={100000}
                                        step={1000}
                                        className="w-full"
                                    />
                                    <div className="flex justify-between text-sm text-muted-foreground">
                                        <span>{formatCurrency(priceRange[0])}</span>
                                        <span>{priceRange[1] >= 100000 ? '$100,000+' : formatCurrency(priceRange[1])}</span>
                                    </div>
                                </div>

                                {activeFiltersCount > 0 && (
                                    <Button
                                        type="button"
                                        variant="outline"
                                        onClick={clearFilters}
                                        className="w-full"
                                    >
                                        Clear All Filters
                                    </Button>
                                )}
                            </div>
                        </SheetContent>
                    </Sheet>
                )}
            </div>

            <div className="flex flex-wrap gap-4">
                <Select value={sortBy} onValueChange={setSortBy}>
                    <SelectTrigger className="w-[180px]">
                        <SelectValue placeholder="Sort by" />
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem value="new">Newest First</SelectItem>
                        <SelectItem value="endingSoon">Ending Soon</SelectItem>
                        <SelectItem value="priceAsc">Price: Low to High</SelectItem>
                        <SelectItem value="priceDesc">Price: High to Low</SelectItem>
                        <SelectItem value="bidsDesc">Most Bids</SelectItem>
                    </SelectContent>
                </Select>

                <Select value={status} onValueChange={setStatus}>
                    <SelectTrigger className="w-[180px]">
                        <SelectValue placeholder="Status" />
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem value="all">All Statuses</SelectItem>
                        <SelectItem value="live">Live</SelectItem>
                        <SelectItem value="endingSoon">Ending Soon</SelectItem>
                        <SelectItem value="finished">Finished</SelectItem>
                    </SelectContent>
                </Select>

                {activeFiltersCount > 0 && (
                    <Button
                        type="button"
                        variant="ghost"
                        size="sm"
                        onClick={clearFilters}
                        className="text-muted-foreground"
                    >
                        <X className="h-4 w-4 mr-1" />
                        Clear filters
                    </Button>
                )}
            </div>
        </form>
    );
}
