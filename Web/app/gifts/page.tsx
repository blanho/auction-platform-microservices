"use client";

import { Suspense } from "react";
import Link from "next/link";
import { MainLayout } from "@/components/layout/main-layout";
import { AuctionList } from "@/features/auction/auction-list";
import { AuctionProvider } from "@/context/auction.context";
import { LoadingSpinner } from "@/components/common/loading-spinner";
import { Button } from "@/components/ui/button";
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
import { faGift, faHeart, faStar, faRibbon, faBoxOpen } from "@fortawesome/free-solid-svg-icons";

function GiftsContent() {
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
                                <BreadcrumbPage>Gift Ideas</BreadcrumbPage>
                            </BreadcrumbItem>
                        </BreadcrumbList>
                    </Breadcrumb>

                    <div className="flex items-center justify-between">
                        <div className="flex items-center gap-3">
                            <div className="p-3 rounded-xl bg-linear-to-br from-pink-500 to-rose-500">
                                <FontAwesomeIcon icon={faGift} className="w-6 h-6 text-white" />
                            </div>
                            <div>
                                <h1 className="text-3xl font-bold flex items-center gap-2">
                                    Gift Ideas
                                </h1>
                                <p className="text-muted-foreground">
                                    Perfect gifts for your loved ones
                                </p>
                            </div>
                        </div>
                    </div>

                    <div className="p-6 rounded-2xl bg-linear-to-r from-pink-50 via-rose-50 to-red-50 dark:from-pink-950/20 dark:via-rose-950/20 dark:to-red-950/20 border border-pink-100 dark:border-pink-900">
                        <div className="flex flex-col md:flex-row items-center gap-6">
                            <div className="flex items-center justify-center w-20 h-20 rounded-2xl bg-linear-to-br from-pink-500 to-rose-500 shadow-lg">
                                <FontAwesomeIcon icon={faGift} className="w-10 h-10 text-white" />
                            </div>
                            <div className="flex-1 text-center md:text-left">
                                <h2 className="text-xl font-bold text-pink-600 dark:text-pink-400 mb-1">
                                    Find the Perfect Gift
                                </h2>
                                <p className="text-muted-foreground">
                                    Browse unique and special items that make perfect gifts for birthdays, holidays, and special occasions.
                                </p>
                            </div>
                            <Button className="bg-linear-to-r from-pink-500 to-rose-500 hover:from-pink-600 hover:to-rose-600">
                                <FontAwesomeIcon icon={faHeart} className="w-4 h-4 mr-2" />
                                View Gift Guide
                            </Button>
                        </div>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-4">
                        <div className="p-4 rounded-xl bg-linear-to-r from-pink-50 to-rose-50 dark:from-pink-950/20 dark:to-rose-950/20 border border-pink-100 dark:border-pink-900">
                            <div className="flex items-center gap-3">
                                <FontAwesomeIcon icon={faHeart} className="w-5 h-5 text-pink-500" />
                                <div>
                                    <p className="font-semibold text-pink-600 dark:text-pink-400">For Her</p>
                                    <p className="text-sm text-muted-foreground">Jewelry & More</p>
                                </div>
                            </div>
                        </div>
                        <div className="p-4 rounded-xl bg-linear-to-r from-blue-50 to-indigo-50 dark:from-blue-950/20 dark:to-indigo-950/20 border border-blue-100 dark:border-blue-900">
                            <div className="flex items-center gap-3">
                                <FontAwesomeIcon icon={faStar} className="w-5 h-5 text-blue-500" />
                                <div>
                                    <p className="font-semibold text-blue-600 dark:text-blue-400">For Him</p>
                                    <p className="text-sm text-muted-foreground">Watches & Tech</p>
                                </div>
                            </div>
                        </div>
                        <div className="p-4 rounded-xl bg-linear-to-r from-amber-50 to-yellow-50 dark:from-amber-950/20 dark:to-yellow-950/20 border border-amber-100 dark:border-amber-900">
                            <div className="flex items-center gap-3">
                                <FontAwesomeIcon icon={faRibbon} className="w-5 h-5 text-amber-500" />
                                <div>
                                    <p className="font-semibold text-amber-600 dark:text-amber-400">Collectibles</p>
                                    <p className="text-sm text-muted-foreground">Rare Finds</p>
                                </div>
                            </div>
                        </div>
                        <div className="p-4 rounded-xl bg-linear-to-r from-green-50 to-emerald-50 dark:from-green-950/20 dark:to-emerald-950/20 border border-green-100 dark:border-green-900">
                            <div className="flex items-center gap-3">
                                <FontAwesomeIcon icon={faBoxOpen} className="w-5 h-5 text-green-500" />
                                <div>
                                    <p className="font-semibold text-green-600 dark:text-green-400">Under $100</p>
                                    <p className="text-sm text-muted-foreground">Budget Friendly</p>
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

export default function GiftsPage() {
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
            <GiftsContent />
        </Suspense>
    );
}
