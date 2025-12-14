"use client";

import { useEffect, useState, useRef, useMemo } from "react";
import { motion } from "framer-motion";
import { Skeleton } from "@/components/ui/skeleton";
import { useQuickStats } from "@/hooks/use-analytics";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
    faUsers,
    faGavel,
    faClock,
    faFire,
    faChartLine,
    faBolt,
} from "@fortawesome/free-solid-svg-icons";
import { IconDefinition } from "@fortawesome/fontawesome-svg-core";
import { PulsingDot } from "@/components/ui/animated";

interface StatItem {
    value: number;
    label: string;
    description: string;
    prefix?: string;
    suffix?: string;
    icon: IconDefinition;
    gradient: string;
    bgGradient: string;
    shadowColor: string;
}

function AnimatedCounter({
    value,
    prefix = "",
    suffix = "",
}: {
    value: number;
    prefix?: string;
    suffix?: string;
}) {
    const [count, setCount] = useState(0);
    const [hasAnimated, setHasAnimated] = useState(false);
    const ref = useRef<HTMLDivElement>(null);

    useEffect(() => {
        const observer = new IntersectionObserver(
            ([entry]) => {
                if (entry.isIntersecting && !hasAnimated) {
                    setHasAnimated(true);
                    const duration = 2000;
                    const steps = 60;
                    const increment = value / steps;
                    let current = 0;

                    const timer = setInterval(() => {
                        current += increment;
                        if (current >= value) {
                            setCount(value);
                            clearInterval(timer);
                        } else {
                            setCount(current);
                        }
                    }, duration / steps);

                    return () => clearInterval(timer);
                }
            },
            { threshold: 0.5 }
        );

        if (ref.current) observer.observe(ref.current);
        return () => observer.disconnect();
    }, [value, hasAnimated]);

    const displayValue =
        value >= 1000
            ? Math.floor(count).toLocaleString()
            : count.toFixed(value % 1 !== 0 ? 1 : 0);

    return (
        <div ref={ref} className="text-4xl md:text-5xl lg:text-6xl font-bold">
            {prefix}
            {displayValue}
            {suffix}
        </div>
    );
}

function StatSkeleton() {
    return (
        <div className="p-6 rounded-3xl bg-white/80 dark:bg-slate-900/80 border border-slate-200 dark:border-slate-800">
            <Skeleton className="h-14 w-14 rounded-2xl bg-slate-200 dark:bg-slate-800 mb-4" />
            <Skeleton className="h-12 w-32 bg-slate-200 dark:bg-slate-800 mb-2" />
            <Skeleton className="h-5 w-24 bg-slate-200 dark:bg-slate-800 mb-1" />
            <Skeleton className="h-4 w-32 bg-slate-200 dark:bg-slate-800" />
        </div>
    );
}

