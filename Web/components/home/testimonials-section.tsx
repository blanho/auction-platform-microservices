"use client";

import { useState, useEffect } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Card } from "@/components/ui/card";
import { Star, ChevronLeft, ChevronRight, Quote } from "lucide-react";
import { Button } from "@/components/ui/button";

const testimonials = [
    {
        id: 1,
        name: "Sarah Johnson",
        avatar: "SJ",
        role: "Collector",
        rating: 5,
        text: "Won my dream vintage watch at 40% below retail! The bidding experience was thrilling.",
        image: "https://randomuser.me/api/portraits/women/1.jpg",
    },
    {
        id: 2,
        name: "Michael Chen",
        avatar: "MC",
        role: "Tech Enthusiast",
        rating: 5,
        text: "Incredible deals on electronics. The real-time notifications kept me ahead of other bidders.",
        image: "https://randomuser.me/api/portraits/men/2.jpg",
    },
    {
        id: 3,
        name: "Emily Williams",
        avatar: "EW",
        role: "Fashion Buyer",
        rating: 5,
        text: "Authentic luxury items at unbeatable prices. The verification process gives me confidence.",
        image: "https://randomuser.me/api/portraits/women/3.jpg",
    },
    {
        id: 4,
        name: "David Martinez",
        avatar: "DM",
        role: "Car Collector",
        rating: 5,
        text: "Found a rare classic car I'd been searching for years. The platform is trustworthy.",
        image: "https://randomuser.me/api/portraits/men/4.jpg",
    },
    {
        id: 5,
        name: "Jessica Lee",
        avatar: "JL",
        role: "Art Investor",
        rating: 5,
        text: "The auction process is transparent and secure. Already won 5 pieces for my collection!",
        image: "https://randomuser.me/api/portraits/women/5.jpg",
    },
];

export function TestimonialsSection() {
    const [currentIndex, setCurrentIndex] = useState(0);
    const [autoPlay, setAutoPlay] = useState(true);

    useEffect(() => {
        if (!autoPlay) return;

        const interval = setInterval(() => {
            setCurrentIndex((prev) => (prev + 1) % testimonials.length);
        }, 5000);

        return () => clearInterval(interval);
    }, [autoPlay]);

    const goToPrev = () => {
        setAutoPlay(false);
        setCurrentIndex((prev) => (prev - 1 + testimonials.length) % testimonials.length);
    };

    const goToNext = () => {
        setAutoPlay(false);
        setCurrentIndex((prev) => (prev + 1) % testimonials.length);
    };

    return (
        <section className="py-20 bg-gradient-to-b from-purple-50 to-white dark:from-slate-900 dark:to-slate-950 overflow-hidden">
            <div className="container mx-auto px-4">
                {/* Header */}
                <div className="text-center mb-16">
                    <motion.div
                        initial={{ opacity: 0, y: 20 }}
                        whileInView={{ opacity: 1, y: 0 }}
                        viewport={{ once: true }}
                    >
                        <h2 className="text-3xl md:text-4xl font-bold text-slate-900 dark:text-white mb-4">
                            What Our Bidders Say
                        </h2>
                        <p className="text-slate-600 dark:text-slate-400 max-w-2xl mx-auto">
                            Join thousands of satisfied customers who found amazing deals
                        </p>
                    </motion.div>
                </div>

                {/* Testimonials carousel */}
                <div className="relative max-w-4xl mx-auto">
                    {/* Navigation buttons */}
                    <Button
                        variant="outline"
                        size="icon"
                        onClick={goToPrev}
                        className="absolute left-0 top-1/2 -translate-y-1/2 -translate-x-4 md:-translate-x-12 z-10 rounded-full shadow-lg"
                    >
                        <ChevronLeft className="w-5 h-5" />
                    </Button>
                    <Button
                        variant="outline"
                        size="icon"
                        onClick={goToNext}
                        className="absolute right-0 top-1/2 -translate-y-1/2 translate-x-4 md:translate-x-12 z-10 rounded-full shadow-lg"
                    >
                        <ChevronRight className="w-5 h-5" />
                    </Button>

                    {/* Card */}
                    <AnimatePresence mode="wait">
                        <motion.div
                            key={currentIndex}
                            initial={{ opacity: 0, x: 50 }}
                            animate={{ opacity: 1, x: 0 }}
                            exit={{ opacity: 0, x: -50 }}
                            transition={{ duration: 0.3 }}
                        >
                            <Card className="p-8 md:p-12 bg-white dark:bg-slate-800 shadow-xl border-0">
                                <div className="flex flex-col items-center text-center">
                                    {/* Quote icon */}
                                    <Quote className="w-12 h-12 text-purple-200 dark:text-purple-800 mb-6" />

                                    {/* Rating */}
                                    <div className="flex gap-1 mb-6">
                                        {Array.from({ length: testimonials[currentIndex].rating }).map((_, i) => (
                                            <Star key={i} className="w-5 h-5 fill-yellow-400 text-yellow-400" />
                                        ))}
                                    </div>

                                    {/* Text */}
                                    <p className="text-xl md:text-2xl text-slate-700 dark:text-slate-200 mb-8 leading-relaxed">
                                        &ldquo;{testimonials[currentIndex].text}&rdquo;
                                    </p>

                                    {/* Author */}
                                    <Avatar className="w-16 h-16 mb-4 ring-4 ring-purple-100 dark:ring-purple-900">
                                        <AvatarImage src={testimonials[currentIndex].image} />
                                        <AvatarFallback className="bg-purple-100 text-purple-600 dark:bg-purple-900 dark:text-purple-300 text-lg font-semibold">
                                            {testimonials[currentIndex].avatar}
                                        </AvatarFallback>
                                    </Avatar>
                                    <div className="font-semibold text-slate-900 dark:text-white">
                                        {testimonials[currentIndex].name}
                                    </div>
                                    <div className="text-sm text-slate-500 dark:text-slate-400">
                                        {testimonials[currentIndex].role}
                                    </div>
                                </div>
                            </Card>
                        </motion.div>
                    </AnimatePresence>

                    {/* Dots indicator */}
                    <div className="flex justify-center gap-2 mt-8">
                        {testimonials.map((_, index) => (
                            <button
                                key={index}
                                onClick={() => {
                                    setAutoPlay(false);
                                    setCurrentIndex(index);
                                }}
                                className={`w-2 h-2 rounded-full transition-all duration-300 ${index === currentIndex
                                        ? "bg-purple-600 w-8"
                                        : "bg-slate-300 dark:bg-slate-600 hover:bg-purple-400"
                                    }`}
                            />
                        ))}
                    </div>
                </div>
            </div>
        </section>
    );
}
