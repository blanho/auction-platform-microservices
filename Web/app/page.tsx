"use client";

import { Header } from "@/components/layout/header";
import {
  HeroSection,
  LiveAuctionTicker,
  ValuePropsBanner,
  CategoriesSection,
  FeaturedAuctionsSection,
  EndingSoonSection,
  NewArrivalsSection,
  StatsCounterSection,
  TrendingSearchesSection,
  PersonalizationSection,
  TopAuctionsSection,
  FooterSection,
} from "@/components/home";
import { TableOfContents } from "@/components/home/table-of-contents";

export default function Home() {
  return (
    <div className="flex min-h-screen flex-col">
      <Header />
      <main className="flex-1">
        <section id="hero">
          <HeroSection />
        </section>
        <LiveAuctionTicker />
        <ValuePropsBanner />
        <section id="categories">
          <CategoriesSection />
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
        <section id="personalization">
          <PersonalizationSection />
        </section>
        <section id="top-auctions">
          <TopAuctionsSection />
        </section>
      </main>
      <FooterSection />
      <TableOfContents />
    </div>
  );
}
