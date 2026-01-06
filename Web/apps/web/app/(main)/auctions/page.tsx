import Link from "next/link";
import { Suspense } from "react";
import { AuctionList } from "@/components/auction/auction-list";
import { AuctionFilters } from "@/components/auction/auction-filters";
import { Skeleton } from "@repo/ui";

export const metadata = {
  title: "Browse Auctions",
};

interface PageProps {
  searchParams: Promise<{
    page?: string;
    category?: string;
    sortBy?: string;
    minPrice?: string;
    maxPrice?: string;
    search?: string;
  }>;
}

export default async function AuctionsPage({ searchParams }: PageProps) {
  const params = await searchParams;

  return (
    <div className="min-h-screen">
      <header className="sticky top-0 z-50 w-full border-b bg-background/95 backdrop-blur">
        <div className="container flex h-16 items-center justify-between">
          <Link href="/" className="text-xl font-bold">
            Auction Platform
          </Link>
          <nav className="flex items-center gap-4">
            <Link href="/login" className="text-sm font-medium hover:underline">
              Sign In
            </Link>
          </nav>
        </div>
      </header>

      <main className="container py-8">
        <div className="mb-8">
          <h1 className="text-3xl font-bold">Browse Auctions</h1>
          <p className="mt-2 text-muted-foreground">
            Discover unique items and place your bids
          </p>
        </div>

        <div className="grid gap-8 lg:grid-cols-4">
          <aside className="lg:col-span-1">
            <AuctionFilters />
          </aside>
          <div className="lg:col-span-3">
            <Suspense fallback={<AuctionListSkeleton />}>
              <AuctionList
                page={Number(params.page) || 1}
                categoryId={params.category}
                sortBy={params.sortBy}
                minPrice={params.minPrice ? Number(params.minPrice) : undefined}
                maxPrice={params.maxPrice ? Number(params.maxPrice) : undefined}
                searchTerm={params.search}
              />
            </Suspense>
          </div>
        </div>
      </main>
    </div>
  );
}

function AuctionListSkeleton() {
  return (
    <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
      {Array.from({ length: 6 }).map((_, i) => (
        <div key={i} className="space-y-3">
          <Skeleton className="aspect-square rounded-lg" />
          <Skeleton className="h-4 w-3/4" />
          <Skeleton className="h-4 w-1/2" />
        </div>
      ))}
    </div>
  );
}
