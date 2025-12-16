"use client";

import { useState, useEffect } from "react";
import Image from "next/image";
import Link from "next/link";
import { motion } from "framer-motion";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faArrowRight,
  faCrown,
} from "@fortawesome/free-solid-svg-icons";
import { Skeleton } from "@/components/ui/skeleton";
import { Badge } from "@/components/ui/badge";
import { Card } from "@/components/ui/card";
import {
  Carousel,
  CarouselContent,
  CarouselItem,
  CarouselPrevious,
  CarouselNext,
} from "@/components/ui/carousel";
import { Brand } from "@/types/auction";
import { auctionService } from "@/services/auction.service";
import { cn } from "@/lib/utils";

interface BrandCardProps {
  brand: Brand;
  index: number;
}

function BrandCard({ brand, index }: BrandCardProps) {
  const gradients = [
    "from-purple-500/10 to-pink-500/10 hover:from-purple-500/20 hover:to-pink-500/20 border-purple-200 dark:border-purple-800",
    "from-blue-500/10 to-cyan-500/10 hover:from-blue-500/20 hover:to-cyan-500/20 border-blue-200 dark:border-blue-800",
    "from-amber-500/10 to-orange-500/10 hover:from-amber-500/20 hover:to-orange-500/20 border-amber-200 dark:border-amber-800",
    "from-emerald-500/10 to-teal-500/10 hover:from-emerald-500/20 hover:to-teal-500/20 border-emerald-200 dark:border-emerald-800",
    "from-rose-500/10 to-red-500/10 hover:from-rose-500/20 hover:to-red-500/20 border-rose-200 dark:border-rose-800",
    "from-indigo-500/10 to-violet-500/10 hover:from-indigo-500/20 hover:to-violet-500/20 border-indigo-200 dark:border-indigo-800",
  ];

  const gradient = gradients[index % gradients.length];

  const defaultLogos: Record<string, string> = {
    Apple: "https://upload.wikimedia.org/wikipedia/commons/f/fa/Apple_logo_black.svg",
    Samsung: "https://upload.wikimedia.org/wikipedia/commons/2/24/Samsung_Logo.svg",
    Sony: "https://upload.wikimedia.org/wikipedia/commons/c/ca/Sony_logo.svg",
    Nike: "https://upload.wikimedia.org/wikipedia/commons/a/a6/Logo_NIKE.svg",
    Rolex: "https://upload.wikimedia.org/wikipedia/en/9/95/Rolex_logo.svg",
    Louis: "https://upload.wikimedia.org/wikipedia/commons/7/76/Louis_Vuitton_logo_and_wordmark.svg",
  };

  const logoUrl = brand.logoUrl || defaultLogos[brand.name] || null;

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.3, delay: index * 0.05 }}
    >
      <Link href={`/brands/${brand.slug}`} className="group block">
        <Card
          className={cn(
            "relative h-[100px] md:h-[120px] p-0",
            "bg-gradient-to-br",
            "flex flex-col items-center justify-center gap-2",
            "transition-all duration-300 hover:scale-105 hover:shadow-xl",
            gradient
          )}
        >
          {brand.auctionCount > 50 && (
            <div className="absolute -top-2 -right-2">
              <Badge className="bg-gradient-to-r from-amber-400 to-orange-500 text-white text-[9px] px-1.5 py-0.5 border-0 shadow-md">
                <FontAwesomeIcon icon={faCrown} className="w-2 h-2 mr-0.5" />
                TOP
              </Badge>
            </div>
          )}

          <div className="w-16 h-10 md:w-20 md:h-12 relative flex items-center justify-center">
            {logoUrl ? (
              <Image
                src={logoUrl}
                alt={brand.name}
                fill
                className="object-contain dark:invert opacity-70 group-hover:opacity-100 transition-opacity"
                unoptimized
              />
            ) : (
              <span className="text-xl md:text-2xl font-bold text-slate-700 dark:text-slate-300 group-hover:text-slate-900 dark:group-hover:text-white transition-colors">
                {brand.name.substring(0, 2).toUpperCase()}
              </span>
            )}
          </div>

          <div className="text-center">
            <p className="text-xs font-medium text-slate-700 dark:text-slate-300 group-hover:text-slate-900 dark:group-hover:text-white transition-colors">
              {brand.name}
            </p>
            <p className="text-[10px] text-slate-500 dark:text-slate-400">
              {brand.auctionCount} items
            </p>
          </div>

          <div className="absolute bottom-2 right-2 opacity-0 group-hover:opacity-100 transition-opacity">
            <div className="w-6 h-6 rounded-full bg-white dark:bg-slate-800 shadow flex items-center justify-center">
              <FontAwesomeIcon icon={faArrowRight} className="w-2.5 h-2.5 text-slate-600 dark:text-slate-400" />
            </div>
          </div>
        </Card>
      </Link>
    </motion.div>
  );
}

