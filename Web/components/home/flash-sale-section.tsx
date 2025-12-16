"use client";

import { useState, useEffect, useMemo } from "react";
import Image from "next/image";
import Link from "next/link";
import { motion } from "framer-motion";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faBolt,
  faClock,
  faFire,
  faPercentage,
  faChevronRight,
} from "@fortawesome/free-solid-svg-icons";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { Progress } from "@/components/ui/progress";
import { Card, CardContent } from "@/components/ui/card";
import {
  Carousel,
  CarouselContent,
  CarouselItem,
  CarouselPrevious,
  CarouselNext,
} from "@/components/ui/carousel";
import { Auction } from "@/types/auction";
import { auctionService } from "@/services/auction.service";
import { getAuctionTitle } from "@/utils/auction";

interface FlashSaleCountdownProps {
  endTime: Date;
}

function FlashSaleCountdown({ endTime }: FlashSaleCountdownProps) {
  const [timeLeft, setTimeLeft] = useState({ hours: 0, minutes: 0, seconds: 0 });

  useEffect(() => {
    const calculate = () => {
      const diff = endTime.getTime() - Date.now();
      if (diff > 0) {
        setTimeLeft({
          hours: Math.floor(diff / (1000 * 60 * 60)),
          minutes: Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60)),
          seconds: Math.floor((diff % (1000 * 60)) / 1000),
        });
      }
    };
    calculate();
    const timer = setInterval(calculate, 1000);
    return () => clearInterval(timer);
  }, [endTime]);

  const pad = (n: number) => n.toString().padStart(2, "0");

  return (
    <div className="flex items-center gap-1.5">
      {[
        { value: timeLeft.hours, label: "h" },
        { value: timeLeft.minutes, label: "m" },
        { value: timeLeft.seconds, label: "s" },
      ].map((item, idx) => (
        <div key={idx} className="flex items-center">
          <span className="bg-white dark:bg-slate-800 text-slate-900 dark:text-white font-mono font-bold text-lg px-2 py-1 rounded-lg shadow-sm">
            {pad(item.value)}
          </span>
          <span className="text-white/80 text-xs ml-0.5 mr-1">{item.label}</span>
          {idx < 2 && <span className="text-white font-bold">:</span>}
        </div>
      ))}
    </div>
  );
}

interface FlashSaleItemProps {
  auction: Auction;
  index: number;
}

function FlashSaleItem({ auction, index }: FlashSaleItemProps) {
  const title = getAuctionTitle(auction);
  const originalPrice = auction.buyNowPrice || auction.reservePrice * 1.5;
  const salePrice = auction.currentHighBid || auction.reservePrice;
  const discount = Math.round(((originalPrice - salePrice) / originalPrice) * 100);
  const soldPercentage = useMemo(() => {
    const hash = auction.id.split("").reduce((acc, char) => acc + char.charCodeAt(0), 0);
    return Math.min((hash % 60) + 30, 95);
  }, [auction.id]);

  const imageUrl =
    auction.files?.find((f) => f.isPrimary)?.url ||
    auction.files?.[0]?.url ||
    "https://images.unsplash.com/photo-1560472354-b33ff0c44a43?w=400";

  return (
    <motion.div
      initial={{ opacity: 0, scale: 0.9 }}
      animate={{ opacity: 1, scale: 1 }}
      transition={{ duration: 0.3, delay: index * 0.05 }}
    >
      <Link href={`/auctions/${auction.id}`} className="group block">
        <Card className="overflow-hidden border-slate-200 dark:border-slate-800 hover:border-orange-400 dark:hover:border-orange-500 transition-all duration-300 hover:shadow-xl hover:shadow-orange-500/10 p-0 gap-0">
          <div className="absolute top-2 left-2 z-10">
            <Badge className="bg-gradient-to-r from-red-500 to-orange-500 text-white text-[10px] font-bold px-2 py-0.5 border-0 shadow-lg">
              <FontAwesomeIcon icon={faPercentage} className="w-2.5 h-2.5 mr-1" />
              -{discount}%
            </Badge>
          </div>

          <div className="relative aspect-square overflow-hidden bg-slate-100 dark:bg-slate-800">
            <Image
              src={imageUrl}
              alt={title}
              fill
              className="object-cover transition-transform duration-500 group-hover:scale-110"
              unoptimized={imageUrl.includes("unsplash")}
            />
            <div className="absolute inset-0 bg-gradient-to-t from-black/40 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity" />
          </div>

          <CardContent className="p-3 space-y-2">
            <h3 className="text-sm font-semibold text-slate-900 dark:text-white truncate group-hover:text-orange-600 dark:group-hover:text-orange-400 transition-colors">
              {title}
            </h3>

            <div className="flex items-center gap-2">
              <span className="text-lg font-bold text-orange-600 dark:text-orange-400">
                ${salePrice.toLocaleString()}
              </span>
              <span className="text-xs text-slate-400 line-through">
                ${originalPrice.toLocaleString()}
              </span>
            </div>

            <div className="space-y-1">
              <div className="flex items-center justify-between text-[10px] text-slate-500 dark:text-slate-400">
                <span>Sold: {soldPercentage}%</span>
                <span className="flex items-center gap-1 text-orange-500">
                  <FontAwesomeIcon icon={faFire} className="w-2.5 h-2.5" />
                  Hot
                </span>
              </div>
              <Progress 
                value={soldPercentage} 
                className="h-1.5 bg-slate-100 dark:bg-slate-800"
              />
            </div>
          </CardContent>
        </Card>
      </Link>
    </motion.div>
  );
}

