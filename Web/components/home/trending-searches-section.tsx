"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { motion } from "framer-motion";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faFire,
  faBolt,
  faArrowTrendUp,
  faSearch,
  faArrowRight,
  faGem,
  faClock,
  faLaptop,
  faPalette,
  faCar,
  faCouch,
  faFootball,
  faGuitar,
  faCamera,
} from "@fortawesome/free-solid-svg-icons";
import { AnimatedSection } from "@/components/ui/animated";
import { useTrendingSearches } from "@/hooks/use-analytics";
import { Skeleton } from "@/components/ui/skeleton";
import { ROUTES } from "@/constants";

const CATEGORY_ICONS: Record<string, typeof faGem> = {
  watches: faClock,
  electronics: faLaptop,
  art: faPalette,
  vehicles: faCar,
  furniture: faCouch,
  sports: faFootball,
  music: faGuitar,
  cameras: faCamera,
  jewelry: faGem,
  collectibles: faGem,
};

const getIconForTerm = (term: string) => {
  const lowerTerm = term.toLowerCase();
  for (const [key, icon] of Object.entries(CATEGORY_ICONS)) {
    if (lowerTerm.includes(key)) return icon;
  }
  return faSearch;
};

const container = {
  hidden: { opacity: 0 },
  visible: {
    opacity: 1,
    transition: {
      staggerChildren: 0.06,
    },
  },
};

const item = {
  hidden: { opacity: 0, y: 20, scale: 0.95 },
  visible: {
    opacity: 1,
    y: 0,
    scale: 1,
    transition: {
      type: "spring" as const,
      stiffness: 300,
      damping: 24,
    },
  },
};

function TrendingSearchSkeleton() {
  return (
    <div className="flex flex-wrap gap-3 justify-center">
      {Array.from({ length: 8 }).map((_, i) => (
        <Skeleton
          key={i}
          className="h-12 rounded-full"
          style={{ width: `${Math.random() * 60 + 100}px` }}
        />
      ))}
    </div>
  );
}

