"use client";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { 
    faUserPlus, 
    faMagnifyingGlass, 
    faGavel, 
    faTrophy,
    faCircleCheck,
    faArrowRight
} from "@fortawesome/free-solid-svg-icons";
import { IconDefinition } from "@fortawesome/fontawesome-svg-core";
import { Button } from "@/components/ui/button";
import Link from "next/link";
import { AnimatedSection, StaggerContainer, StaggerItem } from "@/components/ui/animated";

interface HowItWorksStep {
    step: string;
    icon: IconDefinition;
    title: string;
    description: string;
    features: string[];
    color: string;
    bgColor: string;
}

const STEPS: HowItWorksStep[] = [
    {
        step: "01",
        icon: faUserPlus,
        title: "Create Your Free Account",
        description: "Sign up in under 60 seconds. No credit card required, no hidden fees. Start exploring thousands of auctions immediately.",
        features: [
            "Free forever membership",
            "Instant access to all auctions",
            "Personalized recommendations",
        ],
        color: "from-blue-500 to-indigo-600",
        bgColor: "bg-blue-50 dark:bg-blue-950/30",
    },
    {
        step: "02",
        icon: faMagnifyingGlass,
        title: "Find Your Perfect Item",
        description: "Browse curated collections from verified sellers worldwide. Use smart filters to discover exactly what you're looking for.",
        features: [
            "50,000+ active listings",
            "AI-powered search",
            "Save favorites & set alerts",
        ],
        color: "from-purple-500 to-pink-600",
        bgColor: "bg-purple-50 dark:bg-purple-950/30",
    },
    {
        step: "03",
        icon: faGavel,
        title: "Place Your Winning Bid",
        description: "Bid with confidence using our real-time auction system. Set auto-bid to never miss out, even while you sleep.",
        features: [
            "Real-time bid updates",
            "Smart auto-bidding",
            "Outbid notifications",
        ],
        color: "from-orange-500 to-red-600",
        bgColor: "bg-orange-50 dark:bg-orange-950/30",
    },
    {
        step: "04",
        icon: faTrophy,
        title: "Win & Receive Your Item",
        description: "Secure checkout with buyer protection. Track your delivery in real-time and receive your item with our satisfaction guarantee.",
        features: [
            "Secure escrow payments",
            "Insured global shipping",
            "14-day money-back guarantee",
        ],
        color: "from-green-500 to-emerald-600",
        bgColor: "bg-green-50 dark:bg-green-950/30",
    },
];

export function HowItWorksSection() {
    return (
        <AnimatedSection id="how-it-works" className="py-24 bg-slate-50 dark:bg-slate-900/50">
            <div className="container mx-auto px-4">
                <div className="text-center mb-16">
                    <p className="text-sm font-semibold text-purple-600 dark:text-purple-400 mb-3 uppercase tracking-wider">
                        Simple & Easy
                    </p>
                    <h2 className="text-3xl md:text-4xl lg:text-5xl font-bold text-slate-900 dark:text-white mb-4">
                        How It Works
                    </h2>
                    <p className="text-lg text-slate-600 dark:text-slate-400 max-w-2xl mx-auto">
                        Start winning amazing deals in just 4 simple steps
                    </p>
                </div>

                <StaggerContainer className="grid md:grid-cols-2 lg:grid-cols-4 gap-8">
                    {STEPS.map((step, index) => (
                        <StaggerItem key={index} className="relative group">
                            {index < STEPS.length - 1 && (
                                <div className="hidden lg:block absolute top-20 left-full w-full h-0.5 bg-gradient-to-r from-slate-300 to-transparent dark:from-slate-700 z-0 -translate-x-8" />
                            )}
                            
                            <div className={`relative h-full p-6 rounded-2xl ${step.bgColor} border border-slate-200/50 dark:border-slate-700/50 transition-all duration-300 hover:shadow-xl hover:-translate-y-1`}>
                                <div className="flex items-center justify-between mb-6">
                                    <div className={`w-14 h-14 rounded-2xl bg-gradient-to-br ${step.color} flex items-center justify-center shadow-lg`}>
                                        <FontAwesomeIcon icon={step.icon} className="w-7 h-7 text-white" />
                                    </div>
                                    <span className="text-4xl font-bold text-slate-200 dark:text-slate-800">{step.step}</span>
                                </div>

                                <h3 className="text-lg font-bold text-slate-900 dark:text-white mb-3">
                                    {step.title}
                                </h3>
                                <p className="text-sm text-slate-600 dark:text-slate-400 mb-5 leading-relaxed">
                                    {step.description}
                                </p>

                                <ul className="space-y-2">
                                    {step.features.map((feature, idx) => (
                                        <li key={idx} className="flex items-center gap-2 text-sm text-slate-700 dark:text-slate-300">
                                            <FontAwesomeIcon icon={faCircleCheck} className="w-4 h-4 text-green-500 shrink-0" />
                                            {feature}
                                        </li>
                                    ))}
                                </ul>
                            </div>
                        </StaggerItem>
                    ))}
                </StaggerContainer>

                <div className="mt-16 text-center">
                    <div className="inline-flex flex-col sm:flex-row items-center gap-6 p-8 rounded-3xl bg-gradient-to-r from-purple-600 to-pink-600 shadow-xl">
                        <div className="text-center sm:text-left text-white">
                            <h3 className="text-2xl font-bold mb-1">Ready to start winning?</h3>
                            <p className="text-purple-100">Join thousands of members finding amazing deals every day</p>
                        </div>
                        <Button 
                            size="lg" 
                            className="bg-white text-purple-700 hover:bg-purple-50 font-semibold px-8 h-12 shadow-lg"
                            asChild
                        >
                            <Link href="/auth/register">
                                Create Free Account
                                <FontAwesomeIcon icon={faArrowRight} className="ml-2 w-4 h-4" />
                            </Link>
                        </Button>
                    </div>
                </div>
            </div>
        </AnimatedSection>
    );
}
