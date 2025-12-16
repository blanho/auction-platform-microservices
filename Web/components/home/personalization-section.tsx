"use client";

import { useState, useEffect, useCallback } from "react";
import Image from "next/image";
import Link from "next/link";
import { motion } from "framer-motion";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faWandSparkles,
  faClock,
  faEye,
  faArrowRight,
  faHeart,
  faGavel,
  faFire,
  faBolt,
} from "@fortawesome/free-solid-svg-icons";
import { useSession } from "next-auth/react";
import { CTA_CONTENT } from "@/constants/landing";
import { AnimatedSection } from "@/components/ui/animated";
import { auctionService } from "@/services/auction.service";
import { Auction } from "@/types/auction";
import { Skeleton } from "@/components/ui/skeleton";
import { ROUTES } from "@/constants";
import { getAuctionTitle } from "@/utils/auction";

function getTimeRemaining(endDate: string) {
  const total = new Date(endDate).getTime() - Date.now();
  if (total <= 0) return { text: "Ended", urgent: true };

  const days = Math.floor(total / (1000 * 60 * 60 * 24));
  const hours = Math.floor((total / (1000 * 60 * 60)) % 24);
  const minutes = Math.floor((total / (1000 * 60)) % 60);

  if (days > 0) return { text: `${days}d ${hours}h`, urgent: false };
  if (hours > 0) return { text: `${hours}h ${minutes}m`, urgent: hours < 2 };
  return { text: `${minutes}m`, urgent: true };
}

