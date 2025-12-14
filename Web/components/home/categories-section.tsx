"use client";

import { useEffect, useState, useRef } from "react";
import Link from "next/link";
import Image from "next/image";
import { motion, AnimatePresence } from "framer-motion";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
    faCar,
    faClock,
    faGem,
    faPaintBrush,
    faHome,
    faMobileScreen,
    faBagShopping,
    faWandSparkles,
    faChevronLeft,
    faChevronRight,
    faArrowRight,
    faFire,
} from "@fortawesome/free-solid-svg-icons";
import { IconDefinition } from "@fortawesome/fontawesome-svg-core";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { Category } from "@/types/auction";
import { auctionService } from "@/services/auction.service";
import { PulsingDot } from "@/components/ui/animated";

const iconMap: Record<string, IconDefinition> = {
    car: faCar,
    watch: faClock,
    gem: faGem,
    art: faPaintBrush,
    home: faHome,
    electronics: faMobileScreen,
    fashion: faBagShopping,
    default: faWandSparkles,
};

const gradientMap: Record<string, { from: string; to: string; glow: string }> = {
    car: { from: "from-blue-500", to: "to-cyan-400", glow: "shadow-blue-500/30" },
    watch: { from: "from-amber-500", to: "to-orange-400", glow: "shadow-amber-500/30" },
    gem: { from: "from-purple-500", to: "to-pink-400", glow: "shadow-purple-500/30" },
    art: { from: "from-pink-500", to: "to-rose-400", glow: "shadow-pink-500/30" },
    home: { from: "from-emerald-500", to: "to-teal-400", glow: "shadow-emerald-500/30" },
    electronics: { from: "from-cyan-500", to: "to-blue-400", glow: "shadow-cyan-500/30" },
    fashion: { from: "from-rose-500", to: "to-pink-400", glow: "shadow-rose-500/30" },
    default: { from: "from-indigo-500", to: "to-purple-400", glow: "shadow-indigo-500/30" },
};

const categoryImages: Record<string, string> = {
    car: "https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=400",
    watch: "https://images.unsplash.com/photo-1523170335258-f5ed11844a49?w=400",
    gem: "https://images.unsplash.com/photo-1599643478518-a784e5dc4c8f?w=400",
    art: "https://images.unsplash.com/photo-1579783902614-a3fb3927b6a5?w=400",
    home: "https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=400",
    electronics: "https://images.unsplash.com/photo-1468495244123-6c6c332eeece?w=400",
    fashion: "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=400",
    default: "https://images.unsplash.com/photo-1560472354-b33ff0c44a43?w=400",
};

interface CategoryCardProps {
    category: Category;
    index: number;
    isActive: boolean;
    onHover: () => void;
}

function CategoryCard({ category, index, isActive, onHover }: CategoryCardProps) {
    const iconKey = category.icon?.toLowerCase() || "default";
    const icon = iconMap[iconKey] || iconMap.default;
    const gradient = gradientMap[iconKey] || gradientMap.default;
    const imageUrl = categoryImages[iconKey] || categoryImages.default;

    return (
        <motion.div
            initial={{ opacity: 0, y: 30 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.5, delay: index * 0.1 }}
            onMouseEnter={onHover}
        >
            <Link href={`/auctions?category=${category.slug}`} className="group block">
                <div className={`relative h-72 sm:h-80 rounded-3xl overflow-hidden transition-all duration-500 ${isActive ? 'scale-[1.02] shadow-2xl ' + gradient.glow : 'shadow-lg'}`}>
                    <Image
                        src={imageUrl}
                        alt={category.name}
                        fill
                        className="object-cover transition-transform duration-700 group-hover:scale-110"
                        unoptimized
                    />
                    
                    <div className="absolute inset-0 bg-gradient-to-t from-black/90 via-black/40 to-black/10" />
                    
                    <div className={`absolute inset-0 bg-gradient-to-br ${gradient.from} ${gradient.to} opacity-0 group-hover:opacity-30 transition-opacity duration-500`} />

                    <div className="absolute top-4 left-4 right-4 flex items-center justify-between">
                        <div className={`w-12 h-12 rounded-2xl bg-gradient-to-br ${gradient.from} ${gradient.to} flex items-center justify-center shadow-lg ${gradient.glow}`}>
                            <FontAwesomeIcon icon={icon} className="w-5 h-5 text-white" />
                        </div>
                        {(category.auctionCount || 0) > 50 && (
                            <Badge className="bg-white/20 backdrop-blur-sm text-white border-0 text-xs">
                                <FontAwesomeIcon icon={faFire} className="w-3 h-3 mr-1 text-orange-400" />
                                Popular
                            </Badge>
                        )}
                    </div>

                    <div className="absolute bottom-0 left-0 right-0 p-5">
                        <div className="flex items-center gap-2 mb-2">
                            <PulsingDot />
                            <span className="text-xs text-emerald-400 font-medium">
                                {category.auctionCount?.toLocaleString() || 0} live auctions
                            </span>
                        </div>
                        
                        <h3 className="text-xl sm:text-2xl font-bold text-white mb-3 group-hover:text-transparent group-hover:bg-clip-text group-hover:bg-gradient-to-r group-hover:from-white group-hover:to-slate-300 transition-all">
                            {category.name}
                        </h3>
                        
                        <div className="flex items-center justify-between">
                            <p className="text-sm text-slate-300">
                                Starting from <span className="text-white font-semibold">$99</span>
                            </p>
                            <div className={`flex items-center gap-1.5 px-3 py-1.5 rounded-full bg-white/10 backdrop-blur-sm text-white text-sm font-medium opacity-0 group-hover:opacity-100 translate-y-2 group-hover:translate-y-0 transition-all duration-300`}>
                                <span>Explore</span>
                                <FontAwesomeIcon icon={faArrowRight} className="w-3 h-3" />
                            </div>
                        </div>
                    </div>

                    <div className={`absolute inset-0 rounded-3xl border-2 border-transparent group-hover:border-white/20 transition-colors duration-300`} />
                </div>
            </Link>
        </motion.div>
    );
}

