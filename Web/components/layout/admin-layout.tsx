"use client";

import { useState } from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { useSession } from "next-auth/react";
import {
    Gavel,
    Users,
    Flag,
    CreditCard,
    Settings,
    LayoutDashboard,
    Menu,
    Shield,
    ChevronRight,
} from "lucide-react";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Sheet, SheetContent, SheetTrigger } from "@/components/ui/sheet";
import { MainLayout } from "@/components/layout/main-layout";
import { RequireAdmin } from "@/components/auth/require-admin";

interface NavItem {
    title: string;
    href: string;
    icon: React.ElementType;
    disabled?: boolean;
}

const adminNavItems: NavItem[] = [
    { title: "Dashboard", href: "/admin", icon: LayoutDashboard },
    { title: "Auctions", href: "/admin/auctions", icon: Gavel },
    { title: "Users", href: "/admin/users", icon: Users },
    { title: "Reports", href: "/admin/reports", icon: Flag },
    { title: "Payments", href: "/admin/payments", icon: CreditCard },
    { title: "Settings", href: "/admin/settings", icon: Settings },
];

interface SidebarContentProps {
    pathname: string;
    username?: string;
}

function SidebarContent({ pathname, username }: SidebarContentProps) {
    return (
        <div className="flex flex-col h-full">
            {/* Admin Header */}
            <div className="p-6 border-b border-zinc-200 dark:border-zinc-800">
                <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-lg bg-red-500 flex items-center justify-center">
                        <Shield className="h-5 w-5 text-white" />
                    </div>
                    <div>
                        <p className="font-bold text-zinc-900 dark:text-white">
                            Admin Panel
                        </p>
                        <p className="text-xs text-zinc-500">
                            {username || "Administrator"}
                        </p>
                    </div>
                </div>
            </div>

            <ScrollArea className="flex-1 px-4 py-6">
                <nav className="space-y-1">
                    {adminNavItems.map((item) => {
                        const isActive =
                            item.href === "/admin"
                                ? pathname === "/admin"
                                : pathname.startsWith(item.href);

                        return (
                            <Link
                                key={item.href}
                                href={item.disabled ? "#" : item.href}
                                className={cn(
                                    "flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-colors",
                                    item.disabled && "opacity-50 cursor-not-allowed",
                                    isActive
                                        ? "bg-red-500/10 text-red-600 dark:text-red-400"
                                        : "text-zinc-600 dark:text-zinc-400 hover:bg-zinc-100 dark:hover:bg-zinc-800 hover:text-zinc-900 dark:hover:text-white"
                                )}
                            >
                                <item.icon className="h-5 w-5" />
                                {item.title}
                                {isActive && (
                                    <ChevronRight className="ml-auto h-4 w-4" />
                                )}
                            </Link>
                        );
                    })}
                </nav>
            </ScrollArea>

            {/* System Info */}
            <div className="p-4 border-t border-zinc-200 dark:border-zinc-800">
                <div className="text-xs text-zinc-500">
                    <p>Auction Platform v1.0</p>
                    <p>© 2024 All rights reserved</p>
                </div>
            </div>
        </div>
    );
}

interface AdminLayoutProps {
    children: React.ReactNode;
    title?: string;
    description?: string;
}

function AdminLayoutContent({
    children,
    title,
    description,
}: AdminLayoutProps) {
    const pathname = usePathname();
    const { data: session } = useSession();
    const [mobileOpen, setMobileOpen] = useState(false);

    return (
        <MainLayout>
            <div className="flex min-h-[calc(100vh-64px)]">
                {/* Desktop Sidebar */}
                <aside className="hidden lg:flex lg:flex-col lg:w-64 lg:border-r border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-950">
                    <SidebarContent
                        pathname={pathname}
                        username={session?.user?.name ?? undefined}
                    />
                </aside>

                {/* Mobile Sidebar */}
                <Sheet open={mobileOpen} onOpenChange={setMobileOpen}>
                    <SheetTrigger asChild>
                        <Button
                            variant="ghost"
                            size="icon"
                            className="lg:hidden fixed bottom-4 right-4 z-50 h-14 w-14 rounded-full bg-red-500 hover:bg-red-600 text-white shadow-lg"
                        >
                            <Menu className="h-6 w-6" />
                        </Button>
                    </SheetTrigger>
                    <SheetContent side="left" className="w-64 p-0">
                        <SidebarContent
                            pathname={pathname}
                            username={session?.user?.name ?? undefined}
                        />
                    </SheetContent>
                </Sheet>

                {/* Main Content */}
                <main className="flex-1 overflow-auto bg-zinc-50 dark:bg-zinc-900">
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

export function AdminLayout(props: AdminLayoutProps) {
    return (
        <RequireAdmin>
            <AdminLayoutContent {...props} />
        </RequireAdmin>
    );
}
