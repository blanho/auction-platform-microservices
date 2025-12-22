"use client";

import { Suspense, useState } from "react";
import Link from "next/link";
import { MainLayout } from "@/components/layout/main-layout";
import { AuctionList } from "@/features/auction/auction-list";
import { AuctionProvider } from "@/context/auction.context";
import { LoadingSpinner } from "@/components/common/loading-spinner";
import { Badge } from "@/components/ui/badge";
import { ROUTES } from "@/constants/routes";
import {
    Breadcrumb,
    BreadcrumbItem,
    BreadcrumbLink,
    BreadcrumbList,
    BreadcrumbPage,
    BreadcrumbSeparator,
} from "@/components/ui/breadcrumb";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faBolt, faGlobe, faUsers } from "@fortawesome/free-solid-svg-icons";

function LiveContent() {
    const [totalBidders] = useState(() => Math.floor(Math.random() * 500) + 100);

    return (
        <AuctionProvider>
            <MainLayout>
                <div className="space-y-6">
                    <Breadcrumb>
                        <BreadcrumbList>
                            <BreadcrumbItem>
                                <BreadcrumbLink asChild>
                                    <Link href={ROUTES.HOME}>Home</Link>
                                </BreadcrumbLink>
                            </BreadcrumbItem>
                            <BreadcrumbSeparator />
                            <BreadcrumbItem>
                                <BreadcrumbPage>Live Auctions</BreadcrumbPage>
                            </BreadcrumbItem>
                        </BreadcrumbList>
                    </Breadcrumb>

                    <div className="flex items-center justify-between">
                        <div className="flex items-center gap-3">
                            <div className="p-3 rounded-xl bg-linear-to-br from-green-500 to-emerald-500 animate-pulse">
                                <FontAwesomeIcon icon={faBolt} className="w-6 h-6 text-white" />
                            </div>
                            <div>
                                <h1 className="text-3xl font-bold flex items-center gap-2">
                                    Live Auctions
                                    <Badge className="bg-green-500 animate-pulse">Live</Badge>
                                </h1>
                                <p className="text-muted-foreground">
                                    Real-time auctions happening right now
                                </p>
                            </div>
                        </div>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
                        <div className="p-4 rounded-xl bg-linear-to-r from-green-50 to-emerald-50 dark:from-green-950/20 dark:to-emerald-950/20 border border-green-100 dark:border-green-900">
                            <div className="flex items-center gap-3">
                                <FontAwesomeIcon icon={faBolt} className="w-5 h-5 text-green-500" />
                                <div>
                                    <p className="font-semibold text-green-600 dark:text-green-400">Happening Now</p>
                                    <p className="text-sm text-muted-foreground">Active auctions</p>
                                </div>
                            </div>
                        </div>
                        <div className="p-4 rounded-xl bg-linear-to-r from-blue-50 to-indigo-50 dark:from-blue-950/20 dark:to-indigo-950/20 border border-blue-100 dark:border-blue-900">
                            <div className="flex items-center gap-3">
                                <FontAwesomeIcon icon={faUsers} className="w-5 h-5 text-blue-500" />
                                <div>
                                    <p className="font-semibold text-blue-600 dark:text-blue-400">{totalBidders}+</p>
                                    <p className="text-sm text-muted-foreground">Active bidders</p>
                                </div>
                            </div>
                        </div>
                        <div className="p-4 rounded-xl bg-linear-to-r from-purple-50 to-pink-50 dark:from-purple-950/20 dark:to-pink-950/20 border border-purple-100 dark:border-purple-900">
                            <div className="flex items-center gap-3">
                                <FontAwesomeIcon icon={faGlobe} className="w-5 h-5 text-purple-500" />
                                <div>
                                    <p className="font-semibold text-purple-600 dark:text-purple-400">Real-time</p>
                                    <p className="text-sm text-muted-foreground">Updates every second</p>
                                </div>
                            </div>
                        </div>
                    </div>

                    <AuctionList />
                </div>
            </MainLayout>
        </AuctionProvider>
    );
}

export default function LivePage() {
    return (
        <Suspense
            fallback={
                <MainLayout>
                    <div className="flex items-center justify-center min-h-[400px]">
                        <LoadingSpinner size="lg" />
                    </div>
                </MainLayout>
            }
        >
            <LiveContent />
        </Suspense>
    );
}
