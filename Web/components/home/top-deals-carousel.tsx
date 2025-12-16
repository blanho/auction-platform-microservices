"use client";

import { useState, useEffect, useRef, useCallback, useMemo } from "react";
import Image from "next/image";
import Link from "next/link";
import { motion } from "framer-motion";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faThumbsUp,
  faFire,
  faChevronLeft,
  faChevronRight,
  faHeart,
  faTruck,
  faShieldHalved,
  faClock,
  faStar,
} from "@fortawesome/free-solid-svg-icons";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { Auction } from "@/types/auction";
import { auctionService } from "@/services/auction.service";
import { useCountdown } from "@/hooks/use-countdown";
import { cn } from "@/lib/utils";

interface DealCardProps {
  auction: Auction;
  index: number;
}

function DealCard({ auction, index }: DealCardProps) {
  const [isLiked, setIsLiked] = useState(false);
  const timeLeft = useCountdown(auction.auctionEnd);

  const imageUrl = useMemo(() => {
    const primaryFile = auction.files?.find((f) => f.isPrimary);
    return (
      primaryFile?.url ||
      auction.files?.[0]?.url ||
      "https://images.unsplash.com/photo-1560472354-b33ff0c44a43?w=400"
    );
  }, [auction.files]);

  const handleLikeClick = useCallback((e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setIsLiked(prev => !prev);
  }, []);

  const currentBid = auction.currentHighBid || auction.reservePrice;
  const originalPrice = auction.buyNowPrice || currentBid * 1.3;
  const discount = Math.round(((originalPrice - currentBid) / originalPrice) * 100);

  const formatDate = (dateStr: string) => {
    const date = new Date(dateStr);
    const day = date.getDate();
    const month = date.getMonth() + 1;
    return `${day}/${month}`;
  };

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.3, delay: index * 0.05 }}
      className="shrink-0 w-[180px] md:w-[200px]"
    >
      <Link href={`/auctions/${auction.id}`} className="group block">
        <div className="bg-white dark:bg-slate-900 rounded-xl border border-slate-200 dark:border-slate-800 overflow-hidden hover:shadow-lg hover:border-purple-300 dark:hover:border-purple-600 transition-all duration-300">
          <div className="relative aspect-square overflow-hidden bg-slate-100 dark:bg-slate-800">
            <Image
              src={imageUrl}
              alt={`${auction.make} ${auction.model}`}
              fill
              className="object-cover transition-transform duration-500 group-hover:scale-105"
              unoptimized={imageUrl.includes("unsplash")}
            />
            
            <div className="absolute top-2 left-2 flex flex-col gap-1">
              {timeLeft && !timeLeft.isExpired && (
                <Badge className="bg-purple-600 text-white text-[10px] px-1.5 py-0.5 border-0">
                  {formatDate(auction.auctionEnd)}
                </Badge>
              )}
              {auction.isFeatured && (
                <Badge className="bg-linear-to-r from-orange-500 to-red-500 text-white text-[10px] px-1.5 py-0.5 border-0">
                  <FontAwesomeIcon icon={faFire} className="w-2.5 h-2.5 mr-0.5" />
                  HOT
                </Badge>
              )}
            </div>

            <button
              onClick={handleLikeClick}
              className="absolute top-2 right-2 w-7 h-7 rounded-full bg-white/90 dark:bg-slate-800/90 shadow flex items-center justify-center hover:scale-110 transition-transform"
            >
              <FontAwesomeIcon
                icon={faHeart}
                className={cn(
                  "w-3.5 h-3.5 transition-colors",
                  isLiked ? "text-red-500" : "text-slate-400"
                )}
              />
            </button>

            <div className="absolute bottom-2 left-2 right-2 flex gap-1">
              <Badge className="bg-orange-500 text-white text-[9px] px-1.5 py-0.5 border-0">
                TOP DEAL
              </Badge>
              <Badge className="bg-green-600 text-white text-[9px] px-1.5 py-0.5 border-0">
                <FontAwesomeIcon icon={faTruck} className="w-2 h-2 mr-0.5" />
                FREE
              </Badge>
              <Badge className="bg-blue-600 text-white text-[9px] px-1.5 py-0.5 border-0">
                <FontAwesomeIcon icon={faShieldHalved} className="w-2 h-2" />
              </Badge>
            </div>
          </div>

          <div className="p-3 space-y-2">
            <h4 className="text-sm font-medium text-slate-900 dark:text-white line-clamp-2 min-h-10 group-hover:text-purple-600 dark:group-hover:text-purple-400 transition-colors">
              {auction.make} {auction.model} {auction.year}
            </h4>

            <div className="flex items-center gap-1">
              {Array.from({ length: 5 }).map((_, i) => (
                <FontAwesomeIcon
                  key={i}
                  icon={faStar}
                  className={cn(
                    "w-3 h-3",
                    i < 4 ? "text-amber-400" : "text-slate-300 dark:text-slate-600"
                  )}
                />
              ))}
            </div>

            <div className="space-y-1">
              <div className="flex items-baseline gap-1.5">
                <span className="text-lg font-bold text-red-600">
                  ${currentBid.toLocaleString()}
                </span>
                {discount > 0 && (
                  <span className="text-xs text-slate-400 dark:text-slate-500 line-through">
                    ${originalPrice.toLocaleString()}
                  </span>
                )}
              </div>
              
              {discount > 0 && (
                <div className="flex items-center gap-1.5">
                  <span className="text-xs font-semibold text-red-600">
                    -{discount}%
                  </span>
                </div>
              )}
            </div>

            <div className="flex items-center gap-1 text-xs text-slate-500 dark:text-slate-400">
              <FontAwesomeIcon icon={faClock} className="w-3 h-3" />
              <span>
                Ends {timeLeft?.days ? `${timeLeft.days}d ` : ""}{timeLeft?.hours}h {timeLeft?.minutes}m
              </span>
            </div>
          </div>
        </div>
      </Link>
    </motion.div>
  );
}

