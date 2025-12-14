"use client";

import { useState, useEffect, useCallback } from "react";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Card } from "@/components/ui/card";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { 
    faStar, 
    faChevronLeft, 
    faChevronRight, 
    faQuoteLeft,
    faCircleCheck
} from "@fortawesome/free-solid-svg-icons";
import { Button } from "@/components/ui/button";
import { AnimatedSection } from "@/components/ui/animated";

interface Testimonial {
    id: number;
    name: string;
    role: string;
    location: string;
    avatar: string;
    image: string;
    rating: number;
    text: string;
    savings: string;
    verified: boolean;
}

const TESTIMONIALS: Testimonial[] = [
    {
        id: 1,
        name: "Michael Chen",
        role: "Collector",
        location: "San Francisco, CA",
        avatar: "MC",
        image: "https://api.dicebear.com/9.x/avataaars/svg?seed=Michael",
        rating: 5,
        text: "I&#39;ve been collecting vintage watches for years, and this platform has completely changed how I find rare pieces. Won a 1960s Omega at 40% below market value!",
        savings: "Saved $12,000+",
        verified: true,
    },
    {
        id: 2,
        name: "Sarah Williams",
        role: "Reseller",
        location: "Austin, TX",
        avatar: "SW",
        image: "https://api.dicebear.com/9.x/avataaars/svg?seed=Sarah",
        rating: 5,
        text: "The verification process gives me confidence in every purchase. As a reseller, I need authentic items, and this platform delivers consistently.",
        savings: "Saved $8,500+",
        verified: true,
    },
    {
        id: 3,
        name: "David Park",
        role: "First-time Buyer",
        location: "Seattle, WA",
        avatar: "DP",
        image: "https://api.dicebear.com/9.x/avataaars/svg?seed=David",
        rating: 5,
        text: "Was nervous about online auctions but the buyer protection made it easy. Won my first auction and the item arrived exactly as described!",
        savings: "Saved $2,300+",
        verified: true,
    },
    {
        id: 4,
        name: "Emily Roberts",
        role: "Art Enthusiast",
        location: "New York, NY",
        avatar: "ER",
        image: "https://api.dicebear.com/9.x/avataaars/svg?seed=Emily",
        rating: 5,
        text: "Found a signed lithograph from my favorite artist at an incredible price. The community here is passionate and knowledgeable.",
        savings: "Saved $5,000+",
        verified: true,
    },
    {
        id: 5,
        name: "James Taylor",
        role: "Seller",
        location: "Miami, FL",
        avatar: "JT",
        image: "https://api.dicebear.com/9.x/avataaars/svg?seed=James",
        rating: 5,
        text: "As a seller, I appreciate the low fees and the exposure to serious buyers. My items sell faster here than anywhere else.",
        savings: "Earned $50,000+",
        verified: true,
    },
];

const AUTOPLAY_INTERVAL = 6000;

