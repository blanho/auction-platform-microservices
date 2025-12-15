"use client";

import { useState, useMemo, useCallback } from "react";
import { motion, AnimatePresence } from "framer-motion";
import Image from "next/image";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faGavel,
  faFire,
  faBolt,
  faSearch,
  faArrowRight,
  faUsers,
  faClock,
  faChartLine,
  faShieldHalved,
  faCircleCheck,
  faStar,
  faTag,
  faChevronLeft,
  faChevronRight
} from "@fortawesome/free-solid-svg-icons";
import { useFeaturedAuctions, useCategories } from "@/hooks/use-auctions";
import { useQuickStats, useTrendingSearches } from "@/hooks/use-analytics";
import { useCountdown } from "@/hooks/use-countdown";
import { useCarouselInterval } from "@/hooks/use-interval";
import { Auction, Category } from "@/types/auction";
import { PulsingDot } from "@/components/ui/animated";
import { UI, URGENCY } from "@/constants/config";

interface AuctionCardProps {
  auction: Auction;
  isActive: boolean;
}

function AuctionCard({ auction, isActive }: AuctionCardProps) {
  const timeLeft = useCountdown(auction.auctionEnd);

  const imageUrl = useMemo(() => {
    const primaryFile = auction.files?.find((f) => f.isPrimary);
    return primaryFile?.url || auction.files?.[0]?.url || "https://images.unsplash.com/photo-1614162692292-7ac56d7f7f1e?w=800";
  }, [auction.files]);

  const formatTime = (value: number) => value.toString().padStart(2, "0");

  const isUrgent = timeLeft && (timeLeft.days === 0 && timeLeft.hours < URGENCY.CRITICAL_HOURS);

  return (
    <motion.div
      initial={{ opacity: 0, scale: 0.95 }}
      animate={{ opacity: isActive ? 1 : 0.7, scale: isActive ? 1 : 0.9 }}
      transition={{ duration: 0.4 }}
      className={`relative bg-white rounded-2xl shadow-2xl overflow-hidden transition-all ${isActive ? 'ring-2 ring-purple-500/50' : ''}`}
    >
      <div className="absolute top-3 left-3 z-20 flex gap-2">
        <Badge className="bg-emerald-500 text-white shadow-lg border-0 text-xs">
          <PulsingDot className="mr-1.5" />
          Live
        </Badge>
        {auction.isFeatured && (
          <Badge className="bg-gradient-to-r from-amber-500 to-orange-500 text-white shadow-lg border-0 text-xs">
            <FontAwesomeIcon icon={faFire} className="w-3 h-3 mr-1" />
            Hot
          </Badge>
        )}
      </div>

      <div className="relative h-48 sm:h-56 overflow-hidden">
        <Image
          src={imageUrl}
          alt={`${auction.make} ${auction.model}`}
          fill
          className="object-cover transition-transform duration-500 group-hover:scale-105"
          unoptimized={imageUrl.includes("unsplash")}
          priority
        />
        <div className="absolute inset-0 bg-gradient-to-t from-black/70 via-black/20 to-transparent" />
        <div className="absolute bottom-3 left-3 right-3">
          <p className="text-[10px] text-white/70 uppercase tracking-wider mb-0.5">{auction.categoryName}</p>
          <h3 className="text-base sm:text-lg font-bold text-white line-clamp-1">{auction.make} {auction.model}</h3>
        </div>
      </div>

      <div className="p-4 space-y-3 bg-gradient-to-b from-white to-slate-50">
        <div className="flex items-center justify-between">
          <div>
            <p className="text-[10px] text-slate-500 uppercase tracking-wider">Current Bid</p>
            <p className="text-xl sm:text-2xl font-bold text-slate-900">
              ${(auction.currentHighBid || auction.reservePrice || 0).toLocaleString()}
            </p>
          </div>
          <div className="text-right">
            <p className="text-[10px] text-slate-500 uppercase tracking-wider">Time Left</p>
            <div className={`flex gap-0.5 mt-0.5 ${isUrgent ? 'text-red-600' : 'text-slate-900'}`}>
              {timeLeft && timeLeft.days > 0 && (
                <span className="text-sm font-mono font-bold bg-slate-100 px-1.5 py-0.5 rounded">
                  {timeLeft.days}d
                </span>
              )}
              <span className="text-sm font-mono font-bold bg-slate-100 px-1.5 py-0.5 rounded">
                {formatTime(timeLeft?.hours ?? 0)}h
              </span>
              <span className="text-sm font-mono font-bold bg-slate-100 px-1.5 py-0.5 rounded">
                {formatTime(timeLeft?.minutes ?? 0)}m
              </span>
            </div>
          </div>
        </div>

        <div className="flex items-center justify-between text-xs text-slate-500">
          <span className="flex items-center gap-1">
            <FontAwesomeIcon icon={faUsers} className="w-3 h-3" />
            Active bidders
          </span>
          <span className="flex items-center gap-1 text-emerald-600">
            <FontAwesomeIcon icon={faChartLine} className="w-3 h-3" />
            <span className="font-medium">+${Math.max(0, (auction.currentHighBid || 0) - (auction.reservePrice || 0)).toLocaleString()}</span>
          </span>
        </div>

        <Button
          className="w-full h-10 text-sm font-semibold bg-slate-900 hover:bg-slate-800 text-white"
          asChild
        >
          <Link href={`/auctions/${auction.id}`}>
            Place Bid
            <FontAwesomeIcon icon={faArrowRight} className="ml-2 w-3 h-3" />
          </Link>
        </Button>
      </div>
    </motion.div>
  );
}

