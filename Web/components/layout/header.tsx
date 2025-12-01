// Header component
'use client';

import { useState } from 'react';
import Link from 'next/link';
import { useAuthStore } from '@/store/auth.store';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import {
    Search,
    Heart,
    ShoppingCart,
    Menu,
    ChevronDown,
    Gavel,
    Flame,
    Gift,
    Tag,
    HelpCircle,
    Globe,
} from 'lucide-react';
import {
    Sheet,
    SheetContent,
    SheetTrigger,
} from '@/components/ui/sheet';

const categories = [
    { name: 'Electronics', icon: '📱', href: '/search?category=electronics' },
    { name: 'Fashion', icon: '👔', href: '/search?category=fashion' },
    { name: 'Watches', icon: '⌚', href: '/search?category=watches' },
    { name: 'Sneakers', icon: '👟', href: '/search?category=sneakers' },
    { name: 'Collectibles', icon: '🎨', href: '/search?category=collectibles' },
    { name: 'Home & Garden', icon: '🏡', href: '/search?category=home' },
    { name: 'Sports', icon: '⚽', href: '/search?category=sports' },
    { name: 'More', icon: '📦', href: '/categories' },
];

const navLinks = [
    { name: 'Hot Deals', href: '/deals', icon: Flame },
    { name: 'Live Auctions', href: '/live', icon: Gavel },
    { name: 'Bestsellers', href: '/bestsellers', icon: Tag },
    { name: 'Gift Ideas', href: '/gifts', icon: Gift },
    { name: 'Help', href: '/help', icon: HelpCircle },
];

