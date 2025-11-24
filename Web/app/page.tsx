import Link from "next/link";
import { Button } from "@/components/ui/button";
import { MainLayout } from "@/components/layout/main-layout";

export default function Home() {
  return (
    <MainLayout>
      <div className="flex flex-col items-center justify-center py-12 md:py-20">
        <div className="text-center space-y-6 max-w-3xl">
          <h1 className="text-4xl font-bold tracking-tight sm:text-5xl md:text-6xl">
            Welcome to <span className="text-primary">AuctionHub</span>
          </h1>
          <p className="text-lg text-muted-foreground md:text-xl">
            A modern auction platform built with Next.js frontend and .NET microservices backend.
            Featuring real-time bidding, distributed search, and identity management.
          </p>
          <div className="flex flex-col gap-4 sm:flex-row sm:justify-center sm:gap-6 pt-6">
            <Button size="lg" asChild>
              <Link href="/auctions">Browse Auctions</Link>
            </Button>
            <Button size="lg" variant="outline" asChild>
              <Link href="/search">Search</Link>
            </Button>
          </div>
        </div>

        <div className="mt-20 grid gap-8 md:grid-cols-3 w-full max-w-5xl">
          <div className="rounded-lg border bg-card p-6 text-card-foreground shadow-sm">
            <h3 className="text-xl font-semibold mb-2">Microservices Architecture</h3>
            <p className="text-muted-foreground">
              Built with clean architecture: AuctionService, SearchService, IdentityService, and Gateway using YARP.
            </p>
          </div>
          <div className="rounded-lg border bg-card p-6 text-card-foreground shadow-sm">
            <h3 className="text-xl font-semibold mb-2">Event-Driven Communication</h3>
            <p className="text-muted-foreground">
              RabbitMQ message broker with MassTransit for reliable asynchronous communication between services.
            </p>
          </div>
          <div className="rounded-lg border bg-card p-6 text-card-foreground shadow-sm">
            <h3 className="text-xl font-semibold mb-2">Modern Tech Stack</h3>
            <p className="text-muted-foreground">
              Next.js 15, React Query, shadcn/ui on frontend. .NET 9, PostgreSQL, MongoDB, Redis on backend.
            </p>
          </div>
        </div>
      </div>
    </MainLayout>
  );
}
