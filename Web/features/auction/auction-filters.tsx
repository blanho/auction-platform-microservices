"use client";

import { useAuction, useAuctionFilters } from "@/context/auction.context";
import { Input } from "@/components/ui/input";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue
} from "@/components/ui/select";
import { Button } from "@/components/ui/button";
import { X, Search } from "lucide-react";
import { SearchFilterBy } from "@/types/search";
import debounce from "lodash/debounce";
import { useState, useMemo } from "react";

const STATUS_OPTIONS = [
    { value: "all", label: "All Status" },
    { value: SearchFilterBy.Live, label: "Live" },
    { value: SearchFilterBy.EndingSoon, label: "Ending Soon" },
    { value: SearchFilterBy.Finished, label: "Finished" }
];

const CATEGORY_OPTIONS = [
    { value: "all", label: "All Categories" },
    { value: "sedan", label: "Sedan" },
    { value: "suv", label: "SUV" },
    { value: "truck", label: "Truck" },
    { value: "coupe", label: "Coupe" },
    { value: "convertible", label: "Convertible" },
    { value: "wagon", label: "Wagon" },
    { value: "van", label: "Van" },
    { value: "other", label: "Other" }
];

export function AuctionFilters() {
    const filters = useAuctionFilters();
    const {
        setSearchTerm,
        setStatus,
        setCategory,
        setPriceRange,
        clearFilters
    } = useAuction();

    const [localSearchTerm, setLocalSearchTerm] = useState(filters.searchTerm);

    const debouncedSearch = useMemo(
        () => debounce((value: string) => setSearchTerm(value), 300),
        [setSearchTerm]
    );

    if (localSearchTerm !== filters.searchTerm) {
        setLocalSearchTerm(filters.searchTerm);
    }

    const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const value = e.target.value;
        setLocalSearchTerm(value);
        debouncedSearch(value);
    };

    const handleStatusChange = (value: string) => {
        setStatus(value === "all" ? "" : value);
    };

    const handleCategoryChange = (value: string) => {
        setCategory(value === "all" ? "" : value);
    };

    const hasActiveFilters =
        filters.searchTerm ||
        filters.status ||
        filters.category ||
        filters.minPrice !== undefined ||
        filters.maxPrice !== undefined;

    return (
        <div className="space-y-4">
            <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
                {/* Search Input */}
                <div className="relative flex-1">
                    <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                    <Input
                        type="text"
                        placeholder="Search auctions..."
                        value={localSearchTerm}
                        onChange={handleSearchChange}
                        className="pl-10"
                    />
                </div>

                <Select value={filters.status || "all"} onValueChange={handleStatusChange}>
                    <SelectTrigger className="w-full sm:w-[180px]">
                        <SelectValue placeholder="All Status" />
                    </SelectTrigger>
                    <SelectContent>
                        {STATUS_OPTIONS.map((option) => (
                            <SelectItem key={option.value} value={option.value}>
                                {option.label}
                            </SelectItem>
                        ))}
                    </SelectContent>
                </Select>

                <Select value={filters.category || "all"} onValueChange={handleCategoryChange}>
                    <SelectTrigger className="w-full sm:w-[180px]">
                        <SelectValue placeholder="All Categories" />
                    </SelectTrigger>
                    <SelectContent>
                        {CATEGORY_OPTIONS.map((option) => (
                            <SelectItem key={option.value} value={option.value}>
                                {option.label}
                            </SelectItem>
                        ))}
                    </SelectContent>
                </Select>

                {hasActiveFilters && (
                    <Button variant="ghost" size="icon" onClick={clearFilters}>
                        <X className="h-4 w-4" />
                        <span className="sr-only">Clear filters</span>
                    </Button>
                )}
            </div>

            <div className="flex flex-col gap-2 sm:flex-row sm:items-center">
                <span className="text-sm text-muted-foreground">Price Range:</span>
                <div className="flex items-center gap-2">
                    <Input
                        type="number"
                        placeholder="Min"
                        value={filters.minPrice === undefined ? "" : filters.minPrice}
                        onChange={(e) =>
                            setPriceRange(
                                e.target.value ? Number(e.target.value) : undefined,
                                filters.maxPrice
                            )
                        }
                        className="w-24"
                    />
                    <span className="text-muted-foreground">-</span>
                    <Input
                        type="number"
                        placeholder="Max"
                        value={filters.maxPrice === undefined ? "" : filters.maxPrice}
                        onChange={(e) =>
                            setPriceRange(
                                filters.minPrice,
                                e.target.value ? Number(e.target.value) : undefined
                            )
                        }
                        className="w-24"
                    />
                </div>
            </div>
        </div>
    );
}
