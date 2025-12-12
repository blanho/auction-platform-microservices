"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { motion } from "framer-motion";
import {
    Car,
    Watch,
    Gem,
    Paintbrush,
    Home,
    Smartphone,
    ShoppingBag,
    Sparkles,
    ChevronLeft,
    ChevronRight,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Category } from "@/types/auction";
import { auctionService } from "@/services/auction.service";

const iconMap: Record<string, React.ElementType> = {
    car: Car,
    watch: Watch,
    gem: Gem,
    art: Paintbrush,
    home: Home,
    electronics: Smartphone,
    fashion: ShoppingBag,
    default: Sparkles,
};

const colorMap: Record<string, string> = {
    car: "from-blue-500 to-blue-600",
    watch: "from-amber-500 to-amber-600",
    gem: "from-purple-500 to-purple-600",
    art: "from-pink-500 to-pink-600",
    home: "from-green-500 to-green-600",
    electronics: "from-cyan-500 to-cyan-600",
    fashion: "from-rose-500 to-rose-600",
    default: "from-indigo-500 to-indigo-600",
};

export function CategoriesSection() {
    const [categories, setCategories] = useState<Category[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [scrollPosition, setScrollPosition] = useState(0);

    useEffect(() => {
        const fetchCategories = async () => {
            try {
                const data = await auctionService.getCategories();
                setCategories(data);
            } catch (error) {
                console.error("Failed to fetch categories:", error);
            } finally {
                setIsLoading(false);
            }
        };
        fetchCategories();
    }, []);

    const scroll = (direction: "left" | "right") => {
        const container = document.getElementById("categories-carousel");
        if (container) {
            const scrollAmount = 300;
            const newPosition =
                direction === "left"
                    ? scrollPosition - scrollAmount
                    : scrollPosition + scrollAmount;
            container.scrollTo({ left: newPosition, behavior: "smooth" });
            setScrollPosition(newPosition);
        }
    };

    const getIcon = (iconName: string | undefined) => {
        const key = iconName?.toLowerCase() || "default";
        return iconMap[key] || iconMap.default;
    };

    const getColor = (iconName: string | undefined) => {
        const key = iconName?.toLowerCase() || "default";
        return colorMap[key] || colorMap.default;
    };

    if (isLoading) {
        return (
            <section className="py-16 bg-zinc-50 dark:bg-zinc-900">
                <div className="container mx-auto px-4">
                    <div className="flex gap-4 overflow-hidden">
                        {[...Array(6)].map((_, i) => (
                            <div
                                key={i}
                                className="flex-shrink-0 w-40 h-32 bg-zinc-200 dark:bg-zinc-800 rounded-2xl animate-pulse"
                            />
                        ))}
                    </div>
                </div>
            </section>
        );
    }

    return (
        <section className="py-16 bg-zinc-50 dark:bg-zinc-900">
            <div className="container mx-auto px-4">
                <div className="flex items-center justify-between mb-8">
                    <div>
                        <h2 className="text-3xl font-bold text-zinc-900 dark:text-white">
                            Browse Categories
                        </h2>
                        <p className="text-zinc-600 dark:text-zinc-400 mt-2">
                            Find what you&apos;re looking for
                        </p>
                    </div>
                    <div className="flex gap-2">
                        <Button
                            variant="outline"
                            size="icon"
                            onClick={() => scroll("left")}
                            className="rounded-full"
                        >
                            <ChevronLeft className="h-4 w-4" />
                        </Button>
                        <Button
                            variant="outline"
                            size="icon"
                            onClick={() => scroll("right")}
                            className="rounded-full"
                        >
                            <ChevronRight className="h-4 w-4" />
                        </Button>
                    </div>
                </div>

                <div
                    id="categories-carousel"
                    className="flex gap-4 overflow-x-auto scrollbar-hide pb-4 -mx-4 px-4"
                    style={{ scrollbarWidth: "none", msOverflowStyle: "none" }}
                >
                    {categories.map((category, index) => {
                        const Icon = getIcon(category.icon);
                        const colorClass = getColor(category.icon);

                        return (
                            <motion.div
                                key={category.id}
                                initial={{ opacity: 0, y: 20 }}
                                animate={{ opacity: 1, y: 0 }}
                                transition={{ delay: index * 0.1 }}
                            >
                                <Link
                                    href={`/auctions?category=${category.slug}`}
                                    className="group flex-shrink-0 block"
                                >
                                    <div className="relative w-40 h-32 rounded-2xl overflow-hidden bg-white dark:bg-zinc-800 border border-zinc-200 dark:border-zinc-700 hover:border-amber-400 dark:hover:border-amber-400 transition-all duration-300 hover:shadow-lg hover:shadow-amber-500/10">
                                        <div
                                            className={`absolute inset-0 bg-linear-to-br ${colorClass} opacity-0 group-hover:opacity-100 transition-opacity duration-300`}
                                        />
                                        <div className="relative h-full flex flex-col items-center justify-center p-4">
                                            <div className="w-12 h-12 rounded-full bg-zinc-100 dark:bg-zinc-700 group-hover:bg-white/20 flex items-center justify-center mb-3 transition-colors">
                                                <Icon className="w-6 h-6 text-zinc-700 dark:text-zinc-300 group-hover:text-white transition-colors" />
                                            </div>
                                            <span className="text-sm font-semibold text-zinc-900 dark:text-white group-hover:text-white text-center transition-colors">
                                                {category.name}
                                            </span>
                                            <span className="text-xs text-zinc-500 dark:text-zinc-400 group-hover:text-white/80 mt-1 transition-colors">
                                                {category.auctionCount} items
                                            </span>
                                        </div>
                                    </div>
                                </Link>
                            </motion.div>
                        );
                    })}
                </div>
            </div>
        </section>
    );
}
