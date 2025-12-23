"use client";

import { useState, useEffect, useCallback, useRef, useMemo } from "react";
import { useRouter, useSearchParams, usePathname } from "next/navigation";
import { motion, AnimatePresence } from "framer-motion";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
    faSearch,
    faTimes,
    faClock,
    faFire,
    faArrowRight,
    faFilter,
    faTrash,
    faSpinner,
} from "@fortawesome/free-solid-svg-icons";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";
import { useDebounce } from "@/hooks/use-debounce";
import { auctionService } from "@/services/auction.service";
import { Auction } from "@/types/auction";

const RECENT_SEARCHES_KEY = "auction-recent-searches";
const MAX_RECENT_SEARCHES = 5;

const POPULAR_SEARCHES = [
    "Vintage watches",
    "Art collectibles",
    "Electronics",
    "Jewelry",
    "Rare coins",
];

interface RecentSearch {
    query: string;
    timestamp: number;
}

interface EnhancedSearchBarProps {
    placeholder?: string;
    className?: string;
    showFiltersButton?: boolean;
    onFiltersClick?: () => void;
    activeFiltersCount?: number;
}

export function EnhancedSearchBar({
    placeholder = "Search auctions...",
    className,
    showFiltersButton = false,
    onFiltersClick,
    activeFiltersCount = 0,
}: EnhancedSearchBarProps) {
    const router = useRouter();
    const pathname = usePathname();
    const searchParams = useSearchParams();

    const [query, setQuery] = useState(searchParams.get("q") || "");
    const [isOpen, setIsOpen] = useState(false);
    const [isLoading, setIsLoading] = useState(false);
    const [suggestions, setSuggestions] = useState<Auction[]>([]);
    const [recentSearches, setRecentSearches] = useState<RecentSearch[]>([]);
    const [selectedIndex, setSelectedIndex] = useState(-1);

    const inputRef = useRef<HTMLInputElement>(null);
    const containerRef = useRef<HTMLDivElement>(null);

    const debouncedQuery = useDebounce(query, 300);

    useEffect(() => {
        const stored = localStorage.getItem(RECENT_SEARCHES_KEY);
        if (stored) {
            try {
                setRecentSearches(JSON.parse(stored));
            } catch {
                setRecentSearches([]);
            }
        }
    }, []);

    useEffect(() => {
        const handleClickOutside = (event: MouseEvent) => {
            if (
                containerRef.current &&
                !containerRef.current.contains(event.target as Node)
            ) {
                setIsOpen(false);
            }
        };

        document.addEventListener("mousedown", handleClickOutside);
        return () => document.removeEventListener("mousedown", handleClickOutside);
    }, []);

    useEffect(() => {
        const fetchSuggestions = async () => {
            if (!debouncedQuery || debouncedQuery.length < 2) {
                setSuggestions([]);
                return;
            }

            setIsLoading(true);
            try {
                const result = await auctionService.getAuctions({
                    searchTerm: debouncedQuery,
                    pageSize: 5,
                });
                setSuggestions(result.items || []);
            } catch {
                setSuggestions([]);
            } finally {
                setIsLoading(false);
            }
        };

        fetchSuggestions();
    }, [debouncedQuery]);

    const saveRecentSearch = useCallback((searchQuery: string) => {
        const trimmed = searchQuery.trim();
        if (!trimmed) return;

        setRecentSearches((prev) => {
            const filtered = prev.filter(
                (s) => s.query.toLowerCase() !== trimmed.toLowerCase()
            );
            const updated = [
                { query: trimmed, timestamp: Date.now() },
                ...filtered,
            ].slice(0, MAX_RECENT_SEARCHES);
            localStorage.setItem(RECENT_SEARCHES_KEY, JSON.stringify(updated));
            return updated;
        });
    }, []);

    const clearRecentSearches = useCallback(() => {
        setRecentSearches([]);
        localStorage.removeItem(RECENT_SEARCHES_KEY);
    }, []);

    const removeRecentSearch = useCallback((searchQuery: string) => {
        setRecentSearches((prev) => {
            const updated = prev.filter((s) => s.query !== searchQuery);
            localStorage.setItem(RECENT_SEARCHES_KEY, JSON.stringify(updated));
            return updated;
        });
    }, []);

    const handleSearch = useCallback(
        (searchQuery: string) => {
            const trimmed = searchQuery.trim();
            if (!trimmed) return;

            saveRecentSearch(trimmed);
            setIsOpen(false);
            setQuery(trimmed);

            const params = new URLSearchParams(searchParams.toString());
            params.set("q", trimmed);
            params.delete("page");

            router.push(`/search?${params.toString()}`);
        },
        [router, searchParams, saveRecentSearch]
    );

    const handleSubmit = useCallback(
        (e: React.FormEvent) => {
            e.preventDefault();
            handleSearch(query);
        },
        [query, handleSearch]
    );

    const handleKeyDown = useCallback(
        (e: React.KeyboardEvent) => {
            const totalItems = suggestions.length + recentSearches.length;

            if (e.key === "ArrowDown") {
                e.preventDefault();
                setSelectedIndex((prev) => (prev < totalItems - 1 ? prev + 1 : 0));
            } else if (e.key === "ArrowUp") {
                e.preventDefault();
                setSelectedIndex((prev) => (prev > 0 ? prev - 1 : totalItems - 1));
            } else if (e.key === "Enter" && selectedIndex >= 0) {
                e.preventDefault();
                if (selectedIndex < suggestions.length) {
                    router.push(`/auctions/${suggestions[selectedIndex].id}`);
                } else {
                    const recentIndex = selectedIndex - suggestions.length;
                    handleSearch(recentSearches[recentIndex].query);
                }
            } else if (e.key === "Escape") {
                setIsOpen(false);
                inputRef.current?.blur();
            }
        },
        [suggestions, recentSearches, selectedIndex, router, handleSearch]
    );

    const showDropdown = useMemo(() => {
        return isOpen && (query.length > 0 || recentSearches.length > 0);
    }, [isOpen, query, recentSearches]);

    return (
        <div ref={containerRef} className={cn("relative", className)}>
            <form onSubmit={handleSubmit} className="relative flex items-center gap-2">
                <div className="relative flex-1">
                    <FontAwesomeIcon
                        icon={faSearch}
                        className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400"
                    />
                    <Input
                        ref={inputRef}
                        type="text"
                        value={query}
                        onChange={(e) => setQuery(e.target.value)}
                        onFocus={() => setIsOpen(true)}
                        onKeyDown={handleKeyDown}
                        placeholder={placeholder}
                        className="pl-10 pr-10"
                    />
                    {query && (
                        <button
                            type="button"
                            onClick={() => {
                                setQuery("");
                                inputRef.current?.focus();
                            }}
                            className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600"
                        >
                            <FontAwesomeIcon icon={faTimes} className="w-4 h-4" />
                        </button>
                    )}
                </div>

                {showFiltersButton && (
                    <Button
                        type="button"
                        variant="outline"
                        onClick={onFiltersClick}
                        className="relative"
                    >
                        <FontAwesomeIcon icon={faFilter} className="w-4 h-4" />
                        {activeFiltersCount > 0 && (
                            <span className="absolute -top-1 -right-1 w-5 h-5 rounded-full bg-purple-600 text-white text-xs flex items-center justify-center">
                                {activeFiltersCount}
                            </span>
                        )}
                    </Button>
                )}

                <Button type="submit" className="shrink-0">
                    <FontAwesomeIcon icon={faSearch} className="w-4 h-4 mr-2" />
                    Search
                </Button>
            </form>

            <AnimatePresence>
                {showDropdown && (
                    <motion.div
                        initial={{ opacity: 0, y: -10 }}
                        animate={{ opacity: 1, y: 0 }}
                        exit={{ opacity: 0, y: -10 }}
                        transition={{ duration: 0.15 }}
                        className="absolute top-full left-0 right-0 mt-2 bg-white dark:bg-slate-900 rounded-xl border border-slate-200 dark:border-slate-700 shadow-xl overflow-hidden z-50"
                    >
                        {isLoading && (
                            <div className="p-4 flex items-center justify-center text-slate-400">
                                <FontAwesomeIcon
                                    icon={faSpinner}
                                    className="w-5 h-5 animate-spin mr-2"
                                />
                                Searching...
                            </div>
                        )}

                        {!isLoading && suggestions.length > 0 && (
                            <div className="p-2">
                                <div className="px-3 py-2 text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider">
                                    Suggestions
                                </div>
                                {suggestions.map((auction, index) => (
                                    <motion.button
                                        key={auction.id}
                                        initial={{ opacity: 0, x: -10 }}
                                        animate={{ opacity: 1, x: 0 }}
                                        transition={{ delay: index * 0.05 }}
                                        onClick={() => router.push(`/auctions/${auction.id}`)}
                                        className={cn(
                                            "w-full flex items-center gap-3 px-3 py-2 rounded-lg text-left transition-colors",
                                            selectedIndex === index
                                                ? "bg-purple-50 dark:bg-purple-900/20"
                                                : "hover:bg-slate-50 dark:hover:bg-slate-800"
                                        )}
                                    >
                                        {auction.files?.[0]?.url && (
                                            <img
                                                src={auction.files[0].url}
                                                alt=""
                                                className="w-10 h-10 rounded-lg object-cover"
                                            />
                                        )}
                                        <div className="flex-1 min-w-0">
                                            <div className="font-medium text-slate-900 dark:text-white truncate">
                                                {auction.title || "Untitled"}
                                            </div>
                                            <div className="text-sm text-slate-500 dark:text-slate-400">
                                                ${auction.currentHighBid?.toLocaleString() || auction.reservePrice?.toLocaleString()}
                                            </div>
                                        </div>
                                        <FontAwesomeIcon
                                            icon={faArrowRight}
                                            className="w-4 h-4 text-slate-400"
                                        />
                                    </motion.button>
                                ))}
                            </div>
                        )}

                        {!isLoading && query.length === 0 && recentSearches.length > 0 && (
                            <div className="p-2">
                                <div className="px-3 py-2 flex items-center justify-between">
                                    <span className="text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider">
                                        Recent Searches
                                    </span>
                                    <button
                                        onClick={clearRecentSearches}
                                        className="text-xs text-slate-400 hover:text-red-500 transition-colors"
                                    >
                                        Clear all
                                    </button>
                                </div>
                                {recentSearches.map((search, index) => (
                                    <motion.div
                                        key={search.query}
                                        initial={{ opacity: 0, x: -10 }}
                                        animate={{ opacity: 1, x: 0 }}
                                        transition={{ delay: index * 0.05 }}
                                        className={cn(
                                            "flex items-center gap-3 px-3 py-2 rounded-lg group",
                                            selectedIndex === suggestions.length + index
                                                ? "bg-purple-50 dark:bg-purple-900/20"
                                                : "hover:bg-slate-50 dark:hover:bg-slate-800"
                                        )}
                                    >
                                        <FontAwesomeIcon
                                            icon={faClock}
                                            className="w-4 h-4 text-slate-400"
                                        />
                                        <button
                                            onClick={() => handleSearch(search.query)}
                                            className="flex-1 text-left text-slate-700 dark:text-slate-300"
                                        >
                                            {search.query}
                                        </button>
                                        <button
                                            onClick={() => removeRecentSearch(search.query)}
                                            className="opacity-0 group-hover:opacity-100 text-slate-400 hover:text-red-500 transition-all"
                                        >
                                            <FontAwesomeIcon icon={faTrash} className="w-3 h-3" />
                                        </button>
                                    </motion.div>
                                ))}
                            </div>
                        )}

                        {!isLoading &&
                            query.length === 0 &&
                            recentSearches.length === 0 && (
                                <div className="p-2">
                                    <div className="px-3 py-2 text-xs font-semibold text-slate-500 dark:text-slate-400 uppercase tracking-wider">
                                        Popular Searches
                                    </div>
                                    <div className="px-3 py-2 flex flex-wrap gap-2">
                                        {POPULAR_SEARCHES.map((term) => (
                                            <Badge
                                                key={term}
                                                variant="secondary"
                                                className="cursor-pointer hover:bg-purple-100 dark:hover:bg-purple-900/30 transition-colors"
                                                onClick={() => handleSearch(term)}
                                            >
                                                <FontAwesomeIcon
                                                    icon={faFire}
                                                    className="w-3 h-3 mr-1 text-orange-500"
                                                />
                                                {term}
                                            </Badge>
                                        ))}
                                    </div>
                                </div>
                            )}

                        {!isLoading &&
                            query.length >= 2 &&
                            suggestions.length === 0 && (
                                <div className="p-6 text-center text-slate-500 dark:text-slate-400">
                                    <FontAwesomeIcon
                                        icon={faSearch}
                                        className="w-8 h-8 mb-3 opacity-30"
                                    />
                                    <p>No results found for "{query}"</p>
                                    <p className="text-sm mt-1">
                                        Press Enter to search anyway
                                    </p>
                                </div>
                            )}
                    </motion.div>
                )}
            </AnimatePresence>
        </div>
    );
}
