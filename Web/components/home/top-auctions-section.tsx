"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { motion } from "framer-motion";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faCrown,
  faEye,
  faGavel,
  faArrowRight,
  faFire,
  faTrophy,
  faMedal,
  faAward,
} from "@fortawesome/free-solid-svg-icons";
import { useTopListings } from "@/hooks/use-analytics";
import { Skeleton } from "@/components/ui/skeleton";
import { ROUTES } from "@/constants";
import { AnimatedSection } from "@/components/ui/animated";

const container = {
  hidden: { opacity: 0 },
  visible: {
    opacity: 1,
    transition: { staggerChildren: 0.1 },
  },
};

const item = {
  hidden: { opacity: 0, x: -20 },
  visible: { opacity: 1, x: 0, transition: { duration: 0.4 } },
};

const rankIcons = [faTrophy, faMedal, faAward];
const rankColors = [
  "from-amber-400 to-yellow-500",
  "from-slate-300 to-slate-400",
  "from-amber-600 to-orange-700",
];
const rankShadows = [
  "shadow-amber-500/30",
  "shadow-slate-400/30",
  "shadow-orange-600/30",
];

function TopListingSkeleton() {
  return (
    <div className="space-y-3">
      {Array.from({ length: 5 }).map((_, i) => (
        <div
          key={i}
          className="flex items-center gap-4 p-4 rounded-xl bg-slate-800/30"
        >
          <Skeleton className="w-8 h-8 rounded-full" />
          <div className="flex-1 space-y-2">
            <Skeleton className="h-4 w-3/4" />
            <Skeleton className="h-3 w-1/2" />
          </div>
          <Skeleton className="h-6 w-20" />
        </div>
      ))}
    </div>
  );
}

