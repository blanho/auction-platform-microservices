// Footer component
'use client';

import Link from 'next/link';
import { Facebook, Twitter, Instagram, Youtube, Apple, Smartphone } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Separator } from '@/components/ui/separator';

const footerLinks = {
    categories: [
        { name: 'Electronics', href: '/search?category=electronics' },
        { name: 'Sneakers', href: '/search?category=sneakers' },
        { name: 'Luxury', href: '/search?category=luxury' },
        { name: 'Collectibles', href: '/search?category=collectibles' },
        { name: 'Watches', href: '/search?category=watches' },
        { name: 'Art', href: '/search?category=art' },
    ],
    company: [
        { name: 'About Us', href: '/about' },
        { name: 'Careers', href: '/careers' },
        { name: 'Press', href: '/press' },
        { name: 'Blog', href: '/blog' },
        { name: 'Affiliate Program', href: '/affiliate' },
    ],
    support: [
        { name: 'Help Center', href: '/help' },
        { name: 'Contact Us', href: '/contact' },
        { name: 'Buyer Protection', href: '/buyer-protection' },
        { name: 'Seller Guide', href: '/seller-guide' },
        { name: 'FAQs', href: '/faq' },
    ],
    legal: [
        { name: 'Terms of Service', href: '/terms' },
        { name: 'Privacy Policy', href: '/privacy' },
        { name: 'Cookie Policy', href: '/cookies' },
        { name: 'Accessibility', href: '/accessibility' },
    ],
};

const socialLinks = [
    { name: 'Facebook', icon: Facebook, href: 'https://facebook.com' },
    { name: 'Twitter', icon: Twitter, href: 'https://twitter.com' },
    { name: 'Instagram', icon: Instagram, href: 'https://instagram.com' },
    { name: 'YouTube', icon: Youtube, href: 'https://youtube.com' },
];

export function Footer() {
    return (
        <footer className="border-t bg-muted/30">
            <div className="container mx-auto px-4 md:px-6 lg:px-8">
                {/* Main Footer Content */}
                <div className="py-12 grid grid-cols-2 md:grid-cols-4 lg:grid-cols-6 gap-8">
                    {/* Brand & Description */}
                    <div className="col-span-2">
                        <Link href="/" className="flex items-center space-x-2 mb-4">
                            <span className="text-2xl font-bold">AuctionHub</span>
                        </Link>
                        <p className="text-sm text-muted-foreground mb-6 max-w-xs">
                            The marketplace where value finds you. Bid, win, and sell with confidence.
                        </p>

                        {/* App Download Buttons */}
                        <div className="flex flex-col sm:flex-row gap-2">
                            <Button variant="outline" size="sm" className="justify-start" asChild>
                                <a href="#" className="flex items-center gap-2">
                                    <Apple className="h-4 w-4" />
                                    <div className="text-left">
                                        <div className="text-[10px] opacity-70">Download on the</div>
                                        <div className="text-xs font-semibold">App Store</div>
                                    </div>
                                </a>
                            </Button>
                            <Button variant="outline" size="sm" className="justify-start" asChild>
                                <a href="#" className="flex items-center gap-2">
                                    <Smartphone className="h-4 w-4" />
                                    <div className="text-left">
                                        <div className="text-[10px] opacity-70">Get it on</div>
                                        <div className="text-xs font-semibold">Google Play</div>
                                    </div>
                                </a>
                            </Button>
                        </div>
                    </div>

                    {/* Categories */}
                    <div>
                        <h3 className="font-semibold mb-4">Categories</h3>
                        <ul className="space-y-2">
                            {footerLinks.categories.map((link) => (
                                <li key={link.name}>
                                    <Link
                                        href={link.href}
                                        className="text-sm text-muted-foreground hover:text-primary transition-colors"
                                    >
                                        {link.name}
                                    </Link>
                                </li>
                            ))}
                        </ul>
                    </div>

                    {/* Company */}
                    <div>
                        <h3 className="font-semibold mb-4">Company</h3>
                        <ul className="space-y-2">
                            {footerLinks.company.map((link) => (
                                <li key={link.name}>
                                    <Link
                                        href={link.href}
                                        className="text-sm text-muted-foreground hover:text-primary transition-colors"
                                    >
                                        {link.name}
                                    </Link>
                                </li>
                            ))}
                        </ul>
                    </div>

                    {/* Support */}
                    <div>
                        <h3 className="font-semibold mb-4">Support</h3>
                        <ul className="space-y-2">
                            {footerLinks.support.map((link) => (
                                <li key={link.name}>
                                    <Link
                                        href={link.href}
                                        className="text-sm text-muted-foreground hover:text-primary transition-colors"
                                    >
                                        {link.name}
                                    </Link>
                                </li>
                            ))}
                        </ul>
                    </div>

                    {/* Legal */}
                    <div>
                        <h3 className="font-semibold mb-4">Legal</h3>
                        <ul className="space-y-2">
                            {footerLinks.legal.map((link) => (
                                <li key={link.name}>
                                    <Link
                                        href={link.href}
                                        className="text-sm text-muted-foreground hover:text-primary transition-colors"
                                    >
                                        {link.name}
                                    </Link>
                                </li>
                            ))}
                        </ul>
                    </div>
                </div>

                <Separator />

                {/* Bottom Bar */}
                <div className="py-6 flex flex-col md:flex-row items-center justify-between gap-4">
                    <p className="text-sm text-muted-foreground text-center md:text-left">
                        © {new Date().getFullYear()} AuctionHub. All rights reserved.
                    </p>

                    {/* Social Links */}
                    <div className="flex items-center gap-4">
                        {socialLinks.map((social) => (
                            <a
                                key={social.name}
                                href={social.href}
                                target="_blank"
                                rel="noopener noreferrer"
                                className="p-2 rounded-full bg-muted hover:bg-muted/80 text-muted-foreground hover:text-primary transition-colors"
                                aria-label={social.name}
                            >
                                <social.icon className="h-4 w-4" />
                            </a>
                        ))}
                    </div>
                </div>
            </div>
        </footer>
    );
}
