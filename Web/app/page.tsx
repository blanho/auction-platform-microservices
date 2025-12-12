"use client";

import { Header } from "@/components/layout/header";
import {
  HeroSection,
  TrustSection,
  CategoriesSection,
  FeaturedAuctionsSection,
  EndingSoonSection,
  HowItWorksSection,
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
        <TrustSection />
        <CategoriesSection />
        <FeaturedAuctionsSection />
        <EndingSoonSection />
        <HowItWorksSection />
        <TestimonialsSection />
        <PersonalizationSection />
      </main>
      <FooterSection />
    </div>
  );
}
