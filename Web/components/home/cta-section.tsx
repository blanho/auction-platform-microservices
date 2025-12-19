"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import Image from "next/image";
import { motion, useMotionValue, useTransform, animate } from "framer-motion";
import { Button } from "@/components/ui/button";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faRocket,
  faArrowRight,
  faStar,
  faShieldHalved,
  faBolt,
  faGem,
  faCrown,
  faCheck,
  faUsers,
  faGavel,
} from "@fortawesome/free-solid-svg-icons";
import { cn } from "@/lib/utils";

function AnimatedCounter({ value, duration = 2 }: { value: number; duration?: number }) {
  const count = useMotionValue(0);
  const rounded = useTransform(count, (latest) => Math.round(latest));
  const [displayValue, setDisplayValue] = useState(0);

  useEffect(() => {
    const controls = animate(count, value, { duration });
    const unsubscribe = rounded.on("change", (v) => setDisplayValue(v));
    return () => {
      controls.stop();
      unsubscribe();
    };
  }, [count, rounded, value, duration]);

  return <span>{displayValue.toLocaleString()}</span>;
}

const floatingAvatars = [
  { id: 1, src: "https://i.pravatar.cc/100?img=1", delay: 0 },
  { id: 2, src: "https://i.pravatar.cc/100?img=2", delay: 0.2 },
  { id: 3, src: "https://i.pravatar.cc/100?img=3", delay: 0.4 },
  { id: 4, src: "https://i.pravatar.cc/100?img=4", delay: 0.6 },
  { id: 5, src: "https://i.pravatar.cc/100?img=5", delay: 0.8 },
];

const features = [
  { icon: faShieldHalved, text: "Buyer Protection" },
  { icon: faBolt, text: "Instant Notifications" },
  { icon: faGem, text: "Exclusive Drops" },
];

const particlePositions = [
  { left: 5, top: 10, duration: 3.2, delay: 0.1 },
  { left: 15, top: 25, duration: 4.1, delay: 0.5 },
  { left: 25, top: 8, duration: 3.8, delay: 1.2 },
  { left: 35, top: 45, duration: 4.5, delay: 0.3 },
  { left: 45, top: 20, duration: 3.5, delay: 1.8 },
  { left: 55, top: 60, duration: 4.2, delay: 0.7 },
  { left: 65, top: 15, duration: 3.9, delay: 1.5 },
  { left: 75, top: 35, duration: 4.8, delay: 0.2 },
  { left: 85, top: 50, duration: 3.3, delay: 1.1 },
  { left: 95, top: 5, duration: 4.0, delay: 0.9 },
  { left: 10, top: 70, duration: 3.6, delay: 1.6 },
  { left: 20, top: 85, duration: 4.3, delay: 0.4 },
  { left: 30, top: 65, duration: 3.7, delay: 1.9 },
  { left: 40, top: 90, duration: 4.6, delay: 0.6 },
  { left: 50, top: 75, duration: 3.4, delay: 1.3 },
  { left: 60, top: 80, duration: 4.4, delay: 0.8 },
  { left: 70, top: 55, duration: 3.1, delay: 1.7 },
  { left: 80, top: 95, duration: 4.7, delay: 0.0 },
  { left: 90, top: 40, duration: 3.0, delay: 1.4 },
  { left: 98, top: 30, duration: 4.9, delay: 1.0 },
];