function FlashSaleItemSkeleton() {
  return (
    <Card className="overflow-hidden border-slate-200 dark:border-slate-800 p-0 gap-0">
      <Skeleton className="aspect-square bg-slate-200 dark:bg-slate-800" />
      <CardContent className="p-3 space-y-2">
        <Skeleton className="h-4 w-full bg-slate-200 dark:bg-slate-800" />
        <Skeleton className="h-6 w-20 bg-slate-200 dark:bg-slate-800" />
        <Skeleton className="h-1.5 w-full bg-slate-200 dark:bg-slate-800" />
      </CardContent>
    </Card>
  );
}

export function FlashSaleSection() {
  const [auctions, setAuctions] = useState<Auction[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  const saleEndTime = new Date();
  saleEndTime.setHours(saleEndTime.getHours() + 8);

  useEffect(() => {
    const fetchAuctions = async () => {
      try {
        const result = await auctionService.getAuctions({
          status: "Live",
          pageSize: 10,
          orderBy: "currentHighBid",
          descending: false,
        });
        setAuctions(result.items);
      } catch (error) {
        console.error("Failed to fetch flash sale auctions:", error);
      } finally {
        setIsLoading(false);
      }
    };
    fetchAuctions();
  }, []);

  if (!isLoading && auctions.length === 0) return null;

  return (
    <section className="py-6 bg-gradient-to-r from-orange-500 via-red-500 to-pink-500">
      <div className="container mx-auto px-4">
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-5">
          <div className="flex items-center gap-4">
            <div className="flex items-center gap-2">
              <div className="w-10 h-10 rounded-xl bg-white/20 backdrop-blur-sm flex items-center justify-center">
                <FontAwesomeIcon icon={faBolt} className="w-5 h-5 text-yellow-300 animate-pulse" />
              </div>
              <div>
                <h2 className="text-xl md:text-2xl font-bold text-white">Flash Sale</h2>
                <p className="text-white/70 text-xs">Limited time deals</p>
              </div>
            </div>

            <div className="hidden md:flex items-center gap-2 bg-white/10 backdrop-blur-sm rounded-full px-4 py-2">
              <FontAwesomeIcon icon={faClock} className="w-4 h-4 text-white/80" />
              <span className="text-white/80 text-sm">Ends in:</span>
              <FlashSaleCountdown endTime={saleEndTime} />
            </div>
          </div>

          <Link href="/deals?type=flash-sale">
            <Button 
              variant="outline" 
              className="border-white/30 text-white hover:bg-white/10 hover:text-white rounded-full"
            >
              View All Deals
              <FontAwesomeIcon icon={faChevronRight} className="w-3 h-3 ml-2" />
            </Button>
          </Link>
        </div>

        <div className="md:hidden flex items-center justify-center gap-2 bg-white/10 backdrop-blur-sm rounded-full px-4 py-2 mb-4 w-fit mx-auto">
          <FontAwesomeIcon icon={faClock} className="w-4 h-4 text-white/80" />
          <span className="text-white/80 text-sm">Ends in:</span>
          <FlashSaleCountdown endTime={saleEndTime} />
        </div>

        <Carousel
          opts={{
            align: "start",
            loop: true,
          }}
          className="w-full"
        >
          <CarouselContent className="-ml-3">
            {isLoading
              ? Array.from({ length: 6 }).map((_, i) => (
                  <CarouselItem key={i} className="pl-3 basis-[45%] sm:basis-1/3 md:basis-1/4 lg:basis-1/5 xl:basis-1/6">
                    <FlashSaleItemSkeleton />
                  </CarouselItem>
                ))
              : auctions.map((auction, index) => (
                  <CarouselItem key={auction.id} className="pl-3 basis-[45%] sm:basis-1/3 md:basis-1/4 lg:basis-1/5 xl:basis-1/6">
                    <FlashSaleItem auction={auction} index={index} />
                  </CarouselItem>
                ))}
          </CarouselContent>
          <CarouselPrevious className="left-2 bg-white/90 hover:bg-white border-0 shadow-lg" />
          <CarouselNext className="right-2 bg-white/90 hover:bg-white border-0 shadow-lg" />
        </Carousel>
      </div>
    </section>
  );
}