function BrandCardSkeleton() {
  return (
    <Skeleton className="h-[100px] md:h-[120px] rounded-xl bg-slate-200 dark:bg-slate-800" />
  );
}

const FALLBACK_BRANDS: Brand[] = [
  { id: "1", name: "Apple", slug: "apple", isActive: true, auctionCount: 156 },
  { id: "2", name: "Samsung", slug: "samsung", isActive: true, auctionCount: 89 },
  { id: "3", name: "Sony", slug: "sony", isActive: true, auctionCount: 67 },
  { id: "4", name: "Nike", slug: "nike", isActive: true, auctionCount: 234 },
  { id: "5", name: "Rolex", slug: "rolex", isActive: true, auctionCount: 45 },
  { id: "6", name: "Louis Vuitton", slug: "louis-vuitton", isActive: true, auctionCount: 78 },
  { id: "7", name: "Gucci", slug: "gucci", isActive: true, auctionCount: 92 },
  { id: "8", name: "Canon", slug: "canon", isActive: true, auctionCount: 53 },
];

export function BrandSliderSection() {
  const [brands, setBrands] = useState<Brand[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchBrands = async () => {
      try {
        const data = await auctionService.getBrands();
        setBrands(data.length > 0 ? data : FALLBACK_BRANDS);
      } catch {
        setBrands(FALLBACK_BRANDS);
      } finally {
        setIsLoading(false);
      }
    };
    fetchBrands();
  }, []);

  return (
    <section className="py-8 bg-slate-50 dark:bg-slate-950">
      <div className="container mx-auto px-4">
        <div className="flex items-center justify-between mb-6">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-purple-500 to-pink-500 flex items-center justify-center shadow-lg shadow-purple-500/20">
              <FontAwesomeIcon icon={faCrown} className="w-5 h-5 text-white" />
            </div>
            <div>
              <h2 className="text-lg md:text-xl font-bold text-slate-900 dark:text-white">
                Shop by Brand
              </h2>
              <p className="text-xs text-slate-500 dark:text-slate-400">
                Discover authentic items from top brands
              </p>
            </div>
          </div>
        </div>

        <Carousel
          opts={{
            align: "start",
            loop: true,
          }}
          className="w-full"
        >
          <CarouselContent className="-ml-3">
            {isLoading
              ? Array.from({ length: 8 }).map((_, i) => (
                  <CarouselItem key={i} className="pl-3 basis-[45%] sm:basis-1/3 md:basis-1/4 lg:basis-1/5 xl:basis-1/6">
                    <BrandCardSkeleton />
                  </CarouselItem>
                ))
              : brands.map((brand, index) => (
                  <CarouselItem key={brand.id} className="pl-3 basis-[45%] sm:basis-1/3 md:basis-1/4 lg:basis-1/5 xl:basis-1/6">
                    <BrandCard brand={brand} index={index} />
                  </CarouselItem>
                ))}
          </CarouselContent>
          <CarouselPrevious className="left-2 bg-white dark:bg-slate-800 hover:bg-slate-50 dark:hover:bg-slate-700 shadow-lg" />
          <CarouselNext className="right-2 bg-white dark:bg-slate-800 hover:bg-slate-50 dark:hover:bg-slate-700 shadow-lg" />
        </Carousel>

        <div className="mt-6 text-center">
          <Link
            href="/brands"
            className="inline-flex items-center gap-2 text-sm font-medium text-purple-600 hover:text-purple-700 dark:text-purple-400 dark:hover:text-purple-300 transition-colors"
          >
            View All Brands
            <FontAwesomeIcon icon={faArrowRight} className="w-3 h-3" />
          </Link>
        </div>
      </div>
    </section>
  );
}
