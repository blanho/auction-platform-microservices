// Auctions page
import { Suspense } from 'react';
import Link from 'next/link';
import { Plus } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { MainLayout } from "@/components/layout/main-layout";
import { AuctionList } from "@/features/auction/auction-list";
import { AuctionProvider } from "@/context/auction.context";
import { LoadingSpinner } from "@/components/common/loading-spinner";
import {
    Breadcrumb,
    BreadcrumbItem,
    BreadcrumbLink,
    BreadcrumbList,
    BreadcrumbPage,
    BreadcrumbSeparator,
} from "@/components/ui/breadcrumb";

function AuctionsContent() {
    return (
        <AuctionProvider>
            <MainLayout>
                <div className="space-y-6">
                    <Breadcrumb>
                        <BreadcrumbList>
                            <BreadcrumbItem>
                                <BreadcrumbLink asChild>
                                    <Link href="/">Home</Link>
                                </BreadcrumbLink>
                            </BreadcrumbItem>
                            <BreadcrumbSeparator />
                            <BreadcrumbItem>
                                <BreadcrumbPage>Auctions</BreadcrumbPage>
                            </BreadcrumbItem>
                        </BreadcrumbList>
                    </Breadcrumb>
                    <div className="flex items-center justify-between">
                        <div>
                            <h1 className="text-3xl font-bold">Auctions</h1>
                            <p className="text-muted-foreground">
                                Browse all available auctions
                            </p>
                        </div>
                        <Button asChild>
                            <Link href="/auctions/create">
                                <Plus className="mr-2 h-4 w-4" />
                                Create Auction
                            </Link>
                        </Button>
                    </div>
                    <AuctionList />
                </div>
            </MainLayout>
        </AuctionProvider>
    );
}

function AuctionsLoading() {
    return (
        <MainLayout>
            <div className="flex items-center justify-center min-h-[400px]">
                <LoadingSpinner size="lg" />
            </div>
        </MainLayout>
    );
}

export default function AuctionsPage() {
    return (
        <Suspense fallback={<AuctionsLoading />}>
            <AuctionsContent />
        </Suspense>
    );
}
