"use client";

import { useEffect, useState, useCallback } from "react";
import { useSession } from "next-auth/react";
import { useRouter, useSearchParams } from "next/navigation";
import Link from "next/link";
import Image from "next/image";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faHeart,
  faEye,
  faSpinner,
  faTrash,
  faClock,
  faGripVertical,
  faList,
  faInbox,
} from "@fortawesome/free-solid-svg-icons";

import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { ToggleGroup, ToggleGroupItem } from "@/components/ui/toggle-group";
import { MainLayout } from "@/components/layout/main-layout";
import { PlaceBidDialog } from "@/features/bid/place-bid-dialog";
import { showUndoToast } from "@/components/ui/undo-toast";

import { Auction, AuctionStatus } from "@/types/auction";
import { auctionService } from "@/services/auction.service";
import { wishlistService, WishlistItem } from "@/services/wishlist.service";
import { toast } from "sonner";

import { ROUTES } from "@/constants";
import {
  formatCurrency,
  getTimeRemaining,
  watchlistStorage,
  getAuctionImageUrl,
  getAuctionTitle,
} from "@/utils";

type ViewMode = "grid" | "list";
type TabValue = "wishlist" | "watchlist";

interface TimeLeft {
  hours: number;
  minutes: number;
  seconds: number;
}

function CountdownTimer({ endTime }: { endTime: Date }) {
  const [timeLeft, setTimeLeft] = useState<TimeLeft>({ hours: 0, minutes: 0, seconds: 0 });

  useEffect(() => {
    const calculateTimeLeft = () => {
      const remaining = getTimeRemaining(endTime);
      setTimeLeft({
        hours: remaining.days * 24 + remaining.hours,
        minutes: remaining.minutes,
        seconds: remaining.seconds,
      });
    };

    calculateTimeLeft();
    const timer = setInterval(calculateTimeLeft, 1000);

    return () => clearInterval(timer);
  }, [endTime]);

  const formatTime = (value: number) => value.toString().padStart(2, "0");

  return (
    <span className="font-mono text-sm">
      {formatTime(timeLeft.hours)}:{formatTime(timeLeft.minutes)}:{formatTime(timeLeft.seconds)}
    </span>
  );
}

function EmptyState({ type }: { type: TabValue }) {
  const isWishlist = type === "wishlist";
  
  return (
    <Card className="border-dashed">
      <CardContent className="flex flex-col items-center justify-center py-16">
        <div className="w-16 h-16 rounded-full bg-slate-100 dark:bg-slate-800 flex items-center justify-center mb-4">
          <FontAwesomeIcon 
            icon={isWishlist ? faHeart : faEye} 
            className="h-8 w-8 text-slate-400" 
          />
        </div>
        <h3 className="text-xl font-semibold mb-2 text-slate-900 dark:text-white">
          {isWishlist ? "No saved items" : "No watched items"}
        </h3>
        <p className="text-slate-500 mb-6 text-center max-w-sm">
          {isWishlist 
            ? "Items you save will appear here. Start browsing to find something you love!"
            : "Items you're tracking will appear here. Watch auctions to follow their progress."}
        </p>
        <Button asChild className="bg-gradient-to-r from-purple-600 to-blue-600">
          <Link href={ROUTES.AUCTIONS.LIST}>
            <FontAwesomeIcon icon={faInbox} className="mr-2 h-4 w-4" />
            Browse Auctions
          </Link>
        </Button>
      </CardContent>
    </Card>
  );
}

