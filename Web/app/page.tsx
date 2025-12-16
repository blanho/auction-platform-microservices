"use client";

import { Header } from "@/components/layout/header";
import {
  HeroSectionNew,
  QuickAccessBar,
  TopDealsCarousel,
  LiveAuctionTicker,
  FlashSaleSection,
  BrandSliderSection,
  FeaturedAuctionsSection,
  EndingSoonSection,
  NewArrivalsSection,
  StatsCounterSection,
  TrendingSearchesSection,
  FooterSection,
} from "@/components/home";

export default function Home() {
  return (
    <div className="flex min-h-screen flex-col">
      <Header />
      <main className="flex-1">
        <HeroSectionNew />
        <QuickAccessBar />
        <LiveAuctionTicker />
        <section id="flash-sale">
          <FlashSaleSection />
        </section>
        <section id="top-deals">
          <TopDealsCarousel />
        </section>
        <section id="brands">
          <BrandSliderSection />
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
        <section id="stats">
          <StatsCounterSection />
        </section>
        <section id="trending">
          <TrendingSearchesSection />
        </section>
      </main>
      <FooterSection />
    </div>
  );
}
