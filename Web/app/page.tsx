"use client";

import { Header } from "@/components/layout/header";
import {
  HeroSection,
  LiveAuctionTicker,
  ValuePropsBanner,
  CategoriesSection,
  FeaturedAuctionsSection,
  EndingSoonSection,
  HowItWorksSection,
  StatsCounterSection,
  TestimonialsSection,
  PersonalizationSection,
  FooterSection,
} from "@/components/home";

export default function Home() {
  return (
    <div className="flex min-h-screen flex-col">
      <Header />
      <main className="flex-1">
        <HeroSection />
        <LiveAuctionTicker />
        <ValuePropsBanner />
        <CategoriesSection />
        <FeaturedAuctionsSection />
        <EndingSoonSection />
        <StatsCounterSection />
        <HowItWorksSection />
        <TestimonialsSection />
        <PersonalizationSection />
      </main>
      <FooterSection />
    </div>
  );
}
