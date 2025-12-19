"use client";

import { useState } from "react";
import { Header } from "@/components/layout/header";
import {
  HeroSectionNew,
  QuickAccessBar,
  TopDealsCarousel,
  LiveAuctionTicker,
  BrandSliderSection,
  FeaturedAuctionsSection,
  EndingSoonSection,
  NewArrivalsSection,
  StatsCounterSection,
  TrendingSearchesSection,
  FooterSection,
  ValuePropsBanner,
  CategoriesSection,
  CTASection,
} from "@/components/home";
import { Button } from "@/components/ui/button";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faChevronDown, faWandSparkles } from "@fortawesome/free-solid-svg-icons";
import { motion, AnimatePresence } from "framer-motion";

export default function Home() {
  const [showMoreSections, setShowMoreSections] = useState(false);

  return (
    <div className="flex min-h-screen flex-col">
      <Header />
      <main className="flex-1 bg-slate-50 dark:bg-slate-950">
        <HeroSectionNew />
        <ValuePropsBanner />
        <QuickAccessBar />
        <LiveAuctionTicker />
        
        <section id="top-deals" className="py-8 md:py-12">
          <TopDealsCarousel />
        </section>

        <section id="categories" className="py-8 md:py-12 bg-white dark:bg-slate-900">
          <CategoriesSection />
        </section>

        <section id="featured" className="py-8 md:py-12">
          <FeaturedAuctionsSection />
        </section>

        <section id="ending-soon" className="py-8 md:py-12 bg-white dark:bg-slate-900">
          <EndingSoonSection />
        </section>

        <section id="new-arrivals" className="py-8 md:py-12">
          <NewArrivalsSection />
        </section>

        <AnimatePresence>
          {!showMoreSections && (
            <motion.div
              initial={{ opacity: 1 }}
              exit={{ opacity: 0, height: 0 }}
              className="py-16 bg-linear-to-b from-slate-50 to-white dark:from-slate-950 dark:to-slate-900"
            >
              <div className="container mx-auto px-4">
                <div className="flex flex-col items-center gap-6">
                  <div className="w-16 h-1 bg-linear-to-r from-purple-500 to-pink-500 rounded-full opacity-50"></div>
                  <p className="text-slate-500 dark:text-slate-400 text-base font-medium">
                    Discover more features & insights
                  </p>
                  <Button
                    onClick={() => setShowMoreSections(true)}
                    variant="outline"
                    size="lg"
                    className="gap-3 rounded-full px-8 py-6 text-base bg-white dark:bg-slate-900 shadow-lg hover:shadow-xl transition-all hover:scale-105 border-slate-200 dark:border-slate-700 group"
                  >
                    <FontAwesomeIcon
                      icon={faWandSparkles}
                      className="w-4 h-4 text-purple-500 group-hover:text-purple-600"
                    />
                    <span>Explore Brands, Stats & Trending</span>
                    <FontAwesomeIcon
                      icon={faChevronDown}
                      className="w-4 h-4 text-slate-400 group-hover:translate-y-0.5 transition-transform"
                    />
                  </Button>
                </div>
              </div>
            </motion.div>
          )}
        </AnimatePresence>

        <AnimatePresence>
          {showMoreSections && (
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.5, ease: "easeOut" }}
            >
              <section id="brands" className="py-8 md:py-12 bg-white dark:bg-slate-900">
                <BrandSliderSection />
              </section>
              <section id="stats" className="py-8 md:py-12">
                <StatsCounterSection />
              </section>
              <section id="trending" className="py-8 md:py-12 bg-white dark:bg-slate-900">
                <TrendingSearchesSection />
              </section>
            </motion.div>
          )}
        </AnimatePresence>

        <CTASection />
      </main>
      <FooterSection />
    </div>
  );
}
