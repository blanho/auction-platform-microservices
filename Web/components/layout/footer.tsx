"use client";

import { useState } from "react";
import Link from "next/link";
import { motion } from "framer-motion";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { ThemeToggle } from "@/components/ui/theme-toggle";
import {
    Accordion,
    AccordionContent,
    AccordionItem,
    AccordionTrigger,
} from "@/components/ui/accordion";
import {
    Facebook,
    Twitter,
    Instagram,
    Youtube,
    Apple,
    Smartphone,
    Mail,
    Phone,
    MapPin,
    Shield,
    CreditCard,
    Truck,
    Award,
    Lock,
    CheckCircle,
    ArrowRight,
    Gavel,
    Heart,
    Send,
    Globe,
    Sparkles,
    Users,
    TrendingUp,
    Clock,
} from "lucide-react";
import { cn } from "@/lib/utils";

const footerLinks = {
    categories: [
        { name: "Electronics", href: "/search?category=electronics" },
        { name: "Sneakers", href: "/search?category=sneakers" },
        { name: "Luxury", href: "/search?category=luxury" },
        { name: "Collectibles", href: "/search?category=collectibles" },
        { name: "Watches", href: "/search?category=watches" },
        { name: "Art", href: "/search?category=art" },
    ],
    company: [
        { name: "About Us", href: "/about" },
        { name: "Careers", href: "/careers" },
        { name: "Press", href: "/press" },
        { name: "Blog", href: "/blog" },
        { name: "Affiliate Program", href: "/affiliate" },
    ],
    support: [
        { name: "Help Center", href: "/help" },
        { name: "Contact Us", href: "/contact" },
        { name: "Buyer Protection", href: "/buyer-protection" },
        { name: "Seller Guide", href: "/seller-guide" },
        { name: "FAQs", href: "/faq" },
    ],
    legal: [
        { name: "Terms of Service", href: "/terms" },
        { name: "Privacy Policy", href: "/privacy" },
        { name: "Cookie Policy", href: "/cookies" },
        { name: "Bidding Rules", href: "/bidding-rules" },
    ],
};

const socialLinks = [
    { name: "Facebook", icon: Facebook, href: "https://facebook.com", color: "hover:bg-blue-600 hover:border-blue-600" },
    { name: "Twitter", icon: Twitter, href: "https://twitter.com", color: "hover:bg-sky-500 hover:border-sky-500" },
    { name: "Instagram", icon: Instagram, href: "https://instagram.com", color: "hover:bg-gradient-to-br hover:from-purple-600 hover:to-pink-500 hover:border-pink-500" },
    { name: "YouTube", icon: Youtube, href: "https://youtube.com", color: "hover:bg-red-600 hover:border-red-600" },
];

const trustBadges = [
    { icon: Shield, label: "Buyer Protection", desc: "100% money-back guarantee", color: "from-green-500 to-emerald-600" },
    { icon: CreditCard, label: "Secure Payments", desc: "256-bit SSL encryption", color: "from-blue-500 to-cyan-600" },
    { icon: Truck, label: "Fast Shipping", desc: "Worldwide delivery", color: "from-orange-500 to-amber-600" },
    { icon: Award, label: "Verified Sellers", desc: "Trusted marketplace", color: "from-purple-500 to-violet-600" },
];

const platformStats = [
    { icon: Users, value: "500K+", label: "Active Users" },
    { icon: TrendingUp, value: "$2.5B", label: "Total Sales" },
    { icon: Clock, value: "24/7", label: "Live Support" },
    { icon: Globe, value: "150+", label: "Countries" },
];

const faqs = [
    {
        question: "How do I place a bid?",
        answer: "Simply browse auctions, find an item you like, and enter your maximum bid. Our system will automatically bid on your behalf up to your maximum amount.",
    },
    {
        question: "What payment methods do you accept?",
        answer: "We accept all major credit cards, PayPal, and bank transfers. All payments are processed securely through our encrypted payment system.",
    },
    {
        question: "How is shipping handled?",
        answer: "Sellers are responsible for shipping items. Shipping costs and estimated delivery times are displayed on each auction listing.",
    },
];

