"use client";

import { useState } from "react";
import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useSession, signOut } from "next-auth/react";
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
    ChevronLeft,
    FolderTree,
    Bell,
    FileText,
    LogOut,
    Home,
    User,
    PanelLeftClose,
    PanelLeft,
    Search,
    Moon,
    Sun,
} from "lucide-react";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Sheet, SheetContent, SheetTrigger } from "@/components/ui/sheet";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuLabel,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
    Tooltip,
    TooltipContent,
    TooltipProvider,
    TooltipTrigger,
} from "@/components/ui/tooltip";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Input } from "@/components/ui/input";
import { RequireAdmin } from "@/components/auth/require-admin";
import { useTheme } from "next-themes";

interface NavItem {
    title: string;
    href: string;
    icon: React.ElementType;
    disabled?: boolean;
}

const adminNavItems: NavItem[] = [
    { title: "Dashboard", href: "/admin", icon: LayoutDashboard },
    { title: "Auctions", href: "/admin/auctions", icon: Gavel },
    { title: "Categories", href: "/admin/categories", icon: FolderTree },
    { title: "Users", href: "/admin/users", icon: Users },
    { title: "Reports", href: "/admin/reports", icon: Flag },
    { title: "Payments", href: "/admin/payments", icon: CreditCard },
    { title: "Notifications", href: "/admin/notifications", icon: Bell },
    { title: "Audit Logs", href: "/admin/audit-logs", icon: FileText },
    { title: "Settings", href: "/admin/settings", icon: Settings },
];

interface SidebarContentProps {
    pathname: string;
    username?: string;
    collapsed?: boolean;
    onToggleCollapse?: () => void;
}

function SidebarContent({ pathname, username, collapsed = false, onToggleCollapse }: SidebarContentProps) {
    return (
        <div className="flex flex-col h-full">
            <div className={cn(
                "flex items-center border-b border-zinc-200 dark:border-zinc-800 h-16",
                collapsed ? "justify-center px-2" : "justify-between px-4"
            )}>
                {!collapsed && (
                    <div className="flex items-center gap-3">
                        <div className="w-9 h-9 rounded-lg bg-gradient-to-br from-red-500 to-red-600 flex items-center justify-center shadow-sm">
                            <Shield className="h-5 w-5 text-white" />
                        </div>
                        <div>
                            <p className="font-bold text-sm text-zinc-900 dark:text-white">
                                Admin Panel
                            </p>
                            <p className="text-xs text-zinc-500">
                                {username || "Administrator"}
                            </p>
                        </div>
                    </div>
                )}
                {collapsed && (
                    <div className="w-9 h-9 rounded-lg bg-gradient-to-br from-red-500 to-red-600 flex items-center justify-center shadow-sm">
                        <Shield className="h-5 w-5 text-white" />
                    </div>
                )}
                {onToggleCollapse && !collapsed && (
                    <Button
                        variant="ghost"
                        size="icon"
                        className="h-8 w-8 hidden lg:flex"
                        onClick={onToggleCollapse}
                    >
                        <PanelLeftClose className="h-4 w-4" />
                    </Button>
                )}
            </div>

            <ScrollArea className="flex-1 py-4">
                <nav className={cn("space-y-1", collapsed ? "px-2" : "px-3")}>
                    <TooltipProvider delayDuration={0}>
                        {adminNavItems.map((item) => {
                            const isActive =
                                item.href === "/admin"
                                    ? pathname === "/admin"
                                    : pathname.startsWith(item.href);

                            const linkContent = (
                                <Link
                                    key={item.href}
                                    href={item.disabled ? "#" : item.href}
                                    className={cn(
                                        "flex items-center gap-3 rounded-lg text-sm font-medium transition-all",
                                        collapsed ? "justify-center px-2 py-2.5" : "px-3 py-2.5",
                                        item.disabled && "opacity-50 cursor-not-allowed",
                                        isActive
                                            ? "bg-red-500/10 text-red-600 dark:text-red-400 shadow-sm"
                                            : "text-zinc-600 dark:text-zinc-400 hover:bg-zinc-100 dark:hover:bg-zinc-800 hover:text-zinc-900 dark:hover:text-white"
                                    )}
                                >
                                    <item.icon className={cn("h-5 w-5 flex-shrink-0", isActive && "text-red-500")} />
                                    {!collapsed && (
                                        <>
                                            <span className="flex-1">{item.title}</span>
                                            {isActive && (
                                                <ChevronRight className="h-4 w-4" />
                                            )}
                                        </>
                                    )}
                                </Link>
                            );

                            if (collapsed) {
                                return (
                                    <Tooltip key={item.href}>
                                        <TooltipTrigger asChild>
                                            {linkContent}
                                        </TooltipTrigger>
                                        <TooltipContent side="right" className="font-medium">
                                            {item.title}
                                        </TooltipContent>
                                    </Tooltip>
                                );
                            }

                            return linkContent;
                        })}
                    </TooltipProvider>
                </nav>
            </ScrollArea>

            <div className={cn(
                "border-t border-zinc-200 dark:border-zinc-800",
                collapsed ? "p-2" : "p-3"
            )}>
                {collapsed ? (
                    <TooltipProvider delayDuration={0}>
                        <Tooltip>
                            <TooltipTrigger asChild>
                                <Button
                                    variant="ghost"
                                    size="icon"
                                    className="w-full h-10"
                                    onClick={onToggleCollapse}
                                >
                                    <PanelLeft className="h-4 w-4" />
                                </Button>
                            </TooltipTrigger>
                            <TooltipContent side="right">
                                Expand sidebar
                            </TooltipContent>
                        </Tooltip>
                    </TooltipProvider>
                ) : (
                    <div className="text-xs text-zinc-500 text-center">
                        <p>Auction Platform v1.0</p>
                    </div>
                )}
            </div>
        </div>
    );
}

