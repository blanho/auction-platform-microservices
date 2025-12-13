"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
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
    ArrowRight,
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

const colorMap: Record<string, { bg: string; text: string; border: string }> = {
    car: { bg: "bg-blue-50 dark:bg-blue-950/30", text: "text-blue-600 dark:text-blue-400", border: "border-blue-200 dark:border-blue-800 hover:border-blue-400" },
    watch: { bg: "bg-amber-50 dark:bg-amber-950/30", text: "text-amber-600 dark:text-amber-400", border: "border-amber-200 dark:border-amber-800 hover:border-amber-400" },
    gem: { bg: "bg-purple-50 dark:bg-purple-950/30", text: "text-purple-600 dark:text-purple-400", border: "border-purple-200 dark:border-purple-800 hover:border-purple-400" },
    art: { bg: "bg-pink-50 dark:bg-pink-950/30", text: "text-pink-600 dark:text-pink-400", border: "border-pink-200 dark:border-pink-800 hover:border-pink-400" },
    home: { bg: "bg-green-50 dark:bg-green-950/30", text: "text-green-600 dark:text-green-400", border: "border-green-200 dark:border-green-800 hover:border-green-400" },
    electronics: { bg: "bg-cyan-50 dark:bg-cyan-950/30", text: "text-cyan-600 dark:text-cyan-400", border: "border-cyan-200 dark:border-cyan-800 hover:border-cyan-400" },
    fashion: { bg: "bg-rose-50 dark:bg-rose-950/30", text: "text-rose-600 dark:text-rose-400", border: "border-rose-200 dark:border-rose-800 hover:border-rose-400" },
    default: { bg: "bg-indigo-50 dark:bg-indigo-950/30", text: "text-indigo-600 dark:text-indigo-400", border: "border-indigo-200 dark:border-indigo-800 hover:border-indigo-400" },
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
            const scrollAmount = 320;
            const newPosition = direction === "left" ? scrollPosition - scrollAmount : scrollPosition + scrollAmount;
            container.scrollTo({ left: newPosition, behavior: "smooth" });
            setScrollPosition(newPosition);
        }
    };

    const getIcon = (iconName: string | undefined) => iconMap[iconName?.toLowerCase() || "default"] || iconMap.default;
    const getColors = (iconName: string | undefined) => colorMap[iconName?.toLowerCase() || "default"] || colorMap.default;

    if (isLoading) {
        return (
            <section className="py-20 bg-white dark:bg-slate-950">
                <div className="container mx-auto px-4">
                    <div className="flex gap-4 overflow-hidden">
                        {[...Array(6)].map((_, i) => (
                            <div key={i} className="shrink-0 w-48 h-56 bg-slate-100 dark:bg-slate-800 rounded-2xl animate-pulse" />
                        ))}
                    </div>
                </div>
            </section>
        );
    }

    return (
        <section className="py-20 bg-white dark:bg-slate-950">
            <div className="container mx-auto px-4">
                <div className="flex items-end justify-between mb-12">
                    <div>
                        <p className="text-sm font-semibold text-purple-600 dark:text-purple-400 mb-3 uppercase tracking-wider">
                            Categories
                        </p>
                        <h2 className="text-3xl md:text-4xl lg:text-5xl font-bold text-slate-900 dark:text-white">
                            Shop by Interest
                        </h2>
                        <p className="text-lg text-slate-600 dark:text-slate-400 mt-3 max-w-lg">
                            From vintage timepieces to classic automobiles — find your passion.
                        </p>
                    </div>
                    <div className="hidden sm:flex gap-2">
                        <Button variant="outline" size="icon" onClick={() => scroll("left")} className="rounded-full h-12 w-12">
                            <ChevronLeft className="h-5 w-5" />
                        </Button>
                        <Button variant="outline" size="icon" onClick={() => scroll("right")} className="rounded-full h-12 w-12">
                            <ChevronRight className="h-5 w-5" />
                        </Button>
                    </div>
                </div>

                <div
                    id="categories-carousel"
                    className="flex gap-4 overflow-x-auto scrollbar-hide pb-4 -mx-4 px-4 snap-x snap-mandatory"
                    style={{ scrollbarWidth: "none", msOverflowStyle: "none" }}
                >
                    {categories.map((category) => {
                        const Icon = getIcon(category.icon);
                        const colors = getColors(category.icon);

                        return (
                            <Link
                                key={category.id}
                                href={`/auctions?category=${category.slug}`}
                                className="group shrink-0 snap-start"
                            >
                                <div className={`relative w-48 h-56 rounded-2xl overflow-hidden ${colors.bg} border ${colors.border} transition-all duration-300 hover:shadow-lg hover:-translate-y-1`}>
                                    <div className="absolute inset-0 flex flex-col items-center justify-center p-6">
                                        <div className={`w-16 h-16 rounded-2xl bg-white dark:bg-slate-900 shadow-sm flex items-center justify-center mb-5`}>
                                            <Icon className={`w-8 h-8 ${colors.text}`} />
                                        </div>
                                        <h3 className="text-lg font-bold text-slate-900 dark:text-white text-center">
                                            {category.name}
                                        </h3>
                                        <p className="text-sm text-slate-500 dark:text-slate-400 mt-1">
                                            {category.auctionCount?.toLocaleString() || 0} auctions
                                        </p>
                                        <div className={`mt-4 flex items-center gap-1 ${colors.text} text-sm font-medium opacity-0 group-hover:opacity-100 transition-opacity`}>
                                            <span>Explore</span>
                                            <ArrowRight className="w-4 h-4" />
                                        </div>
                                    </div>
                                </div>
                            </Link>
                        );
                    })}

                    <Link href="/categories" className="group shrink-0 snap-start">
                        <div className="relative w-48 h-56 rounded-2xl overflow-hidden bg-slate-100 dark:bg-slate-900 border-2 border-dashed border-slate-300 dark:border-slate-700 transition-all duration-300 hover:border-purple-500 hover:shadow-lg hover:-translate-y-1">
                            <div className="absolute inset-0 flex flex-col items-center justify-center p-6">
                                <div className="w-16 h-16 rounded-2xl bg-white dark:bg-slate-800 shadow-sm flex items-center justify-center mb-5">
                                    <ArrowRight className="w-8 h-8 text-slate-400 group-hover:text-purple-500 transition-colors" />
                                </div>
                                <h3 className="text-lg font-bold text-slate-600 dark:text-slate-300 group-hover:text-purple-600 dark:group-hover:text-purple-400 text-center transition-colors">
                                    View All
                                </h3>
                                <p className="text-sm text-slate-400 dark:text-slate-500 mt-1">
                                    All categories
                                </p>
                            </div>
                        </div>
                    </Link>
                </div>
            </div>
        </section>
    );
}
