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
      <main className="flex-1">
        <HeroSectionNew />
        <QuickAccessBar />
        <LiveAuctionTicker />
        <section id="top-deals">
          <TopDealsCarousel />
        </section>
        <section id="featured">
          <FeaturedAuctionsSection />
        </section>
        <section id="ending-soon">
          <EndingSoonSection />
        </section>
        <section id="new-arrivals">
          <NewArrivalsSection />
        </section>

        <AnimatePresence>
          {!showMoreSections && (
            <motion.div
              initial={{ opacity: 1 }}
              exit={{ opacity: 0, height: 0 }}
              className="py-12 bg-gradient-to-b from-white to-slate-50 dark:from-slate-900 dark:to-slate-950"
            >
              <div className="container mx-auto px-4">
                <div className="flex flex-col items-center gap-4">
                  <p className="text-slate-500 dark:text-slate-400 text-sm">
                    Discover more features
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
              <section id="brands">
                <BrandSliderSection />
              </section>
              <section id="stats">
                <StatsCounterSection />
              </section>
              <section id="trending">
                <TrendingSearchesSection />
              </section>
            </motion.div>
          )}
        </AnimatePresence>
      </main>
      <FooterSection />
    </div>
  );
}
