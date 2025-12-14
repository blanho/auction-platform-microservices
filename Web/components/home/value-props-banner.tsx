"use client";

import { Shield, Truck, Award, Clock, BadgeCheck, Headphones } from "lucide-react";
import { AnimatedSection } from "@/components/ui/animated";

const VALUE_PROPS = [
    {
        icon: Shield,
        title: "Buyer Protection",
        description: "100% money-back guarantee",
    },
    {
        icon: BadgeCheck,
        title: "Verified Sellers",
        description: "Every seller is vetted",
    },
    {
        icon: Truck,
        title: "Secure Shipping",
        description: "Tracked & insured delivery",
    },
    {
        icon: Clock,
        title: "24/7 Auctions",
        description: "Bid anytime, anywhere",
    },
    {
        icon: Award,
        title: "Authenticity",
        description: "Expert item verification",
    },
    {
        icon: Headphones,
        title: "Support",
        description: "Help when you need it",
    },
];

export function ValuePropsBanner() {
    return (
        <AnimatedSection className="py-6 bg-slate-50 dark:bg-slate-900 border-y border-slate-200 dark:border-slate-800">
            <div className="container mx-auto px-4">
                <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-4 lg:gap-8">
                    {VALUE_PROPS.map((prop, index) => (
                        <div
                            key={index}
                            className="flex items-center gap-3 p-3 lg:p-0"
                        >
                            <div className="shrink-0 w-10 h-10 rounded-xl bg-purple-100 dark:bg-purple-900/30 flex items-center justify-center">
                                <prop.icon className="w-5 h-5 text-purple-600 dark:text-purple-400" />
                            </div>
                            <div className="min-w-0">
                                <p className="text-sm font-semibold text-slate-900 dark:text-white truncate">
                                    {prop.title}
                                </p>
                                <p className="text-xs text-slate-500 dark:text-slate-400 truncate">
                                    {prop.description}
                                </p>
                            </div>
                        </div>
                    ))}
                </div>
            </div>
        </AnimatedSection>
    );
}
