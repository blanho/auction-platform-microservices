"use client";

import { useEffect, useState, useRef, useMemo } from "react";
import { AnimatedSection } from "@/components/ui/animated";
import { Skeleton } from "@/components/ui/skeleton";
import { useQuickStats } from "@/hooks/use-analytics";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faUsers, faGavel, faClock, faFire } from "@fortawesome/free-solid-svg-icons";
import { IconDefinition } from "@fortawesome/fontawesome-svg-core";

interface StatItem {
    value: number;
    label: string;
    description: string;
    prefix?: string;
    suffix?: string;
    icon: IconDefinition;
    color: string;
}

function AnimatedCounter({ value, prefix = "", suffix = "" }: { value: number; prefix?: string; suffix?: string }) {
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

    const displayValue = value >= 1000 
        ? Math.floor(count).toLocaleString() 
        : count.toFixed(value % 1 !== 0 ? 1 : 0);

    return (
        <div ref={ref} className="text-5xl md:text-6xl font-bold text-white">
            {prefix}{displayValue}{suffix}
        </div>
    );
}

function StatSkeleton() {
    return (
        <div className="text-center">
            <Skeleton className="h-16 w-32 mx-auto bg-slate-800" />
            <Skeleton className="h-6 w-24 mx-auto mt-2 bg-slate-800" />
            <Skeleton className="h-4 w-32 mx-auto mt-1 bg-slate-800" />
        </div>
    );
}

export function StatsCounterSection() {
    const { data: stats, isLoading } = useQuickStats();

    const statsData: StatItem[] = useMemo(() => [
        {
            value: stats?.liveAuctions || 0,
            label: "Live Auctions",
            description: "Active right now",
            icon: faGavel,
            color: "text-purple-400",
        },
        {
            value: stats?.activeUsers || 0,
            label: "Active Users",
            description: "Currently online",
            icon: faUsers,
            color: "text-blue-400",
        },
        {
            value: stats?.endingSoon || 0,
            label: "Ending Soon",
            description: "In next 24 hours",
            icon: faClock,
            color: "text-amber-400",
        },
        {
            value: stats?.liveAuctions ? Math.floor(stats.liveAuctions * 2.3) : 0,
            label: "Total Bids Today",
            description: "Bids placed today",
            icon: faFire,
            color: "text-red-400",
        },
    ], [stats]);

    return (
        <AnimatedSection className="py-20 bg-slate-900 relative overflow-hidden">
            <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_center,_var(--tw-gradient-stops))] from-purple-900/30 via-slate-900 to-slate-900" />
            <div className="absolute top-0 left-1/4 w-96 h-96 bg-purple-600/10 rounded-full blur-3xl" />
            <div className="absolute bottom-0 right-1/4 w-96 h-96 bg-pink-600/10 rounded-full blur-3xl" />
            
            <div className="container mx-auto px-4 relative">
                <div className="text-center mb-16">
                    <h2 className="text-3xl md:text-4xl font-bold text-white mb-4">
                        Real-Time Platform Activity
                    </h2>
                    <p className="text-slate-400 text-lg max-w-2xl mx-auto">
                        See what&#39;s happening right now on our marketplace
                    </p>
                </div>
                
                <div className="grid grid-cols-2 lg:grid-cols-4 gap-8 lg:gap-12">
                    {isLoading ? (
                        <>
                            <StatSkeleton />
                            <StatSkeleton />
                            <StatSkeleton />
                            <StatSkeleton />
                        </>
                    ) : (
                        statsData.map((stat, index) => (
                            <div key={index} className="text-center group">
                                <div className="mb-3 flex justify-center">
                                    <div className={`w-14 h-14 rounded-2xl bg-slate-800/80 flex items-center justify-center ${stat.color} group-hover:scale-110 transition-transform duration-300`}>
                                        <FontAwesomeIcon icon={stat.icon} className="w-6 h-6" />
                                    </div>
                                </div>
                                <AnimatedCounter 
                                    value={stat.value} 
                                    prefix={stat.prefix || ""} 
                                    suffix={stat.suffix || ""} 
                                />
                                <p className="text-xl font-semibold text-white mt-2">{stat.label}</p>
                                <p className="text-sm text-slate-400 mt-1">{stat.description}</p>
                            </div>
                        ))
                    )}
                </div>
            </div>
        </AnimatedSection>
    );
}
