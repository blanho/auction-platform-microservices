"use client";

import { useState, useEffect, useCallback } from "react";
import Link from "next/link";
import { motion, AnimatePresence, useScroll, useTransform } from "framer-motion";
import { Button } from "@/components/ui/button";
import { UserMenu } from "@/components/auth/user-menu";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { ThemeToggle } from "@/components/ui/theme-toggle";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
    Search,
    Heart,
    Menu,
    ChevronDown,
    Gavel,
    HelpCircle,
    X,
    Sparkles,
    TrendingUp,
    Shield,
    Zap,
    Clock,
    Star,
    ArrowRight,
} from "lucide-react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
    faFire,
    faBolt,
    faGift,
} from "@fortawesome/free-solid-svg-icons";
import { Sheet, SheetContent, SheetTrigger, SheetClose } from "@/components/ui/sheet";
import { NotificationBell } from "@/components/common/notification-bell";
import { cn } from "@/lib/utils";
import { useCategories } from "@/hooks/use-auctions";

const navLinks = [
    { name: "Hot Deals", href: "/deals", icon: faFire, badge: "New", badgeColor: "bg-red-500" },
    { name: "Live", href: "/live", icon: faBolt, badge: "Live", badgeColor: "bg-green-500 animate-pulse" },
    { name: "Gifts", href: "/gifts", icon: faGift },
];

const trendingSearches = [
    "Vintage Rolex",
    "iPhone 15 Pro",
    "Nike Air Jordan",
    "Pokemon Cards",
];

const quickStats = [
    { label: "Active Auctions", value: "12.5K+", icon: Zap },
    { label: "Ending Soon", value: "847", icon: Clock },
    { label: "Happy Bidders", value: "50K+", icon: Star },
];