function DealCardSkeleton() {
  return (
    <div className="shrink-0 w-[180px] md:w-[200px]">
      <div className="bg-white dark:bg-slate-900 rounded-xl border border-slate-200 dark:border-slate-800 overflow-hidden">
        <Skeleton className="aspect-square w-full" />
        <div className="p-3 space-y-2">
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-4 w-20" />
          <Skeleton className="h-6 w-24" />
          <Skeleton className="h-4 w-16" />
        </div>
      </div>
    </div>
  );
}

interface TopDealsCarouselProps {
  title?: string;
  subtitle?: string;
}

export function TopDealsCarousel({ 
  title = "TOP DEAL • SUPER DEALS"
}: TopDealsCarouselProps) {
  const [auctions, setAuctions] = useState<Auction[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const scrollRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const fetchAuctions = async () => {
      try {
        const result = await auctionService.getAuctions({
          pageSize: 12,
          pageNumber: 1,
        });
        setAuctions(result.items);
      } catch (error) {
        console.error("Failed to fetch auctions:", error);
      } finally {
        setIsLoading(false);
      }
    };
    fetchAuctions();
  }, []);

  const scroll = (direction: "left" | "right") => {
    if (scrollRef.current) {
      const scrollAmount = 220;
      scrollRef.current.scrollBy({
        left: direction === "left" ? -scrollAmount : scrollAmount,
        behavior: "smooth",
      });
    }
  };

  return (
    <section className="py-6 bg-white dark:bg-slate-950">
      <div className="container mx-auto px-4">
        <div className="flex items-center justify-between mb-4">
          <div className="flex items-center gap-3">
            <FontAwesomeIcon icon={faThumbsUp} className="w-5 h-5 text-red-500" />
            <h2 className="text-lg md:text-xl font-bold text-red-600">{title}</h2>
          </div>
          <Link 
            href="/deals" 
            className="text-sm font-medium text-purple-600 hover:text-purple-700 dark:text-purple-400 dark:hover:text-purple-300 transition-colors"
          >
            View All →
          </Link>
        </div>

        <div className="relative group">
          <button
            onClick={() => scroll("left")}
            className="absolute left-0 top-1/2 -translate-y-1/2 -translate-x-2 z-10 w-10 h-10 rounded-full bg-white dark:bg-slate-800 shadow-lg border border-slate-200 dark:border-slate-700 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity hover:bg-slate-50 dark:hover:bg-slate-700"
          >
            <FontAwesomeIcon icon={faChevronLeft} className="w-4 h-4 text-slate-600 dark:text-slate-400" />
          </button>

          <div
            ref={scrollRef}
            className="flex gap-4 overflow-x-auto scrollbar-hide pb-2"
          >
            {isLoading
              ? Array.from({ length: 6 }).map((_, i) => (
                  <DealCardSkeleton key={i} />
                ))
              : auctions.map((auction, index) => (
                  <DealCard key={auction.id} auction={auction} index={index} />
                ))}
          </div>

          <button
            onClick={() => scroll("right")}
            className="absolute right-0 top-1/2 -translate-y-1/2 translate-x-2 z-10 w-10 h-10 rounded-full bg-white dark:bg-slate-800 shadow-lg border border-slate-200 dark:border-slate-700 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity hover:bg-slate-50 dark:hover:bg-slate-700"
          >
            <FontAwesomeIcon icon={faChevronRight} className="w-4 h-4 text-slate-600 dark:text-slate-400" />
          </button>
        </div>
      </div>
    </section>
  );
}
