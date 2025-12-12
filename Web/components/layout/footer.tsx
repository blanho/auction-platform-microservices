"use client";

import { useState } from "react";
import Link from "next/link";
import { motion } from "framer-motion";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
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
    { name: "Facebook", icon: Facebook, href: "https://facebook.com", color: "hover:bg-blue-600" },
    { name: "Twitter", icon: Twitter, href: "https://twitter.com", color: "hover:bg-sky-500" },
    { name: "Instagram", icon: Instagram, href: "https://instagram.com", color: "hover:bg-pink-600" },
    { name: "YouTube", icon: Youtube, href: "https://youtube.com", color: "hover:bg-red-600" },
];

const trustBadges = [
    { icon: Shield, label: "Buyer Protection", desc: "100% money-back guarantee" },
    { icon: CreditCard, label: "Secure Payments", desc: "256-bit SSL encryption" },
    { icon: Truck, label: "Fast Shipping", desc: "Worldwide delivery" },
    { icon: Award, label: "Verified Sellers", desc: "Trusted marketplace" },
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
        <footer className="bg-slate-900 text-white">
            {/* Trust badges section */}
            <div className="border-b border-slate-800">
                <div className="container mx-auto px-4 py-8">
                    <div className="grid grid-cols-2 md:grid-cols-4 gap-6">
                        {trustBadges.map((badge, index) => (
                            <motion.div
                                key={badge.label}
                                initial={{ opacity: 0, y: 20 }}
                                whileInView={{ opacity: 1, y: 0 }}
                                viewport={{ once: true }}
                                transition={{ delay: index * 0.1 }}
                                className="flex items-center gap-4 p-4 rounded-xl bg-slate-800/50 hover:bg-slate-800 transition-colors"
                            >
                                <div className="p-3 rounded-xl bg-gradient-to-br from-purple-600 to-blue-600">
                                    <badge.icon className="w-6 h-6 text-white" />
                                </div>
                                <div>
                                    <h4 className="font-semibold text-white">{badge.label}</h4>
                                    <p className="text-sm text-slate-400">{badge.desc}</p>
                                </div>
                            </motion.div>
                        ))}
                    </div>
                </div>
            </div>

            {/* Newsletter section */}
            <div className="border-b border-slate-800 bg-gradient-to-r from-purple-900/20 via-slate-900 to-blue-900/20">
                <div className="container mx-auto px-4 py-12">
                    <div className="flex flex-col lg:flex-row items-center justify-between gap-8">
                        <div className="text-center lg:text-left">
                            <h3 className="text-2xl font-bold mb-2">Stay in the loop</h3>
                            <p className="text-slate-400 max-w-md">
                                Get notified about new auctions, exclusive deals, and platform updates.
                                Join 100,000+ auction enthusiasts.
                            </p>
                        </div>
                        <form onSubmit={handleSubscribe} className="flex gap-3 w-full lg:w-auto">
                            <div className="relative flex-1 lg:w-80">
                                <Mail className="absolute left-3 top-1/2 -translate-y-1/2 h-5 w-5 text-slate-400" />
                                <Input
                                    type="email"
                                    placeholder="Enter your email"
                                    value={email}
                                    onChange={(e) => setEmail(e.target.value)}
                                    className="pl-10 bg-slate-800 border-slate-700 focus:border-purple-500 h-12"
                                />
                            </div>
                            <Button
                                type="submit"
                                className="h-12 px-6 bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700"
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

            {/* Main footer content */}
            <div className="container mx-auto px-4 py-12">
                <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-8">
                    {/* Brand column */}
                    <div className="col-span-2">
                        <Link href="/" className="flex items-center gap-2 mb-4">
                            <div className="w-10 h-10 bg-gradient-to-br from-purple-600 to-blue-600 rounded-xl flex items-center justify-center">
                                <Gavel className="h-5 w-5 text-white" />
                            </div>
                            <span className="text-xl font-bold">AuctionHub</span>
                        </Link>
                        <p className="text-slate-400 text-sm mb-6 max-w-xs">
                            The premier marketplace where value finds you. Bid, win, and sell with confidence in our trusted community.
                        </p>

                        {/* App download buttons */}
                        <div className="flex flex-col sm:flex-row gap-3 mb-6">
                            <Button
                                variant="outline"
                                className="justify-start bg-slate-800 border-slate-700 hover:bg-slate-700 hover:border-purple-500 transition-all"
                                asChild
                            >
                                <a href="#" className="flex items-center gap-3">
                                    <Apple className="h-6 w-6" />
                                    <div className="text-left">
                                        <div className="text-[10px] text-slate-400">Download on the</div>
                                        <div className="text-sm font-semibold">App Store</div>
                                    </div>
                                </a>
                            </Button>
                            <Button
                                variant="outline"
                                className="justify-start bg-slate-800 border-slate-700 hover:bg-slate-700 hover:border-purple-500 transition-all"
                                asChild
                            >
                                <a href="#" className="flex items-center gap-3">
                                    <Smartphone className="h-6 w-6" />
                                    <div className="text-left">
                                        <div className="text-[10px] text-slate-400">Get it on</div>
                                        <div className="text-sm font-semibold">Google Play</div>
                                    </div>
                                </a>
                            </Button>
                        </div>

                        {/* Social links */}
                        <div className="flex gap-2">
                            {socialLinks.map((social) => (
                                <a
                                    key={social.name}
                                    href={social.href}
                                    target="_blank"
                                    rel="noopener noreferrer"
                                    className={cn(
                                        "p-2.5 rounded-xl bg-slate-800 text-slate-400 hover:text-white transition-all",
                                        social.color
                                    )}
                                    aria-label={social.name}
                                >
                                    <social.icon className="h-5 w-5" />
                                </a>
                            ))}
                        </div>
                    </div>

                    {/* Categories */}
                    <div>
                        <h3 className="font-semibold mb-4 text-white">Categories</h3>
                        <ul className="space-y-2.5">
                            {footerLinks.categories.map((link) => (
                                <li key={link.name}>
                                    <Link
                                        href={link.href}
                                        className="text-sm text-slate-400 hover:text-purple-400 transition-colors flex items-center gap-1 group"
                                    >
                                        <ArrowRight className="w-3 h-3 opacity-0 -ml-4 group-hover:opacity-100 group-hover:ml-0 transition-all" />
                                        {link.name}
                                    </Link>
                                </li>
                            ))}
                        </ul>
                    </div>

                    {/* Company */}
                    <div>
                        <h3 className="font-semibold mb-4 text-white">Company</h3>
                        <ul className="space-y-2.5">
                            {footerLinks.company.map((link) => (
                                <li key={link.name}>
                                    <Link
                                        href={link.href}
                                        className="text-sm text-slate-400 hover:text-purple-400 transition-colors flex items-center gap-1 group"
                                    >
                                        <ArrowRight className="w-3 h-3 opacity-0 -ml-4 group-hover:opacity-100 group-hover:ml-0 transition-all" />
                                        {link.name}
                                    </Link>
                                </li>
                            ))}
                        </ul>
                    </div>

                    {/* Support */}
                    <div>
                        <h3 className="font-semibold mb-4 text-white">Support</h3>
                        <ul className="space-y-2.5">
                            {footerLinks.support.map((link) => (
                                <li key={link.name}>
                                    <Link
                                        href={link.href}
                                        className="text-sm text-slate-400 hover:text-purple-400 transition-colors flex items-center gap-1 group"
                                    >
                                        <ArrowRight className="w-3 h-3 opacity-0 -ml-4 group-hover:opacity-100 group-hover:ml-0 transition-all" />
                                        {link.name}
                                    </Link>
                                </li>
                            ))}
                        </ul>
                    </div>

                    {/* Quick FAQ */}
                    <div className="col-span-2 md:col-span-1">
                        <h3 className="font-semibold mb-4 text-white">Quick FAQ</h3>
                        <Accordion type="single" collapsible className="space-y-2">
                            {faqs.map((faq, index) => (
                                <AccordionItem
                                    key={index}
                                    value={`faq-${index}`}
                                    className="border border-slate-700 rounded-lg px-3"
                                >
                                    <AccordionTrigger className="text-sm text-left hover:text-purple-400 py-3">
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

            {/* Contact bar */}
            <div className="border-t border-slate-800 bg-slate-800/30">
                <div className="container mx-auto px-4 py-6">
                    <div className="flex flex-col md:flex-row items-center justify-center gap-6 text-sm text-slate-400">
                        <a href="mailto:support@auctionhub.com" className="flex items-center gap-2 hover:text-purple-400 transition-colors">
                            <Mail className="w-4 h-4" />
                            support@auctionhub.com
                        </a>
                        <span className="hidden md:inline text-slate-600">•</span>
                        <a href="tel:1-800-AUCTION" className="flex items-center gap-2 hover:text-purple-400 transition-colors">
                            <Phone className="w-4 h-4" />
                            1-800-AUCTION
                        </a>
                        <span className="hidden md:inline text-slate-600">•</span>
                        <span className="flex items-center gap-2">
                            <MapPin className="w-4 h-4" />
                            123 Auction Lane, New York, NY 10001
                        </span>
                    </div>
                </div>
            </div>

            {/* Bottom bar */}
            <div className="border-t border-slate-800">
                <div className="container mx-auto px-4 py-6">
                    <div className="flex flex-col md:flex-row items-center justify-between gap-4">
                        {/* Security badges */}
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

                        {/* Legal links */}
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

                        {/* Copyright */}
                        <div className="flex items-center gap-2 text-sm text-slate-500">
                            <span>Made with</span>
                            <Heart className="w-4 h-4 text-red-500 fill-red-500" />
                            <span>© {new Date().getFullYear()} AuctionHub</span>
                        </div>
                    </div>
                </div>
            </div>
        </footer>
    );
}