export function Footer() {
    const [email, setEmail] = useState("");
    const [isSubscribed, setIsSubscribed] = useState(false);

    const handleSubscribe = (e: React.FormEvent) => {
        e.preventDefault();
        if (email) {
            setIsSubscribed(true);
            setEmail("");
            setTimeout(() => setIsSubscribed(false), 3000);
        }
    };

    return (
        <footer className="bg-slate-900 dark:bg-slate-950 text-white relative overflow-hidden">
            <div className="absolute inset-0 bg-gradient-to-b from-purple-900/5 via-transparent to-blue-900/5 pointer-events-none" />
            <div className="absolute top-0 left-1/4 w-96 h-96 bg-purple-500/10 rounded-full blur-3xl pointer-events-none" />
            <div className="absolute bottom-0 right-1/4 w-96 h-96 bg-blue-500/10 rounded-full blur-3xl pointer-events-none" />

            <div className="relative">
                <div className="border-b border-slate-800/80">
                    <div className="container mx-auto px-4 py-10">
                        <div className="grid grid-cols-2 md:grid-cols-4 gap-4 md:gap-6">
                            {trustBadges.map((badge, index) => (
                                <motion.div
                                    key={badge.label}
                                    initial={{ opacity: 0, y: 20 }}
                                    whileInView={{ opacity: 1, y: 0 }}
                                    viewport={{ once: true }}
                                    transition={{ delay: index * 0.1 }}
                                    className="group relative"
                                >
                                    <div className="absolute inset-0 bg-gradient-to-br from-purple-600/20 to-blue-600/20 rounded-2xl blur-xl opacity-0 group-hover:opacity-100 transition-opacity duration-500" />
                                    <div className="relative flex flex-col items-center text-center p-6 rounded-2xl bg-slate-800/50 dark:bg-slate-900/50 border border-slate-700/50 hover:border-purple-500/50 transition-all duration-300 hover:-translate-y-1">
                                        <div className={cn(
                                            "p-3 rounded-xl bg-gradient-to-br mb-3 shadow-lg",
                                            badge.color
                                        )}>
                                            <badge.icon className="w-6 h-6 text-white" />
                                        </div>
                                        <h4 className="font-semibold text-white mb-1">{badge.label}</h4>
                                        <p className="text-sm text-slate-400">{badge.desc}</p>
                                    </div>
                                </motion.div>
                            ))}
                        </div>
                    </div>
                </div>

                <div className="border-b border-slate-800/80">
                    <div className="container mx-auto px-4 py-12">
                        <div className="relative rounded-3xl overflow-hidden">
                            <div className="absolute inset-0 bg-gradient-to-r from-purple-600 via-indigo-600 to-blue-600" />
                            <div className="absolute inset-0 bg-[url('/grid-pattern.svg')] opacity-10" />
                            <div className="relative px-6 py-10 md:px-12 md:py-14">
                                <div className="flex flex-col lg:flex-row items-center justify-between gap-8">
                                    <div className="text-center lg:text-left">
                                        <motion.div
                                            initial={{ opacity: 0, y: 20 }}
                                            whileInView={{ opacity: 1, y: 0 }}
                                            viewport={{ once: true }}
                                            className="flex items-center justify-center lg:justify-start gap-2 mb-3"
                                        >
                                            <Sparkles className="w-5 h-5 text-yellow-300" />
                                            <span className="text-sm font-medium text-white/80">Join 100,000+ auction enthusiasts</span>
                                        </motion.div>
                                        <h3 className="text-2xl md:text-3xl font-bold mb-2 text-white">
                                            Stay in the loop
                                        </h3>
                                        <p className="text-white/70 max-w-md">
                                            Get notified about new auctions, exclusive deals, and platform updates.
                                        </p>
                                    </div>
                                    <form onSubmit={handleSubscribe} className="flex flex-col sm:flex-row gap-3 w-full lg:w-auto">
                                        <div className="relative flex-1 lg:w-80">
                                            <Mail className="absolute left-4 top-1/2 -translate-y-1/2 h-5 w-5 text-slate-400" />
                                            <Input
                                                type="email"
                                                placeholder="Enter your email"
                                                value={email}
                                                onChange={(e) => setEmail(e.target.value)}
                                                className="pl-12 bg-white/10 backdrop-blur-sm border-white/20 focus:border-white/40 h-12 text-white placeholder:text-white/50 rounded-xl"
                                            />
                                        </div>
                                        <Button
                                            type="submit"
                                            size="lg"
                                            className="h-12 px-8 bg-white text-purple-600 hover:bg-white/90 font-semibold rounded-xl shadow-lg hover:shadow-xl transition-all"
                                        >
                                            {isSubscribed ? (
                                                <>
                                                    <CheckCircle className="w-5 h-5 mr-2" />
                                                    Subscribed!
                                                </>
                                            ) : (
                                                <>
                                                    <Send className="w-5 h-5 mr-2" />
                                                    Subscribe
                                                </>
                                            )}
                                        </Button>
                                    </form>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <div className="container mx-auto px-4 py-12 md:py-16">
                    <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-8 lg:gap-6">
                        <div className="col-span-2">
                            <Link href="/" className="flex items-center gap-2.5 mb-5 group">
                                <motion.div
                                    whileHover={{ rotate: -10 }}
                                    className="w-11 h-11 bg-gradient-to-br from-purple-600 to-blue-600 rounded-xl flex items-center justify-center shadow-lg shadow-purple-500/25"
                                >
                                    <Gavel className="h-6 w-6 text-white" />
                                </motion.div>
                                <div className="flex flex-col">
                                    <span className="text-xl font-bold text-white">AuctionHub</span>
                                    <span className="text-[10px] text-slate-400 -mt-0.5">Where Value Finds You</span>
                                </div>
                            </Link>
                            <p className="text-slate-400 text-sm mb-6 max-w-xs leading-relaxed">
                                The premier marketplace where value finds you. Bid, win, and sell with confidence in our trusted community.
                            </p>

                            <div className="grid grid-cols-2 gap-3 mb-6">
                                {platformStats.map((stat) => (
                                    <div key={stat.label} className="flex items-center gap-2 p-2 rounded-lg bg-slate-800/50 dark:bg-slate-900/50">
                                        <stat.icon className="w-4 h-4 text-purple-400" />
                                        <div>
                                            <div className="text-sm font-bold text-white">{stat.value}</div>
                                            <div className="text-[10px] text-slate-500">{stat.label}</div>
                                        </div>
                                    </div>
                                ))}
                            </div>

                            <div className="flex flex-col sm:flex-row gap-2 mb-6">
                                <Button
                                    variant="outline"
                                    size="sm"
                                    className="justify-start bg-slate-800/50 border-slate-700 hover:bg-slate-700 hover:border-purple-500 text-white transition-all h-auto py-2"
                                    asChild
                                >
                                    <a href="#" className="flex items-center gap-2.5">
                                        <Apple className="h-5 w-5" />
                                        <div className="text-left">
                                            <div className="text-[9px] text-slate-400 leading-tight">Download on the</div>
                                            <div className="text-xs font-semibold leading-tight">App Store</div>
                                        </div>
                                    </a>
                                </Button>
                                <Button
                                    variant="outline"
                                    size="sm"
                                    className="justify-start bg-slate-800/50 border-slate-700 hover:bg-slate-700 hover:border-purple-500 text-white transition-all h-auto py-2"
                                    asChild
                                >
                                    <a href="#" className="flex items-center gap-2.5">
                                        <Smartphone className="h-5 w-5" />
                                        <div className="text-left">
                                            <div className="text-[9px] text-slate-400 leading-tight">Get it on</div>
                                            <div className="text-xs font-semibold leading-tight">Google Play</div>
                                        </div>
                                    </a>
                                </Button>
                            </div>

                            <div className="flex gap-2">
                                {socialLinks.map((social) => (
                                    <a
                                        key={social.name}
                                        href={social.href}
                                        target="_blank"
                                        rel="noopener noreferrer"
                                        className={cn(
                                            "p-2.5 rounded-xl bg-slate-800/50 dark:bg-slate-900/50 border border-slate-700/50 text-slate-400 hover:text-white transition-all duration-300",
                                            social.color
                                        )}
                                        aria-label={social.name}
                                    >
                                        <social.icon className="h-5 w-5" />
                                    </a>
                                ))}
                            </div>
                        </div>

                        <div>
                            <h3 className="font-semibold mb-4 text-white text-sm uppercase tracking-wider">Categories</h3>
                            <ul className="space-y-2.5">
                                {footerLinks.categories.map((link) => (
                                    <li key={link.name}>
                                        <Link
                                            href={link.href}
                                            className="text-sm text-slate-400 hover:text-purple-400 transition-colors flex items-center gap-1 group"
                                        >
                                            <ArrowRight className="w-3 h-3 opacity-0 -ml-4 group-hover:opacity-100 group-hover:ml-0 transition-all duration-200" />
                                            {link.name}
                                        </Link>
                                    </li>
                                ))}
                            </ul>
                        </div>

                        <div>
                            <h3 className="font-semibold mb-4 text-white text-sm uppercase tracking-wider">Company</h3>
                            <ul className="space-y-2.5">
                                {footerLinks.company.map((link) => (
                                    <li key={link.name}>
                                        <Link
                                            href={link.href}
                                            className="text-sm text-slate-400 hover:text-purple-400 transition-colors flex items-center gap-1 group"
                                        >
                                            <ArrowRight className="w-3 h-3 opacity-0 -ml-4 group-hover:opacity-100 group-hover:ml-0 transition-all duration-200" />
                                            {link.name}
                                        </Link>
                                    </li>
                                ))}
                            </ul>
                        </div>

                        <div>
                            <h3 className="font-semibold mb-4 text-white text-sm uppercase tracking-wider">Support</h3>
                            <ul className="space-y-2.5">
                                {footerLinks.support.map((link) => (
                                    <li key={link.name}>
                                        <Link
                                            href={link.href}
                                            className="text-sm text-slate-400 hover:text-purple-400 transition-colors flex items-center gap-1 group"
                                        >
                                            <ArrowRight className="w-3 h-3 opacity-0 -ml-4 group-hover:opacity-100 group-hover:ml-0 transition-all duration-200" />
                                            {link.name}
                                        </Link>
                                    </li>
                                ))}
                            </ul>
                        </div>

                        <div className="col-span-2 md:col-span-1">
                            <h3 className="font-semibold mb-4 text-white text-sm uppercase tracking-wider">Quick FAQ</h3>
                            <Accordion type="single" collapsible className="space-y-2">
                                {faqs.map((faq, index) => (
                                    <AccordionItem
                                        key={index}
                                        value={`faq-${index}`}
                                        className="border border-slate-700/50 rounded-xl px-3 bg-slate-800/30 hover:bg-slate-800/50 transition-colors"
                                    >
                                        <AccordionTrigger className="text-sm text-left hover:text-purple-400 py-3 hover:no-underline">
                                            {faq.question}
                                        </AccordionTrigger>
                                        <AccordionContent className="text-sm text-slate-400 pb-3">
                                            {faq.answer}
                                        </AccordionContent>
                                    </AccordionItem>
                                ))}
                            </Accordion>
                        </div>
                    </div>
                </div>

                <div className="border-t border-slate-800/80 bg-slate-800/20">
                    <div className="container mx-auto px-4 py-6">
                        <div className="flex flex-col md:flex-row items-center justify-center gap-4 md:gap-8 text-sm text-slate-400">
                            <a href="mailto:support@auctionhub.com" className="flex items-center gap-2 hover:text-purple-400 transition-colors">
                                <Mail className="w-4 h-4" />
                                support@auctionhub.com
                            </a>
                            <span className="hidden md:inline text-slate-700">•</span>
                            <a href="tel:1-800-AUCTION" className="flex items-center gap-2 hover:text-purple-400 transition-colors">
                                <Phone className="w-4 h-4" />
                                1-800-AUCTION
                            </a>
                            <span className="hidden md:inline text-slate-700">•</span>
                            <span className="flex items-center gap-2">
                                <MapPin className="w-4 h-4" />
                                123 Auction Lane, New York, NY 10001
                            </span>
                        </div>
                    </div>
                </div>

                <div className="border-t border-slate-800/80">
                    <div className="container mx-auto px-4 py-6">
                        <div className="flex flex-col lg:flex-row items-center justify-between gap-4">
                            <div className="flex items-center gap-4 text-sm text-slate-500">
                                <div className="flex items-center gap-2">
                                    <Lock className="w-4 h-4 text-green-500" />
                                    <span>256-bit SSL</span>
                                </div>
                                <span className="text-slate-700">•</span>
                                <span>PCI DSS Compliant</span>
                                <span className="text-slate-700">•</span>
                                <span>SOC 2 Certified</span>
                            </div>

                            <div className="flex items-center gap-3">
                                <span className="text-sm text-slate-500">Theme:</span>
                                <ThemeToggle variant="expanded" className="bg-slate-800/50 border border-slate-700/50" />
                            </div>

                            <div className="flex flex-wrap items-center justify-center gap-4">
                                {footerLinks.legal.map((link) => (
                                    <Link
                                        key={link.name}
                                        href={link.href}
                                        className="text-sm text-slate-500 hover:text-purple-400 transition-colors"
                                    >
                                        {link.name}
                                    </Link>
                                ))}
                            </div>
                        </div>
                    </div>
                </div>

                <div className="border-t border-slate-800/80 bg-slate-950/50">
                    <div className="container mx-auto px-4 py-4">
                        <div className="flex flex-col sm:flex-row items-center justify-center gap-2 text-sm text-slate-500">
                            <span>Made with</span>
                            <Heart className="w-4 h-4 text-red-500 fill-red-500 animate-pulse" />
                            <span>by the AuctionHub Team</span>
                            <span className="hidden sm:inline text-slate-700">•</span>
                            <span>© {new Date().getFullYear()} AuctionHub. All rights reserved.</span>
                        </div>
                    </div>
                </div>
            </div>
        </footer>
    );
}
