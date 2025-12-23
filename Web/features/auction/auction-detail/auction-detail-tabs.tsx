"use client";

import { useState } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
    faFileAlt,
    faGavel,
    faTruck,
    faStar,
    faChevronRight,
} from "@fortawesome/free-solid-svg-icons";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";
import { IconDefinition } from "@fortawesome/fontawesome-svg-core";

type TabId = "description" | "bids" | "shipping" | "reviews";

interface Tab {
    id: TabId;
    label: string;
    icon: IconDefinition;
    badge?: number | string;
}

interface AuctionDetailTabsProps {
    descriptionContent: React.ReactNode;
    bidsContent: React.ReactNode;
    shippingContent: React.ReactNode;
    reviewsContent: React.ReactNode;
    bidCount?: number;
    reviewCount?: number;
    defaultTab?: TabId;
    className?: string;
}

export function AuctionDetailTabs({
    descriptionContent,
    bidsContent,
    shippingContent,
    reviewsContent,
    bidCount,
    reviewCount,
    defaultTab = "description",
    className,
}: AuctionDetailTabsProps) {
    const [activeTab, setActiveTab] = useState<TabId>(defaultTab);

    const tabs: Tab[] = [
        { id: "description", label: "Description", icon: faFileAlt },
        { id: "bids", label: "Bids", icon: faGavel, badge: bidCount },
        { id: "shipping", label: "Shipping", icon: faTruck },
        { id: "reviews", label: "Reviews", icon: faStar, badge: reviewCount },
    ];

    return (
        <div className={cn("w-full", className)}>
            <Tabs
                value={activeTab}
                onValueChange={(value) => setActiveTab(value as TabId)}
                className="w-full"
            >
                <div className="sticky top-16 z-30 bg-white dark:bg-slate-950 border-b border-slate-200 dark:border-slate-800">
                    <TabsList className="w-full h-auto p-0 bg-transparent rounded-none flex">
                        {tabs.map((tab) => (
                            <TabsTrigger
                                key={tab.id}
                                value={tab.id}
                                className={cn(
                                    "flex-1 relative py-4 px-2 rounded-none border-b-2 border-transparent",
                                    "data-[state=active]:border-purple-600 data-[state=active]:bg-transparent",
                                    "transition-all duration-200"
                                )}
                            >
                                <div className="flex items-center justify-center gap-2">
                                    <FontAwesomeIcon
                                        icon={tab.icon}
                                        className={cn(
                                            "w-4 h-4 transition-colors",
                                            activeTab === tab.id
                                                ? "text-purple-600"
                                                : "text-slate-400"
                                        )}
                                    />
                                    <span
                                        className={cn(
                                            "font-medium text-sm md:text-base transition-colors",
                                            activeTab === tab.id
                                                ? "text-purple-600"
                                                : "text-slate-600 dark:text-slate-400"
                                        )}
                                    >
                                        <span className="hidden sm:inline">{tab.label}</span>
                                        <span className="sm:hidden">{tab.label.slice(0, 4)}</span>
                                    </span>
                                    {tab.badge !== undefined && (typeof tab.badge === 'number' ? tab.badge > 0 : true) && (
                                        <Badge
                                            variant="secondary"
                                            className={cn(
                                                "h-5 min-w-5 px-1.5 text-xs",
                                                activeTab === tab.id
                                                    ? "bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-300"
                                                    : "bg-slate-100 text-slate-600 dark:bg-slate-800 dark:text-slate-400"
                                            )}
                                        >
                                            {tab.badge}
                                        </Badge>
                                    )}
                                </div>

                                {activeTab === tab.id && (
                                    <motion.div
                                        layoutId="activeTabIndicator"
                                        className="absolute bottom-0 left-0 right-0 h-0.5 bg-purple-600"
                                        transition={{ type: "spring", stiffness: 500, damping: 30 }}
                                    />
                                )}
                            </TabsTrigger>
                        ))}
                    </TabsList>
                </div>

                <AnimatePresence mode="wait">
                    <motion.div
                        key={activeTab}
                        initial={{ opacity: 0, y: 10 }}
                        animate={{ opacity: 1, y: 0 }}
                        exit={{ opacity: 0, y: -10 }}
                        transition={{ duration: 0.2 }}
                    >
                        <TabsContent value="description" className="mt-0 pt-6">
                            {descriptionContent}
                        </TabsContent>

                        <TabsContent value="bids" className="mt-0 pt-6">
                            {bidsContent}
                        </TabsContent>

                        <TabsContent value="shipping" className="mt-0 pt-6">
                            {shippingContent}
                        </TabsContent>

                        <TabsContent value="reviews" className="mt-0 pt-6">
                            {reviewsContent}
                        </TabsContent>
                    </motion.div>
                </AnimatePresence>
            </Tabs>
        </div>
    );
}

interface MobileTabNavigationProps {
    tabs: Tab[];
    activeTab: TabId;
    onTabChange: (tab: TabId) => void;
    className?: string;
}

