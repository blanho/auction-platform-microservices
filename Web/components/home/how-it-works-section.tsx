"use client";

import { CheckCircle, ArrowRight } from "lucide-react";
import { Button } from "@/components/ui/button";
import Link from "next/link";
import { HOW_IT_WORKS_STEPS, HOW_IT_WORKS_CONTENT } from "@/constants/landing";
import { AnimatedSection, StaggerContainer, StaggerItem } from "@/components/ui/animated";

export function HowItWorksSection() {
    return (
        <AnimatedSection id="how-it-works" className="py-24 bg-slate-50 dark:bg-slate-900/50">
            <div className="container mx-auto px-4">
                <div className="text-center mb-16">
                    <p className="text-sm font-semibold text-purple-600 dark:text-purple-400 mb-3 uppercase tracking-wider">
                        {HOW_IT_WORKS_CONTENT.LABEL}
                    </p>
                    <h2 className="text-3xl md:text-4xl lg:text-5xl font-bold text-slate-900 dark:text-white mb-4">
                        {HOW_IT_WORKS_CONTENT.TITLE}
                    </h2>
                    <p className="text-lg text-slate-600 dark:text-slate-400 max-w-2xl mx-auto">
                        {HOW_IT_WORKS_CONTENT.DESCRIPTION}
                    </p>
                </div>

                <StaggerContainer className="grid md:grid-cols-2 lg:grid-cols-4 gap-8">
                    {HOW_IT_WORKS_STEPS.map((step, index) => (
                        <StaggerItem key={index} className="relative group">
                            {index < HOW_IT_WORKS_STEPS.length - 1 && (
                                <div className="hidden lg:block absolute top-20 left-full w-full h-0.5 bg-gradient-to-r from-slate-300 to-transparent dark:from-slate-700 z-0 -translate-x-8" />
                            )}
                            
                            <div className={`relative h-full p-6 rounded-2xl ${step.bgColor} border border-slate-200/50 dark:border-slate-700/50 transition-all duration-300 hover:shadow-xl hover:-translate-y-1`}>
                                <div className="flex items-center justify-between mb-6">
                                    <div className={`w-14 h-14 rounded-2xl bg-gradient-to-br ${step.color} flex items-center justify-center shadow-lg`}>
                                        <step.icon className="w-7 h-7 text-white" />
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
                                            <CheckCircle className="w-4 h-4 text-green-500 shrink-0" />
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
                            <h3 className="text-2xl font-bold mb-1">{HOW_IT_WORKS_CONTENT.CTA.TITLE}</h3>
                            <p className="text-purple-100">{HOW_IT_WORKS_CONTENT.CTA.SUBTITLE}</p>
                        </div>
                        <Button 
                            size="lg" 
                            className="bg-white text-purple-700 hover:bg-purple-50 font-semibold px-8 h-12 shadow-lg"
                            asChild
                        >
                            <Link href="/auth/register">
                                {HOW_IT_WORKS_CONTENT.CTA.BUTTON}
                                <ArrowRight className="ml-2 w-4 h-4" />
                            </Link>
                        </Button>
                    </div>
                </div>
            </div>
        </AnimatedSection>
    );
}
