"use client";

import { useAuction, useAuctionPagination } from "@/context/auction.context";
import { Button } from "@/components/ui/button";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue
} from "@/components/ui/select";
import {
    ChevronFirst,
    ChevronLast,
    ChevronLeft,
    ChevronRight
} from "lucide-react";

const PAGE_SIZE_OPTIONS = [
    { value: "4", label: "4 per page" },
    { value: "8", label: "8 per page" },
    { value: "12", label: "12 per page" },
    { value: "24", label: "24 per page" },
    { value: "48", label: "48 per page" }
];

export function AuctionPagination() {
    const pagination = useAuctionPagination();
    const { goToPage, setPageSize } = useAuction();

    const startItem = (pagination.page - 1) * pagination.pageSize + 1;
    const endItem = Math.min(
        pagination.page * pagination.pageSize,
        pagination.totalCount
    );

    const getPageNumbers = () => {
        const pages: (number | "ellipsis")[] = [];
        const { page, totalPages } = pagination;

        if (totalPages <= 7) {
            for (let i = 1; i <= totalPages; i++) {
                pages.push(i);
            }
        } else {
            pages.push(1);

            if (page > 3) {
                pages.push("ellipsis");
            }

            const start = Math.max(2, page - 1);
            const end = Math.min(totalPages - 1, page + 1);

            for (let i = start; i <= end; i++) {
                pages.push(i);
            }

            if (page < totalPages - 2) {
                pages.push("ellipsis");
            }

            if (totalPages > 1) {
                pages.push(totalPages);
            }
        }

        return pages;
    };

    if (pagination.totalCount === 0) {
        return null;
    }

    return (
        <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
            <div className="text-sm text-muted-foreground">
                Showing {startItem} to {endItem} of {pagination.totalCount} results
            </div>

            <div className="flex items-center gap-2">
                <Button
                    variant="outline"
                    size="icon"
                    onClick={() => goToPage("first")}
                    disabled={!pagination.hasPreviousPage}
                >
                    <ChevronFirst className="h-4 w-4" />
                    <span className="sr-only">First page</span>
                </Button>

                <Button
                    variant="outline"
                    size="icon"
                    onClick={() => goToPage("prev")}
                    disabled={!pagination.hasPreviousPage}
                >
                    <ChevronLeft className="h-4 w-4" />
                    <span className="sr-only">Previous page</span>
                </Button>

                <div className="hidden items-center gap-1 sm:flex">
                    {getPageNumbers().map((pageNum, index) =>
                        pageNum === "ellipsis" ? (
                            <span key={`ellipsis-${index}`} className="px-2 text-muted-foreground">
                                ...
                            </span>
                        ) : (
                            <Button
                                key={pageNum}
                                variant={pageNum === pagination.page ? "default" : "outline"}
                                size="icon"
                                onClick={() => goToPage(pageNum)}
                            >
                                {pageNum}
                            </Button>
                        )
                    )}
                </div>

                <span className="px-2 text-sm sm:hidden">
                    Page {pagination.page} of {pagination.totalPages}
                </span>

                <Button
                    variant="outline"
                    size="icon"
                    onClick={() => goToPage("next")}
                    disabled={!pagination.hasNextPage}
                >
                    <ChevronRight className="h-4 w-4" />
                    <span className="sr-only">Next page</span>
                </Button>

                <Button
                    variant="outline"
                    size="icon"
                    onClick={() => goToPage("last")}
                    disabled={!pagination.hasNextPage}
                >
                    <ChevronLast className="h-4 w-4" />
                    <span className="sr-only">Last page</span>
                </Button>
            </div>
            <Select
                value={pagination.pageSize.toString()}
                onValueChange={(value) => setPageSize(Number(value))}
            >
                <SelectTrigger className="w-[140px]">
                    <SelectValue />
                </SelectTrigger>
                <SelectContent>
                    {PAGE_SIZE_OPTIONS.map((option) => (
                        <SelectItem key={option.value} value={option.value}>
                            {option.label}
                        </SelectItem>
                    ))}
                </SelectContent>
            </Select>
        </div>
    );
}