function PremiumAuctionCard({
  auction,
  badge,
  badgeColor = "purple",
  index = 0,
}: {
  auction: Auction;
  badge?: string;
  badgeColor?: "purple" | "blue" | "orange" | "emerald";
  index?: number;
}) {
  const timeLeft = getTimeRemaining(auction.auctionEnd);
  const title = getAuctionTitle(auction);
  const imageUrl =
    auction.files?.find((f) => f.isPrimary)?.url ||
    auction.files?.[0]?.url ||
    "https://images.unsplash.com/photo-1526170375885-4d8ecf77b99f?w=400";

  const badgeColors = {
    purple:
      "bg-gradient-to-r from-purple-500 to-violet-500 text-white shadow-lg shadow-purple-500/30",
    blue: "bg-gradient-to-r from-blue-500 to-cyan-500 text-white shadow-lg shadow-blue-500/30",
    orange:
      "bg-gradient-to-r from-orange-500 to-amber-500 text-white shadow-lg shadow-orange-500/30",
    emerald:
      "bg-gradient-to-r from-emerald-500 to-teal-500 text-white shadow-lg shadow-emerald-500/30",
  };

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      whileInView={{ opacity: 1, y: 0 }}
      viewport={{ once: true }}
      transition={{ duration: 0.4, delay: index * 0.05 }}
    >
      <Link href={`${ROUTES.AUCTIONS.LIST}/${auction.id}`}>
        <motion.div
          whileHover={{ y: -6, scale: 1.02 }}
          transition={{ duration: 0.2 }}
          className="group relative bg-white dark:bg-slate-900 rounded-2xl overflow-hidden border border-slate-200/80 dark:border-slate-800/80 shadow-sm hover:shadow-xl hover:shadow-purple-500/10 dark:hover:shadow-purple-500/5 transition-all duration-300"
        >
          <div className="relative aspect-[4/3] overflow-hidden">
            <Image
              src={imageUrl}
              alt={title}
              fill
              className="object-cover transition-transform duration-500 group-hover:scale-110"
              sizes="(max-width: 768px) 50vw, 25vw"
            />

            <div className="absolute inset-0 bg-gradient-to-t from-black/60 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-300" />

            {badge && (
              <div
                className={`absolute top-3 left-3 px-3 py-1.5 rounded-full text-xs font-bold ${badgeColors[badgeColor]}`}
              >
                <FontAwesomeIcon
                  icon={
                    badgeColor === "purple"
                      ? faWandSparkles
                      : badgeColor === "blue"
                        ? faEye
                        : badgeColor === "orange"
                          ? faFire
                          : faBolt
                  }
                  className="w-3 h-3 mr-1.5"
                />
                {badge}
              </div>
            )}

            {timeLeft.urgent && (
              <div className="absolute top-3 right-3 px-2.5 py-1 rounded-full bg-red-500 text-white text-xs font-bold animate-pulse flex items-center gap-1">
                <FontAwesomeIcon icon={faClock} className="w-3 h-3" />
                {timeLeft.text}
              </div>
            )}

            <div className="absolute bottom-3 left-3 right-3 opacity-0 group-hover:opacity-100 transition-opacity duration-300">
              <div className="flex items-center justify-between text-white text-sm">
                <span className="flex items-center gap-1.5 bg-black/50 backdrop-blur-sm px-2.5 py-1 rounded-full">
                  <FontAwesomeIcon icon={faGavel} className="w-3 h-3" />
                  {auction.categoryName || "Auction"}
                </span>
              </div>
            </div>
          </div>

          <div className="p-4">
            <h4 className="font-semibold text-slate-900 dark:text-white line-clamp-1 group-hover:text-purple-600 dark:group-hover:text-purple-400 transition-colors">
              {title}
            </h4>

            <div className="mt-3 flex items-center justify-between">
              <div>
                <p className="text-xs text-slate-500 dark:text-slate-400 mb-0.5">
                  Current Bid
                </p>
                <p className="text-lg font-bold bg-gradient-to-r from-purple-600 to-blue-600 bg-clip-text text-transparent">
                  ${(auction.currentHighBid || auction.reservePrice || 0).toLocaleString()}
                </p>
              </div>

              {!timeLeft.urgent && (
                <div className="text-right">
                  <p className="text-xs text-slate-500 dark:text-slate-400 mb-0.5">
                    Ends in
                  </p>
                  <p className="text-sm font-medium text-slate-700 dark:text-slate-300 flex items-center gap-1">
                    <FontAwesomeIcon
                      icon={faClock}
                      className="w-3 h-3 text-slate-400"
                    />
                    {timeLeft.text}
                  </p>
                </div>
              )}
            </div>
          </div>

          <div className="absolute inset-0 rounded-2xl ring-2 ring-purple-500/0 group-hover:ring-purple-500/50 transition-all duration-300 pointer-events-none" />
        </motion.div>
      </Link>
    </motion.div>
  );
}

function CardSkeleton() {
  return (
    <div className="bg-white dark:bg-slate-900 rounded-2xl overflow-hidden border border-slate-200/80 dark:border-slate-800/80">
      <Skeleton className="aspect-[4/3]" />
      <div className="p-4 space-y-3">
        <Skeleton className="h-5 w-3/4" />
        <div className="flex justify-between">
          <Skeleton className="h-8 w-24" />
          <Skeleton className="h-6 w-16" />
        </div>
      </div>
    </div>
  );
}

function ExploreMoreCard({ href, icon, text }: { href: string; icon: typeof faWandSparkles; text: string }) {
  return (
    <Link href={href}>
      <motion.div
        whileHover={{ y: -6, scale: 1.02 }}
        className="h-full min-h-[280px] flex flex-col items-center justify-center p-6 rounded-2xl border-2 border-dashed border-slate-300 dark:border-slate-700 hover:border-purple-400 dark:hover:border-purple-500 bg-slate-50/50 dark:bg-slate-900/50 transition-all duration-300 cursor-pointer group"
      >
        <div className="w-14 h-14 rounded-full bg-gradient-to-br from-purple-100 to-blue-100 dark:from-purple-900/50 dark:to-blue-900/50 flex items-center justify-center mb-4 group-hover:scale-110 transition-transform">
          <FontAwesomeIcon
            icon={icon}
            className="w-6 h-6 text-purple-500 dark:text-purple-400"
          />
        </div>
        <span className="text-sm font-medium text-slate-600 dark:text-slate-400 text-center group-hover:text-purple-600 dark:group-hover:text-purple-400 transition-colors">
          {text}
        </span>
        <FontAwesomeIcon
          icon={faArrowRight}
          className="w-4 h-4 mt-2 text-slate-400 group-hover:text-purple-500 group-hover:translate-x-1 transition-all"
        />
      </motion.div>
    </Link>
  );
}

