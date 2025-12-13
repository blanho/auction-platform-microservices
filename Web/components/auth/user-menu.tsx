"use client";

import { signIn, signOut, useSession } from "next-auth/react";
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
import { LogIn, LogOut, User, Settings, Gavel, Shield, LayoutDashboard, Eye, Heart, Wallet, BarChart3, Package, ShoppingBag } from "lucide-react";
import Link from "next/link";
import { Badge } from "@/components/ui/badge";

export function UserMenu() {
    const { data: session, status } = useSession();
    const isAdmin = session?.user?.role === "admin";

    if (status === "loading") {
        return (
            <div className="flex items-center gap-2">
                <div className="h-8 w-8 animate-pulse rounded-full bg-muted" />
            </div>
        );
    }

    if (!session) {
        return (
            <div className="flex items-center gap-2">
                <Button onClick={() => signIn("id-server")} variant="outline" size="sm">
                    <LogIn className="mr-2 h-4 w-4" />
                    Sign In
                </Button>
                <Button asChild variant="default" size="sm">
                    <Link href="/auth/register">Register</Link>
                </Button>
            </div>
        );
    }

    const initials = session.user?.name
        ?.split(" ")
        .map((n) => n[0])
        .join("")
        .toUpperCase() || "U";

    return (
        <DropdownMenu>
            <DropdownMenuTrigger asChild>
                <Button variant="ghost" className="relative h-10 w-10 rounded-full">
                    <Avatar>
                        <AvatarImage src={session.user?.image || undefined} alt={session.user?.name || "User"} />
                        <AvatarFallback>{initials}</AvatarFallback>
                    </Avatar>
                </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent className="w-56" align="end" forceMount>
                <DropdownMenuLabel className="font-normal">
                    <div className="flex flex-col space-y-1">
                        <div className="flex items-center gap-2">
                            <p className="text-sm font-medium leading-none">{session.user?.name}</p>
                            {isAdmin && (
                                <Badge variant="secondary" className="text-xs px-1.5 py-0">
                                    Admin
                                </Badge>
                            )}
                        </div>
                        <p className="text-xs leading-none text-muted-foreground">
                            {session.user?.email}
                        </p>
                    </div>
                </DropdownMenuLabel>
                <DropdownMenuSeparator />
                {isAdmin && (
                    <>
                        <DropdownMenuItem asChild>
                            <Link href="/admin">
                                <Shield className="mr-2 h-4 w-4" />
                                Admin Dashboard
                            </Link>
                        </DropdownMenuItem>
                        <DropdownMenuSeparator />
                    </>
                )}
                <DropdownMenuItem asChild>
                    <Link href="/dashboard">
                        <LayoutDashboard className="mr-2 h-4 w-4" />
                        Dashboard
                    </Link>
                </DropdownMenuItem>
                <DropdownMenuItem asChild>
                    <Link href="/auctions/my-auctions">
                        <Gavel className="mr-2 h-4 w-4" />
                        My Auctions
                    </Link>
                </DropdownMenuItem>
                <DropdownMenuItem asChild>
                    <Link href="/dashboard/listings">
                        <Package className="mr-2 h-4 w-4" />
                        My Listings
                    </Link>
                </DropdownMenuItem>
                <DropdownMenuItem asChild>
                    <Link href="/dashboard/orders">
                        <ShoppingBag className="mr-2 h-4 w-4" />
                        Orders
                    </Link>
                </DropdownMenuItem>
                <DropdownMenuItem asChild>
                    <Link href="/dashboard/bids">
                        <User className="mr-2 h-4 w-4" />
                        My Bids
                    </Link>
                </DropdownMenuItem>
                <DropdownMenuItem asChild>
                    <Link href="/dashboard/watchlist">
                        <Eye className="mr-2 h-4 w-4" />
                        Watchlist
                    </Link>
                </DropdownMenuItem>
                <DropdownMenuItem asChild>
                    <Link href="/wishlist">
                        <Heart className="mr-2 h-4 w-4" />
                        Wishlist
                    </Link>
                </DropdownMenuItem>
                <DropdownMenuItem asChild>
                    <Link href="/dashboard/wallet">
                        <Wallet className="mr-2 h-4 w-4" />
                        Wallet
                    </Link>
                </DropdownMenuItem>
                <DropdownMenuItem asChild>
                    <Link href="/dashboard/analytics">
                        <BarChart3 className="mr-2 h-4 w-4" />
                        Analytics
                    </Link>
                </DropdownMenuItem>
                <DropdownMenuSeparator />
                <DropdownMenuItem asChild>
                    <Link href="/dashboard/settings">
                        <Settings className="mr-2 h-4 w-4" />
                        Settings
                    </Link>
                </DropdownMenuItem>
                <DropdownMenuSeparator />
                <DropdownMenuItem
                    onClick={() => signOut({ callbackUrl: "/" })}
                    className="text-destructive focus:text-destructive"
                >
                    <LogOut className="mr-2 h-4 w-4" />
                    Sign Out
                </DropdownMenuItem>
            </DropdownMenuContent>
        </DropdownMenu>
    );
}
