"use client";

import { useState, useEffect, useCallback } from "react";
import Link from "next/link";
import { motion, AnimatePresence, useScroll, useTransform } from "framer-motion";
import { Button } from "@/components/ui/button";
import { UserMenu } from "@/components/auth/user-menu";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { ThemeToggle } from "@/components/ui/theme-toggle";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
    faFire,
    faBolt,
    faRocket,
    faCrown,
    faTag,
    faShieldHalved,
    faMagnifyingGlass,
    faHeart,
    faBars,
    faGavel,
    faXmark,
    faWandMagicSparkles,
    faArrowTrendUp,
    faClock,
    faArrowRight,
    faUsers,
} from "@fortawesome/free-solid-svg-icons";
import { Sheet, SheetContent, SheetTrigger, SheetClose } from "@/components/ui/sheet";
import { NotificationBell } from "@/components/common/notification-bell";
import { cn } from "@/lib/utils";
import { gradients, gradientText, glass, shadows } from "@/lib/styles";
import { useCategories } from "@/hooks/use-auctions";
import { wishlistService } from "@/services/wishlist.service";
import { analyticsService } from "@/services/analytics.service";
import { useSession } from "next-auth/react";

const navLinks = [
    { 
        name: "Hot Deals", 
        href: "/deals", 
        icon: faFire, 
        badge: "Hot", 
        badgeColor: "bg-gradient-to-r from-orange-500 to-red-500",
    },
    { 
        name: "Live Auctions", 
        href: "/live", 
        icon: faBolt, 
        badge: "Live", 
        badgeColor: "bg-green-500 animate-pulse",
    },
    { 
        name: "Premium", 
        href: "/premium", 
        icon: faCrown, 
        badge: "VIP", 
        badgeColor: "bg-gradient-to-r from-amber-500 to-yellow-500",
    },
    { 
        name: "New Arrivals", 
        href: "/new", 
        icon: faRocket, 
        badge: "New", 
        badgeColor: "bg-gradient-to-r from-purple-500 to-pink-500",
    },
];