function AuctionCarousel({ auctions, isLoading }: { auctions: Auction[]; isLoading: boolean }) {
  const auctionList = Array.isArray(auctions) ? auctions : [];
  const displayCount = Math.min(auctionList.length, UI.CAROUSEL.MAX_ITEMS);
  
  const { activeIndex, setActiveIndex, prev, next } = useCarouselInterval(
    displayCount,
    UI.CAROUSEL.INTERVAL
  );

  const handlePrev = useCallback(() => {
    prev();
  }, [prev]);

  const handleNext = useCallback(() => {
    next();
  }, [next]);

  const handleDotClick = useCallback((idx: number) => {
    setActiveIndex(idx);
  }, [setActiveIndex]);

  if (isLoading) {
    return (
      <div className="relative w-full max-w-sm mx-auto">
        <Skeleton className="h-80 w-full rounded-2xl bg-white/10" />
      </div>
    );
  }

  if (auctionList.length === 0) {
    return (
      <div className="relative w-full max-w-sm mx-auto bg-white/5 border border-white/10 rounded-2xl p-8 text-center">
        <FontAwesomeIcon icon={faGavel} className="w-12 h-12 text-slate-500 mx-auto mb-4" />
        <p className="text-slate-400">No featured auctions available</p>
      </div>
    );
  }

  const displayAuctions = auctionList.slice(0, displayCount);

  return (
    <div className="relative w-full max-w-sm mx-auto">
      <AnimatePresence mode="wait">
        <motion.div
          key={activeIndex}
          initial={{ opacity: 0, x: 20 }}
          animate={{ opacity: 1, x: 0 }}
          exit={{ opacity: 0, x: -20 }}
          transition={{ duration: UI.ANIMATION.FADE_DURATION }}
        >
          <AuctionCard auction={displayAuctions[activeIndex]} isActive={true} />
        </motion.div>
      </AnimatePresence>

      {displayAuctions.length > 1 && (
        <>
          <button
            onClick={handlePrev}
            className="absolute left-0 top-1/2 -translate-y-1/2 -translate-x-4 w-8 h-8 rounded-full bg-white shadow-lg flex items-center justify-center hover:bg-slate-50 transition-colors z-10"
          >
            <FontAwesomeIcon icon={faChevronLeft} className="w-3 h-3 text-slate-700" />
          </button>
          <button
            onClick={handleNext}
            className="absolute right-0 top-1/2 -translate-y-1/2 translate-x-4 w-8 h-8 rounded-full bg-white shadow-lg flex items-center justify-center hover:bg-slate-50 transition-colors z-10"
          >
            <FontAwesomeIcon icon={faChevronRight} className="w-3 h-3 text-slate-700" />
          </button>

          <div className="flex justify-center gap-1.5 mt-4">
            {displayAuctions.map((_, idx) => (
              <button
                key={idx}
                onClick={() => handleDotClick(idx)}
                className={`w-2 h-2 rounded-full transition-all ${
                  idx === activeIndex ? 'bg-white w-6' : 'bg-white/40 hover:bg-white/60'
                }`}
              />
            ))}
          </div>
        </>
      )}
    </div>
  );
}

