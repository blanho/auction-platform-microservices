"use client";

import { motion } from "framer-motion";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
    faEnvelope,
    faPhone,
    faLocationDot,
    faLock,
    faArrowRight,
} from "@fortawesome/free-solid-svg-icons";
import {
    faFacebook,
    faTwitter,
    faInstagram,
    faYoutube,
} from "@fortawesome/free-brands-svg-icons";
import Link from "next/link";

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
                                <FontAwesomeIcon icon={faFacebook} className="w-5 h-5" />
                            </Button>
                            <Button variant="ghost" size="icon" className="hover:text-purple-400">
                                <FontAwesomeIcon icon={faTwitter} className="w-5 h-5" />
                            </Button>
                            <Button variant="ghost" size="icon" className="hover:text-purple-400">
                                <FontAwesomeIcon icon={faInstagram} className="w-5 h-5" />
                            </Button>
                            <Button variant="ghost" size="icon" className="hover:text-purple-400">
                                <FontAwesomeIcon icon={faYoutube} className="w-5 h-5" />
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
                                <FontAwesomeIcon icon={faEnvelope} className="w-4 h-4" />
                                support@auctionhub.com
                            </li>
                            <li className="flex items-center gap-2 text-slate-400 text-sm">
                                <FontAwesomeIcon icon={faPhone} className="w-4 h-4" />
                                1-800-AUCTION
                            </li>
                            <li className="flex items-start gap-2 text-slate-400 text-sm">
                                <FontAwesomeIcon icon={faLocationDot} className="w-4 h-4 mt-0.5" />
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
                            <FontAwesomeIcon icon={faLock} className="w-4 h-4" />
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
