"use client";

import { useState } from "react";
import { CategorySidebar } from "./category-sidebar";
import { BannerSlider, PromoBanner } from "./banner-slider";

export function HeroSectionNew() {
  const [, setHoveredCategory] = useState<string | null>(null);

  return (
    <section className="bg-slate-50 dark:bg-slate-950">
      <div className="container mx-auto px-4 py-6">
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-3 lg:gap-4">
          <div className="hidden lg:block lg:col-span-2">
            <div className="sticky top-24 h-[380px]">
              <CategorySidebar onCategoryHover={setHoveredCategory} />
            </div>
          </div>

          <div className="lg:col-span-8">
            <div className="h-[280px] md:h-[340px] lg:h-[380px]">
              <BannerSlider />
            </div>
          </div>

          <div className="lg:col-span-2 space-y-3">
            <div className="h-[180px] lg:h-[184px]">
              <PromoBanner
                title="Christmas Sale"
                subtitle="Up to 50% Off"
                imageUrl="https://images.unsplash.com/photo-1482517967863-00e15c9b44be?w=600"
                link="/deals?type=christmas"
                gradient="from-red-600/80 to-green-600/80"
              />
            </div>
            <div className="hidden lg:block h-[184px]">
              <PromoBanner
                title="Tech Deals"
                subtitle="Electronics & Gadgets"
                imageUrl="https://images.unsplash.com/photo-1519389950473-47ba0277781c?w=600"
                link="/categories/electronics"
                gradient="from-blue-600/80 to-cyan-600/80"
              />
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
