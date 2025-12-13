"use client";

import { useMemo, useCallback } from "react";
import { motion, useMotionValue, useSpring, useTransform } from "framer-motion";
import Image from "next/image";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
    Flame,
    ArrowRight,
    TrendingUp,
    Users,
    Star,
} from "lucide-react";
import Link from "next/link";
import { useFeaturedAuctions } from "@/hooks/use-auctions";
import { useCountdown } from "@/hooks/use-countdown";
import { VALUE_PROPS, TRUST_LOGOS, HERO_CONTENT } from "@/constants/landing";
import { PulsingDot } from "@/components/ui/animated";

interface AuctionData {
    id?: string;
    make?: string;
    model?: string;
    currentHighBid?: number;
    reservePrice?: number;
    auctionEnd?: string;
    isFeatured?: boolean;
    categoryName?: string;
}

function AuctionShowcase({ auction, imageUrl }: { auction: AuctionData | null | undefined; imageUrl: string }) {
    const mouseX = useMotionValue(0);
    const mouseY = useMotionValue(0);
    const rotateX = useSpring(useTransform(mouseY, [-300, 300], [5, -5]), { stiffness: 150, damping: 20 });
    const rotateY = useSpring(useTransform(mouseX, [-300, 300], [-5, 5]), { stiffness: 150, damping: 20 });

    const handleMouse = useCallback(
        (e: React.MouseEvent<HTMLDivElement>) => {
            const rect = e.currentTarget.getBoundingClientRect();
            mouseX.set(e.clientX - rect.left - rect.width / 2);
            mouseY.set(e.clientY - rect.top - rect.height / 2);
        },
        [mouseX, mouseY]
    );

    const handleMouseLeave = useCallback(() => {
        mouseX.set(0);
        mouseY.set(0);
    }, [mouseX, mouseY]);

    const timeLeft = useCountdown(auction?.auctionEnd || null);

    const formatTime = (value: number) => value.toString().padStart(2, "0");
    const currentBid = auction?.currentHighBid || auction?.reservePrice || 0;
    const title = auction ? `${auction.make} ${auction.model}` : "Luxury Timepiece Collection";

    return (
        <motion.div
            onMouseMove={handleMouse}
            onMouseLeave={handleMouseLeave}
            style={{ rotateX, rotateY, transformStyle: "preserve-3d" }}
            className="relative perspective-1000"
        >
            <div className="relative bg-white rounded-3xl shadow-2xl overflow-hidden">
                <div className="absolute top-4 left-4 z-20 flex gap-2">
                    <Badge className="bg-red-500 text-white shadow-lg border-0">
                        <span className="w-2 h-2 bg-white rounded-full mr-2 animate-pulse" />
                        LIVE NOW
                    </Badge>
                    <Badge className="bg-gradient-to-r from-amber-500 to-orange-500 text-white shadow-lg border-0">
                        <Flame className="w-3 h-3 mr-1" />
                        Hot
                    </Badge>
                </div>

                <div className="relative h-64 lg:h-80 overflow-hidden">
                    <Image
                        src={imageUrl}
                        alt={title}
                        fill
                        className="object-cover"
                        unoptimized={imageUrl.includes("unsplash")}
                        priority
                    />
                    <div className="absolute inset-0 bg-gradient-to-t from-black/80 via-black/20 to-transparent" />
                    <div className="absolute bottom-4 left-4 right-4">
                        <p className="text-xs text-white/70 uppercase tracking-wider mb-1">{auction?.categoryName || "Luxury Watches"}</p>
                        <h3 className="text-xl lg:text-2xl font-bold text-white line-clamp-2">{title}</h3>
                    </div>
                </div>

                <div className="p-5 space-y-4 bg-gradient-to-b from-white to-slate-50">
                    <div className="flex items-center justify-between">
                        <div>
                            <p className="text-xs text-slate-500 uppercase tracking-wider">Current Bid</p>
                            <p className="text-3xl font-bold text-slate-900">
                                ${currentBid.toLocaleString()}
                            </p>
                        </div>
                        <div className="text-right">
                            <p className="text-xs text-slate-500 uppercase tracking-wider">Time Left</p>
                            <div className="flex gap-1 mt-1">
                                {[
                                    { value: timeLeft?.hours ?? 0, label: "h" },
                                    { value: timeLeft?.minutes ?? 0, label: "m" },
                                    { value: timeLeft?.seconds ?? 0, label: "s" },
                                ].map((item, idx) => (
                                    <span key={idx} className="text-lg font-mono font-bold text-slate-900 bg-slate-100 px-2 py-0.5 rounded">
                                        {formatTime(item.value)}{item.label}
                                    </span>
                                ))}
                            </div>
                        </div>
                    </div>

                    <div className="flex items-center justify-between text-sm">
                        <div className="flex items-center gap-4">
                            <span className="flex items-center gap-1 text-slate-600">
                                <Users className="w-4 h-4" />
                                <span className="font-medium">23 bidders</span>
                            </span>
                            <span className="flex items-center gap-1 text-green-600">
                                <TrendingUp className="w-4 h-4" />
                                <span className="font-medium">+$2,400</span>
                            </span>
                        </div>
                    </div>

                    <Button
                        className="w-full h-12 text-base font-semibold bg-slate-900 hover:bg-slate-800 text-white transition-all"
                        asChild
                    >
                        <Link href={auction?.id ? `/auctions/${auction.id}` : "/auctions"}>
                            Place Bid Now
                            <ArrowRight className="ml-2 w-4 h-4" />
                        </Link>
                    </Button>
                </div>
            </div>
        </motion.div>
    );
}

