"use client";

import Link from "next/link";
import { MainLayout } from "@/components/layout/main-layout";
import { ROUTES } from "@/constants/routes";
import { FAQS, CONTACT_OPTIONS, HELP_CATEGORIES } from "@/constants/platform";
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

const ICON_MAP = {
    MessageCircle,
    Mail,
    Phone,
    FileQuestion,
    Gavel,
    CreditCard,
    Shield,
    Package,
    Clock,
} as const;

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
                    {HELP_CATEGORIES.map((category) => {
                        const Icon = ICON_MAP[category.iconName];
                        return (
                            <Card
                                key={category.title}
                                className="hover:border-purple-300 dark:hover:border-purple-700 transition-colors cursor-pointer"
                            >
                                <CardContent className="p-4 text-center">
                                    <div className="inline-flex p-2 rounded-lg bg-purple-100 dark:bg-purple-900/30 mb-2">
                                        <Icon className="w-5 h-5 text-purple-600" />
                                    </div>
                                    <h3 className="font-semibold text-sm">{category.title}</h3>
                                    <p className="text-xs text-muted-foreground mt-1">
                                        {category.description}
                                    </p>
                                </CardContent>
                            </Card>
                        );
                    })}
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
                                    {FAQS.map((faq, index) => (
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
                                {CONTACT_OPTIONS.map((option) => {
                                    const Icon = ICON_MAP[option.iconName];
                                    return (
                                        <div
                                            key={option.title}
                                            className="flex items-start gap-3 p-3 rounded-lg bg-zinc-50 dark:bg-zinc-900"
                                        >
                                            <div className="p-2 rounded-lg bg-purple-100 dark:bg-purple-900/30">
                                                <Icon className="w-4 h-4 text-purple-600" />
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
                                    );
                                })}
                            </CardContent>
                        </Card>
                    </div>
                </div>
            </div>
        </MainLayout>
    );
}
