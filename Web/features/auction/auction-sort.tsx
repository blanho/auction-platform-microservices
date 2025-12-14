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
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
    faArrowUpWideShort,
    faArrowDownWideShort,
    faClock,
    faCalendarPlus,
    faCar,
    faDollarSign,
    faFire,
} from "@fortawesome/free-solid-svg-icons";
import { SearchOrderBy } from "@/types/search";
import { cn } from "@/lib/utils";
import { motion } from "framer-motion";

const SORT_OPTIONS = [
    { value: SearchOrderBy.New, label: "Newest First", icon: faCalendarPlus },
    { value: SearchOrderBy.EndingSoon, label: "Ending Soon", icon: faClock },
    { value: SearchOrderBy.Make, label: "Make", icon: faCar },
];

export function AuctionSort() {
    const sort = useAuctionSort();
    const { setSortBy, toggleSortOrder } = useAuction();

    return (
        <div className="flex items-center gap-2">
            <span className="text-sm font-medium text-slate-500 dark:text-slate-400 hidden sm:inline">
                Sort by
            </span>
            
            <div className="flex items-center bg-white dark:bg-slate-900 rounded-xl border border-slate-200 dark:border-slate-700 h-11 px-1">
                <Select value={sort.sortBy} onValueChange={setSortBy}>
                    <SelectTrigger className="w-[140px] h-9 border-0 bg-transparent focus:ring-0 focus:ring-offset-0">
                        <SelectValue placeholder="Sort by" />
                    </SelectTrigger>
                    <SelectContent className="rounded-xl">
                        {SORT_OPTIONS.map((option) => (
                            <SelectItem key={option.value} value={option.value} className="rounded-lg">
                                <span className="flex items-center gap-2">
                                    <FontAwesomeIcon icon={option.icon} className="w-3.5 h-3.5 text-slate-400" />
                                    {option.label}
                                </span>
                            </SelectItem>
                        ))}
                    </SelectContent>
                </Select>

                <div className="w-px h-6 bg-slate-200 dark:bg-slate-700" />

                <motion.div whileTap={{ scale: 0.95 }}>
                    <Button
                        variant="ghost"
                        size="sm"
                        onClick={toggleSortOrder}
                        className={cn(
                            "h-9 w-9 p-0 rounded-lg transition-colors",
                            sort.sortOrder === "asc"
                                ? "text-purple-600 dark:text-purple-400 bg-purple-50 dark:bg-purple-900/20"
                                : "text-slate-600 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-800"
                        )}
                    >
                        <FontAwesomeIcon
                            icon={sort.sortOrder === "asc" ? faArrowUpWideShort : faArrowDownWideShort}
                            className="w-4 h-4"
                        />
                        <span className="sr-only">
                            {sort.sortOrder === "asc" ? "Ascending" : "Descending"}
                        </span>
                    </Button>
                </motion.div>
            </div>
        </div>
    );
}
