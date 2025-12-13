"use client";

import Link from "next/link";
import { MainLayout } from "@/components/layout/main-layout";
import { ROUTES } from "@/constants/routes";
import {
    Breadcrumb,
    BreadcrumbItem,
    BreadcrumbLink,
    BreadcrumbList,
    BreadcrumbPage,
    BreadcrumbSeparator,
} from "@/components/ui/breadcrumb";
import {
    Accordion,
    AccordionContent,
    AccordionItem,
    AccordionTrigger,
} from "@/components/ui/accordion";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
    HelpCircle,
    MessageCircle,
    Mail,
    Phone,
    FileQuestion,
    Gavel,
    CreditCard,
    Shield,
    Package,
    Clock,
} from "lucide-react";

const faqs = [
    {
        question: "How do I place a bid?",
        answer:
            "To place a bid, navigate to any live auction, enter your bid amount (must be higher than the current bid), and click 'Place Bid'. You must be logged in to bid.",
    },
    {
        question: "What happens when I win an auction?",
        answer:
            "When you win an auction, you'll receive a notification and email. Payment must be completed within 3 days. Once payment is confirmed, the seller will ship your item.",
    },
    {
        question: "How do I sell an item?",
        answer:
            "Click 'Sell' or 'Create Auction' from the navigation. Fill out the item details, set your starting price and auction duration, then submit. Your listing will be reviewed before going live.",
    },
    {
        question: "What payment methods are accepted?",
        answer:
            "We accept major credit cards, debit cards, and bank transfers. All payments are processed securely through our payment partner.",
    },
    {
        question: "How does buyer protection work?",
        answer:
            "All purchases are covered by our Buyer Protection program. If an item doesn't arrive or isn't as described, you can file a claim within 30 days for a full refund.",
    },
    {
        question: "Can I cancel a bid?",
        answer:
            "Bids are binding contracts. You can only retract a bid if you made an obvious error (like bidding $1,000 instead of $100). Contact support immediately if this happens.",
    },
    {
        question: "How do withdrawals work?",
        answer:
            "Sellers can request withdrawals from their wallet balance. Processing typically takes 3-5 business days depending on your payment method.",
    },
    {
        question: "What are the seller fees?",
        answer:
            "We charge a 5% commission on successful sales. There are no listing fees. Additional features like featured listings may have extra costs.",
    },
];

const contactOptions = [
    {
        icon: MessageCircle,
        title: "Live Chat",
        description: "Get instant help from our support team",
        action: "Start Chat",
        available: "24/7",
    },
    {
        icon: Mail,
        title: "Email Support",
        description: "support@auctionhub.com",
        action: "Send Email",
        available: "1-2 business days",
    },
    {
        icon: Phone,
        title: "Phone Support",
        description: "1-800-AUCTION",
        action: "Call Now",
        available: "Mon-Fri 9AM-6PM",
    },
];

const helpCategories = [
    {
        icon: Gavel,
        title: "Bidding & Buying",
        description: "How to bid, win, and pay for items",
    },
    {
        icon: Package,
        title: "Selling",
        description: "Creating listings and managing sales",
    },
    {
        icon: CreditCard,
        title: "Payments",
        description: "Payment methods, fees, and withdrawals",
    },
    {
        icon: Shield,
        title: "Safety & Security",
        description: "Account protection and buyer guarantees",
    },
    {
        icon: Clock,
        title: "Shipping",
        description: "Delivery times and tracking",
    },
    {
        icon: FileQuestion,
        title: "Account Issues",
        description: "Profile, settings, and verification",
    },
];

export default function HelpPage() {
    return (
        <MainLayout>
            <div className="space-y-8">
                <Breadcrumb>
                    <BreadcrumbList>
                        <BreadcrumbItem>
                            <BreadcrumbLink asChild>
                                <Link href={ROUTES.HOME}>Home</Link>
                            </BreadcrumbLink>
                        </BreadcrumbItem>
                        <BreadcrumbSeparator />
                        <BreadcrumbItem>
                            <BreadcrumbPage>Help Center</BreadcrumbPage>
                        </BreadcrumbItem>
                    </BreadcrumbList>
                </Breadcrumb>

                <div className="text-center space-y-4">
                    <div className="inline-flex p-4 rounded-full bg-purple-100 dark:bg-purple-900/30">
                        <HelpCircle className="w-8 h-8 text-purple-600" />
                    </div>
                    <h1 className="text-4xl font-bold">How can we help you?</h1>
                    <p className="text-muted-foreground text-lg max-w-2xl mx-auto">
                        Find answers to common questions or contact our support team
                    </p>
                </div>

                <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-4">
                    {helpCategories.map((category) => (
                        <Card
                            key={category.title}
                            className="hover:border-purple-300 dark:hover:border-purple-700 transition-colors cursor-pointer"
                        >
                            <CardContent className="p-4 text-center">
                                <div className="inline-flex p-2 rounded-lg bg-purple-100 dark:bg-purple-900/30 mb-2">
                                    <category.icon className="w-5 h-5 text-purple-600" />
                                </div>
                                <h3 className="font-semibold text-sm">{category.title}</h3>
                                <p className="text-xs text-muted-foreground mt-1">
                                    {category.description}
                                </p>
                            </CardContent>
                        </Card>
                    ))}
                </div>

                <div className="grid lg:grid-cols-3 gap-8">
                    <div className="lg:col-span-2">
                        <Card>
                            <CardHeader>
                                <CardTitle>Frequently Asked Questions</CardTitle>
                                <CardDescription>
                                    Quick answers to common questions
                                </CardDescription>
                            </CardHeader>
                            <CardContent>
                                <Accordion type="single" collapsible className="w-full">
                                    {faqs.map((faq, index) => (
                                        <AccordionItem key={index} value={`item-${index}`}>
                                            <AccordionTrigger className="text-left">
                                                {faq.question}
                                            </AccordionTrigger>
                                            <AccordionContent className="text-muted-foreground">
                                                {faq.answer}
                                            </AccordionContent>
                                        </AccordionItem>
                                    ))}
                                </Accordion>
                            </CardContent>
                        </Card>
                    </div>

                    <div className="space-y-4">
                        <Card>
                            <CardHeader>
                                <CardTitle>Contact Support</CardTitle>
                                <CardDescription>
                                    Can&apos;t find what you&apos;re looking for?
                                </CardDescription>
                            </CardHeader>
                            <CardContent className="space-y-4">
                                {contactOptions.map((option) => (
                                    <div
                                        key={option.title}
                                        className="flex items-start gap-3 p-3 rounded-lg bg-zinc-50 dark:bg-zinc-900"
                                    >
                                        <div className="p-2 rounded-lg bg-purple-100 dark:bg-purple-900/30">
                                            <option.icon className="w-4 h-4 text-purple-600" />
                                        </div>
                                        <div className="flex-1">
                                            <div className="flex items-center gap-2">
                                                <p className="font-medium text-sm">{option.title}</p>
                                                <Badge variant="secondary" className="text-xs">
                                                    {option.available}
                                                </Badge>
                                            </div>
                                            <p className="text-xs text-muted-foreground">
                                                {option.description}
                                            </p>
                                        </div>
                                        <Button variant="outline" size="sm">
                                            {option.action}
                                        </Button>
                                    </div>
                                ))}
                            </CardContent>
                        </Card>
                    </div>
                </div>
            </div>
        </MainLayout>
    );
}
