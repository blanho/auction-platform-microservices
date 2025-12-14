"use client";

import { useEffect, useState } from "react";
import { motion } from "framer-motion";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faShieldHalved,
  faTruck,
  faAward,
  faClock,
  faCircleCheck,
  faHeadset,
  faUsers,
  faGavel,
  faBolt,
} from "@fortawesome/free-solid-svg-icons";
import { IconDefinition } from "@fortawesome/fontawesome-svg-core";
import { useQuickStats } from "@/hooks/use-analytics";

interface ValueProp {
  icon: IconDefinition;
  title: string;
  description: string;
  gradient: string;
}

const VALUE_PROPS: ValueProp[] = [
  {
    icon: faShieldHalved,
    title: "Buyer Protection",
    description: "100% money-back guarantee",
    gradient: "from-emerald-500 to-teal-500",
  },
  {
    icon: faCircleCheck,
    title: "Verified Sellers",
    description: "Every seller is vetted",
    gradient: "from-blue-500 to-cyan-500",
  },
  {
    icon: faTruck,
    title: "Secure Shipping",
    description: "Tracked & insured",
    gradient: "from-purple-500 to-violet-500",
  },
  {
    icon: faAward,
    title: "Authenticity",
    description: "Expert verification",
    gradient: "from-amber-500 to-orange-500",
  },
];

const container = {
  hidden: { opacity: 0 },
  visible: {
    opacity: 1,
    transition: { staggerChildren: 0.08 },
  },
};

const item = {
  hidden: { opacity: 0, y: 10 },
  visible: { opacity: 1, y: 0, transition: { duration: 0.4 } },
};

export function ValuePropsBanner() {
  const { data: stats, isLoading } = useQuickStats();
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
  }, []);

  if (!mounted) return null;

  return (
    <section className="py-4 md:py-6 bg-gradient-to-r from-slate-50 via-white to-slate-50 dark:from-slate-900 dark:via-slate-950 dark:to-slate-900 border-y border-slate-200/80 dark:border-slate-800/80 overflow-hidden">
      <div className="container mx-auto px-4">
        <motion.div
          variants={container}
          initial="hidden"
          whileInView="visible"
          viewport={{ once: true }}
          className="flex flex-wrap items-center justify-center gap-x-6 gap-y-4 md:gap-x-10 lg:gap-x-14"
        >
          {!isLoading && stats && (
            <>
              <motion.div variants={item} className="flex items-center gap-2.5">
                <div className="w-9 h-9 rounded-lg bg-gradient-to-br from-green-500 to-emerald-600 flex items-center justify-center shadow-sm shadow-green-500/30">
                  <FontAwesomeIcon
                    icon={faGavel}
                    className="w-4 h-4 text-white"
                  />
                </div>
                <div>
                  <p className="text-base md:text-lg font-bold text-slate-900 dark:text-white leading-tight">
                    {stats.liveAuctions.toLocaleString()}+
                  </p>
                  <p className="text-xs text-slate-500 dark:text-slate-400">
                    Live Auctions
                  </p>
                </div>
              </motion.div>

              <motion.div variants={item} className="flex items-center gap-2.5">
                <div className="w-9 h-9 rounded-lg bg-gradient-to-br from-blue-500 to-indigo-600 flex items-center justify-center shadow-sm shadow-blue-500/30">
                  <FontAwesomeIcon
                    icon={faUsers}
                    className="w-4 h-4 text-white"
                  />
                </div>
                <div>
                  <p className="text-base md:text-lg font-bold text-slate-900 dark:text-white leading-tight">
                    {stats.activeUsers.toLocaleString()}+
                  </p>
                  <p className="text-xs text-slate-500 dark:text-slate-400">
                    Active Users
                  </p>
                </div>
              </motion.div>

              <motion.div variants={item} className="flex items-center gap-2.5">
                <div className="w-9 h-9 rounded-lg bg-gradient-to-br from-orange-500 to-red-600 flex items-center justify-center shadow-sm shadow-orange-500/30">
                  <FontAwesomeIcon
                    icon={faBolt}
                    className="w-4 h-4 text-white"
                  />
                </div>
                <div>
                  <p className="text-base md:text-lg font-bold text-slate-900 dark:text-white leading-tight">
                    {stats.endingSoon}
                  </p>
                  <p className="text-xs text-slate-500 dark:text-slate-400">
                    Ending Soon
                  </p>
                </div>
              </motion.div>

              <div className="hidden lg:block w-px h-10 bg-slate-200 dark:bg-slate-700" />
            </>
          )}

          {VALUE_PROPS.map((prop, index) => (
            <motion.div
              key={index}
              variants={item}
              className="flex items-center gap-2.5 group"
            >
              <div
                className={`w-9 h-9 rounded-lg bg-gradient-to-br ${prop.gradient} flex items-center justify-center shadow-sm transition-transform group-hover:scale-110`}
              >
                <FontAwesomeIcon
                  icon={prop.icon}
                  className="w-4 h-4 text-white"
                />
              </div>
              <div>
                <p className="text-sm font-semibold text-slate-900 dark:text-white leading-tight">
                  {prop.title}
                </p>
                <p className="text-xs text-slate-500 dark:text-slate-400">
                  {prop.description}
                </p>
              </div>
            </motion.div>
          ))}
        </motion.div>
      </div>
    </section>
  );
}