export function TestimonialsSection() {
    const [currentIndex, setCurrentIndex] = useState(0);
    const [autoPlay, setAutoPlay] = useState(true);

    const goToNext = useCallback(() => {
        setCurrentIndex((prev) => (prev + 1) % TESTIMONIALS.length);
    }, []);

    useEffect(() => {
        if (!autoPlay) return;

        const interval = setInterval(goToNext, AUTOPLAY_INTERVAL);

        return () => clearInterval(interval);
    }, [autoPlay, goToNext]);

    const goToPrev = () => {
        setAutoPlay(false);
        setCurrentIndex((prev) => (prev - 1 + TESTIMONIALS.length) % TESTIMONIALS.length);
    };

    const handleNext = () => {
        setAutoPlay(false);
        goToNext();
    };

    const current = TESTIMONIALS[currentIndex];

    return (
        <AnimatedSection className="py-24 bg-white dark:bg-slate-950 overflow-hidden">
            <div className="container mx-auto px-4">
                <div className="text-center mb-16">
                    <p className="text-sm font-semibold text-purple-600 dark:text-purple-400 mb-3 uppercase tracking-wider">
                        Success Stories
                    </p>
                    <h2 className="text-3xl md:text-4xl lg:text-5xl font-bold text-slate-900 dark:text-white mb-4">
                        Hear From Our Community
                    </h2>
                    <p className="text-lg text-slate-600 dark:text-slate-400 max-w-2xl mx-auto">
                        Join thousands of members who are saving money and finding treasures every day.
                    </p>
                </div>

                <div className="relative max-w-4xl mx-auto">
                    <Button
                        variant="outline"
                        size="icon"
                        onClick={goToPrev}
                        className="absolute left-0 top-1/2 -translate-y-1/2 -translate-x-2 md:-translate-x-16 z-10 rounded-full shadow-lg bg-white dark:bg-slate-800 w-12 h-12"
                    >
                        <FontAwesomeIcon icon={faChevronLeft} className="w-6 h-6" />
                    </Button>
                    <Button
                        variant="outline"
                        size="icon"
                        onClick={handleNext}
                        className="absolute right-0 top-1/2 -translate-y-1/2 translate-x-2 md:translate-x-16 z-10 rounded-full shadow-lg bg-white dark:bg-slate-800 w-12 h-12"
                    >
                        <FontAwesomeIcon icon={faChevronRight} className="w-6 h-6" />
                    </Button>

                    <Card className="p-8 md:p-12 bg-slate-50 dark:bg-slate-900 border-slate-200 dark:border-slate-800 rounded-3xl">
                        <div className="flex flex-col md:flex-row gap-8 items-center">
                            <div className="flex flex-col items-center text-center md:w-1/3">
                                <div className="relative mb-4">
                                    <Avatar className="w-24 h-24 ring-4 ring-purple-100 dark:ring-purple-900 shadow-xl">
                                        <AvatarImage src={current.image} />
                                        <AvatarFallback className="bg-gradient-to-br from-purple-500 to-pink-500 text-white text-2xl font-bold">
                                            {current.avatar}
                                        </AvatarFallback>
                                    </Avatar>
                                    {current.verified && (
                                        <div className="absolute -bottom-1 -right-1 w-8 h-8 bg-blue-500 rounded-full flex items-center justify-center border-4 border-white dark:border-slate-900">
                                            <FontAwesomeIcon icon={faCircleCheck} className="w-4 h-4 text-white" />
                                        </div>
                                    )}
                                </div>
                                <div className="font-bold text-xl text-slate-900 dark:text-white">
                                    {current.name}
                                </div>
                                <div className="text-sm text-purple-600 dark:text-purple-400 font-medium">
                                    {current.role}
                                </div>
                                <div className="text-sm text-slate-500 dark:text-slate-400">
                                    {current.location}
                                </div>
                                <div className="flex gap-0.5 mt-3">
                                    {Array.from({ length: current.rating }).map((_, i) => (
                                        <FontAwesomeIcon key={i} icon={faStar} className="w-5 h-5 text-yellow-400" />
                                    ))}
                                </div>
                                <div className="mt-3 px-4 py-1.5 bg-green-100 dark:bg-green-900/30 text-green-700 dark:text-green-400 text-sm font-semibold rounded-full">
                                    {current.savings}
                                </div>
                            </div>

                            <div className="flex-1 md:pl-8 md:border-l border-slate-200 dark:border-slate-700">
                                <FontAwesomeIcon icon={faQuoteLeft} className="w-10 h-10 text-purple-200 dark:text-purple-800 mb-4" />
                                <p className="text-xl md:text-2xl text-slate-700 dark:text-slate-200 leading-relaxed font-light">
                                    {current.text}
                                </p>
                            </div>
                        </div>
                    </Card>

                    <div className="flex justify-center gap-2 mt-8">
                        {TESTIMONIALS.map((_, index) => (
                            <button
                                key={index}
                                onClick={() => {
                                    setAutoPlay(false);
                                    setCurrentIndex(index);
                                }}
                                className={`h-2 rounded-full transition-all duration-300 ${index === currentIndex
                                    ? "bg-purple-600 w-8"
                                    : "bg-slate-300 dark:bg-slate-600 hover:bg-purple-400 w-2"
                                    }`}
                            />
                        ))}
                    </div>
                </div>
            </div>
        </AnimatedSection>
    );
}
