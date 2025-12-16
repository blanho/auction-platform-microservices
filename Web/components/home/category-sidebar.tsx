"use client";

import { useState } from "react";
import Link from "next/link";
import { motion } from "framer-motion";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faCar,
  faClock,
  faGem,
  faPaintBrush,
  faHome,
  faMobileScreen,
  faBagShopping,
  faWandSparkles,
  faChevronRight,
  faLayerGroup,
} from "@fortawesome/free-solid-svg-icons";
import { IconDefinition } from "@fortawesome/fontawesome-svg-core";
import { Skeleton } from "@/components/ui/skeleton";
import { Category } from "@/types/auction";
import { useCategoriesQuery } from "@/hooks/queries";
import { cn } from "@/lib/utils";

const iconMap: Record<string, IconDefinition> = {
  car: faCar,
  watch: faClock,
  gem: faGem,
  art: faPaintBrush,
  home: faHome,
  electronics: faMobileScreen,
  fashion: faBagShopping,
  default: faWandSparkles,
};

const colorMap: Record<string, { bg: string; text: string; hover: string }> = {
  car: { 
    bg: "bg-blue-50 dark:bg-blue-900/20", 
    text: "text-blue-600 dark:text-blue-400",
    hover: "hover:bg-blue-100 dark:hover:bg-blue-900/30"
  },
  watch: { 
    bg: "bg-amber-50 dark:bg-amber-900/20", 
    text: "text-amber-600 dark:text-amber-400",
    hover: "hover:bg-amber-100 dark:hover:bg-amber-900/30"
  },
  gem: { 
    bg: "bg-purple-50 dark:bg-purple-900/20", 
    text: "text-purple-600 dark:text-purple-400",
    hover: "hover:bg-purple-100 dark:hover:bg-purple-900/30"
  },
  art: { 
    bg: "bg-pink-50 dark:bg-pink-900/20", 
    text: "text-pink-600 dark:text-pink-400",
    hover: "hover:bg-pink-100 dark:hover:bg-pink-900/30"
  },
  home: { 
    bg: "bg-emerald-50 dark:bg-emerald-900/20", 
    text: "text-emerald-600 dark:text-emerald-400",
    hover: "hover:bg-emerald-100 dark:hover:bg-emerald-900/30"
  },
  electronics: { 
    bg: "bg-cyan-50 dark:bg-cyan-900/20", 
    text: "text-cyan-600 dark:text-cyan-400",
    hover: "hover:bg-cyan-100 dark:hover:bg-cyan-900/30"
  },
  fashion: { 
    bg: "bg-rose-50 dark:bg-rose-900/20", 
    text: "text-rose-600 dark:text-rose-400",
    hover: "hover:bg-rose-100 dark:hover:bg-rose-900/30"
  },
  default: { 
    bg: "bg-indigo-50 dark:bg-indigo-900/20", 
    text: "text-indigo-600 dark:text-indigo-400",
    hover: "hover:bg-indigo-100 dark:hover:bg-indigo-900/30"
  },
};

interface CategorySidebarProps {
  onCategoryHover?: (categoryId: string | null) => void;
}

export function CategorySidebar({ onCategoryHover }: CategorySidebarProps) {
  const { data: categories = [], isLoading } = useCategoriesQuery();
  const [hoveredCategory, setHoveredCategory] = useState<string | null>(null);

  const handleMouseEnter = (categoryId: string) => {
    setHoveredCategory(categoryId);
    onCategoryHover?.(categoryId);
  };

  const handleMouseLeave = () => {
    setHoveredCategory(null);
    onCategoryHover?.(null);
  };

  if (isLoading) {
    return (
      <div className="bg-white dark:bg-slate-900 rounded-xl border border-slate-200 dark:border-slate-800 p-2 h-full">
        <div className="flex items-center gap-2 mb-2 pb-2 border-b border-slate-200 dark:border-slate-700">
          <Skeleton className="w-4 h-4 rounded" />
          <Skeleton className="w-16 h-4" />
        </div>
        <div className="space-y-0.5">
          {Array.from({ length: 10 }).map((_, i) => (
            <div key={i} className="flex items-center gap-2 p-2">
              <Skeleton className="w-6 h-6 rounded-md" />
              <Skeleton className="w-full h-3" />
            </div>
          ))}
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white dark:bg-slate-900 rounded-xl border border-slate-200 dark:border-slate-800 h-full overflow-hidden">
      <div className="flex items-center gap-2 px-3 py-2.5 border-b border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-800/50">
        <FontAwesomeIcon icon={faLayerGroup} className="w-3.5 h-3.5 text-purple-600 dark:text-purple-400" />
        <h3 className="font-semibold text-slate-900 dark:text-white text-xs">Categories</h3>
      </div>
      
      <nav className="p-1.5 overflow-y-auto max-h-[340px]">
        <ul className="space-y-0">
          {categories.map((category, index) => {
            const iconKey = category.icon?.toLowerCase() || "default";
            const icon = iconMap[iconKey] || iconMap.default;
            const colors = colorMap[iconKey] || colorMap.default;
            const isHovered = hoveredCategory === category.id;

            return (
              <motion.li
                key={category.id}
                initial={{ opacity: 0, x: -10 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ duration: 0.2, delay: index * 0.03 }}
              >
                <Link
                  href={`/auctions?category=${category.slug}`}
                  className={cn(
                    "flex items-center gap-2 px-2 py-2 rounded-lg transition-all duration-200 group",
                    isHovered 
                      ? `${colors.bg} ${colors.hover}` 
                      : "hover:bg-slate-100 dark:hover:bg-slate-800"
                  )}
                  onMouseEnter={() => handleMouseEnter(category.id)}
                  onMouseLeave={handleMouseLeave}
                >
                  <div className={cn(
                    "w-6 h-6 rounded-md flex items-center justify-center transition-colors shrink-0",
                    isHovered ? colors.bg : "bg-slate-100 dark:bg-slate-800"
                  )}>
                    <FontAwesomeIcon 
                      icon={icon} 
                      className={cn(
                        "w-3 h-3 transition-colors",
                        isHovered ? colors.text : "text-slate-500 dark:text-slate-400"
                      )} 
                    />
                  </div>
                  <span className={cn(
                    "flex-1 text-xs font-medium transition-colors truncate",
                    isHovered 
                      ? "text-slate-900 dark:text-white" 
                      : "text-slate-700 dark:text-slate-300"
                  )}>
                    {category.name}
                  </span>
                  <FontAwesomeIcon 
                    icon={faChevronRight} 
                    className={cn(
                      "w-2.5 h-2.5 transition-all duration-200 shrink-0",
                      isHovered 
                        ? `${colors.text} translate-x-0.5` 
                        : "text-slate-400 dark:text-slate-500 opacity-0 group-hover:opacity-100"
                    )} 
                  />
                </Link>
              </motion.li>
            );
          })}
        </ul>
      </nav>
    </div>
  );
}
