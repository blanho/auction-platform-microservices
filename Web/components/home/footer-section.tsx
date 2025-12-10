"use client";

import { motion } from "framer-motion";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import {
    Accordion,
    AccordionContent,
    AccordionItem,
    AccordionTrigger,
} from "@/components/ui/accordion";
import {
    Shield,
    CreditCard,
    HelpCircle,
    Mail,
    Phone,
    MapPin,
    Facebook,
    Twitter,
    Instagram,
    Youtube,
    Lock,
    CheckCircle,
    ArrowRight,
} from "lucide-react";
import Link from "next/link";

const feeStructure = [
    { label: "Listing Fee", value: "Free", description: "No cost to list your item" },
    { label: "Seller Commission", value: "5%", description: "Only charged when item sells" },
    { label: "Buyer Premium", value: "2%", description: "Small fee on winning bids" },
    { label: "Payment Processing", value: "2.9% + $0.30", description: "Standard payment fees" },
];

const buyerProtection = [
    "100% money-back guarantee if item doesn't arrive",
    "Item authenticity verification for high-value purchases",
    "Dispute resolution within 48 hours",
    "Secure escrow payments",
    "Seller verification and ratings transparency",
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
    {
        question: "What if I receive a damaged item?",
        answer: "Our buyer protection covers damaged items. File a claim within 7 days of receipt and we'll work with you to resolve the issue or provide a full refund.",
    },
    {
        question: "Can I cancel a bid?",
        answer: "Bids are generally binding. However, you may contact us within 1 hour of placing a bid if you made an error, and we'll review your request.",
    },
];

const quickLinks = [
    { label: "Browse Auctions", href: "/auctions" },
    { label: "Sell an Item", href: "/sell" },
    { label: "My Account", href: "/account" },
    { label: "Watchlist", href: "/watchlist" },
];

const companyLinks = [
    { label: "About Us", href: "/about" },
    { label: "Careers", href: "/careers" },
    { label: "Press", href: "/press" },
    { label: "Blog", href: "/blog" },
];

const supportLinks = [
    { label: "Help Center", href: "/help" },
    { label: "Contact Us", href: "/contact" },
    { label: "Trust & Safety", href: "/trust" },
    { label: "Seller Guide", href: "/seller-guide" },
];

const legalLinks = [
    { label: "Terms of Service", href: "/terms" },
    { label: "Privacy Policy", href: "/privacy" },
    { label: "Cookie Policy", href: "/cookies" },
    { label: "Bidding Rules", href: "/bidding-rules" },
];