function SearchBar() {
  const router = useRouter();
  const [searchTerm, setSearchTerm] = useState("");
  const { data: trendingSearches, isLoading: trendingLoading } = useTrendingSearches(5);
  const { data: categories } = useCategories();

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (searchTerm.trim()) {
      router.push(`/auctions?search=${encodeURIComponent(searchTerm.trim())}`);
    }
  };

  const handleTrendingClick = (term: string) => {
    router.push(`/auctions?search=${encodeURIComponent(term)}`);
  };

  const handleCategoryClick = (categoryId: string) => {
    router.push(`/auctions?category=${categoryId}`);
  };

  const topCategories = categories?.slice(0, 4) || [];

  return (
    <div className="space-y-4">
      <form onSubmit={handleSearch} className="relative">
        <div className="flex">
          <div className="relative flex-1">
            <FontAwesomeIcon
              icon={faSearch}
              className="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400"
            />
            <Input
              type="text"
              placeholder="Search for watches, collectibles, electronics..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="h-12 sm:h-14 pl-11 pr-4 text-base rounded-l-xl rounded-r-none border-r-0 bg-white/95 backdrop-blur border-white/20 focus:border-purple-400 focus:ring-purple-400/20"
            />
          </div>
          <Button
            type="submit"
            className="h-12 sm:h-14 px-6 sm:px-8 rounded-l-none rounded-r-xl bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-700 hover:to-pink-700 text-white font-semibold"
          >
            <span className="hidden sm:inline">Search</span>
            <FontAwesomeIcon icon={faSearch} className="sm:hidden w-4 h-4" />
          </Button>
        </div>
      </form>

      <div className="flex flex-wrap items-center gap-2">
        {!trendingLoading && trendingSearches.length > 0 && (
          <>
            <span className="text-xs text-slate-500 dark:text-slate-400 flex items-center gap-1">
              <FontAwesomeIcon icon={faFire} className="w-3 h-3 text-orange-500 dark:text-orange-400" />
              Trending:
            </span>
            {trendingSearches.slice(0, 4).map((search, idx) => (
              <button
                key={idx}
                onClick={() => handleTrendingClick(search.searchTerm)}
                className="px-2.5 py-1 text-xs rounded-full bg-slate-200/80 dark:bg-white/10 hover:bg-slate-300 dark:hover:bg-white/20 text-slate-700 dark:text-white/80 hover:text-slate-900 dark:hover:text-white transition-colors"
              >
                {search.searchTerm}
              </button>
            ))}
          </>
        )}
      </div>

      {topCategories.length > 0 && (
        <div className="flex flex-wrap items-center gap-2">
          <span className="text-xs text-slate-500 dark:text-slate-400 flex items-center gap-1">
            <FontAwesomeIcon icon={faTag} className="w-3 h-3" />
            Categories:
          </span>
          {topCategories.map((category: Category) => (
            <button
              key={category.id}
              onClick={() => handleCategoryClick(category.id)}
              className="px-2.5 py-1 text-xs rounded-full border border-slate-300 dark:border-white/20 hover:border-slate-400 dark:hover:border-white/40 text-slate-600 dark:text-white/70 hover:text-slate-900 dark:hover:text-white transition-colors"
            >
              {category.name}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}

function LiveStats() {
  const { data: stats, isLoading } = useQuickStats();

  const statItems = [
    {
      icon: faBolt,
      value: stats?.liveAuctions ?? 0,
      label: "Live Auctions",
      change: stats?.liveAuctionsChange,
      color: "text-emerald-400"
    },
    {
      icon: faUsers,
      value: stats?.activeUsers ?? 0,
      label: "Active Bidders",
      change: stats?.activeUsersChange,
      color: "text-blue-400"
    },
    {
      icon: faClock,
      value: stats?.endingSoon ?? 0,
      label: "Ending Soon",
      change: stats?.endingSoonChange,
      color: "text-orange-400"
    }
  ];

  if (isLoading) {
    return (
      <div className="grid grid-cols-3 gap-3 sm:gap-4">
        {[1, 2, 3].map((i) => (
          <div key={i} className="bg-white/5 backdrop-blur-sm rounded-xl p-3 sm:p-4 border border-white/10">
            <Skeleton className="w-6 h-6 rounded-lg bg-white/10 mb-2" />
            <Skeleton className="w-12 h-6 bg-white/10 mb-1" />
            <Skeleton className="w-16 h-3 bg-white/10" />
          </div>
        ))}
      </div>
    );
  }

  return (
    <div className="grid grid-cols-3 gap-3 sm:gap-4">
      {statItems.map((stat, idx) => (
        <motion.div
          key={idx}
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: idx * 0.1 }}
          className="bg-white/80 dark:bg-white/5 backdrop-blur-sm rounded-xl p-3 sm:p-4 border border-slate-200 dark:border-white/10 hover:border-slate-300 dark:hover:border-white/20 transition-colors shadow-sm dark:shadow-none"
        >
          <FontAwesomeIcon icon={stat.icon} className={`w-5 h-5 sm:w-6 sm:h-6 ${stat.color} mb-2`} />
          <p className="text-xl sm:text-2xl font-bold text-slate-900 dark:text-white">{stat.value.toLocaleString()}</p>
          <p className="text-[10px] sm:text-xs text-slate-500 dark:text-slate-400">{stat.label}</p>
          {stat.change && (
            <p className="text-[10px] text-emerald-600 dark:text-emerald-400 mt-0.5">{stat.change}</p>
          )}
        </motion.div>
      ))}
    </div>
  );
}

export function HeroSection() {
  const { data: featuredAuctions, isLoading: auctionsLoading } = useFeaturedAuctions(5);

  return (
    <section className="relative min-h-[90vh] overflow-hidden bg-slate-50 dark:bg-slate-950">
      <div className="absolute inset-0">
        <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top,_var(--tw-gradient-stops))] from-purple-200/50 via-slate-50 to-slate-50 dark:from-purple-900/30 dark:via-slate-950 dark:to-slate-950" />
        <div className="absolute top-0 left-1/4 w-[600px] h-[600px] bg-purple-400/20 dark:bg-purple-600/20 rounded-full blur-[120px]" />
        <div className="absolute bottom-0 right-1/4 w-[500px] h-[500px] bg-pink-400/15 dark:bg-pink-600/15 rounded-full blur-[120px]" />
        <div className="absolute top-1/2 right-0 w-[400px] h-[400px] bg-blue-400/10 dark:bg-blue-600/10 rounded-full blur-[100px]" />
      </div>

      <div className="container relative mx-auto px-4 pt-16 pb-12 sm:pt-20 sm:pb-16 lg:pt-24 lg:pb-20">
        <div className="grid lg:grid-cols-2 gap-10 lg:gap-16 items-center">
          <div className="space-y-8 text-center lg:text-left">
            <div className="space-y-5">
              <motion.div
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                className="inline-flex items-center gap-2 px-3 py-1.5 rounded-full bg-emerald-500/10 border border-emerald-500/30"
              >
                <PulsingDot />
                <span className="text-xs sm:text-sm font-medium text-emerald-600 dark:text-emerald-400">Live Bidding Active</span>
              </motion.div>

              <motion.h1
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.1 }}
                className="text-3xl sm:text-4xl lg:text-5xl xl:text-6xl font-bold leading-[1.1] tracking-tight"
              >
                <span className="block text-slate-900 dark:text-white">
                  Discover Unique
                </span>
                <span className="block mt-1 sm:mt-2 text-transparent bg-clip-text bg-gradient-to-r from-purple-400 via-pink-400 to-orange-400">
                  Treasures & Deals
                </span>
              </motion.h1>

              <motion.p
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.2 }}
                className="text-sm sm:text-base lg:text-lg text-slate-600 dark:text-slate-400 max-w-lg mx-auto lg:mx-0 leading-relaxed"
              >
                Join thousands of collectors and savvy shoppers. Bid on exclusive items, sell your valuables, and save up to <span className="text-emerald-600 dark:text-emerald-400 font-semibold">50% off retail prices</span>.
              </motion.p>
            </div>

            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.3 }}
            >
              <SearchBar />
            </motion.div>

            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.4 }}
            >
              <LiveStats />
            </motion.div>

            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.5 }}
              className="flex flex-wrap items-center justify-center lg:justify-start gap-4 sm:gap-6 pt-2"
            >
              {[
                { icon: faShieldHalved, text: "Buyer Protection" },
                { icon: faCircleCheck, text: "Verified Sellers" },
                { icon: faStar, text: "Trusted Platform" }
              ].map((item, idx) => (
                <div key={idx} className="flex items-center gap-1.5 text-slate-500 dark:text-slate-400">
                  <FontAwesomeIcon icon={item.icon} className="w-3.5 h-3.5 text-emerald-600 dark:text-emerald-400" />
                  <span className="text-xs sm:text-sm">{item.text}</span>
                </div>
              ))}
            </motion.div>

            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.6 }}
              className="flex flex-col sm:flex-row gap-3 justify-center lg:justify-start pt-2"
            >
              <Button
                size="lg"
                className="h-11 sm:h-12 px-6 sm:px-8 text-sm sm:text-base font-semibold bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-700 hover:to-pink-700 text-white shadow-lg shadow-purple-500/25"
                asChild
              >
                <Link href="/auctions">
                  Explore Auctions
                  <FontAwesomeIcon icon={faArrowRight} className="ml-2 w-4 h-4" />
                </Link>
              </Button>

              <Button
                size="lg"
                className="h-11 sm:h-12 px-6 sm:px-8 text-sm sm:text-base font-semibold border border-slate-300 dark:border-white/30 bg-white/80 dark:bg-white/5 text-slate-700 dark:text-white hover:bg-white dark:hover:bg-white/10 backdrop-blur-sm"
                asChild
              >
                <Link href="/dashboard/listings/new">
                  <FontAwesomeIcon icon={faGavel} className="mr-2 w-4 h-4" />
                  Start Selling
                </Link>
              </Button>
            </motion.div>
          </div>

          <motion.div
            initial={{ opacity: 0, scale: 0.95 }}
            animate={{ opacity: 1, scale: 1 }}
            transition={{ delay: 0.3, duration: 0.5 }}
            className="relative"
          >
            <AuctionCarousel auctions={featuredAuctions || []} isLoading={auctionsLoading} />

            <div className="hidden lg:block absolute -bottom-4 -left-8 z-10">
              <motion.div
                initial={{ opacity: 0, x: -20 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ delay: 0.8 }}
                className="flex items-center gap-3 px-4 py-3 rounded-xl bg-white shadow-xl"
              >
                <div className="w-10 h-10 rounded-lg bg-gradient-to-br from-emerald-500 to-teal-600 flex items-center justify-center">
                  <FontAwesomeIcon icon={faChartLine} className="w-5 h-5 text-white" />
                </div>
                <div>
                  <p className="text-lg font-bold text-slate-900">$2.4M+</p>
                  <p className="text-xs text-slate-500">Saved this month</p>
                </div>
              </motion.div>
            </div>

            <div className="hidden lg:block absolute -top-2 -right-6 z-10">
              <motion.div
                initial={{ opacity: 0, x: 20 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ delay: 0.9 }}
                className="flex items-center gap-2 px-3 py-2 rounded-lg bg-white shadow-xl"
              >
                <div className="flex -space-x-1.5">
                  {[1, 2, 3, 4].map((i) => (
                    <div key={i} className="w-6 h-6 rounded-full bg-gradient-to-br from-purple-400 to-pink-400 border-2 border-white flex items-center justify-center text-[8px] font-bold text-white">
                      {String.fromCharCode(64 + i)}
                    </div>
                  ))}
                </div>
                <div className="pl-1">
                  <div className="flex items-center gap-0.5">
                    {[1, 2, 3, 4, 5].map((i) => (
                      <FontAwesomeIcon key={i} icon={faStar} className="w-2.5 h-2.5 text-yellow-400" />
                    ))}
                  </div>
                  <p className="text-[10px] text-slate-600">12.8k reviews</p>
                </div>
              </motion.div>
            </div>
          </motion.div>
        </div>
      </div>
    </section>
  );
}
