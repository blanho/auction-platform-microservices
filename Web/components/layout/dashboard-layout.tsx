"use client";

import { useState } from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
    faUser,
    faGavel,
    faHeart,
    faEye,
    faBox,
    faCirclePlus,
    faWallet,
    faGear,
    faChartLine,
    faBars,
    faChevronRight,
    faBagShopping,
} from "@fortawesome/free-solid-svg-icons";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Separator } from "@/components/ui/separator";
import { Sheet, SheetContent, SheetTrigger } from "@/components/ui/sheet";
import { MainLayout } from "@/components/layout/main-layout";
import { useAuthSession } from "@/hooks/use-auth-session";
import { IconDefinition } from "@fortawesome/fontawesome-svg-core";

interface NavItem {
    title: string;
    href: string;
    icon: IconDefinition;
    badge?: number;
}

const buyerNavItems: NavItem[] = [
    { title: "Profile", href: "/dashboard", icon: faUser },
    { title: "My Bids", href: "/dashboard/bids", icon: faGavel },
    { title: "Orders", href: "/dashboard/orders", icon: faBagShopping },
    { title: "Watchlist", href: "/dashboard/watchlist", icon: faEye },
    { title: "Wishlist", href: "/wishlist", icon: faHeart },
    { title: "Settings", href: "/dashboard/settings", icon: faGear },
];

const sellerNavItems: NavItem[] = [
    { title: "My Listings", href: "/dashboard/listings", icon: faBox },
    { title: "Create Listing", href: "/auctions/create", icon: faCirclePlus },
    { title: "Analytics", href: "/dashboard/analytics", icon: faChartLine },
    { title: "Wallet", href: "/dashboard/wallet", icon: faWallet },
];

interface SidebarContentProps {
    pathname: string;
    username?: string;
}

function SidebarContent({ pathname, username }: SidebarContentProps) {
    return (
        <div className="flex flex-col h-full">
            {/* User Profile Summary */}
            <div className="p-6 border-b border-zinc-200 dark:border-zinc-800">
                <div className="flex items-center gap-4">
                    <Avatar className="h-12 w-12">
                        <AvatarFallback className="bg-amber-500 text-white font-bold">
                            {username?.charAt(0).toUpperCase() || "U"}
                        </AvatarFallback>
                    </Avatar>
                    <div className="flex-1 min-w-0">
                        <p className="font-semibold text-zinc-900 dark:text-white truncate">
                            {username || "User"}
                        </p>
                        <p className="text-sm text-zinc-500 dark:text-zinc-400">
                            Member
                        </p>
                    </div>
                </div>
            </div>

            <ScrollArea className="flex-1 px-4 py-6">
                {/* Buyer Section */}
                <div className="mb-6">
                    <h3 className="px-3 mb-2 text-xs font-semibold text-zinc-500 dark:text-zinc-400 uppercase tracking-wider">
                        Buyer
                    </h3>
                    <nav className="space-y-1">
                        {buyerNavItems.map((item) => (
                            <Link
                                key={item.href}
                                href={item.href}
                                className={cn(
                                    "flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-colors",
                                    pathname === item.href
                                        ? "bg-amber-500/10 text-amber-600 dark:text-amber-400"
                                        : "text-zinc-600 dark:text-zinc-400 hover:bg-zinc-100 dark:hover:bg-zinc-800 hover:text-zinc-900 dark:hover:text-white"
                                )}
                            >
                                <FontAwesomeIcon icon={item.icon} className="h-5 w-5" />
                                {item.title}
                                {item.badge !== undefined && item.badge > 0 && (
                                    <span className="ml-auto bg-amber-500 text-white text-xs font-bold px-2 py-0.5 rounded-full">
                                        {item.badge}
                                    </span>
                                )}
                                {pathname === item.href && (
                                    <FontAwesomeIcon icon={faChevronRight} className="ml-auto h-4 w-4" />
                                )}
                            </Link>
                        ))}
                    </nav>
                </div>

                <Separator className="my-4" />

                {/* Seller Section */}
                <div>
                    <h3 className="px-3 mb-2 text-xs font-semibold text-zinc-500 dark:text-zinc-400 uppercase tracking-wider">
                        Seller
                    </h3>
                    <nav className="space-y-1">
                        {sellerNavItems.map((item) => (
                            <Link
                                key={item.href}
                                href={item.href}
                                className={cn(
                                    "flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-colors",
                                    pathname === item.href
                                        ? "bg-amber-500/10 text-amber-600 dark:text-amber-400"
                                        : "text-zinc-600 dark:text-zinc-400 hover:bg-zinc-100 dark:hover:bg-zinc-800 hover:text-zinc-900 dark:hover:text-white"
                                )}
                            >
                                <FontAwesomeIcon icon={item.icon} className="h-5 w-5" />
                                {item.title}
                                {pathname === item.href && (
                                    <FontAwesomeIcon icon={faChevronRight} className="ml-auto h-4 w-4" />
                                )}
                            </Link>
                        ))}
                    </nav>
                </div>
            </ScrollArea>

            {/* Bottom Action */}
            <div className="p-4 border-t border-zinc-200 dark:border-zinc-800">
                <Button
                    asChild
                    className="w-full bg-amber-500 hover:bg-amber-600 text-white"
                >
                    <Link href="/auctions/create">
                        <FontAwesomeIcon icon={faCirclePlus} className="mr-2 h-4 w-4" />
                        Create Auction
                    </Link>
                </Button>
            </div>
        </div>
    );
}

interface DashboardLayoutProps {
    children: React.ReactNode;
    title?: string;
    description?: string;
}

export function DashboardLayout({
    children,
    title,
    description,
}: DashboardLayoutProps) {
    const pathname = usePathname();
    const { user } = useAuthSession();
    const [mobileOpen, setMobileOpen] = useState(false);

    return (
        <MainLayout>
            <div className="flex min-h-[calc(100vh-64px)]">
                {/* Desktop Sidebar */}
                <aside className="hidden lg:flex lg:flex-col lg:w-64 lg:border-r border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-950">
                    <SidebarContent
                        pathname={pathname}
                        username={user?.name ?? undefined}
                    />
                </aside>

                {/* Mobile Sidebar */}
                <Sheet open={mobileOpen} onOpenChange={setMobileOpen}>
                    <SheetTrigger asChild>
                        <Button
                            variant="ghost"
                            size="icon"
                            className="lg:hidden fixed bottom-4 right-4 z-50 h-14 w-14 rounded-full bg-amber-500 hover:bg-amber-600 text-white shadow-lg"
                        >
                            <FontAwesomeIcon icon={faBars} className="h-6 w-6" />
                        </Button>
                    </SheetTrigger>
                    <SheetContent side="left" className="w-64 p-0">
                        <SidebarContent
                            pathname={pathname}
                            username={user?.name ?? undefined}
                        />
                    </SheetContent>
                </Sheet>

                {/* Main Content */}
                <main className="flex-1 overflow-auto">
                    <div className="container py-8 px-4 lg:px-8">
                        {(title || description) && (
                            <div className="mb-8">
                                {title && (
                                    <h1 className="text-3xl font-bold text-zinc-900 dark:text-white">
                                        {title}
                                    </h1>
                                )}
                                {description && (
                                    <p className="mt-2 text-zinc-600 dark:text-zinc-400">
                                        {description}
                                    </p>
                                )}
                            </div>
                        )}
                        {children}
                    </div>
                </main>
            </div>
        </MainLayout>
    );
}
