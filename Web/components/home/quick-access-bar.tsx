"use client";

import Link from "next/link";
import { motion } from "framer-motion";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faCrown,
  faBolt,
  faTicket,
  faGift,
  faHome,
  faFire,
  faSpa,
  faTag,
  faPercent,
  faTrophy,
} from "@fortawesome/free-solid-svg-icons";
import { IconDefinition } from "@fortawesome/fontawesome-svg-core";
import { cn } from "@/lib/utils";

interface QuickAccessItem {
  id: string;
  label: string;
  icon: IconDefinition;
  href: string;
  gradient: string;
  bgColor: string;
}

const QUICK_ACCESS_ITEMS: QuickAccessItem[] = [
  {
    id: "vip",
    label: "VIP Deals",
    icon: faCrown,
    href: "/deals?type=vip",
    gradient: "from-amber-400 to-yellow-300",
    bgColor: "bg-amber-50 dark:bg-amber-900/20",
  },
  {
    id: "live",
    label: "Live Auctions",
    icon: faBolt,
    href: "/live",
    gradient: "from-green-500 to-emerald-400",
    bgColor: "bg-green-50 dark:bg-green-900/20",
  },
  {
    id: "coupons",
    label: "Hot Coupons",
    icon: faTicket,
    href: "/deals?type=coupons",
    gradient: "from-purple-500 to-violet-400",
    bgColor: "bg-purple-50 dark:bg-purple-900/20",
  },
  {
    id: "gifts",
    label: "Gift Ideas",
    icon: faGift,
    href: "/gifts",
    gradient: "from-red-500 to-rose-400",
    bgColor: "bg-red-50 dark:bg-red-900/20",
  },
  {
    id: "home",
    label: "Home & Living",
    icon: faHome,
    href: "/categories/home",
    gradient: "from-orange-500 to-amber-400",
    bgColor: "bg-orange-50 dark:bg-orange-900/20",
  },
  {
    id: "trending",
    label: "Trending Now",
    icon: faFire,
    href: "/auctions?sort=trending",
    gradient: "from-pink-500 to-rose-400",
    bgColor: "bg-pink-50 dark:bg-pink-900/20",
  },
  {
    id: "wellness",
    label: "Wellness",
    icon: faSpa,
    href: "/categories/wellness",
    gradient: "from-teal-500 to-cyan-400",
    bgColor: "bg-teal-50 dark:bg-teal-900/20",
  },
  {
    id: "clearance",
    label: "Clearance",
    icon: faTag,
    href: "/deals?type=clearance",
    gradient: "from-blue-500 to-indigo-400",
    bgColor: "bg-blue-50 dark:bg-blue-900/20",
  },
  {
    id: "flash",
    label: "Flash Sale",
    icon: faPercent,
    href: "/deals?type=flash",
    gradient: "from-rose-500 to-red-400",
    bgColor: "bg-rose-50 dark:bg-rose-900/20",
  },
  {
    id: "premium",
    label: "Premium",
    icon: faTrophy,
    href: "/auctions?featured=true",
    gradient: "from-indigo-500 to-purple-400",
    bgColor: "bg-indigo-50 dark:bg-indigo-900/20",
  },
];

const container = {
  hidden: { opacity: 0 },
  visible: {
    opacity: 1,
    transition: { staggerChildren: 0.05 },
  },
};

const item = {
  hidden: { opacity: 0, y: 10 },
  visible: { opacity: 1, y: 0, transition: { duration: 0.3 } },
};

export function QuickAccessBar() {
  return (
    <div className="bg-white dark:bg-slate-900 border-y border-slate-200 dark:border-slate-800 py-4 overflow-hidden">
      <div className="container mx-auto px-4">
        <motion.div
          variants={container}
          initial="hidden"
          whileInView="visible"
          viewport={{ once: true }}
          className="flex items-center gap-4 overflow-x-auto pb-2 scrollbar-hide"
        >
          {QUICK_ACCESS_ITEMS.map((accessItem) => (
            <motion.div key={accessItem.id} variants={item} className="shrink-0">
              <Link
                href={accessItem.href}
                className="flex flex-col items-center gap-2 group"
              >
                <div className={cn(
                  "w-14 h-14 md:w-16 md:h-16 rounded-2xl flex items-center justify-center transition-all duration-300",
                  accessItem.bgColor,
                  "group-hover:scale-110 group-hover:shadow-lg"
                )}>
                  <div className={cn(
                    "w-10 h-10 md:w-12 md:h-12 rounded-xl bg-linear-to-br flex items-center justify-center shadow-sm",
                    accessItem.gradient
                  )}>
                    <FontAwesomeIcon 
                      icon={accessItem.icon} 
                      className="w-5 h-5 md:w-6 md:h-6 text-white" 
                    />
                  </div>
                </div>
                <span className="text-xs font-medium text-slate-700 dark:text-slate-300 text-center whitespace-nowrap group-hover:text-slate-900 dark:group-hover:text-white transition-colors">
                  {accessItem.label}
                </span>
              </Link>
            </motion.div>
          ))}
        </motion.div>
      </div>
    </div>
  );
}
