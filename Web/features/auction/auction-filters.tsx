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
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
    faSearch,
    faXmark,
    faFilter,
    faBolt,
    faClock,
    faCheckCircle,
    faLayerGroup,
    faDollarSign,
    faSliders,
} from "@fortawesome/free-solid-svg-icons";
import { SearchFilterBy } from "@/types/search";
import debounce from "lodash/debounce";
import { useState, useMemo } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { cn } from "@/lib/utils";

const STATUS_OPTIONS = [
    { value: "all", label: "All Status", icon: faLayerGroup, color: "text-slate-500" },
    { value: SearchFilterBy.Live, label: "Live", icon: faBolt, color: "text-green-500" },
    { value: SearchFilterBy.EndingSoon, label: "Ending Soon", icon: faClock, color: "text-amber-500" },
    { value: SearchFilterBy.Finished, label: "Finished", icon: faCheckCircle, color: "text-slate-400" }
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

const PRICE_PRESETS = [
    { min: undefined, max: undefined, label: "Any Price" },
    { min: 0, max: 1000, label: "Under $1,000" },
    { min: 1000, max: 5000, label: "$1,000 - $5,000" },
    { min: 5000, max: 10000, label: "$5,000 - $10,000" },
    { min: 10000, max: 25000, label: "$10,000 - $25,000" },
    { min: 25000, max: undefined, label: "$25,000+" },
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
    const [showAdvanced, setShowAdvanced] = useState(false);

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

    const handlePricePreset = (min: number | undefined, max: number | undefined) => {
        setPriceRange(min, max);
    };

    const hasActiveFilters =
        filters.searchTerm ||
        filters.status ||
        filters.category ||
        filters.minPrice !== undefined ||
        filters.maxPrice !== undefined;

    const activeFilterCount = [
        filters.searchTerm,
        filters.status,
        filters.category,
        filters.minPrice !== undefined || filters.maxPrice !== undefined
    ].filter(Boolean).length;

    return (
        <div className="space-y-4">
            <div className="flex flex-col lg:flex-row lg:items-center gap-3">
                <div className="relative flex-1">
                    <div className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400">
                        <FontAwesomeIcon icon={faSearch} className="w-4 h-4" />
                    </div>
                    <Input
                        type="text"
                        placeholder="Search auctions by title, make, model..."
                        value={localSearchTerm}
                        onChange={handleSearchChange}
                        className="pl-11 pr-4 h-11 bg-white dark:bg-slate-900 border-slate-200 dark:border-slate-700 rounded-xl focus:ring-2 focus:ring-purple-500/20 focus:border-purple-500 transition-all"
                    />
                    {localSearchTerm && (
                        <button
                            onClick={() => {
                                setLocalSearchTerm("");
                                setSearchTerm("");
                            }}
                            className="absolute right-4 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600 transition-colors"
                        >
                            <FontAwesomeIcon icon={faXmark} className="w-4 h-4" />
                        </button>
                    )}
                </div>

                <div className="flex items-center gap-2">
                    <Select value={filters.status || "all"} onValueChange={handleStatusChange}>
                        <SelectTrigger className="w-[140px] h-11 bg-white dark:bg-slate-900 border-slate-200 dark:border-slate-700 rounded-xl">
                            <SelectValue placeholder="All Status" />
                        </SelectTrigger>
                        <SelectContent className="rounded-xl">
                            {STATUS_OPTIONS.map((option) => (
                                <SelectItem key={option.value} value={option.value} className="rounded-lg">
                                    <span className="flex items-center gap-2">
                                        <FontAwesomeIcon icon={option.icon} className={cn("w-3.5 h-3.5", option.color)} />
                                        {option.label}
                                    </span>
                                </SelectItem>
                            ))}
                        </SelectContent>
                    </Select>

                    <Select value={filters.category || "all"} onValueChange={handleCategoryChange}>
                        <SelectTrigger className="w-[150px] h-11 bg-white dark:bg-slate-900 border-slate-200 dark:border-slate-700 rounded-xl">
                            <SelectValue placeholder="Category" />
                        </SelectTrigger>
                        <SelectContent className="rounded-xl">
                            {CATEGORY_OPTIONS.map((option) => (
                                <SelectItem key={option.value} value={option.value} className="rounded-lg">
                                    {option.label}
                                </SelectItem>
                            ))}
                        </SelectContent>
                    </Select>

                    <Button
                        variant="outline"
                        onClick={() => setShowAdvanced(!showAdvanced)}
                        className={cn(
                            "h-11 px-4 rounded-xl border-slate-200 dark:border-slate-700 transition-all",
                            showAdvanced && "bg-purple-50 dark:bg-purple-900/20 border-purple-300 dark:border-purple-700"
                        )}
                    >
                        <FontAwesomeIcon icon={faSliders} className="w-4 h-4" />
                        <span className="hidden sm:inline ml-2">Filters</span>
                        {activeFilterCount > 0 && (
                            <span className="ml-2 w-5 h-5 rounded-full bg-purple-600 text-white text-xs flex items-center justify-center">
                                {activeFilterCount}
                            </span>
                        )}
                    </Button>

                    {hasActiveFilters && (
                        <Button
                            variant="ghost"
                            onClick={clearFilters}
                            className="h-11 px-4 rounded-xl text-slate-500 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-900/20"
                        >
                            <FontAwesomeIcon icon={faXmark} className="w-4 h-4" />
                            <span className="hidden sm:inline ml-2">Clear</span>
                        </Button>
                    )}
                </div>
            </div>

            <AnimatePresence>
                {showAdvanced && (
                    <motion.div
                        initial={{ opacity: 0, height: 0 }}
                        animate={{ opacity: 1, height: "auto" }}
                        exit={{ opacity: 0, height: 0 }}
                        transition={{ duration: 0.2 }}
                        className="overflow-hidden"
                    >
                        <div className="p-4 bg-slate-50 dark:bg-slate-900/50 rounded-xl border border-slate-200 dark:border-slate-800 space-y-4">
                            <div>
                                <label className="flex items-center gap-2 text-sm font-medium text-slate-700 dark:text-slate-300 mb-3">
                                    <FontAwesomeIcon icon={faDollarSign} className="w-4 h-4 text-green-500" />
                                    Price Range
                                </label>
                                <div className="flex flex-wrap gap-2">
                                    {PRICE_PRESETS.map((preset, index) => {
                                        const isActive = filters.minPrice === preset.min && filters.maxPrice === preset.max;
                                        return (
                                            <button
                                                key={index}
                                                onClick={() => handlePricePreset(preset.min, preset.max)}
                                                className={cn(
                                                    "px-3 py-1.5 rounded-lg text-sm font-medium transition-all",
                                                    isActive
                                                        ? "bg-purple-600 text-white shadow-md shadow-purple-500/25"
                                                        : "bg-white dark:bg-slate-800 text-slate-600 dark:text-slate-300 border border-slate-200 dark:border-slate-700 hover:border-purple-300 dark:hover:border-purple-600"
                                                )}
                                            >
                                                {preset.label}
                                            </button>
                                        );
                                    })}
                                </div>
                            </div>

                            <div className="flex items-center gap-3">
                                <div className="flex-1">
                                    <label className="text-xs text-slate-500 dark:text-slate-400 mb-1 block">Min Price</label>
                                    <div className="relative">
                                        <span className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400">$</span>
                                        <Input
                                            type="number"
                                            placeholder="0"
                                            value={filters.minPrice === undefined ? "" : filters.minPrice}
                                            onChange={(e) =>
                                                setPriceRange(
                                                    e.target.value ? Number(e.target.value) : undefined,
                                                    filters.maxPrice
                                                )
                                            }
                                            className="pl-7 h-10 bg-white dark:bg-slate-800 rounded-lg"
                                        />
                                    </div>
                                </div>
                                <span className="text-slate-400 mt-5">—</span>
                                <div className="flex-1">
                                    <label className="text-xs text-slate-500 dark:text-slate-400 mb-1 block">Max Price</label>
                                    <div className="relative">
                                        <span className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400">$</span>
                                        <Input
                                            type="number"
                                            placeholder="Any"
                                            value={filters.maxPrice === undefined ? "" : filters.maxPrice}
                                            onChange={(e) =>
                                                setPriceRange(
                                                    filters.minPrice,
                                                    e.target.value ? Number(e.target.value) : undefined
                                                )
                                            }
                                            className="pl-7 h-10 bg-white dark:bg-slate-800 rounded-lg"
                                        />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </motion.div>
                )}
            </AnimatePresence>

            {hasActiveFilters && (
                <motion.div
                    initial={{ opacity: 0, y: -10 }}
                    animate={{ opacity: 1, y: 0 }}
                    className="flex flex-wrap gap-2"
                >
                    {filters.searchTerm && (
                        <span className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-full text-sm bg-purple-100 dark:bg-purple-900/30 text-purple-700 dark:text-purple-300">
                            <FontAwesomeIcon icon={faSearch} className="w-3 h-3" />
                            &quot;{filters.searchTerm}&quot;
                            <button onClick={() => setSearchTerm("")} className="ml-1 hover:text-purple-900 dark:hover:text-white">
                                <FontAwesomeIcon icon={faXmark} className="w-3 h-3" />
                            </button>
                        </span>
                    )}
                    {filters.status && (
                        <span className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-full text-sm bg-green-100 dark:bg-green-900/30 text-green-700 dark:text-green-300">
                            <FontAwesomeIcon icon={faBolt} className="w-3 h-3" />
                            {STATUS_OPTIONS.find(o => o.value === filters.status)?.label}
                            <button onClick={() => setStatus("")} className="ml-1 hover:text-green-900 dark:hover:text-white">
                                <FontAwesomeIcon icon={faXmark} className="w-3 h-3" />
                            </button>
                        </span>
                    )}
                    {filters.category && (
                        <span className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-full text-sm bg-blue-100 dark:bg-blue-900/30 text-blue-700 dark:text-blue-300">
                            <FontAwesomeIcon icon={faFilter} className="w-3 h-3" />
                            {CATEGORY_OPTIONS.find(o => o.value === filters.category)?.label}
                            <button onClick={() => setCategory("")} className="ml-1 hover:text-blue-900 dark:hover:text-white">
                                <FontAwesomeIcon icon={faXmark} className="w-3 h-3" />
                            </button>
                        </span>
                    )}
                    {(filters.minPrice !== undefined || filters.maxPrice !== undefined) && (
                        <span className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-full text-sm bg-amber-100 dark:bg-amber-900/30 text-amber-700 dark:text-amber-300">
                            <FontAwesomeIcon icon={faDollarSign} className="w-3 h-3" />
                            {filters.minPrice !== undefined && filters.maxPrice !== undefined
                                ? `$${filters.minPrice.toLocaleString()} - $${filters.maxPrice.toLocaleString()}`
                                : filters.minPrice !== undefined
                                    ? `$${filters.minPrice.toLocaleString()}+`
                                    : `Up to $${filters.maxPrice?.toLocaleString()}`}
                            <button onClick={() => setPriceRange(undefined, undefined)} className="ml-1 hover:text-amber-900 dark:hover:text-white">
                                <FontAwesomeIcon icon={faXmark} className="w-3 h-3" />
                            </button>
                        </span>
                    )}
                </motion.div>
            )}
        </div>
    );
}
