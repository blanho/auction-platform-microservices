"use client";

import { useEffect, useState, useCallback } from "react";
import { useSession } from "next-auth/react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import Image from "next/image";
import {
    Heart,
    Loader2,
    Trash2,
    Clock,
    ExternalLink,
    AlertCircle,
} from "lucide-react";

import { formatCurrency, formatRelativeTime } from "@/utils";
import { MESSAGES } from "@/constants";

import { Button } from "@/components/ui/button";
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Header } from "@/components/layout/header";
import { Footer } from "@/components/layout/footer";
import { toast } from "sonner";

import { wishlistService, WishlistItem } from "@/services/wishlist.service";

export default function WishlistPage() {
    const { status } = useSession();
    const router = useRouter();
    const [isLoading, setIsLoading] = useState(true);
    const [wishlist, setWishlist] = useState<WishlistItem[]>([]);

    const fetchWishlist = useCallback(async () => {
        try {
            setIsLoading(true);
            const data = await wishlistService.getWishlist();
            setWishlist(data);
        } catch (error) {
            toast.error("Failed to load wishlist");
        } finally {
            setIsLoading(false);
        }
    }, []);

    useEffect(() => {
        if (status === "unauthenticated") {
            router.push("/auth/signin?callbackUrl=/wishlist");
            return;
        }

        if (status === "authenticated") {
            fetchWishlist();
        }
    }, [status, router, fetchWishlist]);

    const handleRemove = async (auctionId: string) => {
        try {
            await wishlistService.removeFromWishlist(auctionId);
            setWishlist((prev) => prev.filter((item) => item.auctionId !== auctionId));
            toast.success(MESSAGES.SUCCESS.WISHLIST_REMOVED);
        } catch (error) {
            toast.error("Failed to remove from wishlist");
        }
    };

    const getStatusColor = (status: string) => {
        switch (status.toLowerCase()) {
            case "live":
                return "bg-green-500";
            case "finished":
                return "bg-zinc-500";
            case "reservenotmet":
                return "bg-yellow-500";
            default:
                return "bg-blue-500";
        }
    };

    if (status === "loading" || isLoading) {
        return (
            <>
                <Header />
                <main className="min-h-screen bg-zinc-50 dark:bg-zinc-950">
                    <div className="container mx-auto px-4 py-8">
                        <div className="flex justify-center py-12">
                            <Loader2 className="h-8 w-8 animate-spin text-amber-500" />
                        </div>
                    </div>
                </main>
                <Footer />
            </>
        );
    }

    return (
        <>
            <Header />
            <main className="min-h-screen bg-zinc-50 dark:bg-zinc-950">
                <div className="container mx-auto px-4 py-8">
                    <div className="mb-8">
                        <h1 className="text-3xl font-bold flex items-center gap-3">
                            <Heart className="h-8 w-8 text-red-500 fill-red-500" />
                            My Wishlist
                        </h1>
                        <p className="text-zinc-600 dark:text-zinc-400 mt-2">
                            Auctions you&apos;ve loved
                        </p>
                    </div>

                    {wishlist.length === 0 ? (
                        <Card>
                            <CardContent className="flex flex-col items-center justify-center py-12">
                                <Heart className="h-16 w-16 text-zinc-300 dark:text-zinc-700 mb-4" />
                                <h3 className="text-xl font-semibold mb-2">Your wishlist is empty</h3>
                                <p className="text-zinc-500 mb-6 text-center max-w-md">
                                    Start exploring auctions and click the heart icon to add items to your wishlist
                                </p>
                                <Button asChild>
                                    <Link href="/auctions">Browse Auctions</Link>
                                </Button>
                            </CardContent>
                        </Card>
                    ) : (
                        <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
                            {wishlist.map((item) => (
                                <Card key={item.id} className="overflow-hidden group">
                                    <div className="relative aspect-[4/3]">
                                        <Image
                                            src={item.auctionImageUrl || "https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=400"}
                                            alt={item.auctionTitle}
                                            fill
                                            className="object-cover transition-transform group-hover:scale-105"
                                        />
                                        <Badge
                                            className={`absolute top-2 left-2 ${getStatusColor(item.status)}`}
                                        >
                                            {item.status}
                                        </Badge>
                                        <Button
                                            variant="destructive"
                                            size="icon"
                                            className="absolute top-2 right-2 opacity-0 group-hover:opacity-100 transition-opacity"
                                            onClick={() => handleRemove(item.auctionId)}
                                        >
                                            <Trash2 className="h-4 w-4" />
                                        </Button>
                                    </div>
                                    <CardHeader className="pb-2">
                                        <CardTitle className="text-lg line-clamp-1">
                                            {item.auctionTitle}
                                        </CardTitle>
                                        <CardDescription className="flex items-center gap-1">
                                            <Clock className="h-3 w-3" />
                                            Added {formatRelativeTime(item.addedAt)}
                                        </CardDescription>
                                    </CardHeader>
                                    <CardContent>
                                        <div className="flex items-center justify-between">
                                            <div>
                                                <p className="text-sm text-zinc-500">Current Bid</p>
                                                <p className="text-lg font-bold text-amber-600">
                                                    {formatCurrency(item.currentBid)}
                                                </p>
                                            </div>
                                            <Button asChild size="sm">
                                                <Link href={`/auctions/${item.auctionId}`}>
                                                    <ExternalLink className="h-4 w-4 mr-1" />
                                                    View
                                                </Link>
                                            </Button>
                                        </div>
                                        {item.status.toLowerCase() === "live" && (
                                            <p className="text-xs text-zinc-500 mt-2 flex items-center gap-1">
                                                <AlertCircle className="h-3 w-3" />
                                                Ends {formatRelativeTime(item.auctionEnd)}
                                            </p>
                                        )}
                                    </CardContent>
                                </Card>
                            ))}
                        </div>
                    )}
                </div>
            </main>
            <Footer />
        </>
    );
}