export function PersonalizationSection() {
    const { data: session } = useSession();

    if (!session) {
        return (
            <AnimatedSection className="py-24 bg-slate-950 relative overflow-hidden">
                <div className="absolute inset-0">
                    <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top,_var(--tw-gradient-stops))] from-purple-900/50 via-slate-950 to-slate-950" />
                    <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[600px] h-[600px] bg-purple-600/20 rounded-full blur-[150px]" />
                </div>
                
                <div className="container mx-auto px-4 relative z-10">
                    <div className="max-w-4xl mx-auto text-center">
                        <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-white/5 border border-white/10 backdrop-blur-sm mb-8">
                            <FontAwesomeIcon icon={faWandSparkles} className="w-4 h-4 text-purple-400" />
                            <span className="text-sm font-medium text-purple-300">{CTA_CONTENT.BADGE}</span>
                        </div>
                        
                        <h2 className="text-4xl md:text-5xl lg:text-6xl font-bold text-white mb-6 leading-tight">
                            {CTA_CONTENT.TITLE.split(".")[0]}.<br />
                            <span className="text-transparent bg-clip-text bg-gradient-to-r from-purple-400 via-pink-400 to-orange-400">
                                {CTA_CONTENT.TITLE.split(".")[1]?.trim() || "Start Winning."}
                            </span>
                        </h2>
                        <p className="text-lg md:text-xl text-slate-400 mb-10 max-w-2xl mx-auto leading-relaxed">
                            {CTA_CONTENT.DESCRIPTION}
                        </p>
                        
                        <div className="flex flex-col sm:flex-row items-center justify-center gap-4 mb-10">
                            <Button
                                size="lg"
                                className="bg-white text-slate-900 hover:bg-slate-100 font-semibold px-10 h-14 text-lg rounded-xl shadow-xl shadow-white/10 transition-all"
                                asChild
                            >
                                <Link href="/auth/register">
                                    {CTA_CONTENT.BUTTON}
                                    <FontAwesomeIcon icon={faArrowRight} className="ml-2 w-5 h-5" />
                                </Link>
                            </Button>
                            <Button
                                size="lg"
                                variant="outline"
                                className="border-2 border-white/20 text-white hover:bg-white/10 font-semibold px-10 h-14 text-lg rounded-xl backdrop-blur-sm"
                                asChild
                            >
                                <Link href="/auctions">
                                    Browse Auctions
                                </Link>
                            </Button>
                        </div>
                        
                        <div className="flex flex-wrap items-center justify-center gap-x-8 gap-y-4 text-slate-400">
                            {CTA_CONTENT.FEATURES.map((feature, idx) => (
                                <div key={idx} className="flex items-center gap-2">
                                    <div className="w-1.5 h-1.5 rounded-full bg-green-500"></div>
                                    <span className="text-sm">{feature}</span>
                                </div>
                            ))}
                        </div>
                    </div>
                </div>
            </AnimatedSection>
        );
    }

    return <LoggedInPersonalization />;
}

const RECENTLY_VIEWED_KEY = "auction_recently_viewed";

