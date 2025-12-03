"use client";

import { useAuction, useAuctionSort } from "@/context/auction.context";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue
} from "@/components/ui/select";
import { Button } from "@/components/ui/button";
import { ArrowUp, ArrowDown } from "lucide-react";
import { SearchOrderBy } from "@/types/search";

const SORT_OPTIONS = [
    { value: SearchOrderBy.New, label: "Newest" },
    { value: SearchOrderBy.Make, label: "Make" },
    { value: SearchOrderBy.EndingSoon, label: "Ending Soon" }
];

export function AuctionSort() {
    const sort = useAuctionSort();
    const { setSortBy, toggleSortOrder } = useAuction();

    return (
        <div className="flex items-center gap-2">
            <span className="text-sm text-muted-foreground">Sort by:</span>
            <Select value={sort.sortBy} onValueChange={setSortBy}>
                <SelectTrigger className="w-[150px]">
                    <SelectValue placeholder="Sort by" />
                </SelectTrigger>
                <SelectContent>
                    {SORT_OPTIONS.map((option) => (
                        <SelectItem key={option.value} value={option.value}>
                            {option.label}
                        </SelectItem>
                    ))}
                </SelectContent>
            </Select>

            <Button variant="outline" size="icon" onClick={toggleSortOrder}>
                {sort.sortOrder === "asc" ? (
                    <ArrowUp className="h-4 w-4" />
                ) : (
                    <ArrowDown className="h-4 w-4" />
                )}
                <span className="sr-only">Toggle sort order</span>
            </Button>
        </div>
    );
}
