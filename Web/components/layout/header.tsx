"use client";

import { useState, useEffect } from "react";
import Link from "next/link";
import { motion, AnimatePresence } from "framer-motion";
import { Button } from "@/components/ui/button";
import { UserMenu } from "@/components/auth/user-menu";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
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

// Simplified nav links with Font Awesome icons
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

export function Header() {
    const [searchQuery, setSearchQuery] = useState("");
    const [isSearchFocused, setIsSearchFocused] = useState(false);
    const [isScrolled, setIsScrolled] = useState(false);
    const { data: categories, isLoading: categoriesLoading } = useCategories();

    useEffect(() => {
        const handleScroll = () => {
            setIsScrolled(window.scrollY > 10);
        };
        window.addEventListener("scroll", handleScroll);
        return () => window.removeEventListener("scroll", handleScroll);
    }, []);

    return (
        <header
            className={cn(
                "sticky top-0 z-50 w-full transition-all duration-300",
                isScrolled
                    ? "bg-white/95 dark:bg-slate-950/95 backdrop-blur-md shadow-lg"
                    : "bg-white dark:bg-slate-950"
            )}
        >
            {/* Top announcement bar */}
            <div className="bg-gradient-to-r from-purple-600 via-blue-600 to-purple-600 text-white py-2 text-center text-sm">
                <div className="container mx-auto px-4 flex items-center justify-center gap-2">
                    <Sparkles className="w-4 h-4 shrink-0" />
                    <span className="font-medium">Free shipping on orders over $100</span>
                    <Badge variant="secondary" className="bg-white/20 hover:bg-white/30 text-white border-0 text-xs hidden sm:inline-flex">
                        CODE: WELCOME10
                    </Badge>
                </div>
            </div>

            {/* Main header */}
            <div className="border-b border-slate-200 dark:border-slate-800">
                <div className="container mx-auto px-4">
                    <div className="flex h-16 items-center gap-4">
                        {/* Logo */}
                        <Link href="/" className="flex items-center gap-2 shrink-0">
                            <motion.div
                                whileHover={{ rotate: -10, scale: 1.05 }}
                                className="w-10 h-10 rounded-xl bg-gradient-to-br from-purple-600 to-blue-600 flex items-center justify-center shadow-lg shadow-purple-500/25"
                            >
                                <Gavel className="h-5 w-5 text-white" />
                            </motion.div>
                            <span className="text-xl font-bold text-purple-600 hidden sm:block">
                                AuctionHub
                            </span>
                        </Link>

                        {/* Search bar - Desktop */}
                        <div className="hidden md:flex flex-1 max-w-xl mx-4 relative">
                            <div className="flex w-full rounded-lg border border-slate-200 dark:border-slate-700 overflow-hidden bg-white dark:bg-slate-900">
                                <DropdownMenu>
                                    <DropdownMenuTrigger asChild>
                                        <Button
                                            variant="ghost"
                                            className="rounded-none border-r border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-800 hover:bg-slate-100 dark:hover:bg-slate-700 shrink-0 h-10 px-3"
                                        >
                                            All
                                            <ChevronDown className="ml-1 h-4 w-4" />
                                        </Button>
                                    </DropdownMenuTrigger>
                                    <DropdownMenuContent align="start" className="w-56">
                                        <DropdownMenuItem>All categories</DropdownMenuItem>
                                        <DropdownMenuSeparator />
                                        {!categoriesLoading && categories.map((cat) => (
                                            <DropdownMenuItem key={cat.id} asChild>
                                                <Link href={`/search?category=${cat.id}`} className="flex items-center gap-2 cursor-pointer">
                                                    <i className={`fa-solid ${cat.icon} w-4 text-center`}></i>
                                                    {cat.name}
                                                </Link>
                                            </DropdownMenuItem>
                                        ))}
                                    </DropdownMenuContent>
                                </DropdownMenu>

                                <Input
                                    type="text"
                                    placeholder="Search for anything..."
                                    value={searchQuery}
                                    onChange={(e) => setSearchQuery(e.target.value)}
                                    onFocus={() => setIsSearchFocused(true)}
                                    onBlur={() => setTimeout(() => setIsSearchFocused(false), 200)}
                                    className="flex-1 border-0 h-10 focus-visible:ring-0 focus-visible:ring-offset-0 rounded-none"
                                />

                                <Button className="rounded-none h-10 px-4 bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700">
                                    <Search className="h-4 w-4" />
                                </Button>
                            </div>

                            {/* Search suggestions dropdown */}
                            <AnimatePresence>
                                {isSearchFocused && (
                                    <motion.div
                                        initial={{ opacity: 0, y: 10 }}
                                        animate={{ opacity: 1, y: 0 }}
                                        exit={{ opacity: 0, y: 10 }}
                                        className="absolute top-full left-0 right-0 mt-2 bg-white dark:bg-slate-900 rounded-xl shadow-xl border border-slate-200 dark:border-slate-700 p-4 z-50"
                                    >
                                        <div className="flex items-center gap-2 mb-3 text-sm text-slate-500">
                                            <TrendingUp className="w-4 h-4" />
                                            <span>Trending searches</span>
                                        </div>
                                        <div className="flex flex-wrap gap-2">
                                            {trendingSearches.map((term) => (
                                                <button
                                                    key={term}
                                                    onClick={() => setSearchQuery(term)}
                                                    className="px-3 py-1.5 text-sm bg-slate-100 dark:bg-slate-800 hover:bg-purple-100 dark:hover:bg-purple-900/30 hover:text-purple-600 rounded-full transition-colors"
                                                >
                                                    {term}
                                                </button>
                                            ))}
                                        </div>
                                    </motion.div>
                                )}
                            </AnimatePresence>
                        </div>

                        {/* Right side actions */}
                        <div className="flex items-center gap-1 sm:gap-2 ml-auto">
                            <NotificationBell />

                            <Button
                                variant="ghost"
                                size="icon"
                                className="relative hover:bg-red-50 dark:hover:bg-red-950/30 hover:text-red-500"
                                asChild
                            >
                                <Link href="/wishlist">
                                    <Heart className="h-5 w-5" />
                                </Link>
                            </Button>

                            <div className="hidden sm:block h-6 w-px bg-slate-200 dark:bg-slate-700 mx-1" />

                            <UserMenu />

                            {/* Mobile menu */}
                            <Sheet>
                                <SheetTrigger asChild>
                                    <Button variant="ghost" size="icon" className="md:hidden">
                                        <Menu className="h-5 w-5" />
                                    </Button>
                                </SheetTrigger>
                                <SheetContent side="left" className="w-80 p-0">
                                    <div className="flex flex-col h-full">
                                        {/* Mobile header */}
                                        <div className="p-4 border-b border-slate-200 dark:border-slate-700 bg-gradient-to-r from-purple-600 to-blue-600">
                                            <div className="flex items-center justify-between">
                                                <div className="flex items-center gap-2">
                                                    <div className="w-8 h-8 rounded-lg bg-white/20 flex items-center justify-center">
                                                        <Gavel className="h-4 w-4 text-white" />
                                                    </div>
                                                    <span className="text-lg font-bold text-white">AuctionHub</span>
                                                </div>
                                                <SheetClose asChild>
                                                    <Button variant="ghost" size="icon" className="text-white hover:bg-white/20">
                                                        <X className="h-5 w-5" />
                                                    </Button>
                                                </SheetClose>
                                            </div>
                                        </div>

                                        {/* Mobile search */}
                                        <div className="p-4 border-b border-slate-200 dark:border-slate-700">
                                            <div className="relative">
                                                <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
                                                <Input
                                                    placeholder="Search for anything..."
                                                    className="pl-10 bg-slate-50 dark:bg-slate-900"
                                                />
                                            </div>
                                        </div>

                                        {/* Mobile navigation */}
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
                                                            className="flex items-center justify-between p-3 rounded-xl hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors"
                                                        >
                                                            <div className="flex items-center gap-3">
                                                                <FontAwesomeIcon icon={link.icon} className="h-5 w-5 text-purple-500" />
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
                                                            className="flex items-center gap-2 p-3 rounded-xl bg-slate-50 dark:bg-slate-900 hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors"
                                                        >
                                                            <i className={`fa-solid ${cat.icon} text-purple-500`}></i>
                                                            <span className="text-sm font-medium truncate">{cat.name}</span>
                                                        </Link>
                                                    ))}
                                                </div>
                                            </div>
                                        </div>

                                        {/* Mobile footer */}
                                        <div className="p-4 border-t border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-900">
                                            <Button
                                                className="w-full bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700"
                                                asChild
                                            >
                                                <Link href="/auctions/create">
                                                    <Gavel className="h-4 w-4 mr-2" />
                                                    Start Selling
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

            {/* Navigation bar - Desktop (Simplified) */}
            <nav className="hidden md:block border-b border-slate-200 dark:border-slate-800 bg-slate-50 dark:bg-slate-900">
                <div className="container mx-auto px-4">
                    <div className="flex items-center justify-between h-11">
                        {/* Left: Categories and quick links */}
                        <div className="flex items-center gap-4">
                            {/* Categories dropdown */}
                            <DropdownMenu>
                                <DropdownMenuTrigger asChild>
                                    <Button variant="ghost" size="sm" className="gap-1 font-medium h-9">
                                        <Menu className="h-4 w-4" />
                                        Categories
                                        <ChevronDown className="h-3 w-3" />
                                    </Button>
                                </DropdownMenuTrigger>
                                <DropdownMenuContent align="start" className="w-56">
                                    {!categoriesLoading && categories.map((cat) => (
                                        <DropdownMenuItem key={cat.id} asChild>
                                            <Link
                                                href={`/search?category=${cat.slug}`}
                                                className="flex items-center gap-2 cursor-pointer"
                                            >
                                                <i className={`fa-solid ${cat.icon} w-4 text-center`}></i>
                                                <span>{cat.name}</span>
                                            </Link>
                                        </DropdownMenuItem>
                                    ))}
                                </DropdownMenuContent>
                            </DropdownMenu>

                            <div className="h-5 w-px bg-slate-300 dark:bg-slate-700" />

                            {/* Nav links */}
                            {navLinks.map((link) => (
                                <Link
                                    key={link.name}
                                    href={link.href}
                                    className="flex items-center gap-1.5 px-2 py-1.5 text-sm font-medium text-slate-600 dark:text-slate-300 hover:text-purple-600 dark:hover:text-purple-400 transition-colors"
                                >
                                    <FontAwesomeIcon icon={link.icon} className="h-4 w-4" />
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
                                className="flex items-center gap-1.5 px-2 py-1.5 text-sm font-medium text-slate-600 dark:text-slate-300 hover:text-purple-600 dark:hover:text-purple-400 transition-colors"
                            >
                                <HelpCircle className="h-4 w-4" />
                                Help
                            </Link>
                        </div>

                        {/* Right: Trust badge and Sell button */}
                        <div className="flex items-center gap-4">
                            <div className="hidden lg:flex items-center gap-1.5 text-sm text-slate-500">
                                <Shield className="w-4 h-4 text-green-500" />
                                <span>Buyer Protection</span>
                            </div>
                            <Button
                                size="sm"
                                className="h-8 bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700"
                                asChild
                            >
                                <Link href="/auctions/create">
                                    <Gavel className="h-4 w-4 mr-1" />
                                    Sell
                                </Link>
                            </Button>
                        </div>
                    </div>
                </div>
            </nav>
        </header>
    );
}
