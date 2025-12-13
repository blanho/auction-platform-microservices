"use client";

import { Suspense } from "react";
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
import { faFire, faPercent, faTags } from "@fortawesome/free-solid-svg-icons";

function DealsContent() {
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
                                <BreadcrumbPage>Hot Deals</BreadcrumbPage>
                            </BreadcrumbItem>
                        </BreadcrumbList>
                    </Breadcrumb>

                    <div className="flex items-center justify-between">
                        <div className="flex items-center gap-3">
                            <div className="p-3 rounded-xl bg-linear-to-br from-red-500 to-orange-500">
                                <FontAwesomeIcon icon={faFire} className="w-6 h-6 text-white" />
                            </div>
                            <div>
                                <h1 className="text-3xl font-bold flex items-center gap-2">
                                    Hot Deals
                                    <Badge className="bg-red-500">New</Badge>
                                </h1>
                                <p className="text-muted-foreground">
                                    Best deals with the lowest starting prices
                                </p>
                            </div>
                        </div>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
                        <div className="p-4 rounded-xl bg-linear-to-r from-red-50 to-orange-50 dark:from-red-950/20 dark:to-orange-950/20 border border-red-100 dark:border-red-900">
                            <div className="flex items-center gap-3">
                                <FontAwesomeIcon icon={faPercent} className="w-5 h-5 text-red-500" />
                                <div>
                                    <p className="font-semibold text-red-600 dark:text-red-400">Up to 70% Off</p>
                                    <p className="text-sm text-muted-foreground">From starting prices</p>
                                </div>
                            </div>
                        </div>
                        <div className="p-4 rounded-xl bg-linear-to-r from-amber-50 to-yellow-50 dark:from-amber-950/20 dark:to-yellow-950/20 border border-amber-100 dark:border-amber-900">
                            <div className="flex items-center gap-3">
                                <FontAwesomeIcon icon={faTags} className="w-5 h-5 text-amber-500" />
                                <div>
                                    <p className="font-semibold text-amber-600 dark:text-amber-400">Best Value</p>
                                    <p className="text-sm text-muted-foreground">Handpicked selections</p>
                                </div>
                            </div>
                        </div>
                        <div className="p-4 rounded-xl bg-linear-to-r from-green-50 to-emerald-50 dark:from-green-950/20 dark:to-emerald-950/20 border border-green-100 dark:border-green-900">
                            <div className="flex items-center gap-3">
                                <FontAwesomeIcon icon={faFire} className="w-5 h-5 text-green-500" />
                                <div>
                                    <p className="font-semibold text-green-600 dark:text-green-400">Daily Updates</p>
                                    <p className="text-sm text-muted-foreground">Fresh deals every day</p>
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

export default function DealsPage() {
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
            <DealsContent />
        </Suspense>
    );
}