export function FooterSection() {
    return (
        <footer className="bg-slate-900 text-white">
            {/* Pre-footer Transparency Section */}
            <div className="border-b border-slate-800">
                <div className="container mx-auto px-4 py-16">
                    <motion.div
                        initial={{ opacity: 0, y: 20 }}
                        whileInView={{ opacity: 1, y: 0 }}
                        viewport={{ once: true }}
                        className="text-center mb-12"
                    >
                        <h2 className="text-3xl md:text-4xl font-bold mb-4">
                            Transparent, Fair, Secure
                        </h2>
                        <p className="text-slate-400 max-w-2xl mx-auto">
                            We believe in complete transparency. Here&apos;s everything you need to know about our fees and buyer protection.
                        </p>
                    </motion.div>

                    <div className="grid md:grid-cols-3 gap-8">
                        {/* Fee Structure */}
                        <motion.div
                            initial={{ opacity: 0, y: 20 }}
                            whileInView={{ opacity: 1, y: 0 }}
                            viewport={{ once: true }}
                            className="bg-slate-800/50 rounded-2xl p-6"
                        >
                            <div className="flex items-center gap-3 mb-6">
                                <div className="p-2 rounded-lg bg-purple-500/20">
                                    <CreditCard className="w-6 h-6 text-purple-400" />
                                </div>
                                <h3 className="text-xl font-bold">Fee Structure</h3>
                            </div>
                            <div className="space-y-4">
                                {feeStructure.map((fee) => (
                                    <div key={fee.label} className="flex justify-between items-start">
                                        <div>
                                            <span className="text-white font-medium">{fee.label}</span>
                                            <p className="text-sm text-slate-400">{fee.description}</p>
                                        </div>
                                        <Badge
                                            variant="outline"
                                            className="border-purple-500/50 text-purple-400"
                                        >
                                            {fee.value}
                                        </Badge>
                                    </div>
                                ))}
                            </div>
                        </motion.div>

                        {/* Buyer Protection */}
                        <motion.div
                            initial={{ opacity: 0, y: 20 }}
                            whileInView={{ opacity: 1, y: 0 }}
                            viewport={{ once: true }}
                            transition={{ delay: 0.1 }}
                            className="bg-slate-800/50 rounded-2xl p-6"
                        >
                            <div className="flex items-center gap-3 mb-6">
                                <div className="p-2 rounded-lg bg-green-500/20">
                                    <Shield className="w-6 h-6 text-green-400" />
                                </div>
                                <h3 className="text-xl font-bold">Buyer Protection</h3>
                            </div>
                            <ul className="space-y-3">
                                {buyerProtection.map((item, index) => (
                                    <li key={index} className="flex items-start gap-3">
                                        <CheckCircle className="w-5 h-5 text-green-400 shrink-0 mt-0.5" />
                                        <span className="text-slate-300">{item}</span>
                                    </li>
                                ))}
                            </ul>
                        </motion.div>

                        {/* FAQ */}
                        <motion.div
                            initial={{ opacity: 0, y: 20 }}
                            whileInView={{ opacity: 1, y: 0 }}
                            viewport={{ once: true }}
                            transition={{ delay: 0.2 }}
                            className="bg-slate-800/50 rounded-2xl p-6"
                        >
                            <div className="flex items-center gap-3 mb-6">
                                <div className="p-2 rounded-lg bg-blue-500/20">
                                    <HelpCircle className="w-6 h-6 text-blue-400" />
                                </div>
                                <h3 className="text-xl font-bold">Common Questions</h3>
                            </div>
                            <Accordion type="single" collapsible className="space-y-2">
                                {faqs.slice(0, 3).map((faq, index) => (
                                    <AccordionItem
                                        key={index}
                                        value={`faq-${index}`}
                                        className="border-slate-700"
                                    >
                                        <AccordionTrigger className="text-sm text-left hover:text-purple-400">
                                            {faq.question}
                                        </AccordionTrigger>
                                        <AccordionContent className="text-sm text-slate-400">
                                            {faq.answer}
                                        </AccordionContent>
                                    </AccordionItem>
                                ))}
                            </Accordion>
                            <Button
                                variant="link"
                                className="text-purple-400 hover:text-purple-300 mt-4 p-0"
                                asChild
                            >
                                <Link href="/faq">
                                    View all FAQs
                                    <ArrowRight className="ml-2 w-4 h-4" />
                                </Link>
                            </Button>
                        </motion.div>
                    </div>
                </div>
            </div>

            {/* Newsletter Section */}
            <div className="border-b border-slate-800">
                <div className="container mx-auto px-4 py-12">
                    <div className="flex flex-col md:flex-row items-center justify-between gap-6">
                        <div>
                            <h3 className="text-xl font-bold mb-2">Stay in the loop</h3>
                            <p className="text-slate-400">
                                Get notified about new auctions, exclusive deals, and platform updates.
                            </p>
                        </div>
                        <div className="flex gap-3 w-full md:w-auto">
                            <Input
                                type="email"
                                placeholder="Enter your email"
                                className="bg-slate-800 border-slate-700 focus:border-purple-500 w-full md:w-80"
                            />
                            <Button className="bg-purple-600 hover:bg-purple-700 whitespace-nowrap">
                                Subscribe
                            </Button>
                        </div>
                    </div>
                </div>
            </div>

            {/* Main Footer Links */}
            <div className="container mx-auto px-4 py-12">
                <div className="grid grid-cols-2 md:grid-cols-5 gap-8">
                    {/* Brand Column */}
                    <div className="col-span-2 md:col-span-1">
                        <Link href="/" className="flex items-center gap-2 mb-4">
                            <div className="w-8 h-8 bg-gradient-to-br from-purple-600 to-blue-600 rounded-lg flex items-center justify-center">
                                <span className="text-white font-bold">A</span>
                            </div>
                            <span className="text-xl font-bold">AuctionHub</span>
                        </Link>
                        <p className="text-slate-400 text-sm mb-4">
                            The premier platform for discovering unique items and winning exciting auctions.
                        </p>
                        <div className="flex gap-3">
                            <Button variant="ghost" size="icon" className="hover:text-purple-400">
                                <Facebook className="w-5 h-5" />
                            </Button>
                            <Button variant="ghost" size="icon" className="hover:text-purple-400">
                                <Twitter className="w-5 h-5" />
                            </Button>
                            <Button variant="ghost" size="icon" className="hover:text-purple-400">
                                <Instagram className="w-5 h-5" />
                            </Button>
                            <Button variant="ghost" size="icon" className="hover:text-purple-400">
                                <Youtube className="w-5 h-5" />
                            </Button>
                        </div>
                    </div>

                    {/* Quick Links */}
                    <div>
                        <h4 className="font-semibold mb-4">Quick Links</h4>
                        <ul className="space-y-2">
                            {quickLinks.map((link) => (
                                <li key={link.label}>
                                    <Link
                                        href={link.href}
                                        className="text-slate-400 hover:text-purple-400 text-sm transition-colors"
                                    >
                                        {link.label}
                                    </Link>
                                </li>
                            ))}
                        </ul>
                    </div>

                    {/* Company */}
                    <div>
                        <h4 className="font-semibold mb-4">Company</h4>
                        <ul className="space-y-2">
                            {companyLinks.map((link) => (
                                <li key={link.label}>
                                    <Link
                                        href={link.href}
                                        className="text-slate-400 hover:text-purple-400 text-sm transition-colors"
                                    >
                                        {link.label}
                                    </Link>
                                </li>
                            ))}
                        </ul>
                    </div>

                    {/* Support */}
                    <div>
                        <h4 className="font-semibold mb-4">Support</h4>
                        <ul className="space-y-2">
                            {supportLinks.map((link) => (
                                <li key={link.label}>
                                    <Link
                                        href={link.href}
                                        className="text-slate-400 hover:text-purple-400 text-sm transition-colors"
                                    >
                                        {link.label}
                                    </Link>
                                </li>
                            ))}
                        </ul>
                    </div>

                    {/* Contact */}
                    <div>
                        <h4 className="font-semibold mb-4">Contact</h4>
                        <ul className="space-y-3">
                            <li className="flex items-center gap-2 text-slate-400 text-sm">
                                <Mail className="w-4 h-4" />
                                support@auctionhub.com
                            </li>
                            <li className="flex items-center gap-2 text-slate-400 text-sm">
                                <Phone className="w-4 h-4" />
                                1-800-AUCTION
                            </li>
                            <li className="flex items-start gap-2 text-slate-400 text-sm">
                                <MapPin className="w-4 h-4 mt-0.5" />
                                123 Auction Lane
                                <br />
                                New York, NY 10001
                            </li>
                        </ul>
                    </div>
                </div>
            </div>

            {/* Bottom Bar */}
            <div className="border-t border-slate-800">
                <div className="container mx-auto px-4 py-6">
                    <div className="flex flex-col md:flex-row items-center justify-between gap-4">
                        <div className="flex items-center gap-2 text-slate-400 text-sm">
                            <Lock className="w-4 h-4" />
                            <span>256-bit SSL Encryption</span>
                            <span className="mx-2">•</span>
                            <span>PCI DSS Compliant</span>
                        </div>

                        <div className="flex flex-wrap items-center justify-center gap-4">
                            {legalLinks.map((link) => (
                                <Link
                                    key={link.label}
                                    href={link.href}
                                    className="text-slate-400 hover:text-purple-400 text-sm transition-colors"
                                >
                                    {link.label}
                                </Link>
                            ))}
                        </div>

                        <p className="text-slate-500 text-sm">
                            © {new Date().getFullYear()} AuctionHub. All rights reserved.
                        </p>
                    </div>
                </div>
            </div>
        </footer>
    );
}