export function Header() {
    const [searchQuery, setSearchQuery] = useState("");
    const [isSearchFocused, setIsSearchFocused] = useState(false);
    const [isScrolled, setIsScrolled] = useState(false);
    const [showAnnouncement, setShowAnnouncement] = useState(true);
    const { data: categories, isLoading: categoriesLoading } = useCategories();

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
            className={cn(
                "sticky top-0 z-50 w-full transition-all duration-500",
                isScrolled
                    ? "bg-white/80 dark:bg-slate-950/80 backdrop-blur-xl shadow-lg shadow-slate-200/20 dark:shadow-slate-900/50"
                    : "bg-white dark:bg-slate-950"
            )}
        >
            <AnimatePresence>
                {showAnnouncement && (
                    <motion.div
                        initial={{ height: 0, opacity: 0 }}
                        animate={{ height: "auto", opacity: 1 }}
                        exit={{ height: 0, opacity: 0 }}
                        className="relative overflow-hidden"
                    >
                        <div className="bg-gradient-to-r from-purple-600 via-indigo-600 to-blue-600 dark:from-purple-700 dark:via-indigo-700 dark:to-blue-700 text-white py-2.5">
                            <div className="container mx-auto px-4">
                                <div className="flex items-center justify-center gap-3 text-sm">
                                    <motion.div
                                        animate={{ rotate: [0, 15, -15, 0] }}
                                        transition={{ duration: 2, repeat: Infinity, repeatDelay: 3 }}
                                    >
                                        <Sparkles className="w-4 h-4" />
                                    </motion.div>
                                    <span className="font-medium">
                                        <span className="hidden sm:inline">Welcome to AuctionHub! </span>
                                        Free shipping on orders over $100
                                    </span>
                                    <Badge 
                                        variant="secondary" 
                                        className="bg-white/20 hover:bg-white/30 text-white border-0 text-xs font-bold hidden sm:inline-flex"
                                    >
                                        WELCOME10
                                    </Badge>
                                    <button
                                        onClick={() => setShowAnnouncement(false)}
                                        className="absolute right-4 top-1/2 -translate-y-1/2 p-1 hover:bg-white/20 rounded-full transition-colors"
                                    >
                                        <X className="w-3.5 h-3.5" />
                                    </button>
                                </div>
                            </div>
                        </div>
                    </motion.div>
                )}
            </AnimatePresence>

            <div className="border-b border-slate-200/80 dark:border-slate-800/80">
                <div className="container mx-auto px-4">
                    <div className="flex h-16 lg:h-[72px] items-center gap-4">
                        <Link href="/" className="flex items-center gap-2.5 shrink-0 group">
                            <motion.div
                                whileHover={{ rotate: -10, scale: 1.05 }}
                                whileTap={{ scale: 0.95 }}
                                className="relative w-10 h-10 lg:w-11 lg:h-11"
                            >
                                <div className="absolute inset-0 rounded-xl bg-gradient-to-br from-purple-600 to-blue-600 shadow-lg shadow-purple-500/30 dark:shadow-purple-500/20" />
                                <div className="absolute inset-0 rounded-xl bg-gradient-to-br from-purple-500 to-blue-500 opacity-0 group-hover:opacity-100 transition-opacity" />
                                <div className="absolute inset-0 flex items-center justify-center">
                                    <Gavel className="h-5 w-5 lg:h-6 lg:w-6 text-white" />
                                </div>
                            </motion.div>
                            <div className="hidden sm:flex flex-col">
                                <span className="text-xl font-bold bg-gradient-to-r from-purple-600 to-blue-600 bg-clip-text text-transparent">
                                    AuctionHub
                                </span>
                                <span className="text-[10px] text-slate-500 dark:text-slate-400 font-medium -mt-0.5">
                                    Where Value Finds You
                                </span>
                            </div>
                        </Link>

                        <form onSubmit={handleSearchSubmit} className="hidden md:flex flex-1 max-w-2xl mx-4 relative">
                            <div className={cn(
                                "flex w-full rounded-xl border overflow-hidden transition-all duration-300",
                                isSearchFocused
                                    ? "border-purple-500 dark:border-purple-400 ring-2 ring-purple-500/20 dark:ring-purple-400/20 shadow-lg shadow-purple-500/10"
                                    : "border-slate-200 dark:border-slate-700 bg-slate-50/50 dark:bg-slate-900/50"
                            )}>
                                <DropdownMenu>
                                    <DropdownMenuTrigger asChild>
                                        <Button
                                            variant="ghost"
                                            className="rounded-none border-r border-slate-200 dark:border-slate-700 bg-slate-100/80 dark:bg-slate-800/80 hover:bg-slate-200 dark:hover:bg-slate-700 shrink-0 h-11 px-4 font-medium"
                                        >
                                            All
                                            <ChevronDown className="ml-1.5 h-4 w-4 opacity-50" />
                                        </Button>
                                    </DropdownMenuTrigger>
                                    <DropdownMenuContent align="start" className="w-60">
                                        <DropdownMenuItem className="font-medium">
                                            All categories
                                        </DropdownMenuItem>
                                        <DropdownMenuSeparator />
                                        {!categoriesLoading && categories.map((cat) => (
                                            <DropdownMenuItem key={cat.id} asChild>
                                                <Link href={`/search?category=${cat.id}`} className="flex items-center gap-2 cursor-pointer">
                                                    <i className={`fa-solid ${cat.icon} w-4 text-center text-purple-500`}></i>
                                                    {cat.name}
                                                </Link>
                                            </DropdownMenuItem>
                                        ))}
                                    </DropdownMenuContent>
                                </DropdownMenu>

                                <div className="relative flex-1">
                                    <Input
                                        type="text"
                                        placeholder="Search for anything..."
                                        value={searchQuery}
                                        onChange={(e) => setSearchQuery(e.target.value)}
                                        onFocus={() => setIsSearchFocused(true)}
                                        onBlur={() => setTimeout(() => setIsSearchFocused(false), 200)}
                                        className="flex-1 border-0 h-11 focus-visible:ring-0 focus-visible:ring-offset-0 rounded-none bg-transparent text-base"
                                    />
                                </div>

                                <Button 
                                    type="submit"
                                    className="rounded-none h-11 px-5 bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700 shadow-inner"
                                >
                                    <Search className="h-4 w-4 mr-2" />
                                    <span className="hidden lg:inline font-medium">Search</span>
                                </Button>
                            </div>

                            <AnimatePresence>
                                {isSearchFocused && (
                                    <motion.div
                                        initial={{ opacity: 0, y: 10, scale: 0.95 }}
                                        animate={{ opacity: 1, y: 0, scale: 1 }}
                                        exit={{ opacity: 0, y: 10, scale: 0.95 }}
                                        transition={{ duration: 0.2 }}
                                        className="absolute top-full left-0 right-0 mt-2 bg-white dark:bg-slate-900 rounded-xl shadow-2xl border border-slate-200 dark:border-slate-700 p-4 z-50"
                                    >
                                        <div className="flex items-center gap-2 mb-3 text-sm text-slate-500 dark:text-slate-400">
                                            <TrendingUp className="w-4 h-4 text-purple-500" />
                                            <span className="font-medium">Trending searches</span>
                                        </div>
                                        <div className="flex flex-wrap gap-2">
                                            {trendingSearches.map((term) => (
                                                <button
                                                    key={term}
                                                    onClick={() => setSearchQuery(term)}
                                                    className="px-3 py-1.5 text-sm bg-slate-100 dark:bg-slate-800 hover:bg-purple-100 dark:hover:bg-purple-900/30 hover:text-purple-600 dark:hover:text-purple-400 rounded-full transition-all hover:scale-105"
                                                >
                                                    {term}
                                                </button>
                                            ))}
                                        </div>
                                        <div className="mt-4 pt-4 border-t border-slate-200 dark:border-slate-700 grid grid-cols-3 gap-3">
                                            {quickStats.map((stat) => (
                                                <div key={stat.label} className="text-center p-2 rounded-lg bg-slate-50 dark:bg-slate-800/50">
                                                    <stat.icon className="w-4 h-4 mx-auto mb-1 text-purple-500" />
                                                    <div className="text-lg font-bold text-slate-900 dark:text-white">{stat.value}</div>
                                                    <div className="text-xs text-slate-500 dark:text-slate-400">{stat.label}</div>
                                                </div>
                                            ))}
                                        </div>
                                    </motion.div>
                                )}
                            </AnimatePresence>
                        </form>

                        <div className="flex items-center gap-1 sm:gap-2 ml-auto">
                            <ThemeToggle variant="icon" />
                            
                            <NotificationBell />

                            <Button
                                variant="ghost"
                                size="icon"
                                className="relative hover:bg-red-50 dark:hover:bg-red-950/30 hover:text-red-500 transition-colors"
                                asChild
                            >
                                <Link href="/wishlist">
                                    <Heart className="h-5 w-5" />
                                </Link>
                            </Button>

                            <div className="hidden sm:block h-6 w-px bg-slate-200 dark:bg-slate-700 mx-1" />

                            <UserMenu />

                            <Sheet>
                                <SheetTrigger asChild>
                                    <Button variant="ghost" size="icon" className="md:hidden">
                                        <Menu className="h-5 w-5" />
                                    </Button>
                                </SheetTrigger>
                                <SheetContent side="left" className="w-80 p-0">
                                    <div className="flex flex-col h-full">
                                        <div className="p-4 border-b border-slate-200 dark:border-slate-700 bg-gradient-to-r from-purple-600 to-blue-600">
                                            <div className="flex items-center justify-between">
                                                <div className="flex items-center gap-2">
                                                    <div className="w-9 h-9 rounded-lg bg-white/20 backdrop-blur-sm flex items-center justify-center">
                                                        <Gavel className="h-5 w-5 text-white" />
                                                    </div>
                                                    <div>
                                                        <span className="text-lg font-bold text-white">AuctionHub</span>
                                                        <span className="text-[10px] text-white/70 block -mt-0.5">Where Value Finds You</span>
                                                    </div>
                                                </div>
                                                <SheetClose asChild>
                                                    <Button variant="ghost" size="icon" className="text-white hover:bg-white/20">
                                                        <X className="h-5 w-5" />
                                                    </Button>
                                                </SheetClose>
                                            </div>
                                        </div>

                                        <div className="p-4 border-b border-slate-200 dark:border-slate-700">
                                            <form onSubmit={handleSearchSubmit} className="relative">
                                                <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
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
                                            <Button
                                                className="w-full bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700 shadow-lg shadow-purple-500/25"
                                                asChild
                                            >
                                                <Link href="/auctions/create">
                                                    <Gavel className="h-4 w-4 mr-2" />
                                                    Start Selling
                                                    <ArrowRight className="h-4 w-4 ml-2" />
                                                </Link>
                                            </Button>
                                        </div>
                                    </div>
                                </SheetContent>
                            </Sheet>
                        </div>
                    </div>
                </div>
            </div>

            <nav className="hidden md:block border-b border-slate-200/80 dark:border-slate-800/80 bg-slate-50/80 dark:bg-slate-900/80 backdrop-blur-sm">
                <div className="container mx-auto px-4">
                    <div className="flex items-center justify-between h-11">
                        <div className="flex items-center gap-1">
                            <DropdownMenu>
                                <DropdownMenuTrigger asChild>
                                    <Button variant="ghost" size="sm" className="gap-1.5 font-medium h-9 hover:bg-purple-100 dark:hover:bg-purple-900/30 hover:text-purple-600 dark:hover:text-purple-400">
                                        <Menu className="h-4 w-4" />
                                        Categories
                                        <ChevronDown className="h-3 w-3 opacity-50" />
                                    </Button>
                                </DropdownMenuTrigger>
                                <DropdownMenuContent align="start" className="w-60">
                                    {!categoriesLoading && categories.map((cat) => (
                                        <DropdownMenuItem key={cat.id} asChild>
                                            <Link
                                                href={`/search?category=${cat.slug}`}
                                                className="flex items-center gap-2 cursor-pointer"
                                            >
                                                <i className={`fa-solid ${cat.icon} w-4 text-center text-purple-500`}></i>
                                                <span>{cat.name}</span>
                                            </Link>
                                        </DropdownMenuItem>
                                    ))}
                                </DropdownMenuContent>
                            </DropdownMenu>

                            <div className="h-5 w-px bg-slate-300 dark:bg-slate-700 mx-1" />

                            {navLinks.map((link) => (
                                <Link
                                    key={link.name}
                                    href={link.href}
                                    className="flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium text-slate-600 dark:text-slate-300 hover:text-purple-600 dark:hover:text-purple-400 hover:bg-purple-50 dark:hover:bg-purple-900/20 rounded-lg transition-all"
                                >
                                    <FontAwesomeIcon icon={link.icon} className="h-3.5 w-3.5" />
                                    {link.name}
                                    {link.badge && (
                                        <Badge className={cn("text-[10px] px-1.5 py-0 h-5 text-white border-0", link.badgeColor)}>
                                            {link.badge}
                                        </Badge>
                                    )}
                                </Link>
                            ))}

                            <Link
                                href="/help"
                                className="flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium text-slate-600 dark:text-slate-300 hover:text-purple-600 dark:hover:text-purple-400 hover:bg-purple-50 dark:hover:bg-purple-900/20 rounded-lg transition-all"
                            >
                                <HelpCircle className="h-3.5 w-3.5" />
                                Help
                            </Link>
                        </div>

                        <div className="flex items-center gap-4">
                            <div className="hidden lg:flex items-center gap-1.5 text-sm text-slate-500 dark:text-slate-400">
                                <Shield className="w-4 h-4 text-green-500" />
                                <span>Buyer Protection</span>
                            </div>
                            <Button
                                size="sm"
                                className="h-8 bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700 shadow-md shadow-purple-500/20 hover:shadow-lg hover:shadow-purple-500/30 transition-all"
                                asChild
                            >
                                <Link href="/auctions/create">
                                    <Gavel className="h-3.5 w-3.5 mr-1.5" />
                                    Sell Now
                                </Link>
                            </Button>
                        </div>
                    </div>
                </div>
            </nav>
        </motion.header>
    );
}
