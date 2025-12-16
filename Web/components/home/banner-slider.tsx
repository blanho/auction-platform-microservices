"use client";

import { useCallback } from "react";
import Image from "next/image";
import Link from "next/link";
import { motion, AnimatePresence } from "framer-motion";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faChevronLeft,
  faChevronRight,
  faArrowRight,
} from "@fortawesome/free-solid-svg-icons";
import { Button } from "@/components/ui/button";
import { useCarouselInterval } from "@/hooks/use-interval";
import { cn } from "@/lib/utils";

interface BannerSlide {
  id: string;
  title: string;
  subtitle: string;
  description: string;
  imageUrl: string;
  link: string;
  buttonText: string;
  gradient: string;
}

const BANNER_SLIDES: BannerSlide[] = [
  {
    id: "1",
    title: "Holiday Sale",
    subtitle: "Up to 50% Off",
    description: "Discover amazing deals on premium collectibles",
    imageUrl: "https://images.unsplash.com/photo-1607083206869-4c7672e72a8a?w=1200",
    link: "/deals",
    buttonText: "Shop Now",
    gradient: "from-purple-600/90 to-pink-600/90",
  },
  {
    id: "2",
    title: "New Arrivals",
    subtitle: "Fresh Listings Daily",
    description: "Be the first to bid on the latest items",
    imageUrl: "https://images.unsplash.com/photo-1556742049-0cfed4f6a45d?w=1200",
    link: "/auctions?sort=newest",
    buttonText: "Explore",
    gradient: "from-blue-600/90 to-cyan-600/90",
  },
  {
    id: "3",
    title: "Luxury Watches",
    subtitle: "Exclusive Collection",
    description: "Premium timepieces from top brands",
    imageUrl: "https://images.unsplash.com/photo-1523170335258-f5ed11844a49?w=1200",
    link: "/categories/watches",
    buttonText: "View Collection",
    gradient: "from-amber-600/90 to-orange-600/90",
  },
  {
    id: "4",
    title: "Electronics",
    subtitle: "Tech Deals",
    description: "Latest gadgets at auction prices",
    imageUrl: "https://images.unsplash.com/photo-1468495244123-6c6c332eeece?w=1200",
    link: "/categories/electronics",
    buttonText: "Browse Tech",
    gradient: "from-emerald-600/90 to-teal-600/90",
  },
];

export function BannerSlider() {
  const { activeIndex, setActiveIndex, prev, next, pause, resume } = useCarouselInterval(
    BANNER_SLIDES.length,
    5000
  );

  const handlePrev = useCallback(() => {
    prev();
  }, [prev]);

  const handleNext = useCallback(() => {
    next();
  }, [next]);

  const handleDotClick = useCallback((idx: number) => {
    setActiveIndex(idx);
  }, [setActiveIndex]);

  const currentSlide = BANNER_SLIDES[activeIndex];

  return (
    <div 
      className="relative w-full h-full rounded-xl overflow-hidden group"
      onMouseEnter={pause}
      onMouseLeave={resume}
    >
      <AnimatePresence mode="wait">
        <motion.div
          key={activeIndex}
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          transition={{ duration: 0.5 }}
          className="absolute inset-0"
        >
          <Image
            src={currentSlide.imageUrl}
            alt={currentSlide.title}
            fill
            className="object-cover"
            unoptimized
            priority
          />
          <div className={cn(
            "absolute inset-0 bg-linear-to-r",
            currentSlide.gradient
          )} />
        </motion.div>
      </AnimatePresence>

      <div className="absolute inset-0 flex flex-col justify-center p-6 md:p-8 lg:p-10">
        <AnimatePresence mode="wait">
          <motion.div
            key={activeIndex}
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -20 }}
            transition={{ duration: 0.4, delay: 0.1 }}
            className="max-w-md"
          >
            <span className="inline-block px-3 py-1 mb-3 text-xs font-semibold text-white bg-white/20 backdrop-blur-sm rounded-full">
              {currentSlide.subtitle}
            </span>
            <h2 className="text-2xl md:text-3xl lg:text-4xl font-bold text-white mb-2">
              {currentSlide.title}
            </h2>
            <p className="text-sm md:text-base text-white/90 mb-4">
              {currentSlide.description}
            </p>
            <Button
              asChild
              className="bg-white text-slate-900 hover:bg-white/90 font-semibold shadow-lg"
            >
              <Link href={currentSlide.link}>
                {currentSlide.buttonText}
                <FontAwesomeIcon icon={faArrowRight} className="ml-2 w-3 h-3" />
              </Link>
            </Button>
          </motion.div>
        </AnimatePresence>
      </div>

      <button
        onClick={handlePrev}
        className="absolute left-3 top-1/2 -translate-y-1/2 w-10 h-10 rounded-full bg-white/20 backdrop-blur-sm flex items-center justify-center hover:bg-white/30 transition-colors opacity-0 group-hover:opacity-100 z-10"
      >
        <FontAwesomeIcon icon={faChevronLeft} className="w-4 h-4 text-white" />
      </button>
      <button
        onClick={handleNext}
        className="absolute right-3 top-1/2 -translate-y-1/2 w-10 h-10 rounded-full bg-white/20 backdrop-blur-sm flex items-center justify-center hover:bg-white/30 transition-colors opacity-0 group-hover:opacity-100 z-10"
      >
        <FontAwesomeIcon icon={faChevronRight} className="w-4 h-4 text-white" />
      </button>

      <div className="absolute bottom-4 left-1/2 -translate-x-1/2 flex gap-2 z-10">
        {BANNER_SLIDES.map((_, idx) => (
          <button
            key={idx}
            onClick={() => handleDotClick(idx)}
            className={cn(
              "w-2 h-2 rounded-full transition-all",
              idx === activeIndex 
                ? "bg-white w-6" 
                : "bg-white/50 hover:bg-white/70"
            )}
          />
        ))}
      </div>
    </div>
  );
}

interface PromoBannerProps {
  title: string;
  subtitle: string;
  imageUrl: string;
  link: string;
  gradient?: string;
}

export function PromoBanner({ 
  title, 
  subtitle, 
  imageUrl, 
  link, 
  gradient = "from-blue-600/80 to-purple-600/80" 
}: PromoBannerProps) {
  return (
    <Link href={link} className="block relative w-full h-full rounded-xl overflow-hidden group">
      <Image
        src={imageUrl}
        alt={title}
        fill
        className="object-cover transition-transform duration-500 group-hover:scale-105"
        unoptimized
      />
      <div className={cn(
        "absolute inset-0 bg-linear-to-t",
        gradient
      )} />
      <div className="absolute inset-0 flex flex-col justify-end p-3">
        <span className="text-[10px] text-white/80 mb-0.5">{subtitle}</span>
        <h3 className="text-sm font-bold text-white mb-1.5">{title}</h3>
        <span className="inline-flex items-center gap-1 text-xs text-white font-medium group-hover:underline">
          Shop Now
          <FontAwesomeIcon icon={faArrowRight} className="w-2.5 h-2.5 transition-transform group-hover:translate-x-1" />
        </span>
      </div>
    </Link>
  );
}