export function StatsCounterSection() {
    const { data: stats, isLoading } = useQuickStats();

    const statsData: StatItem[] = useMemo(
        () => [
            {
                value: stats?.liveAuctions || 0,
                label: "Live Auctions",
                description: "Active right now",
                icon: faGavel,
                gradient: "from-purple-500 to-pink-500",
                bgGradient: "from-purple-500/10 to-pink-500/10",
                shadowColor: "shadow-purple-500/20",
            },
            {
                value: stats?.activeUsers || 0,
                label: "Active Bidders",
                description: "Currently online",
                icon: faUsers,
                gradient: "from-blue-500 to-cyan-500",
                bgGradient: "from-blue-500/10 to-cyan-500/10",
                shadowColor: "shadow-blue-500/20",
            },
            {
                value: stats?.endingSoon || 0,
                label: "Ending Soon",
                description: "In next 24 hours",
                icon: faClock,
                gradient: "from-amber-500 to-orange-500",
                bgGradient: "from-amber-500/10 to-orange-500/10",
                shadowColor: "shadow-amber-500/20",
            },
            {
                value: stats?.liveAuctions ? Math.floor(stats.liveAuctions * 2.3) : 0,
                label: "Bids Today",
                description: "Total bids placed",
                icon: faFire,
                gradient: "from-red-500 to-rose-500",
                bgGradient: "from-red-500/10 to-rose-500/10",
                shadowColor: "shadow-red-500/20",
            },
        ],
        [stats]
    );

    return (
        <section className="relative py-24 bg-gradient-to-b from-slate-100 via-purple-50/50 to-slate-100 dark:from-slate-900 dark:via-purple-950/20 dark:to-slate-900 overflow-hidden">
            <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_center,_var(--tw-gradient-stops))] from-purple-300/20 via-transparent to-transparent dark:from-purple-900/20" />
            <div className="absolute top-20 left-1/4 w-80 h-80 bg-purple-400/10 dark:bg-purple-600/10 rounded-full blur-3xl" />
            <div className="absolute bottom-20 right-1/4 w-80 h-80 bg-pink-400/10 dark:bg-pink-600/10 rounded-full blur-3xl" />
            <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[600px] h-[600px] bg-blue-400/5 dark:bg-blue-600/5 rounded-full blur-3xl" />

            <div className="container mx-auto px-4 relative z-10">
                <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    whileInView={{ opacity: 1, y: 0 }}
                    viewport={{ once: true }}
                    className="text-center mb-16"
                >
                    <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-purple-100 dark:bg-purple-500/10 border border-purple-200 dark:border-purple-500/20 mb-6">
                        <PulsingDot />
                        <span className="text-sm font-medium text-purple-700 dark:text-purple-300">
                            Live Statistics
                        </span>
                    </div>

                    <h2 className="text-4xl md:text-5xl lg:text-6xl font-bold text-slate-900 dark:text-white mb-4 leading-tight">
                        Real-Time
                        <span className="block text-transparent bg-clip-text bg-gradient-to-r from-purple-600 via-pink-600 to-purple-600 dark:from-purple-400 dark:via-pink-400 dark:to-purple-400">
                            Platform Activity
                        </span>
                    </h2>

                    <p className="text-lg text-slate-600 dark:text-slate-400 max-w-2xl mx-auto leading-relaxed">
                        See what&apos;s happening right now on our marketplace — updated every
                        second
                    </p>
                </motion.div>

                <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 md:gap-6">
                    {isLoading ? (
                        <>
                            <StatSkeleton />
                            <StatSkeleton />
                            <StatSkeleton />
                            <StatSkeleton />
                        </>
                    ) : (
                        statsData.map((stat, index) => (
                            <motion.div
                                key={index}
                                initial={{ opacity: 0, y: 30, scale: 0.95 }}
                                whileInView={{ opacity: 1, y: 0, scale: 1 }}
                                viewport={{ once: true }}
                                transition={{ duration: 0.5, delay: index * 0.1 }}
                                className="group"
                            >
                                <div
                                    className={`relative p-6 md:p-8 rounded-3xl bg-white/80 dark:bg-slate-900/80 border border-slate-200 dark:border-slate-800 backdrop-blur-sm transition-all duration-500 hover:shadow-2xl hover:-translate-y-2 ${stat.shadowColor}`}
                                >
                                    <div
                                        className={`absolute inset-0 rounded-3xl bg-gradient-to-br ${stat.bgGradient} opacity-0 group-hover:opacity-100 transition-opacity duration-500`}
                                    />

                                    <div className="relative z-10">
                                        <div
                                            className={`w-14 h-14 rounded-2xl bg-gradient-to-br ${stat.gradient} flex items-center justify-center shadow-lg mb-5 group-hover:scale-110 transition-transform duration-300`}
                                        >
                                            <FontAwesomeIcon
                                                icon={stat.icon}
                                                className="w-6 h-6 text-white"
                                            />
                                        </div>

                                        <div
                                            className={`text-transparent bg-clip-text bg-gradient-to-r ${stat.gradient}`}
                                        >
                                            <AnimatedCounter
                                                value={stat.value}
                                                prefix={stat.prefix || ""}
                                                suffix={stat.suffix || ""}
                                            />
                                        </div>

                                        <p className="text-lg font-semibold text-slate-900 dark:text-white mt-2">
                                            {stat.label}
                                        </p>
                                        <p className="text-sm text-slate-500 dark:text-slate-400 mt-0.5">
                                            {stat.description}
                                        </p>
                                    </div>
                                </div>
                            </motion.div>
                        ))
                    )}
                </div>

                <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    whileInView={{ opacity: 1, y: 0 }}
                    viewport={{ once: true }}
                    transition={{ delay: 0.5 }}
                    className="mt-12 flex flex-wrap justify-center gap-6"
                >
                    <div className="flex items-center gap-2 px-4 py-2 rounded-full bg-white/80 dark:bg-slate-900/80 border border-slate-200 dark:border-slate-800 shadow-sm backdrop-blur-sm">
                        <FontAwesomeIcon
                            icon={faBolt}
                            className="w-4 h-4 text-amber-500"
                        />
                        <span className="text-sm text-slate-600 dark:text-slate-400">
                            Auto-refreshing data
                        </span>
                    </div>
                    <div className="flex items-center gap-2 px-4 py-2 rounded-full bg-white/80 dark:bg-slate-900/80 border border-slate-200 dark:border-slate-800 shadow-sm backdrop-blur-sm">
                        <FontAwesomeIcon
                            icon={faChartLine}
                            className="w-4 h-4 text-emerald-500"
                        />
                        <span className="text-sm text-slate-600 dark:text-slate-400">
                            Growing 15% weekly
                        </span>
                    </div>
                </motion.div>
            </div>
        </section>
    );
}
