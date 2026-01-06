import Link from "next/link";
import { Button } from "@repo/ui";

export default function HomePage() {
  return (
    <div className="flex min-h-screen flex-col">
      <header className="sticky top-0 z-50 w-full border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
        <div className="container flex h-16 items-center justify-between">
          <Link href="/" className="text-xl font-bold">
            Auction Platform
          </Link>
          <nav className="flex items-center gap-4">
            <Link href="/auctions" className="text-sm font-medium hover:underline">
              Browse
            </Link>
            <Link href="/login">
              <Button variant="outline" size="sm">
                Sign In
              </Button>
            </Link>
            <Link href="/register">
              <Button size="sm">Get Started</Button>
            </Link>
          </nav>
        </div>
      </header>

      <main className="flex-1">
        <section className="container py-24 text-center">
          <h1 className="text-4xl font-bold tracking-tight sm:text-6xl">
            Buy & Sell with
            <span className="text-primary"> Live Auctions</span>
          </h1>
          <p className="mx-auto mt-6 max-w-2xl text-lg text-muted-foreground">
            Discover unique items, place bids in real-time, and win amazing deals.
            Join thousands of buyers and sellers on our trusted auction platform.
          </p>
          <div className="mt-10 flex items-center justify-center gap-4">
            <Link href="/auctions">
              <Button size="lg">Browse Auctions</Button>
            </Link>
            <Link href="/register">
              <Button variant="outline" size="lg">
                Start Selling
              </Button>
            </Link>
          </div>
        </section>

        <section className="container py-16">
          <h2 className="mb-8 text-center text-2xl font-bold">How It Works</h2>
          <div className="grid gap-8 md:grid-cols-3">
            <div className="text-center">
              <div className="mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-primary text-primary-foreground">
                1
              </div>
              <h3 className="mb-2 font-semibold">Browse & Discover</h3>
              <p className="text-sm text-muted-foreground">
                Explore thousands of auctions across various categories
              </p>
            </div>
            <div className="text-center">
              <div className="mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-primary text-primary-foreground">
                2
              </div>
              <h3 className="mb-2 font-semibold">Place Your Bid</h3>
              <p className="text-sm text-muted-foreground">
                Bid in real-time and track auctions as they happen
              </p>
            </div>
            <div className="text-center">
              <div className="mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-primary text-primary-foreground">
                3
              </div>
              <h3 className="mb-2 font-semibold">Win & Receive</h3>
              <p className="text-sm text-muted-foreground">
                Secure checkout and fast delivery to your door
              </p>
            </div>
          </div>
        </section>
      </main>

      <footer className="border-t py-8">
        <div className="container text-center text-sm text-muted-foreground">
          © 2024 Auction Platform. All rights reserved.
        </div>
      </footer>
    </div>
  );
}