export function HeroSection() {
    const { data: featuredAuctions } = useFeaturedAuctions(1);
    const heroAuction = featuredAuctions?.[0] as AuctionData | undefined;

    const imageUrl = useMemo(() => {
        if (!heroAuction) return "https://images.unsplash.com/photo-1614162692292-7ac56d7f7f1e?w=800";
        const auctionData = heroAuction as { files?: Array<{ isPrimary?: boolean; url: string }> };
        const primaryFile = auctionData.files?.find((f) => f.isPrimary);
        return primaryFile?.url || auctionData.files?.[0]?.url || "https://images.unsplash.com/photo-1614162692292-7ac56d7f7f1e?w=800";
    }, [heroAuction]);

    return (
        <section className="relative min-h-screen overflow-hidden bg-slate-950">
            <div className="absolute inset-0">
                <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top,_var(--tw-gradient-stops))] from-purple-900/40 via-slate-950 to-slate-950" />
                <div className="absolute top-0 left-1/2 -translate-x-1/2 w-[800px] h-[600px] bg-purple-600/20 rounded-full blur-[120px]" />
                <div className="absolute bottom-0 right-0 w-[600px] h-[600px] bg-pink-600/10 rounded-full blur-[120px]" />
            </div>

            <div className="container relative mx-auto px-4 pt-20 pb-16 lg:pt-32 lg:pb-24">
                <div className="grid lg:grid-cols-2 gap-12 lg:gap-16 items-center">
                    <div className="space-y-10 text-center lg:text-left">
                        <div className="space-y-6">
                            <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-white/5 border border-white/10 backdrop-blur-sm">
                                <PulsingDot />
                                <span className="text-sm font-medium text-green-400">{HERO_CONTENT.BADGE_TEXT}</span>
                            </div>

                            <h1 className="text-4xl sm:text-5xl lg:text-6xl xl:text-7xl font-bold leading-[1.1] tracking-tight">
                                <span className="block text-white">
                                    {HERO_CONTENT.HEADLINE.LINE_1}
                                </span>
                                <span className="block mt-2 text-transparent bg-clip-text bg-gradient-to-r from-purple-400 via-pink-400 to-orange-400">
                                    {HERO_CONTENT.HEADLINE.LINE_2}
                                </span>
                            </h1>

                            <p className="text-lg sm:text-xl text-slate-400 max-w-xl mx-auto lg:mx-0 leading-relaxed">
                                {HERO_CONTENT.SUBHEADLINE.TEXT} <span className="text-white font-semibold">{HERO_CONTENT.SUBHEADLINE.MEMBERS}</span> {HERO_CONTENT.SUBHEADLINE.SAVINGS_TEXT}{" "}
                                <span className="text-green-400 font-semibold">{HERO_CONTENT.SUBHEADLINE.SAVINGS_AMOUNT}</span>. 
                                {HERO_CONTENT.SUBHEADLINE.ENDING}
                            </p>
                        </div>

                        <div className="flex flex-col sm:flex-row gap-4 justify-center lg:justify-start">
                            <Button
                                size="lg"
                                className="h-14 px-8 text-base font-semibold bg-white text-slate-900 hover:bg-slate-100 shadow-xl shadow-white/10 transition-all duration-300"
                                asChild
                            >
                                <Link href="/auctions">
                                    {HERO_CONTENT.CTA.PRIMARY}
                                    <ArrowRight className="ml-2 w-5 h-5" />
                                </Link>
                            </Button>

                            <Button
                                size="lg"
                                variant="outline"
                                className="h-14 px-8 text-base font-semibold border-white/20 text-white hover:bg-white/10 backdrop-blur-sm transition-colors"
                                asChild
                            >
                                <Link href="/auth/register">
                                    {HERO_CONTENT.CTA.SECONDARY}
                                </Link>
                            </Button>
                        </div>

                        <div className="flex flex-wrap items-center justify-center lg:justify-start gap-x-6 gap-y-3 pt-2">
                            {VALUE_PROPS.map((prop, idx) => (
                                <div key={idx} className="flex items-center gap-2 text-slate-400">
                                    <prop.icon className="w-4 h-4 text-green-400" />
                                    <span className="text-sm">{prop.text}</span>
                                </div>
                            ))}
                        </div>

                        <div className="pt-6 border-t border-white/10">
                            <p className="text-xs text-slate-500 mb-3 uppercase tracking-wider">Featured in</p>
                            <div className="flex items-center justify-center lg:justify-start gap-8">
                                {TRUST_LOGOS.map((logo, idx) => (
                                    <span key={idx} className="text-slate-600 font-bold text-sm tracking-wide">
                                        {logo.text}
                                    </span>
                                ))}
                            </div>
                        </div>
                    </div>

                    <div className="relative">
                        <AuctionShowcase auction={heroAuction} imageUrl={imageUrl} />
                        
                        <div className="absolute -bottom-6 -left-6 hidden lg:block">
                            <div className="flex items-center gap-3 px-5 py-4 rounded-2xl bg-white shadow-2xl">
                                <div className="w-12 h-12 rounded-xl bg-gradient-to-br from-green-500 to-emerald-600 flex items-center justify-center">
                                    <TrendingUp className="w-6 h-6 text-white" />
                                </div>
                                <div>
                                    <p className="text-2xl font-bold text-slate-900">$2.4M+</p>
                                    <p className="text-sm text-slate-500">Saved by members this month</p>
                                </div>
                            </div>
                        </div>

                        <div className="absolute -top-4 -right-4 hidden lg:block">
                            <div className="flex items-center gap-2 px-4 py-3 rounded-xl bg-white shadow-2xl">
                                <div className="flex -space-x-2">
                                    {[1, 2, 3, 4].map((i) => (
                                        <div key={i} className="w-8 h-8 rounded-full bg-gradient-to-br from-purple-400 to-pink-400 border-2 border-white flex items-center justify-center text-xs font-bold text-white">
                                            {String.fromCharCode(64 + i)}
                                        </div>
                                    ))}
                                </div>
                                <div className="pl-2">
                                    <div className="flex items-center gap-1">
                                        {[1, 2, 3, 4, 5].map((i) => (
                                            <Star key={i} className="w-3 h-3 fill-yellow-400 text-yellow-400" />
                                        ))}
                                    </div>
                                    <p className="text-xs text-slate-600 font-medium">12,847 reviews</p>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </section>
    );
}