export function CTASection() {
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    const timer = setTimeout(() => setMounted(true), 0);
    return () => clearTimeout(timer);
  }, []);

  return (
    <section className="py-24 md:py-32 relative overflow-hidden bg-linear-to-b from-slate-50 via-purple-50/30 to-slate-100 dark:from-slate-950 dark:via-purple-950/20 dark:to-slate-900">
      <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top,rgba(168,85,247,0.12),transparent_60%)] dark:bg-[radial-gradient(ellipse_at_top,rgba(168,85,247,0.08),transparent_60%)]" />
      <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_bottom_right,rgba(59,130,246,0.08),transparent_50%)] dark:bg-[radial-gradient(ellipse_at_bottom_right,rgba(59,130,246,0.05),transparent_50%)]" />
      
      <motion.div
        animate={{ y: [0, -20, 0], rotate: [0, 5, 0] }}
        transition={{ duration: 8, repeat: Infinity, ease: "easeInOut" }}
        className="absolute top-20 left-[10%] w-72 h-72 bg-linear-to-br from-purple-400/30 to-pink-400/20 dark:from-purple-500/20 dark:to-pink-500/10 rounded-full blur-3xl"
      />
      <motion.div
        animate={{ y: [0, 20, 0], rotate: [0, -5, 0] }}
        transition={{ duration: 10, repeat: Infinity, ease: "easeInOut", delay: 1 }}
        className="absolute bottom-20 right-[10%] w-80 h-80 bg-linear-to-br from-blue-400/25 to-cyan-400/15 dark:from-blue-500/15 dark:to-cyan-500/10 rounded-full blur-3xl"
      />
      <motion.div
        animate={{ scale: [1, 1.1, 1], opacity: [0.5, 0.8, 0.5] }}
        transition={{ duration: 6, repeat: Infinity, ease: "easeInOut" }}
        className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[600px] h-[600px] bg-linear-to-r from-violet-400/10 to-indigo-400/10 dark:from-violet-500/5 dark:to-indigo-500/5 rounded-full blur-3xl"
      />

      <div className="absolute inset-0 overflow-hidden pointer-events-none">
        {particlePositions.map((particle, i) => (
          <motion.div
            key={i}
            className="absolute w-1 h-1 bg-purple-400/40 dark:bg-purple-400/30 rounded-full"
            style={{
              left: `${particle.left}%`,
              top: `${particle.top}%`,
            }}
            animate={{
              y: [-20, 20, -20],
              opacity: [0.2, 0.8, 0.2],
            }}
            transition={{
              duration: particle.duration,
              repeat: Infinity,
              delay: particle.delay,
            }}
          />
        ))}
      </div>

      <div className="container mx-auto px-4 relative z-10">
        <div className="max-w-6xl mx-auto">
          <div className="grid lg:grid-cols-2 gap-12 lg:gap-16 items-center">
            <motion.div
              initial={{ opacity: 0, x: -30 }}
              whileInView={{ opacity: 1, x: 0 }}
              viewport={{ once: true }}
              transition={{ duration: 0.7 }}
              className="text-center lg:text-left"
            >
              <motion.div
                initial={{ opacity: 0, y: 10 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-linear-to-r from-purple-100 to-indigo-100 dark:from-purple-900/40 dark:to-indigo-900/40 border border-purple-200 dark:border-purple-700/50 mb-6"
              >
                <FontAwesomeIcon icon={faCrown} className="w-4 h-4 text-amber-500" />
                <span className="text-sm font-semibold text-purple-700 dark:text-purple-300">
                  #1 Trusted Auction Platform
                </span>
              </motion.div>

              <h2 className="text-4xl md:text-5xl lg:text-6xl font-bold tracking-tight mb-6">
                <span className="text-slate-900 dark:text-white">Start Your</span>
                <br />
                <span className="bg-linear-to-r from-purple-600 via-indigo-600 to-blue-600 bg-clip-text text-transparent">
                  Bidding Journey
                </span>
              </h2>

              <p className="text-lg md:text-xl text-slate-600 dark:text-slate-300 mb-8 leading-relaxed max-w-xl mx-auto lg:mx-0">
                Join over <span className="font-bold text-purple-600 dark:text-purple-400">500,000+</span> collectors 
                and enthusiasts. Get exclusive access to rare items, real-time bidding, and premium features.
              </p>

              <div className="flex flex-wrap justify-center lg:justify-start gap-3 mb-8">
                {features.map((feature, index) => (
                  <motion.div
                    key={feature.text}
                    initial={{ opacity: 0, scale: 0.8 }}
                    whileInView={{ opacity: 1, scale: 1 }}
                    viewport={{ once: true }}
                    transition={{ delay: index * 0.1 + 0.3 }}
                    className="flex items-center gap-2 px-4 py-2 rounded-full bg-white dark:bg-slate-800/80 border border-slate-200 dark:border-slate-700 shadow-sm"
                  >
                    <FontAwesomeIcon icon={feature.icon} className="w-4 h-4 text-purple-500" />
                    <span className="text-sm font-medium text-slate-700 dark:text-slate-300">{feature.text}</span>
                  </motion.div>
                ))}
              </div>

              <div className="flex flex-col sm:flex-row items-center justify-center lg:justify-start gap-4 mb-8">
                <Link href="/register">
                  <motion.div whileHover={{ scale: 1.02 }} whileTap={{ scale: 0.98 }}>
                    <Button
                      size="lg"
                      className="h-14 px-8 text-lg rounded-2xl bg-linear-to-r from-purple-600 via-indigo-600 to-blue-600 hover:from-purple-700 hover:via-indigo-700 hover:to-blue-700 text-white shadow-xl shadow-purple-500/25 dark:shadow-purple-500/20 transition-all group"
                    >
                      <FontAwesomeIcon icon={faRocket} className="mr-2 group-hover:animate-bounce" />
                      Get Started Free
                      <FontAwesomeIcon icon={faArrowRight} className="ml-2 group-hover:translate-x-1 transition-transform" />
                    </Button>
                  </motion.div>
                </Link>
                <Link href="/auctions">
                  <Button
                    variant="outline"
                    size="lg"
                    className="h-14 px-8 text-lg rounded-2xl border-2 border-slate-300 dark:border-slate-600 hover:border-purple-400 dark:hover:border-purple-500 hover:bg-purple-50 dark:hover:bg-purple-950/30 text-slate-700 dark:text-slate-200 transition-all"
                  >
                    <FontAwesomeIcon icon={faGavel} className="mr-2" />
                    Browse Auctions
                  </Button>
                </Link>
              </div>

              <div className="flex items-center justify-center lg:justify-start gap-4">
                <div className="flex -space-x-3">
                  {floatingAvatars.slice(0, 4).map((avatar) => (
                    <div
                      key={avatar.id}
                      className="w-10 h-10 rounded-full border-2 border-white dark:border-slate-800 overflow-hidden shadow-md"
                    >
                      <Image
                        src={avatar.src}
                        alt="User"
                        width={40}
                        height={40}
                        className="w-full h-full object-cover"
                      />
                    </div>
                  ))}
                  <div className="w-10 h-10 rounded-full bg-linear-to-r from-purple-500 to-indigo-500 border-2 border-white dark:border-slate-800 flex items-center justify-center text-xs font-bold text-white shadow-md">
                    +99k
                  </div>
                </div>
                <div className="text-left">
                  <div className="flex items-center gap-1">
                    {[...Array(5)].map((_, i) => (
                      <FontAwesomeIcon key={i} icon={faStar} className="w-4 h-4 text-amber-400" />
                    ))}
                    <span className="ml-1 font-bold text-slate-900 dark:text-white">4.9</span>
                  </div>
                  <p className="text-sm text-slate-500 dark:text-slate-400">from 50,000+ reviews</p>
                </div>
              </div>
            </motion.div>

            <motion.div
              initial={{ opacity: 0, x: 30 }}
              whileInView={{ opacity: 1, x: 0 }}
              viewport={{ once: true }}
              transition={{ duration: 0.7, delay: 0.2 }}
              className="relative"
            >
              <div className="relative">
                <motion.div
                  animate={{ rotate: [0, 360] }}
                  transition={{ duration: 60, repeat: Infinity, ease: "linear" }}
                  className="absolute -inset-4 bg-linear-to-r from-purple-500 via-indigo-500 to-blue-500 rounded-3xl opacity-20 blur-2xl"
                />
                
                <div className="relative bg-white/90 dark:bg-slate-800/90 backdrop-blur-xl rounded-3xl border border-slate-200/80 dark:border-slate-700/80 shadow-2xl shadow-purple-500/10 dark:shadow-purple-500/5 p-8 md:p-10">
                  <div className="absolute -top-3 -right-3 px-4 py-2 bg-linear-to-r from-green-500 to-emerald-500 rounded-full shadow-lg">
                    <span className="text-sm font-bold text-white flex items-center gap-1">
                      <span className="w-2 h-2 bg-white rounded-full animate-pulse" />
                      LIVE
                    </span>
                  </div>

                  <div className="text-center mb-8">
                    <h3 className="text-2xl font-bold text-slate-900 dark:text-white mb-2">Platform Stats</h3>
                    <p className="text-slate-500 dark:text-slate-400">Real-time activity</p>
                  </div>

                  <div className="grid grid-cols-2 gap-6 mb-8">
                    {[
                      { label: "Active Bidders", value: mounted ? 12847 : 0, icon: faUsers, color: "text-purple-500" },
                      { label: "Live Auctions", value: mounted ? 3429 : 0, icon: faGavel, color: "text-blue-500" },
                      { label: "Items Sold", value: mounted ? 89234 : 0, icon: faCheck, color: "text-green-500" },
                      { label: "Total Bids", value: mounted ? 567891 : 0, icon: faBolt, color: "text-amber-500" },
                    ].map((stat, index) => (
                      <motion.div
                        key={stat.label}
                        initial={{ opacity: 0, y: 20 }}
                        whileInView={{ opacity: 1, y: 0 }}
                        viewport={{ once: true }}
                        transition={{ delay: index * 0.1 + 0.4 }}
                        className="relative group"
                      >
                        <div className="absolute inset-0 bg-linear-to-br from-purple-500/10 to-blue-500/10 rounded-2xl opacity-0 group-hover:opacity-100 transition-opacity" />
                        <div className="relative p-4 rounded-2xl bg-slate-50 dark:bg-slate-700/50 border border-slate-200/50 dark:border-slate-600/50">
                          <FontAwesomeIcon icon={stat.icon} className={cn("w-5 h-5 mb-2", stat.color)} />
                          <div className="text-2xl md:text-3xl font-bold text-slate-900 dark:text-white">
                            <AnimatedCounter value={stat.value} />
                          </div>
                          <p className="text-sm text-slate-500 dark:text-slate-400">{stat.label}</p>
                        </div>
                      </motion.div>
                    ))}
                  </div>

                  <div className="space-y-3">
                    {[
                      { user: "Sarah M.", action: "won", item: "Vintage Rolex", time: "2m ago", amount: "$12,500" },
                      { user: "James K.", action: "bid on", item: "Nike Air Max", time: "5m ago", amount: "$850" },
                      { user: "Emily R.", action: "listed", item: "Art Print", time: "8m ago", amount: "$2,300" },
                    ].map((activity, index) => (
                      <motion.div
                        key={index}
                        initial={{ opacity: 0, x: -20 }}
                        whileInView={{ opacity: 1, x: 0 }}
                        viewport={{ once: true }}
                        transition={{ delay: index * 0.1 + 0.6 }}
                        className="flex items-center justify-between p-3 rounded-xl bg-slate-50/80 dark:bg-slate-700/30 border border-slate-200/50 dark:border-slate-600/30"
                      >
                        <div className="flex items-center gap-3">
                          <div className="w-8 h-8 rounded-full bg-linear-to-br from-purple-500 to-indigo-500 flex items-center justify-center text-white text-xs font-bold">
                            {activity.user[0]}
                          </div>
                          <div>
                            <p className="text-sm font-medium text-slate-900 dark:text-white">
                              {activity.user} <span className="text-slate-500 dark:text-slate-400">{activity.action}</span> {activity.item}
                            </p>
                            <p className="text-xs text-slate-400">{activity.time}</p>
                          </div>
                        </div>
                        <span className="text-sm font-bold text-green-600 dark:text-green-400">{activity.amount}</span>
                      </motion.div>
                    ))}
                  </div>
                </div>
              </div>

              <motion.div
                animate={{ y: [-5, 5, -5] }}
                transition={{ duration: 3, repeat: Infinity, ease: "easeInOut" }}
                className="absolute -bottom-6 -left-6 px-4 py-3 bg-white dark:bg-slate-800 rounded-2xl shadow-xl border border-slate-200 dark:border-slate-700"
              >
                <div className="flex items-center gap-2">
                  <div className="w-10 h-10 rounded-xl bg-linear-to-br from-green-500 to-emerald-500 flex items-center justify-center">
                    <FontAwesomeIcon icon={faShieldHalved} className="w-5 h-5 text-white" />
                  </div>
                  <div>
                    <p className="font-bold text-slate-900 dark:text-white text-sm">100% Protected</p>
                    <p className="text-xs text-slate-500 dark:text-slate-400">Money-back guarantee</p>
                  </div>
                </div>
              </motion.div>

              <motion.div
                animate={{ y: [5, -5, 5] }}
                transition={{ duration: 4, repeat: Infinity, ease: "easeInOut", delay: 1 }}
                className="absolute -top-4 -right-4 px-4 py-3 bg-white dark:bg-slate-800 rounded-2xl shadow-xl border border-slate-200 dark:border-slate-700"
              >
                <div className="flex items-center gap-2">
                  <div className="w-10 h-10 rounded-xl bg-linear-to-br from-amber-500 to-orange-500 flex items-center justify-center">
                    <FontAwesomeIcon icon={faCrown} className="w-5 h-5 text-white" />
                  </div>
                  <div>
                    <p className="font-bold text-slate-900 dark:text-white text-sm">Premium Access</p>
                    <p className="text-xs text-slate-500 dark:text-slate-400">VIP auctions</p>
                  </div>
                </div>
              </motion.div>
            </motion.div>
          </div>
        </div>
      </div>
    </section>
  );
}