function AdminHeader({ onMobileMenuClick }: { onMobileMenuClick: () => void }) {
    const { data: session } = useSession();
    const router = useRouter();
    const { theme, setTheme } = useTheme();

    const handleSignOut = async () => {
        await signOut({ redirect: false });
        router.push("/");
    };

    const initials = session?.user?.name
        ?.split(" ")
        .map((n) => n[0])
        .join("")
        .toUpperCase()
        .slice(0, 2) || "AD";

    return (
        <header className="h-16 border-b border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-950 flex items-center justify-between px-4 lg:px-6">
            <div className="flex items-center gap-4">
                <Button
                    variant="ghost"
                    size="icon"
                    className="lg:hidden"
                    onClick={onMobileMenuClick}
                >
                    <Menu className="h-5 w-5" />
                </Button>

                <div className="hidden md:flex items-center gap-2 bg-zinc-100 dark:bg-zinc-800 rounded-lg px-3 py-1.5 w-64">
                    <Search className="h-4 w-4 text-zinc-500" />
                    <Input
                        type="text"
                        placeholder="Search..."
                        className="border-0 bg-transparent h-7 p-0 focus-visible:ring-0 focus-visible:ring-offset-0 placeholder:text-zinc-500"
                    />
                </div>
            </div>

            <div className="flex items-center gap-2">
                <Button
                    variant="ghost"
                    size="icon"
                    asChild
                    className="text-zinc-600 dark:text-zinc-400"
                >
                    <Link href="/">
                        <Home className="h-5 w-5" />
                    </Link>
                </Button>

                <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => setTheme(theme === "dark" ? "light" : "dark")}
                    className="text-zinc-600 dark:text-zinc-400"
                >
                    <Sun className="h-5 w-5 rotate-0 scale-100 transition-all dark:-rotate-90 dark:scale-0" />
                    <Moon className="absolute h-5 w-5 rotate-90 scale-0 transition-all dark:rotate-0 dark:scale-100" />
                </Button>

                <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                        <Button variant="ghost" className="relative h-9 w-9 rounded-full">
                            <Avatar className="h-9 w-9">
                                <AvatarFallback className="bg-red-500/10 text-red-600 font-semibold">
                                    {initials}
                                </AvatarFallback>
                            </Avatar>
                        </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end" className="w-56">
                        <DropdownMenuLabel>
                            <div className="flex flex-col space-y-1">
                                <p className="text-sm font-medium">{session?.user?.name}</p>
                                <p className="text-xs text-zinc-500">{session?.user?.email}</p>
                            </div>
                        </DropdownMenuLabel>
                        <DropdownMenuSeparator />
                        <DropdownMenuItem asChild>
                            <Link href="/dashboard/settings" className="cursor-pointer">
                                <User className="mr-2 h-4 w-4" />
                                Profile
                            </Link>
                        </DropdownMenuItem>
                        <DropdownMenuItem asChild>
                            <Link href="/admin/settings" className="cursor-pointer">
                                <Settings className="mr-2 h-4 w-4" />
                                Settings
                            </Link>
                        </DropdownMenuItem>
                        <DropdownMenuSeparator />
                        <DropdownMenuItem
                            className="cursor-pointer text-red-600 focus:text-red-600"
                            onClick={handleSignOut}
                        >
                            <LogOut className="mr-2 h-4 w-4" />
                            Sign out
                        </DropdownMenuItem>
                    </DropdownMenuContent>
                </DropdownMenu>
            </div>
        </header>
    );
}

interface AdminLayoutProps {
    children: React.ReactNode;
    title?: string;
    description?: string;
}

function AdminLayoutContent({
    children,
}: AdminLayoutProps) {
    const pathname = usePathname();
    const { data: session } = useSession();
    const [mobileOpen, setMobileOpen] = useState(false);
    const [sidebarCollapsed, setSidebarCollapsed] = useState(false);

    return (
        <div className="h-screen flex flex-col bg-zinc-50 dark:bg-zinc-900">
            <div className="flex flex-1 overflow-hidden">
                <aside className={cn(
                    "hidden lg:flex lg:flex-col border-r border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-950 transition-all duration-300",
                    sidebarCollapsed ? "lg:w-16" : "lg:w-64"
                )}>
                    <SidebarContent
                        pathname={pathname}
                        username={session?.user?.name ?? undefined}
                        collapsed={sidebarCollapsed}
                        onToggleCollapse={() => setSidebarCollapsed(!sidebarCollapsed)}
                    />
                </aside>

                <Sheet open={mobileOpen} onOpenChange={setMobileOpen}>
                    <SheetContent side="left" className="w-64 p-0">
                        <SidebarContent
                            pathname={pathname}
                            username={session?.user?.name ?? undefined}
                        />
                    </SheetContent>
                </Sheet>

                <div className="flex-1 flex flex-col min-w-0">
                    <AdminHeader onMobileMenuClick={() => setMobileOpen(true)} />

                    <main className="flex-1 overflow-auto">
                        <div className="h-full">
                            {children}
                        </div>
                    </main>
                </div>
            </div>
        </div>
    );
}

export function AdminLayout(props: AdminLayoutProps) {
    return (
        <RequireAdmin>
            <AdminLayoutContent {...props} />
        </RequireAdmin>
    );
}
