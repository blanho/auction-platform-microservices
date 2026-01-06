"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useAuth } from "@repo/hooks";
import { Button, Separator } from "@repo/ui";
import {
  LayoutDashboard,
  Package,
  Plus,
  ShoppingCart,
  BarChart3,
  ArrowLeft,
  LogOut,
} from "lucide-react";

const sidebarItems = [
  { href: "/seller", label: "Overview", icon: LayoutDashboard },
  { href: "/seller/auctions", label: "My Auctions", icon: Package },
  { href: "/seller/auctions/new", label: "Create Auction", icon: Plus },
  { href: "/seller/orders", label: "Orders", icon: ShoppingCart },
  { href: "/seller/analytics", label: "Analytics", icon: BarChart3 },
];

export default function SellerLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const pathname = usePathname();
  const { user, logout } = useAuth();

  return (
    <div className="min-h-screen">
      <header className="sticky top-0 z-50 w-full border-b bg-background/95 backdrop-blur">
        <div className="container flex h-16 items-center justify-between">
          <Link href="/" className="text-xl font-bold">
            Auction Platform
          </Link>
          <nav className="flex items-center gap-4">
            <span className="text-sm text-muted-foreground">Seller Dashboard</span>
            <span className="text-sm">{user?.userName}</span>
          </nav>
        </div>
      </header>

      <div className="container flex gap-8 py-8">
        <aside className="hidden w-64 flex-shrink-0 lg:block">
          <nav className="space-y-1">
            {sidebarItems.map((item) => {
              const Icon = item.icon;
              const isActive =
                item.href === "/seller"
                  ? pathname === "/seller"
                  : pathname.startsWith(item.href);
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

            <Link
              href="/dashboard"
              className="flex items-center gap-3 rounded-lg px-3 py-2 text-sm hover:bg-muted"
            >
              <ArrowLeft className="h-4 w-4" />
              Back to Dashboard
            </Link>

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