export function TrendingSearchesSection() {
  const { data: trendingSearches, isLoading, error } = useTrendingSearches(12);
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
  }, []);

  if (!mounted) return null;

  const hasData = trendingSearches && trendingSearches.length > 0;

  return (
    <AnimatedSection className="py-20 md:py-28 bg-gradient-to-b from-slate-50 via-white to-slate-50 dark:from-slate-950 dark:via-slate-900 dark:to-slate-950 overflow-hidden relative">
      <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top,rgba(168,85,247,0.05),transparent_50%)] dark:bg-[radial-gradient(ellipse_at_top,rgba(168,85,247,0.08),transparent_50%)]" />
      <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_bottom_right,rgba(59,130,246,0.05),transparent_50%)] dark:bg-[radial-gradient(ellipse_at_bottom_right,rgba(59,130,246,0.08),transparent_50%)]" />

      <div className="container mx-auto px-4 relative">
        <div className="text-center mb-12 md:mb-16">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.5 }}
            className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-gradient-to-r from-orange-500/10 to-red-500/10 dark:from-orange-500/20 dark:to-red-500/20 border border-orange-200/50 dark:border-orange-500/30 mb-6"
          >
            <FontAwesomeIcon
              icon={faFire}
              className="w-4 h-4 text-orange-500"
            />
            <span className="text-sm font-semibold text-orange-600 dark:text-orange-400">
              Popular Right Now
            </span>
          </motion.div>

          <motion.h2
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.5, delay: 0.1 }}
            className="text-3xl md:text-4xl lg:text-5xl font-bold text-slate-900 dark:text-white mb-4"
          >
            Discover What&apos;s{" "}
            <span className="bg-gradient-to-r from-orange-500 via-red-500 to-pink-500 bg-clip-text text-transparent">
              Trending
            </span>
          </motion.h2>

          <motion.p
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.5, delay: 0.2 }}
            className="text-lg text-slate-600 dark:text-slate-400 max-w-2xl mx-auto"
          >
            See what thousands of bidders are searching for right now. Join the
            action and find your next treasure.
          </motion.p>
        </div>

        {isLoading ? (
          <TrendingSearchSkeleton />
        ) : error || !hasData ? (
          <div className="text-center text-slate-500 dark:text-slate-400 py-12">
            <FontAwesomeIcon
              icon={faSearch}
              className="w-12 h-12 mx-auto mb-4 opacity-30"
            />
            <p>Trending searches coming soon</p>
          </div>
        ) : (
          <motion.div
            variants={container}
            initial="hidden"
            whileInView="visible"
            viewport={{ once: true, margin: "-50px" }}
            className="flex flex-wrap gap-3 md:gap-4 justify-center max-w-5xl mx-auto"
          >
            {trendingSearches.map((search, index) => (
              <motion.div key={`${search.searchTerm}-${index}`} variants={item}>
                <Link
                  href={`${ROUTES.AUCTIONS.LIST}?search=${encodeURIComponent(search.searchTerm)}`}
                  className="group relative"
                >
                  <motion.div
                    whileHover={{ scale: 1.05, y: -2 }}
                    whileTap={{ scale: 0.98 }}
                    className={`
                      relative flex items-center gap-2.5 px-5 py-3 rounded-full
                      font-medium text-sm md:text-base
                      transition-all duration-300
                      ${
                        search.hot
                          ? "bg-gradient-to-r from-orange-500 to-red-500 text-white shadow-lg shadow-orange-500/25 dark:shadow-orange-500/20"
                          : search.trending
                            ? "bg-gradient-to-r from-purple-500 to-blue-500 text-white shadow-lg shadow-purple-500/25 dark:shadow-purple-500/20"
                            : search.isNew
                              ? "bg-gradient-to-r from-emerald-500 to-teal-500 text-white shadow-lg shadow-emerald-500/25 dark:shadow-emerald-500/20"
                              : "bg-white dark:bg-slate-800 text-slate-700 dark:text-slate-200 border border-slate-200 dark:border-slate-700 hover:border-purple-300 dark:hover:border-purple-600 hover:bg-purple-50 dark:hover:bg-slate-700/80 shadow-sm"
                      }
                    `}
                  >
                    <FontAwesomeIcon
                      icon={
                        search.hot
                          ? faFire
                          : search.trending
                            ? faArrowTrendUp
                            : search.isNew
                              ? faBolt
                              : getIconForTerm(search.searchTerm)
                      }
                      className={`w-4 h-4 ${
                        search.hot || search.trending || search.isNew
                          ? "text-white/90"
                          : "text-slate-400 dark:text-slate-500 group-hover:text-purple-500 dark:group-hover:text-purple-400"
                      } transition-colors`}
                    />
                    <span>{search.searchTerm}</span>

                    {search.count && search.count > 0 && (
                      <span
                        className={`text-xs px-2 py-0.5 rounded-full ${
                          search.hot || search.trending || search.isNew
                            ? "bg-white/20 text-white"
                            : "bg-slate-100 dark:bg-slate-700 text-slate-500 dark:text-slate-400"
                        }`}
                      >
                        {search.count.toLocaleString()}
                      </span>
                    )}

                    {(search.hot || search.trending) && (
                      <motion.span
                        initial={{ scale: 0.8 }}
                        animate={{ scale: [0.8, 1.1, 0.8] }}
                        transition={{
                          duration: 2,
                          repeat: Infinity,
                          ease: "easeInOut",
                        }}
                        className="absolute -top-1 -right-1 w-3 h-3 bg-yellow-400 rounded-full border-2 border-white dark:border-slate-900"
                      />
                    )}
                  </motion.div>
                </Link>
              </motion.div>
            ))}
          </motion.div>
        )}

        <motion.div
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          transition={{ duration: 0.5, delay: 0.4 }}
          className="mt-12 md:mt-16 text-center"
        >
          <div className="inline-flex flex-wrap items-center justify-center gap-4 md:gap-8 text-sm text-slate-500 dark:text-slate-400">
            <div className="flex items-center gap-2">
              <span className="w-3 h-3 rounded-full bg-gradient-to-r from-orange-500 to-red-500" />
              <span>Hot</span>
            </div>
            <div className="flex items-center gap-2">
              <span className="w-3 h-3 rounded-full bg-gradient-to-r from-purple-500 to-blue-500" />
              <span>Trending</span>
            </div>
            <div className="flex items-center gap-2">
              <span className="w-3 h-3 rounded-full bg-gradient-to-r from-emerald-500 to-teal-500" />
              <span>New</span>
            </div>
          </div>
        </motion.div>

        <motion.div
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          transition={{ duration: 0.5, delay: 0.5 }}
          className="mt-10 text-center"
        >
          <Link
            href={ROUTES.AUCTIONS.LIST}
            className="inline-flex items-center gap-2 px-6 py-3 rounded-full bg-slate-900 dark:bg-white text-white dark:text-slate-900 font-semibold hover:bg-slate-800 dark:hover:bg-slate-100 transition-all shadow-lg hover:shadow-xl hover:-translate-y-0.5"
          >
            <FontAwesomeIcon icon={faSearch} className="w-4 h-4" />
            <span>Explore All Auctions</span>
            <FontAwesomeIcon icon={faArrowRight} className="w-4 h-4" />
          </Link>
        </motion.div>
      </div>
    </AnimatedSection>
  );
}
