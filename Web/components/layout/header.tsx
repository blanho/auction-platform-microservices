// Header component
'use client';

import Link from 'next/link';
import { useAuthStore } from '@/store/auth.store';
import { Button } from '@/components/ui/button';
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';

export function Header() {
    const { isAuthenticated, user, logout } = useAuthStore();

    return (
        <header className="sticky top-0 z-50 w-full border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
            <div className="container mx-auto flex h-16 items-center justify-between px-4 md:px-6 lg:px-8">
                <div className="flex items-center gap-6">
                    <Link href="/" className="flex items-center space-x-2">
                        <span className="text-2xl font-bold">AuctionHub</span>
                    </Link>
                    <nav className="hidden md:flex items-center gap-6">
                        <Link
                            href="/auctions"
                            className="text-sm font-medium transition-colors hover:text-primary"
                        >
                            Auctions
                        </Link>
                        <Link
                            href="/search"
                            className="text-sm font-medium transition-colors hover:text-primary"
                        >
                            Search
                        </Link>
                    </nav>
                </div>

                <div className="flex items-center gap-4">
                    {isAuthenticated ? (
                        <>
                            <Button asChild variant="outline">
                                <Link href="/auctions/create">Create Auction</Link>
                            </Button>
                            <DropdownMenu>
                                <DropdownMenuTrigger asChild>
                                    <Button variant="ghost" className="relative h-10 w-10 rounded-full">
                                        <Avatar>
                                            <AvatarFallback>
                                                {user?.username?.charAt(0).toUpperCase() || 'U'}
                                            </AvatarFallback>
                                        </Avatar>
                                    </Button>
                                </DropdownMenuTrigger>
                                <DropdownMenuContent align="end">
                                    <div className="flex items-center justify-start gap-2 p-2">
                                        <div className="flex flex-col space-y-1">
                                            <p className="text-sm font-medium">{user?.username}</p>
                                            <p className="text-xs text-muted-foreground">{user?.email}</p>
                                        </div>
                                    </div>
                                    <DropdownMenuSeparator />
                                    <DropdownMenuItem asChild>
                                        <Link href="/profile">Profile</Link>
                                    </DropdownMenuItem>
                                    <DropdownMenuItem asChild>
                                        <Link href="/my-auctions">My Auctions</Link>
                                    </DropdownMenuItem>
                                    <DropdownMenuSeparator />
                                    <DropdownMenuItem onClick={logout}>Log out</DropdownMenuItem>
                                </DropdownMenuContent>
                            </DropdownMenu>
                        </>
                    ) : (
                        <>
                            <Button asChild variant="ghost">
                                <Link href="/auth/login">Sign In</Link>
                            </Button>
                            <Button asChild>
                                <Link href="/auth/register">Sign Up</Link>
                            </Button>
                        </>
                    )}
                </div>
            </div>
        </header>
    );
}