export function Header() {
    const { isAuthenticated, user, logout } = useAuthStore();
    const [searchQuery, setSearchQuery] = useState('');
    const [selectedCategory, setSelectedCategory] = useState('All categories');

    return (
        <header className="sticky top-0 z-50 w-full bg-white dark:bg-gray-950 shadow-sm">
            {/* Top Bar */}
            <div className="border-b border-gray-100 dark:border-gray-800">
                <div className="container mx-auto px-4 md:px-6 lg:px-8">
                    <div className="flex h-16 items-center justify-between gap-4">
                        {/* Logo */}
                        <Link href="/" className="flex items-center gap-2 shrink-0">
                            <div className="w-9 h-9 rounded-lg bg-linear-to-br from-blue-500 to-indigo-600 flex items-center justify-center">
                                <Gavel className="h-5 w-5 text-white" />
                            </div>
                            <span className="text-xl font-bold bg-linear-to-r from-blue-600 to-indigo-600 bg-clip-text text-transparent hidden sm:block">
                                AuctionHub
                            </span>
                        </Link>

                        {/* Search Bar - Desktop */}
                        <div className="hidden md:flex flex-1 max-w-2xl">
                            <div className="flex w-full">
                                <DropdownMenu>
                                    <DropdownMenuTrigger asChild>
                                        <Button
                                            variant="outline"
                                            className="rounded-r-none border-r-0 bg-gray-50 dark:bg-gray-900 hover:bg-gray-100 dark:hover:bg-gray-800 shrink-0"
                                        >
                                            {selectedCategory}
                                            <ChevronDown className="ml-2 h-4 w-4" />
                                        </Button>
                                    </DropdownMenuTrigger>
                                    <DropdownMenuContent align="start" className="w-48">
                                        <DropdownMenuItem onClick={() => setSelectedCategory('All categories')}>
                                            All categories
                                        </DropdownMenuItem>
                                        <DropdownMenuSeparator />
                                        {categories.map((cat) => (
                                            <DropdownMenuItem key={cat.name} onClick={() => setSelectedCategory(cat.name)}>
                                                <span className="mr-2">{cat.icon}</span>
                                                {cat.name}
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
                                        className="rounded-none border-x-0 h-10 focus-visible:ring-0 focus-visible:ring-offset-0"
                                    />
                                </div>
                                <Button className="rounded-l-none bg-blue-500 hover:bg-blue-600 px-6">
                                    <Search className="h-4 w-4" />
                                    <span className="ml-2 hidden lg:inline">Search</span>
                                </Button>
                            </div>
                        </div>

                        {/* Right Actions */}
                        <div className="flex items-center gap-1 sm:gap-2">
                            {/* Location/Language */}
                            <Button variant="ghost" size="sm" className="hidden lg:flex items-center gap-1 text-sm">
                                <Globe className="h-4 w-4" />
                                <span>EN</span>
                            </Button>

                            {/* Wishlist */}
                            <Button variant="ghost" size="icon" className="relative" asChild>
                                <Link href="/wishlist">
                                    <Heart className="h-5 w-5" />
                                    <span className="absolute -top-1 -right-1 h-4 w-4 rounded-full bg-red-500 text-[10px] font-medium text-white flex items-center justify-center">
                                        3
                                    </span>
                                </Link>
                            </Button>

                            {/* Cart */}
                            <Button variant="ghost" size="icon" className="relative" asChild>
                                <Link href="/cart">
                                    <ShoppingCart className="h-5 w-5" />
                                    <span className="absolute -top-1 -right-1 h-4 w-4 rounded-full bg-blue-500 text-[10px] font-medium text-white flex items-center justify-center">
                                        2
                                    </span>
                                </Link>
                            </Button>

                            {/* Auth */}
                            {isAuthenticated ? (
                                <DropdownMenu>
                                    <DropdownMenuTrigger asChild>
                                        <Button variant="ghost" className="relative h-9 w-9 rounded-full">
                                            <Avatar className="h-9 w-9">
                                                <AvatarFallback className="bg-linear-to-br from-blue-500 to-indigo-600 text-white">
                                                    {user?.username?.charAt(0).toUpperCase() || 'U'}
                                                </AvatarFallback>
                                            </Avatar>
                                        </Button>
                                    </DropdownMenuTrigger>
                                    <DropdownMenuContent align="end" className="w-56">
                                        <div className="flex items-center gap-3 p-3">
                                            <Avatar className="h-10 w-10">
                                                <AvatarFallback className="bg-linear-to-br from-blue-500 to-indigo-600 text-white">
                                                    {user?.username?.charAt(0).toUpperCase() || 'U'}
                                                </AvatarFallback>
                                            </Avatar>
                                            <div>
                                                <p className="font-medium">{user?.username}</p>
                                                <p className="text-xs text-muted-foreground">{user?.email}</p>
                                            </div>
                                        </div>
                                        <DropdownMenuSeparator />
                                        <DropdownMenuItem asChild>
                                            <Link href="/profile" className="cursor-pointer">My Profile</Link>
                                        </DropdownMenuItem>
                                        <DropdownMenuItem asChild>
                                            <Link href="/my-auctions" className="cursor-pointer">My Auctions</Link>
                                        </DropdownMenuItem>
                                        <DropdownMenuItem asChild>
                                            <Link href="/orders" className="cursor-pointer">Orders</Link>
                                        </DropdownMenuItem>
                                        <DropdownMenuItem asChild>
                                            <Link href="/watchlist" className="cursor-pointer">Watchlist</Link>
                                        </DropdownMenuItem>
                                        <DropdownMenuSeparator />
                                        <DropdownMenuItem onClick={logout} className="text-red-500 cursor-pointer">
                                            Log out
                                        </DropdownMenuItem>
                                    </DropdownMenuContent>
                                </DropdownMenu>
                            ) : (
                                <div className="flex items-center gap-2">
                                    <Button variant="ghost" size="sm" asChild className="hidden sm:flex">
                                        <Link href="/auth/login">Sign In</Link>
                                    </Button>
                                    <Button size="sm" asChild className="bg-linear-to-r from-blue-500 to-indigo-600 hover:from-blue-600 hover:to-indigo-700">
                                        <Link href="/auth/register">Sign Up</Link>
                                    </Button>
                                </div>
                            )}

                            {/* Mobile Menu */}
                            <Sheet>
                                <SheetTrigger asChild>
                                    <Button variant="ghost" size="icon" className="md:hidden">
                                        <Menu className="h-5 w-5" />
                                    </Button>
                                </SheetTrigger>
                                <SheetContent side="left" className="w-80">
                                    <div className="flex flex-col gap-6 mt-6">
                                        {/* Mobile Search */}
                                        <div className="relative">
                                            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                                            <Input placeholder="Search..." className="pl-9" />
                                        </div>

                                        {/* Mobile Categories */}
                                        <div>
                                            <h3 className="font-semibold mb-3">Categories</h3>
                                            <div className="grid grid-cols-2 gap-2">
                                                {categories.map((cat) => (
                                                    <Link
                                                        key={cat.name}
                                                        href={cat.href}
                                                        className="flex items-center gap-2 p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800"
                                                    >
                                                        <span>{cat.icon}</span>
                                                        <span className="text-sm">{cat.name}</span>
                                                    </Link>
                                                ))}
                                            </div>
                                        </div>

                                        {/* Mobile Nav Links */}
                                        <div className="border-t pt-4">
                                            {navLinks.map((link) => (
                                                <Link
                                                    key={link.name}
                                                    href={link.href}
                                                    className="flex items-center gap-3 py-2 text-sm hover:text-blue-500"
                                                >
                                                    <link.icon className="h-4 w-4" />
                                                    {link.name}
                                                </Link>
                                            ))}
                                        </div>
                                    </div>
                                </SheetContent>
                            </Sheet>
                        </div>
                    </div>
                </div>
            </div>

            {/* Navigation Bar */}
            <nav className="hidden md:block border-b border-gray-100 dark:border-gray-800 bg-gray-50/50 dark:bg-gray-900/50">
                <div className="container mx-auto px-4 md:px-6 lg:px-8">
                    <div className="flex items-center gap-1 h-12">
                        {/* Categories Dropdown */}
                        <DropdownMenu>
                            <DropdownMenuTrigger asChild>
                                <Button variant="ghost" size="sm" className="gap-2">
                                    <Menu className="h-4 w-4" />
                                    All Categories
                                    <ChevronDown className="h-3 w-3" />
                                </Button>
                            </DropdownMenuTrigger>
                            <DropdownMenuContent align="start" className="w-56">
                                {categories.map((cat) => (
                                    <DropdownMenuItem key={cat.name} asChild>
                                        <Link href={cat.href} className="flex items-center gap-2 cursor-pointer">
                                            <span>{cat.icon}</span>
                                            {cat.name}
                                        </Link>
                                    </DropdownMenuItem>
                                ))}
                            </DropdownMenuContent>
                        </DropdownMenu>

                        <div className="h-6 w-px bg-gray-200 dark:bg-gray-700 mx-2" />

                        {/* Nav Links */}
                        {navLinks.map((link) => (
                            <Link
                                key={link.name}
                                href={link.href}
                                className="flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium text-gray-600 dark:text-gray-300 hover:text-blue-500 dark:hover:text-blue-400 transition-colors"
                            >
                                <link.icon className="h-4 w-4" />
                                {link.name}
                            </Link>
                        ))}

                        {/* Sell Button */}
                        <div className="ml-auto">
                            <Button size="sm" variant="outline" className="border-blue-500 text-blue-500 hover:bg-blue-50 dark:hover:bg-blue-950" asChild>
                                <Link href="/sell">
                                    <Gavel className="h-4 w-4 mr-1" />
                                    Start Selling
                                </Link>
                            </Button>
                        </div>
                    </div>
                </div>
            </nav>
        </header>
    );
}
