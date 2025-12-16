"use client";

import { useEffect, useState, useCallback } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import { MainLayout } from "@/components/layout/main-layout";
import { LoadingSpinner } from "@/components/common/loading-spinner";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Progress } from "@/components/ui/progress";
import {
    Breadcrumb,
    BreadcrumbItem,
    BreadcrumbLink,
    BreadcrumbList,
    BreadcrumbPage,
    BreadcrumbSeparator,
} from "@/components/ui/breadcrumb";
import { Star, Package, Gavel, Calendar, CheckCircle, Shield } from "lucide-react";
import { ROUTES } from "@/constants/routes";
import { reviewService, UserRatingSummary, Review, getRatingColor, getRatingLabel } from "@/services/review.service";
import { auctionService } from "@/services/auction.service";
import { AuctionCard } from "@/features/auction/auction-card";
import { Auction } from "@/types/auction";
import { formatRelativeTime } from "@/utils";

export default function UserProfilePage() {
    const params = useParams();
    const username = params?.username as string;
    const [ratingSummary, setRatingSummary] = useState<UserRatingSummary | null>(null);
    const [reviews, setReviews] = useState<Review[]>([]);
    const [auctions, setAuctions] = useState<Auction[]>([]);
    const [isLoading, setIsLoading] = useState(true);

    const fetchData = useCallback(async () => {
        if (!username) return;

        try {
            const [summaryData, reviewsData, auctionsData] = await Promise.all([
                reviewService.getUserRatingSummary(username),
                reviewService.getReviewsForUser(username),
                auctionService.getAuctions({ sellerUsername: username, pageSize: 8 }),
            ]);

            setRatingSummary(summaryData);
            setReviews(reviewsData);
            setAuctions(auctionsData.items);
        } catch (error) {\n        } finally {
            setIsLoading(false);
        }
    }, [username]);

    useEffect(() => {
        fetchData();
    }, [fetchData]);

    if (isLoading) {
        return (
            <MainLayout>
                <div className="flex items-center justify-center min-h-[400px]">
                    <LoadingSpinner size="lg" />
                </div>
            </MainLayout>
        );
    }

    const initials = username?.slice(0, 2).toUpperCase() || "U";

    return (
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
                            <BreadcrumbPage>User Profile</BreadcrumbPage>
                        </BreadcrumbItem>
                    </BreadcrumbList>
                </Breadcrumb>

                <div className="grid gap-6 lg:grid-cols-3">
                    <Card className="lg:col-span-1">
                        <CardContent className="pt-6">
                            <div className="flex flex-col items-center text-center">
                                <Avatar className="h-24 w-24 mb-4">
                                    <AvatarFallback className="text-2xl bg-amber-500 text-white">
                                        {initials}
                                    </AvatarFallback>
                                </Avatar>
                                <h1 className="text-2xl font-bold">{username}</h1>
                                <div className="flex items-center gap-2 mt-2">
                                    <Badge variant="secondary" className="flex items-center gap-1">
                                        <Shield className="h-3 w-3" />
                                        Verified Seller
                                    </Badge>
                                </div>

                                {ratingSummary && ratingSummary.totalReviews > 0 && (
                                    <div className="mt-4 space-y-2 w-full">
                                        <div className="flex items-center justify-center gap-2">
                                            <Star className={`h-6 w-6 fill-current ${getRatingColor(ratingSummary.averageRating)}`} />
                                            <span className={`text-2xl font-bold ${getRatingColor(ratingSummary.averageRating)}`}>
                                                {ratingSummary.averageRating.toFixed(1)}
                                            </span>
                                        </div>
                                        <p className="text-sm text-muted-foreground">
                                            {getRatingLabel(ratingSummary.averageRating)} · {ratingSummary.totalReviews} reviews
                                        </p>
                                        <p className="text-sm text-green-600">
                                            {ratingSummary.positivePercentage}% positive feedback
                                        </p>
                                    </div>
                                )}

                                <div className="grid grid-cols-2 gap-4 mt-6 w-full">
                                    <div className="text-center p-3 bg-zinc-50 dark:bg-zinc-900 rounded-lg">
                                        <Package className="h-5 w-5 mx-auto mb-1 text-muted-foreground" />
                                        <p className="text-lg font-semibold">{auctions.length}</p>
                                        <p className="text-xs text-muted-foreground">Listings</p>
                                    </div>
                                    <div className="text-center p-3 bg-zinc-50 dark:bg-zinc-900 rounded-lg">
                                        <CheckCircle className="h-5 w-5 mx-auto mb-1 text-muted-foreground" />
                                        <p className="text-lg font-semibold">{ratingSummary?.totalReviews || 0}</p>
                                        <p className="text-xs text-muted-foreground">Sales</p>
                                    </div>
                                </div>
                            </div>
                        </CardContent>
                    </Card>

                    <div className="lg:col-span-2 space-y-6">
                        {ratingSummary && ratingSummary.totalReviews > 0 && (
                            <Card>
                                <CardHeader>
                                    <CardTitle>Rating Breakdown</CardTitle>
                                </CardHeader>
                                <CardContent>
                                    <div className="space-y-3">
                                        {[5, 4, 3, 2, 1].map((stars) => {
                                            const count = ratingSummary[`${['one', 'two', 'three', 'four', 'five'][stars - 1]}StarCount` as keyof UserRatingSummary] as number;
                                            const percentage = ratingSummary.totalReviews > 0
                                                ? (count / ratingSummary.totalReviews) * 100
                                                : 0;

                                            return (
                                                <div key={stars} className="flex items-center gap-3">
                                                    <span className="w-12 text-sm">{stars} star</span>
                                                    <Progress value={percentage} className="flex-1 h-2" />
                                                    <span className="w-8 text-sm text-muted-foreground">{count}</span>
                                                </div>
                                            );
                                        })}
                                    </div>
                                </CardContent>
                            </Card>
                        )}

                        <Tabs defaultValue="listings">
                            <TabsList>
                                <TabsTrigger value="listings" className="flex items-center gap-2">
                                    <Gavel className="h-4 w-4" />
                                    Listings
                                </TabsTrigger>
                                <TabsTrigger value="reviews" className="flex items-center gap-2">
                                    <Star className="h-4 w-4" />
                                    Reviews ({reviews.length})
                                </TabsTrigger>
                            </TabsList>

                            <TabsContent value="listings" className="mt-4">
                                {auctions.length === 0 ? (
                                    <Card>
                                        <CardContent className="py-12 text-center">
                                            <Gavel className="h-12 w-12 mx-auto text-muted-foreground mb-4" />
                                            <p className="text-muted-foreground">No active listings</p>
                                        </CardContent>
                                    </Card>
                                ) : (
                                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                                        {auctions.map((auction) => (
                                            <AuctionCard key={auction.id} auction={auction} />
                                        ))}
                                    </div>
                                )}
                            </TabsContent>

                            <TabsContent value="reviews" className="mt-4">
                                {reviews.length === 0 ? (
                                    <Card>
                                        <CardContent className="py-12 text-center">
                                            <Star className="h-12 w-12 mx-auto text-muted-foreground mb-4" />
                                            <p className="text-muted-foreground">No reviews yet</p>
                                        </CardContent>
                                    </Card>
                                ) : (
                                    <div className="space-y-4">
                                        {reviews.map((review) => (
                                            <Card key={review.id}>
                                                <CardContent className="pt-4">
                                                    <div className="flex items-start justify-between">
                                                        <div className="flex items-center gap-3">
                                                            <Avatar className="h-10 w-10">
                                                                <AvatarFallback>
                                                                    {review.reviewerUsername.slice(0, 2).toUpperCase()}
                                                                </AvatarFallback>
                                                            </Avatar>
                                                            <div>
                                                                <p className="font-medium">{review.reviewerUsername}</p>
                                                                <div className="flex items-center gap-1">
                                                                    {Array.from({ length: 5 }).map((_, i) => (
                                                                        <Star
                                                                            key={i}
                                                                            className={`h-4 w-4 ${
                                                                                i < review.rating
                                                                                    ? "fill-amber-400 text-amber-400"
                                                                                    : "text-gray-300"
                                                                            }`}
                                                                        />
                                                                    ))}
                                                                </div>
                                                            </div>
                                                        </div>
                                                        <div className="flex items-center gap-2 text-sm text-muted-foreground">
                                                            <Calendar className="h-4 w-4" />
                                                            {formatRelativeTime(review.createdAt)}
                                                        </div>
                                                    </div>
                                                    {review.title && (
                                                        <h4 className="font-semibold mt-3">{review.title}</h4>
                                                    )}
                                                    {review.comment && (
                                                        <p className="mt-2 text-muted-foreground">{review.comment}</p>
                                                    )}
                                                    {review.isVerifiedPurchase && (
                                                        <Badge variant="outline" className="mt-3">
                                                            <CheckCircle className="h-3 w-3 mr-1" />
                                                            Verified Purchase
                                                        </Badge>
                                                    )}
                                                    {review.sellerResponse && (
                                                        <div className="mt-4 p-3 bg-zinc-50 dark:bg-zinc-900 rounded-lg">
                                                            <p className="text-sm font-medium">Seller Response:</p>
                                                            <p className="text-sm text-muted-foreground mt-1">
                                                                {review.sellerResponse}
                                                            </p>
                                                        </div>
                                                    )}
                                                </CardContent>
                                            </Card>
                                        ))}
                                    </div>
                                )}
                            </TabsContent>
                        </Tabs>
                    </div>
                </div>
            </div>
        </MainLayout>
    );
}
