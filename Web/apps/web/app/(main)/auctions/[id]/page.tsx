import { Suspense } from "react";
import Link from "next/link";
import { notFound } from "next/navigation";
import { auctionApi } from "@repo/api-client";
import { Skeleton } from "@repo/ui";
import { AuctionDetail } from "@/components/auction/auction-detail";
import { BidSection } from "@/components/bid/bid-section";

interface PageProps {
  params: Promise<{ id: string }>;
}

export async function generateMetadata({ params }: PageProps) {
  const { id } = await params;
  try {
    const auction = await auctionApi.getById(id);
    return {
      title: auction.title,
      description: auction.description.slice(0, 160),
    };
  } catch {
    return { title: "Auction Not Found" };
  }
}

export default async function AuctionDetailPage({ params }: PageProps) {
  const { id } = await params;
  
  let auction;
  try {
    auction = await auctionApi.getById(id);
  } catch {
    notFound();
  }

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
            <Link href="/login" className="text-sm font-medium hover:underline">
              Sign In
            </Link>
          </nav>
        </div>
      </header>

      <main className="container py-8">
        <div className="mb-4">
          <Link href="/auctions" className="text-sm text-muted-foreground hover:underline">
            ← Back to Auctions
          </Link>
        </div>

        <div className="grid gap-8 lg:grid-cols-3">
          <div className="lg:col-span-2">
            <AuctionDetail auction={auction} />
          </div>
          <div className="lg:col-span-1">
            <Suspense fallback={<BidSectionSkeleton />}>
              <BidSection auctionId={auction.id} initialData={auction} />
            </Suspense>
          </div>
        </div>
      </main>
    </div>
  );
}

function BidSectionSkeleton() {
  return (
    <div className="space-y-4">
      <Skeleton className="h-32" />
      <Skeleton className="h-48" />
    </div>
  );
}