export function MobileTabNavigation({
    tabs,
    activeTab,
    onTabChange,
    className,
}: MobileTabNavigationProps) {
    return (
        <div className={cn("md:hidden", className)}>
            <div className="divide-y divide-slate-200 dark:divide-slate-800 border rounded-xl border-slate-200 dark:border-slate-800 overflow-hidden">
                {tabs.map((tab) => (
                    <button
                        key={tab.id}
                        onClick={() => onTabChange(tab.id)}
                        className={cn(
                            "w-full flex items-center justify-between p-4 transition-colors",
                            activeTab === tab.id
                                ? "bg-purple-50 dark:bg-purple-900/20"
                                : "bg-white dark:bg-slate-900 hover:bg-slate-50 dark:hover:bg-slate-800"
                        )}
                    >
                        <div className="flex items-center gap-3">
                            <div
                                className={cn(
                                    "w-10 h-10 rounded-full flex items-center justify-center",
                                    activeTab === tab.id
                                        ? "bg-purple-100 dark:bg-purple-900/30"
                                        : "bg-slate-100 dark:bg-slate-800"
                                )}
                            >
                                <FontAwesomeIcon
                                    icon={tab.icon}
                                    className={cn(
                                        "w-4 h-4",
                                        activeTab === tab.id
                                            ? "text-purple-600"
                                            : "text-slate-500"
                                    )}
                                />
                            </div>
                            <div className="text-left">
                                <div
                                    className={cn(
                                        "font-medium",
                                        activeTab === tab.id
                                            ? "text-purple-600"
                                            : "text-slate-900 dark:text-white"
                                    )}
                                >
                                    {tab.label}
                                </div>
                                {tab.badge !== undefined && (
                                    <div className="text-sm text-slate-500 dark:text-slate-400">
                                        {tab.badge} {tab.id === "bids" ? "bids" : "reviews"}
                                    </div>
                                )}
                            </div>
                        </div>
                        <FontAwesomeIcon
                            icon={faChevronRight}
                            className="w-4 h-4 text-slate-400"
                        />
                    </button>
                ))}
            </div>
        </div>
    );
}

interface ShippingInfoProps {
    shippingOptions?: {
        method: string;
        price: number;
        estimatedDays: string;
        carrier?: string;
    }[];
    sellerLocation?: string;
    shipsTo?: string[];
    returnPolicy?: string;
    currency?: string;
}

export function ShippingInfo({
    shippingOptions = [],
    sellerLocation,
    shipsTo = [],
    returnPolicy,
    currency = "USD",
}: ShippingInfoProps) {
    const formatPrice = (price: number) => {
        if (price === 0) return "Free";
        return new Intl.NumberFormat("en-US", {
            style: "currency",
            currency,
        }).format(price);
    };

    return (
        <div className="space-y-6">
            {shippingOptions.length > 0 && (
                <div>
                    <h4 className="font-semibold text-slate-900 dark:text-white mb-3">
                        Shipping Options
                    </h4>
                    <div className="space-y-3">
                        {shippingOptions.map((option, index) => (
                            <div
                                key={index}
                                className="flex items-center justify-between p-4 rounded-lg bg-slate-50 dark:bg-slate-800/50 border border-slate-200 dark:border-slate-700"
                            >
                                <div>
                                    <div className="font-medium text-slate-900 dark:text-white">
                                        {option.method}
                                    </div>
                                    <div className="text-sm text-slate-500 dark:text-slate-400">
                                        {option.estimatedDays}
                                        {option.carrier && ` via ${option.carrier}`}
                                    </div>
                                </div>
                                <div
                                    className={cn(
                                        "font-semibold",
                                        option.price === 0
                                            ? "text-green-600"
                                            : "text-slate-900 dark:text-white"
                                    )}
                                >
                                    {formatPrice(option.price)}
                                </div>
                            </div>
                        ))}
                    </div>
                </div>
            )}

            {sellerLocation && (
                <div>
                    <h4 className="font-semibold text-slate-900 dark:text-white mb-2">
                        Item Location
                    </h4>
                    <p className="text-slate-600 dark:text-slate-400">{sellerLocation}</p>
                </div>
            )}

            {shipsTo.length > 0 && (
                <div>
                    <h4 className="font-semibold text-slate-900 dark:text-white mb-2">
                        Ships To
                    </h4>
                    <div className="flex flex-wrap gap-2">
                        {shipsTo.map((location) => (
                            <Badge key={location} variant="secondary">
                                {location}
                            </Badge>
                        ))}
                    </div>
                </div>
            )}

            {returnPolicy && (
                <div>
                    <h4 className="font-semibold text-slate-900 dark:text-white mb-2">
                        Return Policy
                    </h4>
                    <p className="text-slate-600 dark:text-slate-400">{returnPolicy}</p>
                </div>
            )}

            {shippingOptions.length === 0 && !sellerLocation && !returnPolicy && (
                <div className="text-center py-8 text-slate-500 dark:text-slate-400">
                    <FontAwesomeIcon icon={faTruck} className="w-8 h-8 mb-3 opacity-30" />
                    <p>Shipping information not available</p>
                </div>
            )}
        </div>
    );
}
