"use client";

import { signOut } from "next-auth/react";
import { Button } from "@/components/ui/button";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuLabel,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
    faRightToBracket,
    faUserPlus,
    faRightFromBracket,
    faUser,
    faGear,
    faGavel,
    faShieldHalved,
    faTableColumns,
    faEye,
    faHeart,
    faWallet,
    faChartLine,
    faBox,
    faBagShopping,
} from "@fortawesome/free-solid-svg-icons";
import Link from "next/link";
import { Badge } from "@/components/ui/badge";
import { AuthDialog } from "@/features/auth";
import { useAuthSession } from "@/hooks/use-auth-session";

export function UserMenu() {
    const { user, isAuthenticated, isLoading, isAdmin } = useAuthSession();

    if (isLoading) {
        return (
            <div className="flex items-center gap-2">
                <div className="h-8 w-8 animate-pulse rounded-full bg-muted" />
            </div>
        );
    }

    if (!isAuthenticated) {
        return (
            <div className="flex items-center gap-2">
                <AuthDialog
                    defaultMode="signin"
                    trigger={
                        <Button variant="outline" size="sm" className="gap-2 border-slate-200 dark:border-slate-700 hover:border-purple-300 dark:hover:border-purple-700 hover:bg-purple-50 dark:hover:bg-purple-950/30">
                            <FontAwesomeIcon icon={faRightToBracket} className="h-4 w-4" />
                            Sign In
                        </Button>
                    }
                />
                <AuthDialog
                    defaultMode="register"
                    trigger={
                        <Button size="sm" className="gap-2 bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700">
                            <FontAwesomeIcon icon={faUserPlus} className="h-4 w-4" />
                            Register
                        </Button>
                    }
                />
            </div>
        );
    }

    const initials = user?.name
        ?.split(" ")
        .map((n) => n[0])
        .join("")
        .toUpperCase() || "U";

    return (
        <DropdownMenu>
            <DropdownMenuTrigger asChild>
                <Button variant="ghost" className="relative h-10 w-10 rounded-full">
                    <Avatar>
                        <AvatarImage src={user?.image || undefined} alt={user?.name || "User"} />
                        <AvatarFallback>{initials}</AvatarFallback>
                    </Avatar>
                </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent className="w-56" align="end" forceMount>
                <DropdownMenuLabel className="font-normal">
                    <div className="flex flex-col space-y-1">
                        <div className="flex items-center gap-2">
                            <p className="text-sm font-medium leading-none">{user?.name}</p>
                            {isAdmin && (
                                <Badge variant="secondary" className="text-xs px-1.5 py-0">
                                    Admin
                                </Badge>
                            )}
                        </div>
                        <p className="text-xs leading-none text-muted-foreground">
                            {user?.email}
                        </p>
                    </div>
                </DropdownMenuLabel>
                <DropdownMenuSeparator />
                {isAdmin && (
                    <>
                        <DropdownMenuItem asChild>
                            <Link href="/admin">
                                <FontAwesomeIcon icon={faShieldHalved} className="mr-2 h-4 w-4" />
                                Admin Dashboard
                            </Link>
                        </DropdownMenuItem>
                        <DropdownMenuSeparator />
                    </>
                )}
                <DropdownMenuItem asChild>
                    <Link href="/dashboard">
                        <FontAwesomeIcon icon={faTableColumns} className="mr-2 h-4 w-4" />
                        Dashboard
                    </Link>
                </DropdownMenuItem>
                <DropdownMenuItem asChild>
                    <Link href="/auctions/my-auctions">
                        <FontAwesomeIcon icon={faGavel} className="mr-2 h-4 w-4" />
                        My Auctions
                    </Link>
                </DropdownMenuItem>
                <DropdownMenuItem asChild>
                    <Link href="/dashboard/listings">
                        <FontAwesomeIcon icon={faBox} className="mr-2 h-4 w-4" />
                        My Listings
                    </Link>
                </DropdownMenuItem>
                <DropdownMenuItem asChild>
                    <Link href="/dashboard/orders">
                        <FontAwesomeIcon icon={faBagShopping} className="mr-2 h-4 w-4" />
                        Orders
                    </Link>
                </DropdownMenuItem>
                <DropdownMenuItem asChild>
                    <Link href="/dashboard/bids">
                        <FontAwesomeIcon icon={faUser} className="mr-2 h-4 w-4" />
                        My Bids
                    </Link>
                </DropdownMenuItem>
                <DropdownMenuItem asChild>
                    <Link href="/dashboard/watchlist">
                        <FontAwesomeIcon icon={faEye} className="mr-2 h-4 w-4" />
                        Watchlist
                    </Link>
                </DropdownMenuItem>
                <DropdownMenuItem asChild>
                    <Link href="/wishlist">
                        <FontAwesomeIcon icon={faHeart} className="mr-2 h-4 w-4" />
                        Wishlist
                    </Link>
                </DropdownMenuItem>
                <DropdownMenuItem asChild>
                    <Link href="/dashboard/wallet">
                        <FontAwesomeIcon icon={faWallet} className="mr-2 h-4 w-4" />
                        Wallet
                    </Link>
                </DropdownMenuItem>
                <DropdownMenuItem asChild>
                    <Link href="/dashboard/analytics">
                        <FontAwesomeIcon icon={faChartLine} className="mr-2 h-4 w-4" />
                        Analytics
                    </Link>
                </DropdownMenuItem>
                <DropdownMenuSeparator />
                <DropdownMenuItem asChild>
                    <Link href="/dashboard/settings">
                        <FontAwesomeIcon icon={faGear} className="mr-2 h-4 w-4" />
                        Settings
                    </Link>
                </DropdownMenuItem>
                <DropdownMenuSeparator />
                <DropdownMenuItem
                    onClick={() => signOut({ callbackUrl: "/" })}
                    className="text-destructive focus:text-destructive"
                >
                    <FontAwesomeIcon icon={faRightFromBracket} className="mr-2 h-4 w-4" />
                    Sign Out
                </DropdownMenuItem>
            </DropdownMenuContent>
        </DropdownMenu>
    );
}
