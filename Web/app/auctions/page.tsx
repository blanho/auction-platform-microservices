import { Suspense } from 'react';
import Link from 'next/link';
import { MainLayout } from "@/components/layout/main-layout";
import { AuctionList } from "@/features/auction/auction-list";
import { AuctionProvider } from "@/context/auction.context";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faGavel, faHome, faChevronRight, faPlus } from "@fortawesome/free-solid-svg-icons";
import { Button } from "@/components/ui/button";

function AuctionsContent() {
    return (
        <AuctionProvider>
            <MainLayout>
                <div className="space-y-8">
                    <nav className="flex items-center gap-2 text-sm">
                        <Link 
                            href="/" 
                            className="flex items-center gap-1.5 text-slate-500 dark:text-slate-400 hover:text-purple-600 dark:hover:text-purple-400 transition-colors"
                        >
                            <FontAwesomeIcon icon={faHome} className="w-3.5 h-3.5" />
                            Home
                        </Link>
                        <FontAwesomeIcon icon={faChevronRight} className="w-3 h-3 text-slate-400" />
                        <span className="font-medium text-slate-900 dark:text-white">Auctions</span>
                    </nav>

                    <div className="flex flex-col sm:flex-row sm:items-end justify-between gap-4">
                        <div className="space-y-2">
                            <div className="flex items-center gap-3">
                                <div className="w-12 h-12 rounded-xl bg-gradient-to-br from-purple-500 to-blue-600 flex items-center justify-center shadow-lg shadow-purple-500/25">
                                    <FontAwesomeIcon icon={faGavel} className="w-5 h-5 text-white" />
                                </div>
                                <div>
                                    <h1 className="text-3xl md:text-4xl font-bold text-slate-900 dark:text-white">
                                        Auctions
                                    </h1>
                                    <p className="text-slate-500 dark:text-slate-400">
                                        Discover and bid on amazing items
                                    </p>
                                </div>
                            </div>
                        </div>

                        <Button asChild className="gap-2 bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700 shadow-lg shadow-purple-500/25 h-11 px-6">
                            <Link href="/auctions/create">
                                <FontAwesomeIcon icon={faPlus} className="w-4 h-4" />
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
            <div className="flex flex-col items-center justify-center min-h-[400px] gap-4">
                <div className="relative">
                    <div className="w-16 h-16 rounded-full bg-gradient-to-r from-purple-500 to-blue-500 animate-spin" style={{ animationDuration: '1.5s' }}>
                        <div className="absolute inset-1 bg-white dark:bg-slate-950 rounded-full" />
                    </div>
                    <div className="absolute inset-0 flex items-center justify-center">
                        <FontAwesomeIcon icon={faGavel} className="w-6 h-6 text-purple-600" />
                    </div>
                </div>
                <p className="text-slate-500 dark:text-slate-400 text-sm">Loading auctions...</p>
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