function AuctionCard({
  auction,
  viewMode,
  onRemove,
}: {
  auction: Auction | WishlistItem;
  type: TabValue;
  viewMode: ViewMode;
  onRemove: (id: string) => void;
}) {
  const isWishlistItem = "auctionId" in auction;
  const id = isWishlistItem ? auction.auctionId : auction.id;
  const title = isWishlistItem ? auction.auctionTitle : getAuctionTitle(auction);
  const imageUrl = isWishlistItem ? auction.auctionImageUrl : getAuctionImageUrl(auction.files);
  const currentBid = isWishlistItem ? auction.currentBid : auction.currentHighBid;
  const status = isWishlistItem ? auction.status : auction.status;
  const auctionEnd = isWishlistItem 
    ? new Date(auction.auctionEnd) 
    : new Date(auction.auctionEnd);
  const reservePrice = isWishlistItem ? 0 : auction.reservePrice;

  const isLive = status === AuctionStatus.Live;
  const isEnded = status === AuctionStatus.Finished;

  const handleRemoveClick = () => {
    onRemove(id);
  };

  if (viewMode === "list") {
    return (
      <Card className="overflow-hidden hover:shadow-lg transition-shadow">
        <CardContent className="p-0">
          <div className="flex items-center gap-4">
            <Link href={`/auctions/${id}`} className="shrink-0">
              <div className="relative w-24 h-24 sm:w-32 sm:h-32">
                <Image
                  src={imageUrl || "/placeholder-car.jpg"}
                  alt={title}
                  fill
                  className="object-cover"
                />
                {isLive && (
                  <div className="absolute top-2 left-2">
                    <Badge className="bg-green-500 text-white text-xs animate-pulse">
                      Live
                    </Badge>
                  </div>
                )}
              </div>
            </Link>

            <div className="flex-1 py-3 pr-3 min-w-0">
              <Link href={`/auctions/${id}`}>
                <h3 className="font-semibold text-slate-900 dark:text-white truncate hover:text-purple-600 transition-colors">
                  {title}
                </h3>
              </Link>
              
              <div className="flex items-center gap-4 mt-2 text-sm text-slate-500">
                <span className="font-semibold text-slate-900 dark:text-white">
                  {formatCurrency(currentBid || 0)}
                </span>
                {isLive && (
                  <div className="flex items-center gap-1 text-orange-500">
                    <FontAwesomeIcon icon={faClock} className="h-3 w-3" />
                    <CountdownTimer endTime={auctionEnd} />
                  </div>
                )}
                {isEnded && (
                  <Badge variant="secondary" className="text-xs">
                    Ended
                  </Badge>
                )}
              </div>
            </div>

            <div className="flex items-center gap-2 pr-4">
              {isLive && !isWishlistItem && (
                <PlaceBidDialog
                  auctionId={id}
                  currentHighBid={currentBid || 0}
                  reservePrice={reservePrice}
                  auctionTitle={title}
                  trigger={
                    <Button size="sm" className="bg-gradient-to-r from-purple-600 to-blue-600">
                      Bid
                    </Button>
                  }
                />
              )}
              <Button
                variant="ghost"
                size="icon"
                onClick={handleRemoveClick}
                className="text-slate-400 hover:text-red-500"
              >
                <FontAwesomeIcon icon={faTrash} className="h-4 w-4" />
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card className="overflow-hidden hover:shadow-xl transition-all group">
      <Link href={`/auctions/${id}`}>
        <div className="relative aspect-square">
          <Image
            src={imageUrl || "/placeholder-car.jpg"}
            alt={title}
            fill
            className="object-cover group-hover:scale-105 transition-transform duration-300"
          />
          {isLive && (
            <div className="absolute top-3 left-3">
              <Badge className="bg-green-500 text-white animate-pulse">
                Live
              </Badge>
            </div>
          )}
          {isEnded && (
            <div className="absolute top-3 left-3">
              <Badge variant="secondary">Ended</Badge>
            </div>
          )}
          <Button
            variant="ghost"
            size="icon"
            onClick={(e) => {
              e.preventDefault();
              handleRemoveClick();
            }}
            className="absolute top-3 right-3 bg-white/80 dark:bg-slate-900/80 backdrop-blur-sm hover:bg-white dark:hover:bg-slate-900 text-slate-500 hover:text-red-500"
          >
            <FontAwesomeIcon icon={faTrash} className="h-4 w-4" />
          </Button>
        </div>
      </Link>

      <CardContent className="p-4">
        <Link href={`/auctions/${id}`}>
          <h3 className="font-semibold text-slate-900 dark:text-white truncate hover:text-purple-600 transition-colors">
            {title}
          </h3>
        </Link>

        <div className="flex items-center justify-between mt-3">
          <span className="text-lg font-bold text-slate-900 dark:text-white">
            {formatCurrency(currentBid || 0)}
          </span>
          {isLive && (
            <div className="flex items-center gap-1 text-xs text-orange-500">
              <FontAwesomeIcon icon={faClock} className="h-3 w-3" />
              <CountdownTimer endTime={auctionEnd} />
            </div>
          )}
        </div>

        {isLive && !isWishlistItem && (
          <PlaceBidDialog
            auctionId={id}
            currentHighBid={currentBid || 0}
            reservePrice={reservePrice}
            auctionTitle={title}
            trigger={
              <Button className="w-full mt-3 bg-gradient-to-r from-purple-600 to-blue-600">
                Place Bid
              </Button>
            }
          />
        )}
      </CardContent>
    </Card>
  );
}

export default function SavedItemsPage() {
  const { status } = useSession();
  const router = useRouter();
  const searchParams = useSearchParams();
  
  const initialTab = (searchParams.get("tab") as TabValue) || "wishlist";
  const [activeTab, setActiveTab] = useState<TabValue>(initialTab);
  const [viewMode, setViewMode] = useState<ViewMode>("grid");
  
  const [isLoadingWishlist, setIsLoadingWishlist] = useState(true);
  const [wishlistItems, setWishlistItems] = useState<WishlistItem[]>([]);
  
  const [isLoadingWatchlist, setIsLoadingWatchlist] = useState(true);
  const [watchlistItems, setWatchlistItems] = useState<Auction[]>([]);

  const fetchWishlist = useCallback(async () => {
    try {
      setIsLoadingWishlist(true);
      const data = await wishlistService.getWishlist();
      setWishlistItems(data);
    } catch {
      toast.error("Failed to load wishlist");
    } finally {
      setIsLoadingWishlist(false);
    }
  }, []);

  const fetchWatchlist = useCallback(async () => {
    try {
      setIsLoadingWatchlist(true);
      const watchlistIds = watchlistStorage.get();
      
      if (watchlistIds.length === 0) {
        setWatchlistItems([]);
        return;
      }

      const auctionsPromises = watchlistIds.map((id: string) => 
        auctionService.getAuctionById(id).catch(() => null)
      );
      
      const results = await Promise.all(auctionsPromises);
      const validAuctions = results.filter((a): a is Auction => a !== null);
      
      const removedIds = watchlistIds.filter(
        (id: string) => !validAuctions.some((a: Auction) => a.id === id)
      );
      removedIds.forEach((id: string) => watchlistStorage.remove(id));
      
      setWatchlistItems(validAuctions);
    } catch {
      toast.error("Failed to load watchlist");
    } finally {
      setIsLoadingWatchlist(false);
    }
  }, []);

  useEffect(() => {
    if (status === "unauthenticated") {
      router.push("/auth/signin?callbackUrl=/saved");
      return;
    }

    if (status === "authenticated") {
      fetchWishlist();
      fetchWatchlist();
    }
  }, [status, router, fetchWishlist, fetchWatchlist]);

  const handleRemoveFromWishlist = async (auctionId: string) => {
    const removedItem = wishlistItems.find(item => item.auctionId === auctionId);
    
    setWishlistItems(prev => prev.filter(item => item.auctionId !== auctionId));
    
    showUndoToast({
      message: "Removed from wishlist",
      variant: "remove",
      onConfirm: async () => {
        try {
          await wishlistService.removeFromWishlist(auctionId);
        } catch {
          if (removedItem) {
            setWishlistItems(prev => [...prev, removedItem]);
          }
          toast.error("Failed to remove from wishlist");
        }
      },
      onUndo: () => {
        if (removedItem) {
          setWishlistItems(prev => [...prev, removedItem]);
        }
      },
    });
  };

  const handleRemoveFromWatchlist = (auctionId: string) => {
    const removedItem = watchlistItems.find(item => item.id === auctionId);
    
    setWatchlistItems(prev => prev.filter(item => item.id !== auctionId));
    watchlistStorage.remove(auctionId);
    
    showUndoToast({
      message: "Removed from watchlist",
      variant: "remove",
      onConfirm: () => {},
      onUndo: () => {
        if (removedItem) {
          setWatchlistItems(prev => [...prev, removedItem]);
          watchlistStorage.add(auctionId);
        }
      },
    });
  };

  const handleTabChange = (value: string) => {
    setActiveTab(value as TabValue);
    const url = new URL(window.location.href);
    url.searchParams.set("tab", value);
    window.history.pushState({}, "", url);
  };

  const isLoading = status === "loading" || 
    (activeTab === "wishlist" && isLoadingWishlist) ||
    (activeTab === "watchlist" && isLoadingWatchlist);

  return (
    <MainLayout>
      <div className="container mx-auto px-4 py-8">
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-8">
          <div>
            <h1 className="text-3xl font-bold text-slate-900 dark:text-white">
              Saved Items
            </h1>
            <p className="text-slate-500 mt-1">
              Manage your wishlist and watched auctions
            </p>
          </div>

          <ToggleGroup
            type="single"
            value={viewMode}
            onValueChange={(value) => value && setViewMode(value as ViewMode)}
            className="bg-slate-100 dark:bg-slate-800 rounded-lg p-1"
          >
            <ToggleGroupItem value="grid" aria-label="Grid view" className="px-3">
              <FontAwesomeIcon icon={faGripVertical} className="h-4 w-4" />
            </ToggleGroupItem>
            <ToggleGroupItem value="list" aria-label="List view" className="px-3">
              <FontAwesomeIcon icon={faList} className="h-4 w-4" />
            </ToggleGroupItem>
          </ToggleGroup>
        </div>

        <Tabs value={activeTab} onValueChange={handleTabChange}>
          <TabsList className="mb-6 bg-slate-100 dark:bg-slate-800">
            <TabsTrigger value="wishlist" className="gap-2">
              <FontAwesomeIcon icon={faHeart} className="h-4 w-4" />
              Wishlist
              {wishlistItems.length > 0 && (
                <Badge variant="secondary" className="ml-1 h-5 px-1.5">
                  {wishlistItems.length}
                </Badge>
              )}
            </TabsTrigger>
            <TabsTrigger value="watchlist" className="gap-2">
              <FontAwesomeIcon icon={faEye} className="h-4 w-4" />
              Watchlist
              {watchlistItems.length > 0 && (
                <Badge variant="secondary" className="ml-1 h-5 px-1.5">
                  {watchlistItems.length}
                </Badge>
              )}
            </TabsTrigger>
          </TabsList>

          {isLoading ? (
            <div className="flex justify-center py-16">
              <FontAwesomeIcon 
                icon={faSpinner} 
                className="h-8 w-8 animate-spin text-purple-500" 
              />
            </div>
          ) : (
            <>
              <TabsContent value="wishlist">
                {wishlistItems.length === 0 ? (
                  <EmptyState type="wishlist" />
                ) : (
                  <div className={
                    viewMode === "grid"
                      ? "grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6"
                      : "space-y-4"
                  }>
                    {wishlistItems.map((item) => (
                      <AuctionCard
                        key={item.auctionId}
                        auction={item}
                        type="wishlist"
                        viewMode={viewMode}
                        onRemove={handleRemoveFromWishlist}
                      />
                    ))}
                  </div>
                )}
              </TabsContent>

              <TabsContent value="watchlist">
                {watchlistItems.length === 0 ? (
                  <EmptyState type="watchlist" />
                ) : (
                  <div className={
                    viewMode === "grid"
                      ? "grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6"
                      : "space-y-4"
                  }>
                    {watchlistItems.map((item) => (
                      <AuctionCard
                        key={item.id}
                        auction={item}
                        type="watchlist"
                        viewMode={viewMode}
                        onRemove={handleRemoveFromWatchlist}
                      />
                    ))}
                  </div>
                )}
              </TabsContent>
            </>
          )}
        </Tabs>
      </div>
    </MainLayout>
  );
}