export function Header() {
    const { data: session } = useSession();
    const user = session?.user;
    const [searchQuery, setSearchQuery] = useState("");
    const [isSearchFocused, setIsSearchFocused] = useState(false);
    const [isScrolled, setIsScrolled] = useState(false);
    const [showAnnouncement, setShowAnnouncement] = useState(true);
    const [wishlistCount, setWishlistCount] = useState(0);
    const [quickStatsData, setQuickStatsData] = useState<any>(null);
    const [trendingSearchesData, setTrendingSearchesData] = useState<any[]>([]);
    const { data: categories, isLoading: categoriesLoading } = useCategories();

    useEffect(() => {
        const fetchAllData = async () => {
            const [wishlistData, quickStats, trendingSearches] = await Promise.all([
                user?.id ? wishlistService.getWishlist().catch(() => []) : Promise.resolve([]),
                analyticsService.getQuickStats().catch(() => null),
                analyticsService.getTrendingSearches(6).catch(() => [])
            ]);

            if (user?.id) setWishlistCount(wishlistData.length);
            setQuickStatsData(quickStats);
            setTrendingSearchesData(trendingSearches);
        };

        fetchAllData();
        const interval = setInterval(fetchAllData, 60000);
        return () => clearInterval(interval);
    }, [user?.id]);

    const trendingSearches = trendingSearchesData
        .filter((item) => item.searchTerm)
        .map((item, index) => ({
            id: `trending-${index}`,
            term: item.searchTerm,
            icon: item.icon || "fa-search",
            trending: item.trending || false,
            hot: item.hot || false,
            new: item.isNew || false,
        }));

    const quickStats = quickStatsData ? [
        { 
            label: "Live Now", 
            value: quickStatsData.liveAuctions?.toString() || "0", 
            icon: faBolt, 
            color: "text-green-500", 
            change: quickStatsData.liveAuctionsChange || "" 
        },
        { 
            label: "Ending Soon", 
            value: quickStatsData.endingSoon?.toString() || "0", 
            icon: faClock, 
            color: "text-orange-500", 
            change: quickStatsData.endingSoonChange || "" 
        },
        { 
            label: "Active Users", 
            value: quickStatsData.activeUsers >= 1000 
                ? `${(quickStatsData.activeUsers / 1000).toFixed(1)}K` 
                : quickStatsData.activeUsers?.toString() || "0", 
            icon: faUsers, 
            color: "text-purple-500", 
            change: quickStatsData.activeUsersChange || "" 
        },
    ] : [];

    const { scrollY } = useScroll();
    const headerOpacity = useTransform(scrollY, [0, 50], [1, 0.98]);

    useEffect(() => {
        const handleScroll = () => {
            setIsScrolled(window.scrollY > 10);
        };
        window.addEventListener("scroll", handleScroll);
        return () => window.removeEventListener("scroll", handleScroll);
    }, []);

    const handleSearchSubmit = useCallback((e: React.FormEvent) => {
        e.preventDefault();
        if (searchQuery.trim()) {
            window.location.href = `/search?q=${encodeURIComponent(searchQuery)}`;
        }
    }, [searchQuery]);

    return (
        <motion.header
            style={{ opacity: headerOpacity }}
            initial={{ y: -20, opacity: 0 }}
            animate={{ y: 0, opacity: 1 }}
            transition={{ duration: 0.4, ease: "easeOut" }}
            className={cn(
                "sticky top-0 z-50 w-full transition-all duration-500",
                isScrolled
                    ? cn(glass.default, "shadow-lg shadow-slate-200/20 dark:shadow-slate-900/50")
                    : "bg-white dark:bg-slate-950"
            )}
        >
            <AnimatePresence>
                {showAnnouncement && (
                    <motion.div
                        initial={{ height: 0, opacity: 0 }}
                        animate={{ height: "auto", opacity: 1 }}
                        exit={{ height: 0, opacity: 0 }}
                        transition={{ duration: 0.3, ease: "easeInOut" }}
                        className="relative overflow-hidden"
                    >
                        <div className={cn(gradients.primary, "text-white py-2 relative")}>
                            <div className="container mx-auto px-4 relative z-10">
                                <div className="flex items-center justify-center gap-3 text-sm">
                                    <span className="font-medium">🎉 Get 10% off first bid:</span>
                                    <Badge 
                                        variant="secondary" 
                                        className="bg-white/20 hover:bg-white/30 text-white border-0 font-bold px-2 py-0 text-xs cursor-pointer"
                                    >
                                        WELCOME10
                                    </Badge>
                                    <button
                                        onClick={() => setShowAnnouncement(false)}
                                        className="absolute right-4 top-1/2 -translate-y-1/2 p-1 hover:bg-white/20 rounded-full transition-colors"
                                    >
                                        <FontAwesomeIcon icon={faXmark} className="w-3.5 h-3.5" />
                                    </button>
                                </div>
                            </div>
                        </div>
                    </motion.div>
                )}
            </AnimatePresence>

            <div className="border-b border-slate-200/80 dark:border-slate-800/80">
                <div className="container mx-auto px-4">
                    <div className="flex h-20 items-center gap-6">
                        <Link href="/" className="flex items-center gap-3 shrink-0 group">
                            <motion.div
                                whileHover={{ rotate: -10, scale: 1.1 }}
                                whileTap={{ scale: 0.95 }}
                                className="relative w-12 h-12"
                            >
                                <div className="absolute inset-0 rounded-2xl bg-gradient-to-br from-purple-600 via-purple-500 to-blue-600 shadow-xl shadow-purple-500/30 dark:shadow-purple-500/20" />
                                <div className="absolute inset-0 rounded-2xl bg-gradient-to-br from-purple-500 via-blue-500 to-indigo-500 opacity-0 group-hover:opacity-100 transition-opacity duration-300" />
                                <div className="absolute inset-0 flex items-center justify-center">
                                    <FontAwesomeIcon icon={faGavel} className="h-6 w-6 text-white drop-shadow-lg" />
                                </div>
                                <div className="absolute -top-1 -right-1 w-3 h-3 bg-green-500 rounded-full animate-pulse" />
                            </motion.div>
                            <div className="flex flex-col">
                                <span className={cn("text-2xl font-bold", gradientText.primary)}>
                                    AuctionHub
                                </span>
                                <span className="text-[10px] text-slate-500 dark:text-slate-400 font-medium -mt-1 tracking-wide">
                                    WIN BIG, SAVE MORE
                                </span>
                            </div>
                        </Link>

                        <form onSubmit={handleSearchSubmit} className="hidden md:flex flex-1 max-w-3xl relative">
                            <motion.div 
                                initial={{ opacity: 0, y: -10 }}
                                animate={{ opacity: 1, y: 0 }}
                                transition={{ delay: 0.1 }}
                                className={cn(
                                    "flex w-full rounded-2xl border-2 overflow-hidden transition-all duration-300 shadow-sm",
                                    isSearchFocused
                                        ? "border-purple-500 dark:border-purple-400 ring-4 ring-purple-500/10 dark:ring-purple-400/10 shadow-xl shadow-purple-500/20"
                                        : "border-slate-200 dark:border-slate-700 bg-slate-50/80 dark:bg-slate-900/50 hover:border-slate-300 dark:hover:border-slate-600"
                                )}
                            >
                                <div className="relative flex-1 flex items-center">
                                    <motion.div
                                        animate={isSearchFocused ? { scale: 1.1, color: "#9333ea" } : { scale: 1 }}
                                        className="absolute left-5"
                                    >
                                        <FontAwesomeIcon icon={faMagnifyingGlass} className="h-5 w-5 text-slate-400 dark:text-slate-500" />
                                    </motion.div>
                                    <Input
                                        type="text"
                                        placeholder="Search luxury watches, electronics, art..."
                                        value={searchQuery}
                                        onChange={(e) => setSearchQuery(e.target.value)}
                                        onFocus={() => setIsSearchFocused(true)}
                                        onBlur={() => setTimeout(() => setIsSearchFocused(false), 200)}
                                        className="flex-1 border-0 h-14 pl-14 pr-4 focus-visible:ring-0 focus-visible:ring-offset-0 rounded-none bg-transparent text-base placeholder:text-slate-400"
                                    />
                                    {searchQuery && (
                                        <motion.button
                                            initial={{ scale: 0 }}
                                            animate={{ scale: 1 }}
                                            exit={{ scale: 0 }}
                                            type="button"
                                            onClick={() => setSearchQuery("")}
                                            className="absolute right-4 p-1.5 hover:bg-slate-200 dark:hover:bg-slate-700 rounded-full transition-colors"
                                        >
                                            <FontAwesomeIcon icon={faXmark} className="h-4 w-4 text-slate-400" />
                                        </motion.button>
                                    )}
                                </div>

                                <motion.div whileHover={{ scale: 1.02 }} whileTap={{ scale: 0.98 }}>
                                    <Button 
                                        type="submit"
                                        className={cn(
                                            "rounded-none h-14 px-8 shadow-inner border-l-2 border-slate-200 dark:border-slate-700",
                                            gradients.primary,
                                            "hover:from-purple-700 hover:to-blue-700",
                                            "font-semibold text-base"
                                        )}
                                    >
                                        Search
                                        <FontAwesomeIcon icon={faArrowRight} className="ml-2 h-4 w-4" />
                                    </Button>
                                </motion.div>
                            </motion.div>

                            <AnimatePresence>
                                {isSearchFocused && (
                                    <motion.div
                                        initial={{ opacity: 0, y: 10, scale: 0.95 }}
                                        animate={{ opacity: 1, y: 0, scale: 1 }}
                                        exit={{ opacity: 0, y: 10, scale: 0.95 }}
                                        transition={{ duration: 0.2 }}
                                        className={cn(
                                            "absolute top-full left-0 right-0 mt-3 rounded-2xl shadow-2xl border z-50 overflow-hidden",
                                            "bg-white dark:bg-slate-900 border-slate-200 dark:border-slate-700"
                                        )}
                                    >
                                        <div className="p-6">
                                            <div className="flex items-center gap-2 mb-4">
                                                <div className="flex items-center gap-2 text-sm font-semibold text-slate-700 dark:text-slate-300">
                                                    <FontAwesomeIcon icon={faArrowTrendUp} className="w-4 h-4 text-purple-500" />
                                                    Trending Searches
                                                </div>
                                                <div className="flex-1 h-px bg-gradient-to-r from-slate-200 to-transparent dark:from-slate-700" />
                                            </div>
                                            <div className="grid grid-cols-2 gap-2 mb-6">
                                                {trendingSearches.map((item) => (
                                                    <motion.button
                                                        key={item.id}
                                                        whileHover={{ scale: 1.02, x: 4 }}
                                                        whileTap={{ scale: 0.98 }}
                                                        onClick={() => setSearchQuery(item.term)}
                                                        className="flex items-center gap-3 p-3 text-left bg-slate-50 dark:bg-slate-800/50 hover:bg-purple-50 dark:hover:bg-purple-900/20 rounded-xl transition-all group border border-transparent hover:border-purple-200 dark:hover:border-purple-800"
                                                    >
                                                        <div className="w-10 h-10 rounded-lg bg-gradient-to-br from-purple-100 to-blue-100 dark:from-purple-900/30 dark:to-blue-900/30 flex items-center justify-center group-hover:scale-110 transition-transform">
                                                            <i className={`fa-solid ${item.icon} text-purple-600 dark:text-purple-400`}></i>
                                                        </div>
                                                        <div className="flex-1 min-w-0">
                                                            <div className="font-medium text-slate-900 dark:text-white group-hover:text-purple-600 dark:group-hover:text-purple-400 truncate">
                                                                {item.term}
                                                            </div>
                                                            {item.trending && (
                                                                <div className="flex items-center gap-1 text-xs text-orange-500">
                                                                    <FontAwesomeIcon icon={faArrowTrendUp} className="w-3 h-3" />
                                                                    <span>Trending</span>
                                                                </div>
                                                            )}
                                                            {item.hot && (
                                                                <div className="flex items-center gap-1 text-xs text-red-500">
                                                                    <FontAwesomeIcon icon={faFire} className="w-3 h-3" />
                                                                    <span>Hot</span>
                                                                </div>
                                                            )}
                                                            {item.new && (
                                                                <div className="flex items-center gap-1 text-xs text-green-500">
                                                                    <FontAwesomeIcon icon={faWandMagicSparkles} className="w-3 h-3" />
                                                                    <span>New</span>
                                                                </div>
                                                            )}
                                                        </div>
                                                        <FontAwesomeIcon icon={faArrowRight} className="w-4 h-4 text-slate-400 group-hover:text-purple-500 opacity-0 group-hover:opacity-100 transition-all" />
                                                    </motion.button>
                                                ))}
                                            </div>
                                            
                                            <div className="grid grid-cols-3 gap-4 pt-4 border-t border-slate-200 dark:border-slate-700">
                                                {quickStats.map((stat, index) => (
                                                    <motion.div
                                                        key={stat.label}
                                                        initial={{ opacity: 0, y: 10 }}
                                                        animate={{ opacity: 1, y: 0 }}
                                                        transition={{ delay: index * 0.05 }}
                                                        className="text-center p-4 rounded-xl bg-gradient-to-br from-slate-50 to-slate-100 dark:from-slate-800/50 dark:to-slate-800/30 border border-slate-200 dark:border-slate-700"
                                                    >
                                                        <FontAwesomeIcon icon={stat.icon} className={cn("w-5 h-5 mx-auto mb-2", stat.color)} />
                                                        <div className="flex items-center justify-center gap-1.5 mb-1">
                                                            <div className="text-2xl font-bold text-slate-900 dark:text-white">
                                                                {stat.value}
                                                            </div>
                                                            <span className="text-xs text-green-500 font-semibold">
                                                                {stat.change}
                                                            </span>
                                                        </div>
                                                        <div className="text-xs text-slate-500 dark:text-slate-400 font-medium">
                                                            {stat.label}
                                                        </div>
                                                    </motion.div>
                                                ))}
                                            </div>
                                        </div>
                                    </motion.div>
                                )}
                            </AnimatePresence>
                        </form>

                        <div className="flex items-center gap-2 ml-auto">
                            <motion.div whileHover={{ scale: 1.05 }} whileTap={{ scale: 0.95 }}>
                                <ThemeToggle variant="icon" />
                            </motion.div>
                            
                            <NotificationBell />

                            <motion.div whileHover={{ scale: 1.1, rotate: 5 }} whileTap={{ scale: 0.95 }}>
                                <Button
                                    variant="ghost"
                                    size="icon"
                                    className="relative hover:bg-red-50 dark:hover:bg-red-950/30 hover:text-red-500 transition-colors"
                                    asChild
                                >
                                    <Link href="/wishlist">
                                        <FontAwesomeIcon icon={faHeart} className="h-5 w-5" />
                                        {wishlistCount > 0 && (
                                            <span className="absolute -top-1 -right-1 w-5 h-5 bg-red-500 text-white text-xs rounded-full flex items-center justify-center font-bold">
                                                {wishlistCount > 99 ? '99+' : wishlistCount}
                                            </span>
                                        )}
                                    </Link>
                                </Button>
                            </motion.div>

                            <div className="hidden sm:block h-8 w-px bg-gradient-to-b from-transparent via-slate-300 dark:via-slate-600 to-transparent mx-2" />

                            <UserMenu />

                            <Sheet>
                                <SheetTrigger asChild>
                                    <Button variant="ghost" size="icon" className="lg:hidden">
                                        <FontAwesomeIcon icon={faBars} className="h-5 w-5" />
                                    </Button>
                                </SheetTrigger>
                                <SheetContent side="left" className="w-80 p-0">
                                    <div className="flex flex-col h-full">
                                        <div className="p-4 border-b border-slate-200 dark:border-slate-700 bg-gradient-to-r from-purple-600 to-blue-600">
                                            <div className="flex items-center justify-between">
                                                <div className="flex items-center gap-2">
                                                    <div className="w-9 h-9 rounded-lg bg-white/20 backdrop-blur-sm flex items-center justify-center">
                                                        <FontAwesomeIcon icon={faGavel} className="h-5 w-5 text-white" />
                                                    </div>
                                                    <div>
                                                        <span className="text-lg font-bold text-white">AuctionHub</span>
                                                        <span className="text-[10px] text-white/70 block -mt-0.5">Where Value Finds You</span>
                                                    </div>
                                                </div>
                                                <SheetClose asChild>
                                                    <Button variant="ghost" size="icon" className="text-white hover:bg-white/20">
                                                        <FontAwesomeIcon icon={faXmark} className="h-5 w-5" />
                                                    </Button>
                                                </SheetClose>
                                            </div>
                                        </div>

                                        <div className="p-4 border-b border-slate-200 dark:border-slate-700">
                                            <form onSubmit={handleSearchSubmit} className="relative">
                                                <FontAwesomeIcon icon={faMagnifyingGlass} className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
                                                <Input
                                                    placeholder="Search for anything..."
                                                    value={searchQuery}
                                                    onChange={(e) => setSearchQuery(e.target.value)}
                                                    className="pl-10 bg-slate-50 dark:bg-slate-900 border-slate-200 dark:border-slate-700"
                                                />
                                            </form>
                                        </div>

                                        <div className="flex-1 overflow-y-auto p-4">
                                            <div className="mb-6">
                                                <h3 className="text-xs font-semibold text-slate-500 uppercase tracking-wider mb-3">
                                                    Quick Links
                                                </h3>
                                                <div className="space-y-1">
                                                    {navLinks.map((link) => (
                                                        <Link
                                                            key={link.name}
                                                            href={link.href}
                                                            className="flex items-center justify-between p-3 rounded-xl hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors group"
                                                        >
                                                            <div className="flex items-center gap-3">
                                                                <div className="w-9 h-9 rounded-lg bg-purple-100 dark:bg-purple-900/30 flex items-center justify-center group-hover:bg-purple-200 dark:group-hover:bg-purple-900/50 transition-colors">
                                                                    <FontAwesomeIcon icon={link.icon} className="h-4 w-4 text-purple-600 dark:text-purple-400" />
                                                                </div>
                                                                <span className="font-medium">{link.name}</span>
                                                            </div>
                                                            {link.badge && (
                                                                <Badge className={cn("text-white border-0", link.badgeColor)}>
                                                                    {link.badge}
                                                                </Badge>
                                                            )}
                                                        </Link>
                                                    ))}
                                                </div>
                                            </div>

                                            <div>
                                                <h3 className="text-xs font-semibold text-slate-500 uppercase tracking-wider mb-3">
                                                    Categories
                                                </h3>
                                                <div className="grid grid-cols-2 gap-2">
                                                    {!categoriesLoading && categories.map((cat) => (
                                                        <Link
                                                            key={cat.id}
                                                            href={`/search?category=${cat.slug}`}
                                                            className="flex items-center gap-2 p-3 rounded-xl bg-slate-50 dark:bg-slate-900 hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors group"
                                                        >
                                                            <i className={`fa-solid ${cat.icon} text-purple-500 group-hover:scale-110 transition-transform`}></i>
                                                            <span className="text-sm font-medium truncate">{cat.name}</span>
                                                        </Link>
                                                    ))}
                                                </div>
                                            </div>
                                        </div>

                                        <div className="p-4 border-t border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-900 space-y-3">
                                            <div className="flex items-center justify-between">
                                                <span className="text-sm text-slate-600 dark:text-slate-400">Theme</span>
                                                <ThemeToggle variant="expanded" />
                                            </div>
                                            <motion.div whileHover={{ scale: 1.01 }} whileTap={{ scale: 0.98 }}>
                                                <Button
                                                    className={cn(
                                                        "w-full",
                                                        gradients.primary,
                                                        "hover:from-purple-700 hover:to-blue-700",
                                                        shadows.glow.purple
                                                    )}
                                                    asChild
                                                >
                                                    <Link href="/auctions/create">
                                                        <Gavel className="h-4 w-4 mr-2" />
                                                        Start Selling
                                                        <ArrowRight className="h-4 w-4 ml-2" />
                                                    </Link>
                                                </Button>
                                            </motion.div>
                                        </div>
                                    </div>
                                </SheetContent>
                            </Sheet>
                        </div>
                    </div>
                </div>
            </div>

            <motion.nav
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                transition={{ delay: 0.2, duration: 0.3 }}
                className={cn(
                    "hidden lg:block border-b border-slate-200/80 dark:border-slate-800/80",
                    glass.subtle
                )}
            >
                <div className="container mx-auto px-4">
                    <div className="flex items-center justify-between h-10">
                        <div className="flex items-center gap-1">
                            {navLinks.map((link) => (
                                <Link
                                    key={link.name}
                                    href={link.href}
                                    className="flex items-center gap-1.5 px-2.5 py-1.5 text-sm font-medium text-slate-600 dark:text-slate-400 hover:text-purple-600 dark:hover:text-purple-400 rounded-lg hover:bg-purple-50 dark:hover:bg-purple-900/20 transition-colors"
                                >
                                    <FontAwesomeIcon icon={link.icon} className="h-3.5 w-3.5" />
                                    <span>{link.name}</span>
                                    {link.badge && (
                                        <Badge className={cn(
                                            "text-[9px] px-1.5 py-0 h-4 text-white border-0 font-bold",
                                            link.badgeColor
                                        )}>
                                            {link.badge}
                                        </Badge>
                                    )}
                                </Link>
                            ))}
                        </div>

                        <div className="flex items-center gap-2">
                            <Link
                                href="/buyer-protection"
                                className="flex items-center gap-1.5 text-xs font-medium text-green-600 dark:text-green-400 hover:text-green-700 dark:hover:text-green-300"
                            >
                                <FontAwesomeIcon icon={faShieldHalved} className="w-3.5 h-3.5" />
                                <span>Buyer Protection</span>
                            </Link>

                            <div className="h-4 w-px bg-slate-300 dark:bg-slate-700" />

                            <Button
                                size="sm"
                                className={cn(
                                    "h-7 px-3 text-xs font-bold",
                                    gradients.primary
                                )}
                                asChild
                            >
                                <Link href="/auctions/create">
                                    <FontAwesomeIcon icon={faTag} className="h-3 w-3 mr-1.5" />
                                    Sell
                                </Link>
                            </Button>
                        </div>
                    </div>
                </div>
            </motion.nav>
        </motion.header>
    );
}
