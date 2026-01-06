"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useAuth } from "@repo/hooks";
import { Button, Separator } from "@repo/ui";
import {
  LayoutDashboard,
  Gavel,
  Trophy,
  Bookmark,
  Wallet,
  Package,
  Bell,
  Settings,
  LogOut,
  Store,
} from "lucide-react";

const sidebarItems = [
  { href: "/dashboard", label: "Overview", icon: LayoutDashboard },
  { href: "/dashboard/bids", label: "My Bids", icon: Gavel },
  { href: "/dashboard/won", label: "Won Auctions", icon: Trophy },
  { href: "/dashboard/bookmarks", label: "Bookmarks", icon: Bookmark },
  { href: "/dashboard/wallet", label: "Wallet", icon: Wallet },
  { href: "/dashboard/orders", label: "Orders", icon: Package },
  { href: "/dashboard/notifications", label: "Notifications", icon: Bell },
  { href: "/dashboard/settings", label: "Settings", icon: Settings },
];

export default function DashboardLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const pathname = usePathname();
  const { user, logout, isSeller } = useAuth();

  return (
    <div className="min-h-screen">
      <header className="sticky top-0 z-50 w-full border-b bg-background/95 backdrop-blur">
        <div className="container flex h-16 items-center justify-between">
          <Link href="/" className="text-xl font-bold">
            Auction Platform
          </Link>
          <nav className="flex items-center gap-4">
            <Link href="/auctions" className="text-sm font-medium hover:underline">
              Browse
            </Link>
            <span className="text-sm text-muted-foreground">
              {user?.userName || user?.email}
            </span>
          </nav>
        </div>
      </header>

      <div className="container flex gap-8 py-8">
        <aside className="hidden w-64 flex-shrink-0 lg:block">
          <nav className="space-y-1">
            {sidebarItems.map((item) => {
              const Icon = item.icon;
              const isActive = pathname === item.href;
              return (
                <Link
                  key={item.href}
                  href={item.href}
                  className={`flex items-center gap-3 rounded-lg px-3 py-2 text-sm transition-colors ${
                    isActive
                      ? "bg-primary text-primary-foreground"
                      : "hover:bg-muted"
                  }`}
                >
                  <Icon className="h-4 w-4" />
                  {item.label}
                </Link>
              );
            })}

            <Separator className="my-4" />

            {isSeller ? (
              <Link
                href="/seller"
                className="flex items-center gap-3 rounded-lg px-3 py-2 text-sm hover:bg-muted"
              >
                <Store className="h-4 w-4" />
                Seller Dashboard
              </Link>
            ) : (
              <Link
                href="/seller/become-seller"
                className="flex items-center gap-3 rounded-lg px-3 py-2 text-sm hover:bg-muted"
              >
                <Store className="h-4 w-4" />
                Become a Seller
              </Link>
            )}

            <button
              onClick={() => logout()}
              className="flex w-full items-center gap-3 rounded-lg px-3 py-2 text-sm text-destructive hover:bg-destructive/10"
            >
              <LogOut className="h-4 w-4" />
              Sign Out
            </button>
          </nav>
        </aside>

        <main className="flex-1 min-w-0">{children}</main>
      </div>
    </div>
  );
}
