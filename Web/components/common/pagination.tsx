"use client";

import { motion } from "framer-motion";
import {
    ChevronLeft,
    ChevronRight,
    ChevronsLeft,
    ChevronsRight,
} from "lucide-react";
import { Button } from "@/components/ui/button";

interface PaginationProps {
    currentPage: number;
    totalPages: number;
    onPageChange: (page: number) => void;
    showFirstLast?: boolean;
    maxVisiblePages?: number;
}

const MotionButton = motion.create(Button);

export function Pagination({
    currentPage,
    totalPages,
    onPageChange,
    showFirstLast = true,
    maxVisiblePages = 5,
}: PaginationProps) {
    if (totalPages <= 1) return null;

    const getVisiblePages = () => {
        const pages: number[] = [];
        let startPage = Math.max(1, currentPage - Math.floor(maxVisiblePages / 2));
        const endPage = Math.min(totalPages, startPage + maxVisiblePages - 1);

        if (endPage - startPage + 1 < maxVisiblePages) {
            startPage = Math.max(1, endPage - maxVisiblePages + 1);
        }

        for (let i = startPage; i <= endPage; i++) {
            pages.push(i);
        }

        return pages;
    };

    const visiblePages = getVisiblePages();

    return (
        <motion.nav
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.3 }}
            className="flex items-center justify-center gap-1"
            aria-label="Pagination"
        >
            {showFirstLast && (
                <MotionButton
                    whileHover={{ scale: 1.05 }}
                    whileTap={{ scale: 0.95 }}
                    variant="outline"
                    size="icon"
                    onClick={() => onPageChange(1)}
                    disabled={currentPage === 1}
                    aria-label="Go to first page"
                    className="border-slate-200 dark:border-slate-700 hover:border-purple-300 dark:hover:border-purple-700"
                >
                    <ChevronsLeft className="h-4 w-4" />
                </MotionButton>
            )}

            <MotionButton
                whileHover={{ scale: 1.05 }}
                whileTap={{ scale: 0.95 }}
                variant="outline"
                size="icon"
                onClick={() => onPageChange(currentPage - 1)}
                disabled={currentPage === 1}
                aria-label="Go to previous page"
                className="border-slate-200 dark:border-slate-700 hover:border-purple-300 dark:hover:border-purple-700"
            >
                <ChevronLeft className="h-4 w-4" />
            </MotionButton>

            <div className="flex items-center gap-1">
                {visiblePages[0] > 1 && (
                    <>
                        <MotionButton
                            whileHover={{ scale: 1.05 }}
                            whileTap={{ scale: 0.95 }}
                            variant="outline"
                            size="icon"
                            onClick={() => onPageChange(1)}
                            className="border-slate-200 dark:border-slate-700 hover:border-purple-300 dark:hover:border-purple-700"
                        >
                            1
                        </MotionButton>
                        {visiblePages[0] > 2 && (
                            <span className="px-2 text-slate-400 dark:text-slate-500">...</span>
                        )}
                    </>
                )}

                {visiblePages.map((page) => (
                    <MotionButton
                        key={page}
                        whileHover={{ scale: 1.05 }}
                        whileTap={{ scale: 0.95 }}
                        variant={currentPage === page ? "default" : "outline"}
                        size="icon"
                        onClick={() => onPageChange(page)}
                        aria-current={currentPage === page ? "page" : undefined}
                        className={
                            currentPage === page
                                ? "bg-gradient-to-r from-purple-600 to-blue-500 text-white border-0 shadow-lg shadow-purple-500/25"
                                : "border-slate-200 dark:border-slate-700 hover:border-purple-300 dark:hover:border-purple-700"
                        }
                    >
                        {page}
                    </MotionButton>
                ))}

                {visiblePages[visiblePages.length - 1] < totalPages && (
                    <>
                        {visiblePages[visiblePages.length - 1] < totalPages - 1 && (
                            <span className="px-2 text-slate-400 dark:text-slate-500">...</span>
                        )}
                        <MotionButton
                            whileHover={{ scale: 1.05 }}
                            whileTap={{ scale: 0.95 }}
                            variant="outline"
                            size="icon"
                            onClick={() => onPageChange(totalPages)}
                            className="border-slate-200 dark:border-slate-700 hover:border-purple-300 dark:hover:border-purple-700"
                        >
                            {totalPages}
                        </MotionButton>
                    </>
                )}
            </div>

            <MotionButton
                whileHover={{ scale: 1.05 }}
                whileTap={{ scale: 0.95 }}
                variant="outline"
                size="icon"
                onClick={() => onPageChange(currentPage + 1)}
                disabled={currentPage === totalPages}
                aria-label="Go to next page"
                className="border-slate-200 dark:border-slate-700 hover:border-purple-300 dark:hover:border-purple-700"
            >
                <ChevronRight className="h-4 w-4" />
            </MotionButton>

            {showFirstLast && (
                <MotionButton
                    whileHover={{ scale: 1.05 }}
                    whileTap={{ scale: 0.95 }}
                    variant="outline"
                    size="icon"
                    onClick={() => onPageChange(totalPages)}
                    disabled={currentPage === totalPages}
                    aria-label="Go to last page"
                    className="border-slate-200 dark:border-slate-700 hover:border-purple-300 dark:hover:border-purple-700"
                >
                    <ChevronsRight className="h-4 w-4" />
                </MotionButton>
            )}
        </motion.nav>
    );
}
