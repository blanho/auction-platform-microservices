"use client";

import { useEffect, useState, useRef } from "react";
import { STATS, STATS_CONTENT } from "@/constants/landing";
import { AnimatedSection } from "@/components/ui/animated";

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
        ? count.toLocaleString(undefined, { maximumFractionDigits: 0 }) 
        : count.toFixed(value % 1 !== 0 ? 1 : 0);

    return (
        <div ref={ref} className="text-5xl md:text-6xl font-bold text-white">
            {prefix}{displayValue}{suffix}
        </div>
    );
}

export function StatsCounterSection() {
    return (
        <AnimatedSection className="py-20 bg-slate-900 relative overflow-hidden">
            <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_center,_var(--tw-gradient-stops))] from-purple-900/30 via-slate-900 to-slate-900" />
            <div className="absolute top-0 left-1/4 w-96 h-96 bg-purple-600/10 rounded-full blur-3xl" />
            <div className="absolute bottom-0 right-1/4 w-96 h-96 bg-pink-600/10 rounded-full blur-3xl" />
            
            <div className="container mx-auto px-4 relative">
                <div className="text-center mb-16">
                    <h2 className="text-3xl md:text-4xl font-bold text-white mb-4">
                        {STATS_CONTENT.TITLE}
                    </h2>
                    <p className="text-slate-400 text-lg max-w-2xl mx-auto">
                        {STATS_CONTENT.DESCRIPTION}
                    </p>
                </div>
                
                <div className="grid grid-cols-2 lg:grid-cols-4 gap-8 lg:gap-12">
                    {STATS.map((stat, index) => (
                        <div key={index} className="text-center">
                            <AnimatedCounter 
                                value={stat.value} 
                                prefix={stat.prefix || ""} 
                                suffix={stat.suffix} 
                            />
                            <p className="text-xl font-semibold text-white mt-2">{stat.label}</p>
                            <p className="text-sm text-slate-400 mt-1">{stat.description}</p>
                        </div>
                    ))}
                </div>
            </div>
        </AnimatedSection>
    );
}