function LoggedInPersonalization() {
  const [recommendedAuctions, setRecommendedAuctions] = useState<Auction[]>([]);
  const [recentlyViewedAuctions, setRecentlyViewedAuctions] = useState<Auction[]>([]);
  const [isLoadingRecommended, setIsLoadingRecommended] = useState(true);
  const [isLoadingRecent, setIsLoadingRecent] = useState(true);

  const fetchRecommendedAuctions = useCallback(async () => {
    try {
      setIsLoadingRecommended(true);
      const result = await auctionService.getAuctions({
        status: "Live",
        isFeatured: true,
        pageSize: 7,
        orderBy: "currentHighBid",
        descending: true,
      });
      setRecommendedAuctions(result.items || []);
    } catch (error) {
      console.error("Failed to fetch recommended auctions:", error);
    } finally {
      setIsLoadingRecommended(false);
    }
  }, []);

  const fetchRecentlyViewed = useCallback(async () => {
    try {
      setIsLoadingRecent(true);
      const storedIds = localStorage.getItem(RECENTLY_VIEWED_KEY);
      if (!storedIds) {
        const result = await auctionService.getAuctions({
          status: "Live",
          pageSize: 4,
          orderBy: "updatedAt",
          descending: true,
        });
        setRecentlyViewedAuctions(result.items || []);
      } else {
        const ids: string[] = JSON.parse(storedIds);
        const validAuctions = await auctionService.getAuctionsByIds(ids.slice(0, 4));
        if (validAuctions.length === 0) {
          const result = await auctionService.getAuctions({
            status: "Live",
            pageSize: 4,
            orderBy: "updatedAt",
            descending: true,
          });
          setRecentlyViewedAuctions(result.items || []);
        } else {
          setRecentlyViewedAuctions(validAuctions);
        }
      }
    } catch (error) {
      console.error("Failed to fetch recently viewed:", error);
    } finally {
      setIsLoadingRecent(false);
    }
  }, []);

  useEffect(() => {
    fetchRecommendedAuctions();
    fetchRecentlyViewed();
  }, [fetchRecommendedAuctions, fetchRecentlyViewed]);

  return (
    <AnimatedSection className="py-20 md:py-28 bg-gradient-to-b from-slate-50 via-white to-slate-100 dark:from-slate-950 dark:via-slate-900 dark:to-slate-950 overflow-hidden relative">
      <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top_left,rgba(168,85,247,0.05),transparent_50%)] dark:bg-[radial-gradient(ellipse_at_top_left,rgba(168,85,247,0.1),transparent_50%)]" />
      <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_bottom_right,rgba(59,130,246,0.05),transparent_50%)] dark:bg-[radial-gradient(ellipse_at_bottom_right,rgba(59,130,246,0.1),transparent_50%)]" />

      <div className="container mx-auto px-4 relative">
        <div className="space-y-16">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
          >
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-8">
              <div className="flex items-center gap-3">
                <div className="w-12 h-12 rounded-xl bg-gradient-to-br from-purple-500 to-violet-600 flex items-center justify-center shadow-lg shadow-purple-500/30">
                  <FontAwesomeIcon
                    icon={faWandSparkles}
                    className="w-5 h-5 text-white"
                  />
                </div>
                <div>
                  <h3 className="text-2xl font-bold text-slate-900 dark:text-white">
                    Recommended for You
                  </h3>
                  <p className="text-sm text-slate-500 dark:text-slate-400">
                    Curated picks based on your interests
                  </p>
                </div>
              </div>
              <Button
                variant="outline"
                size="sm"
                className="self-start sm:self-auto rounded-full border-purple-200 dark:border-purple-800 hover:bg-purple-50 dark:hover:bg-purple-900/30"
                asChild
              >
                <Link href={`${ROUTES.AUCTIONS.LIST}?featured=true`}>
                  View All
                  <FontAwesomeIcon icon={faArrowRight} className="ml-2 w-4 h-4" />
                </Link>
              </Button>
            </div>

            <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4 md:gap-6">
              {isLoadingRecommended ? (
                Array.from({ length: 4 }).map((_, i) => <CardSkeleton key={i} />)
              ) : (
                <>
                  {recommendedAuctions.slice(0, 7).map((auction, index) => (
                    <PremiumAuctionCard
                      key={auction.id}
                      auction={auction}
                      badge="For You"
                      badgeColor="purple"
                      index={index}
                    />
                  ))}
                  <ExploreMoreCard
                    href={`${ROUTES.AUCTIONS.LIST}?featured=true`}
                    icon={faWandSparkles}
                    text="Explore more recommendations"
                  />
                </>
              )}
            </div>
          </motion.div>

          <motion.div
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ delay: 0.1 }}
          >
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-8">
              <div className="flex items-center gap-3">
                <div className="w-12 h-12 rounded-xl bg-gradient-to-br from-blue-500 to-cyan-600 flex items-center justify-center shadow-lg shadow-blue-500/30">
                  <FontAwesomeIcon
                    icon={faEye}
                    className="w-5 h-5 text-white"
                  />
                </div>
                <div>
                  <h3 className="text-2xl font-bold text-slate-900 dark:text-white">
                    Recently Viewed
                  </h3>
                  <p className="text-sm text-slate-500 dark:text-slate-400">
                    Continue where you left off
                  </p>
                </div>
              </div>
              <Button
                variant="outline"
                size="sm"
                className="self-start sm:self-auto rounded-full border-blue-200 dark:border-blue-800 hover:bg-blue-50 dark:hover:bg-blue-900/30"
                asChild
              >
                <Link href={ROUTES.AUCTIONS.LIST}>
                  View History
                  <FontAwesomeIcon icon={faArrowRight} className="ml-2 w-4 h-4" />
                </Link>
              </Button>
            </div>

            <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4 md:gap-6">
              {isLoadingRecent ? (
                Array.from({ length: 4 }).map((_, i) => <CardSkeleton key={i} />)
              ) : recentlyViewedAuctions.length > 0 ? (
                <>
                  {recentlyViewedAuctions.map((auction, index) => (
                    <PremiumAuctionCard
                      key={auction.id}
                      auction={auction}
                      badge="Viewed"
                      badgeColor="blue"
                      index={index}
                    />
                  ))}
                  {recentlyViewedAuctions.length < 4 && (
                    <ExploreMoreCard
                      href={ROUTES.AUCTIONS.LIST}
                      icon={faEye}
                      text="Discover more auctions"
                    />
                  )}
                </>
              ) : (
                <div className="col-span-full">
                  <div className="text-center py-12 px-6 rounded-2xl border-2 border-dashed border-slate-200 dark:border-slate-800 bg-slate-50/50 dark:bg-slate-900/50">
                    <div className="w-16 h-16 rounded-full bg-gradient-to-br from-blue-100 to-cyan-100 dark:from-blue-900/50 dark:to-cyan-900/50 flex items-center justify-center mx-auto mb-4">
                      <FontAwesomeIcon
                        icon={faEye}
                        className="w-7 h-7 text-blue-500 dark:text-blue-400"
                      />
                    </div>
                    <h4 className="text-lg font-semibold text-slate-900 dark:text-white mb-2">
                      Start exploring auctions
                    </h4>
                    <p className="text-slate-500 dark:text-slate-400 mb-6 max-w-md mx-auto">
                      Your recently viewed items will appear here so you can easily find them again
                    </p>
                    <Button
                      className="bg-gradient-to-r from-blue-500 to-cyan-500 hover:from-blue-600 hover:to-cyan-600 text-white rounded-full px-6"
                      asChild
                    >
                      <Link href={ROUTES.AUCTIONS.LIST}>
                        Browse Auctions
                        <FontAwesomeIcon icon={faArrowRight} className="ml-2 w-4 h-4" />
                      </Link>
                    </Button>
                  </div>
                </div>
              )}
            </div>
          </motion.div>
        </div>
      </div>
    </AnimatedSection>
  );
}