export function TopAuctionsSection() {
  const { data: topListings, isLoading, error } = useTopListings(6);
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
  }, []);

  if (!mounted) return null;

  const hasData = topListings && topListings.length > 0;

  return (
    <AnimatedSection className="py-16 md:py-20 bg-gradient-to-b from-slate-900 via-slate-950 to-slate-900 overflow-hidden relative">
      <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top_left,rgba(168,85,247,0.1),transparent_50%)]" />
      <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_bottom_right,rgba(234,179,8,0.08),transparent_50%)]" />

      <div className="container mx-auto px-4 relative">
        <div className="flex flex-col lg:flex-row gap-12 lg:gap-16">
          <div className="lg:w-1/3">
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true }}
              transition={{ duration: 0.5 }}
              className="sticky top-24"
            >
              <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-gradient-to-r from-amber-500/20 to-yellow-500/20 border border-amber-500/30 mb-6">
                <FontAwesomeIcon
                  icon={faCrown}
                  className="w-4 h-4 text-amber-400"
                />
                <span className="text-sm font-semibold text-amber-400">
                  Most Popular
                </span>
              </div>

              <h2 className="text-3xl md:text-4xl font-bold text-white mb-4">
                Top{" "}
                <span className="bg-gradient-to-r from-amber-400 to-yellow-500 bg-clip-text text-transparent">
                  Auctions
                </span>
              </h2>

              <p className="text-slate-400 mb-8 leading-relaxed">
                The hottest items our community is bidding on right now. Don&apos;t
                miss your chance to win these popular auctions.
              </p>

              <div className="flex flex-wrap gap-6 mb-8">
                <div className="flex items-center gap-2">
                  <div className="w-10 h-10 rounded-lg bg-gradient-to-br from-purple-500/20 to-violet-500/20 flex items-center justify-center">
                    <FontAwesomeIcon
                      icon={faEye}
                      className="w-4 h-4 text-purple-400"
                    />
                  </div>
                  <div>
                    <p className="text-white font-semibold">High Views</p>
                    <p className="text-xs text-slate-500">Most watched</p>
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <div className="w-10 h-10 rounded-lg bg-gradient-to-br from-green-500/20 to-emerald-500/20 flex items-center justify-center">
                    <FontAwesomeIcon
                      icon={faGavel}
                      className="w-4 h-4 text-green-400"
                    />
                  </div>
                  <div>
                    <p className="text-white font-semibold">Active Bids</p>
                    <p className="text-xs text-slate-500">Competitive</p>
                  </div>
                </div>
              </div>

              <Link
                href={ROUTES.AUCTIONS.LIST}
                className="inline-flex items-center gap-2 px-6 py-3 rounded-full bg-gradient-to-r from-amber-500 to-yellow-500 text-slate-900 font-semibold hover:from-amber-400 hover:to-yellow-400 transition-all shadow-lg shadow-amber-500/25 hover:shadow-xl hover:-translate-y-0.5"
              >
                <span>View All Auctions</span>
                <FontAwesomeIcon icon={faArrowRight} className="w-4 h-4" />
              </Link>
            </motion.div>
          </div>

          <div className="lg:w-2/3">
            {isLoading ? (
              <TopListingSkeleton />
            ) : error || !hasData ? (
              <div className="text-center text-slate-400 py-12">
                <FontAwesomeIcon
                  icon={faCrown}
                  className="w-12 h-12 mx-auto mb-4 opacity-30"
                />
                <p>Top auctions loading...</p>
              </div>
            ) : (
              <motion.div
                variants={container}
                initial="hidden"
                whileInView="visible"
                viewport={{ once: true }}
                className="space-y-3"
              >
                {topListings.map((listing, index) => (
                  <motion.div key={listing.id} variants={item}>
                    <Link href={`${ROUTES.AUCTIONS.LIST}/${listing.id}`}>
                      <motion.div
                        whileHover={{ x: 8, scale: 1.01 }}
                        className={`
                          group relative flex items-center gap-4 p-4 rounded-2xl
                          bg-slate-800/40 hover:bg-slate-800/60
                          border border-slate-700/50 hover:border-slate-600
                          transition-all duration-300 cursor-pointer
                          ${index < 3 ? "ring-1 ring-amber-500/20" : ""}
                        `}
                      >
                        <div
                          className={`
                            relative w-10 h-10 rounded-xl flex items-center justify-center shrink-0
                            ${
                              index < 3
                                ? `bg-gradient-to-br ${rankColors[index]} ${rankShadows[index]} shadow-lg`
                                : "bg-slate-700/50"
                            }
                          `}
                        >
                          {index < 3 ? (
                            <FontAwesomeIcon
                              icon={rankIcons[index]}
                              className="w-5 h-5 text-white"
                            />
                          ) : (
                            <span className="text-sm font-bold text-slate-400">
                              #{index + 1}
                            </span>
                          )}
                        </div>

                        <div className="flex-1 min-w-0">
                          <h4 className="font-semibold text-white truncate group-hover:text-amber-400 transition-colors">
                            {listing.title}
                          </h4>
                          <div className="flex items-center gap-4 mt-1">
                            <span className="text-xs text-slate-400 flex items-center gap-1">
                              <FontAwesomeIcon
                                icon={faEye}
                                className="w-3 h-3"
                              />
                              {listing.views.toLocaleString()} views
                            </span>
                            <span className="text-xs text-slate-400 flex items-center gap-1">
                              <FontAwesomeIcon
                                icon={faGavel}
                                className="w-3 h-3"
                              />
                              {listing.bids} bids
                            </span>
                          </div>
                        </div>

                        <div className="text-right shrink-0">
                          <p className="text-lg font-bold bg-gradient-to-r from-amber-400 to-yellow-500 bg-clip-text text-transparent">
                            ${listing.currentBid.toLocaleString()}
                          </p>
                          <p className="text-xs text-slate-500">Current bid</p>
                        </div>

                        {index === 0 && (
                          <div className="absolute -top-2 -right-2 px-2 py-1 rounded-full bg-gradient-to-r from-orange-500 to-red-500 text-white text-xs font-bold flex items-center gap-1 shadow-lg">
                            <FontAwesomeIcon
                              icon={faFire}
                              className="w-3 h-3"
                            />
                            HOT
                          </div>
                        )}
                      </motion.div>
                    </Link>
                  </motion.div>
                ))}
              </motion.div>
            )}
          </div>
        </div>
      </div>
    </AnimatedSection>
  );
}
