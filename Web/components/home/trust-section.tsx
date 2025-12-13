"use client";

import { TRUST_FEATURES } from "@/constants/landing";
import { AnimatedSection, StaggerContainer, StaggerItem } from "@/components/ui/animated";

export function TrustSection() {
    return (
        <AnimatedSection className="py-20 bg-white dark:bg-slate-950">
            <div className="container mx-auto px-4">
                <div className="text-center mb-16">
                    <p className="text-sm font-semibold text-purple-600 dark:text-purple-400 mb-3 uppercase tracking-wider">
                        Why Trust Us
                    </p>
                    <h2 className="text-3xl md:text-4xl lg:text-5xl font-bold text-slate-900 dark:text-white mb-4">
                        Your Security Is Our Priority
                    </h2>
                    <p className="text-lg text-slate-600 dark:text-slate-400 max-w-2xl mx-auto">
                        We&apos;ve built the most trusted online auction platform with industry-leading 
                        security measures and customer protection policies.
                    </p>
                </div>

                <StaggerContainer className="grid md:grid-cols-2 lg:grid-cols-3 gap-6 lg:gap-8">
                    {TRUST_FEATURES.map((feature, index) => (
                        <StaggerItem
                            key={index}
                            className="group relative p-8 rounded-2xl bg-slate-50 dark:bg-slate-900/50 border border-slate-200 dark:border-slate-800 hover:border-purple-300 dark:hover:border-purple-700 transition-all duration-300 hover:shadow-lg"
                        >
                            <div className="flex items-start justify-between mb-6">
                                <div className="w-14 h-14 rounded-2xl bg-gradient-to-br from-purple-500 to-pink-500 flex items-center justify-center shadow-lg shadow-purple-500/20">
                                    <feature.icon className="w-7 h-7 text-white" />
                                </div>
                                <div className="text-right">
                                    <p className="text-2xl font-bold text-slate-900 dark:text-white">{feature.stat}</p>
                                    <p className="text-xs text-slate-500 dark:text-slate-400 uppercase tracking-wider">{feature.statLabel}</p>
                                </div>
                            </div>
                            <h3 className="text-xl font-bold text-slate-900 dark:text-white mb-3">
                                {feature.title}
                            </h3>
                            <p className="text-slate-600 dark:text-slate-400 leading-relaxed">
                                {feature.description}
                            </p>
                        </StaggerItem>
                    ))}
                </StaggerContainer>
            </div>
        </AnimatedSection>
    );
}