function CategorySkeleton() {
    return (
        <div className="h-72 sm:h-80 rounded-3xl overflow-hidden bg-slate-800/50 animate-pulse">
            <Skeleton className="w-full h-full bg-slate-700/50" />
        </div>
    );
}

export function CategoriesSection() {
    const [categories, setCategories] = useState<Category[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [activeIndex, setActiveIndex] = useState(0);
    const scrollRef = useRef<HTMLDivElement>(null);

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
        if (scrollRef.current) {
            const scrollAmount = 340;
            scrollRef.current.scrollBy({
                left: direction === "left" ? -scrollAmount : scrollAmount,
                behavior: "smooth",
            });
        }
    };

    const totalAuctions = categories.reduce((sum, cat) => sum + (cat.auctionCount || 0), 0);

    return (
        <section className="relative py-24 bg-slate-50 dark:bg-slate-950 overflow-hidden">
            <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top,_var(--tw-gradient-stops))] from-purple-200/30 via-slate-50 to-slate-50 dark:from-purple-900/20 dark:via-slate-950 dark:to-slate-950" />
            <div className="absolute top-0 left-1/4 w-96 h-96 bg-purple-400/10 dark:bg-purple-600/10 rounded-full blur-3xl" />
            <div className="absolute bottom-0 right-1/4 w-96 h-96 bg-blue-400/10 dark:bg-blue-600/10 rounded-full blur-3xl" />

            <div className="container mx-auto px-4 relative z-10">
                <div className="flex flex-col lg:flex-row lg:items-end lg:justify-between gap-8 mb-12">
                    <motion.div
                        initial={{ opacity: 0, y: 20 }}
                        whileInView={{ opacity: 1, y: 0 }}
                        viewport={{ once: true }}
                        className="max-w-xl"
                    >
                        <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-purple-100 dark:bg-white/5 border border-purple-200 dark:border-white/10 backdrop-blur-sm mb-6">
                            <FontAwesomeIcon icon={faWandSparkles} className="w-4 h-4 text-purple-600 dark:text-purple-400" />
                            <span className="text-sm font-medium text-purple-700 dark:text-purple-300">Explore Categories</span>
                        </div>
                        
                        <h2 className="text-4xl md:text-5xl lg:text-6xl font-bold text-slate-900 dark:text-white mb-4 leading-tight">
                            Find Your
                            <span className="block text-transparent bg-clip-text bg-gradient-to-r from-purple-600 via-pink-600 to-orange-500 dark:from-purple-400 dark:via-pink-400 dark:to-orange-400">
                                Perfect Category
                            </span>
                        </h2>
                        
                        <p className="text-lg text-slate-600 dark:text-slate-400 leading-relaxed">
                            From vintage timepieces to classic automobiles — discover {totalAuctions.toLocaleString()}+ live auctions across all categories.
                        </p>
                    </motion.div>

                    <motion.div
                        initial={{ opacity: 0, y: 20 }}
                        whileInView={{ opacity: 1, y: 0 }}
                        viewport={{ once: true }}
                        transition={{ delay: 0.2 }}
                        className="flex items-center gap-4"
                    >
                        <div className="hidden sm:flex gap-2">
                            <Button
                                variant="outline"
                                size="icon"
                                onClick={() => scroll("left")}
                                className="w-12 h-12 rounded-full border-slate-300 dark:border-white/20 bg-white/80 dark:bg-white/5 hover:bg-white dark:hover:bg-white/10 text-slate-700 dark:text-white"
                            >
                                <FontAwesomeIcon icon={faChevronLeft} className="w-4 h-4" />
                            </Button>
                            <Button
                                variant="outline"
                                size="icon"
                                onClick={() => scroll("right")}
                                className="w-12 h-12 rounded-full border-slate-300 dark:border-white/20 bg-white/80 dark:bg-white/5 hover:bg-white dark:hover:bg-white/10 text-slate-700 dark:text-white"
                            >
                                <FontAwesomeIcon icon={faChevronRight} className="w-4 h-4" />
                            </Button>
                        </div>
                        
                        <Button
                            variant="outline"
                            className="h-12 px-6 rounded-full border-slate-300 dark:border-white/20 bg-white/80 dark:bg-white/5 hover:bg-white dark:hover:bg-white/10 text-slate-700 dark:text-white font-medium"
                            asChild
                        >
                            <Link href="/categories">
                                View All
                                <FontAwesomeIcon icon={faArrowRight} className="ml-2 w-4 h-4" />
                            </Link>
                        </Button>
                    </motion.div>
                </div>

                <div
                    ref={scrollRef}
                    className="flex gap-5 overflow-x-auto scrollbar-hide pb-4 -mx-4 px-4 snap-x snap-mandatory"
                    style={{ scrollbarWidth: "none", msOverflowStyle: "none" }}
                >
                    {isLoading ? (
                        [...Array(5)].map((_, i) => (
                            <div key={i} className="shrink-0 w-72 sm:w-80 snap-start">
                                <CategorySkeleton />
                            </div>
                        ))
                    ) : (
                        <>
                            {categories.map((category, index) => (
                                <div key={category.id} className="shrink-0 w-72 sm:w-80 snap-start">
                                    <CategoryCard
                                        category={category}
                                        index={index}
                                        isActive={activeIndex === index}
                                        onHover={() => setActiveIndex(index)}
                                    />
                                </div>
                            ))}
                            
                            <div className="shrink-0 w-72 sm:w-80 snap-start">
                                <Link href="/categories" className="group block">
                                    <div className="relative h-72 sm:h-80 rounded-3xl overflow-hidden border-2 border-dashed border-slate-300 dark:border-white/20 hover:border-purple-500/50 transition-all duration-500 bg-white/80 dark:bg-white/5 hover:bg-white dark:hover:bg-white/10">
                                        <div className="absolute inset-0 flex flex-col items-center justify-center p-6 text-center">
                                            <div className="w-16 h-16 rounded-2xl bg-gradient-to-br from-purple-500 to-pink-500 flex items-center justify-center mb-6 shadow-lg shadow-purple-500/30 group-hover:scale-110 transition-transform duration-300">
                                                <FontAwesomeIcon icon={faArrowRight} className="w-6 h-6 text-white" />
                                            </div>
                                            <h3 className="text-2xl font-bold text-slate-900 dark:text-white mb-2 group-hover:text-purple-600 dark:group-hover:text-purple-400 transition-colors">
                                                View All
                                            </h3>
                                            <p className="text-slate-500 dark:text-slate-400 text-sm">
                                                Explore {categories.length}+ categories
                                            </p>
                                        </div>
                                    </div>
                                </Link>
                            </div>
                        </>
                    )}
                </div>

                <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    whileInView={{ opacity: 1, y: 0 }}
                    viewport={{ once: true }}
                    transition={{ delay: 0.3 }}
                    className="mt-12 flex flex-wrap justify-center gap-6 text-center"
                >
                    <div className="px-6 py-3 rounded-2xl bg-white/80 dark:bg-white/5 border border-slate-200 dark:border-white/10 shadow-sm dark:shadow-none">
                        <p className="text-2xl font-bold text-slate-900 dark:text-white">{categories.length}+</p>
                        <p className="text-xs text-slate-500 dark:text-slate-400 uppercase tracking-wider">Categories</p>
                    </div>
                    <div className="px-6 py-3 rounded-2xl bg-white/80 dark:bg-white/5 border border-slate-200 dark:border-white/10 shadow-sm dark:shadow-none">
                        <p className="text-2xl font-bold text-slate-900 dark:text-white">{totalAuctions.toLocaleString()}+</p>
                        <p className="text-xs text-slate-500 dark:text-slate-400 uppercase tracking-wider">Live Auctions</p>
                    </div>
                    <div className="px-6 py-3 rounded-2xl bg-white/80 dark:bg-white/5 border border-slate-200 dark:border-white/10 shadow-sm dark:shadow-none">
                        <p className="text-2xl font-bold text-emerald-600 dark:text-emerald-400">24/7</p>
                        <p className="text-xs text-slate-500 dark:text-slate-400 uppercase tracking-wider">Active Trading</p>
                    </div>
                </motion.div>
            </div>
        </section>
    );
}
