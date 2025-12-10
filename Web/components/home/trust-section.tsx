"use client";

import { motion } from "framer-motion";
import { Shield, CreditCard, RefreshCcw, Star } from "lucide-react";

const trustItems = [
    {
        icon: Shield,
        title: "Verified Sellers",
        description: "All sellers are vetted and verified",
        color: "text-blue-500",
        bgColor: "bg-blue-50 dark:bg-blue-950",
    },
    {
        icon: CreditCard,
        title: "Secure Payments",
        description: "256-bit SSL encrypted transactions",
        color: "text-green-500",
        bgColor: "bg-green-50 dark:bg-green-950",
    },
    {
        icon: RefreshCcw,
        title: "Money-Back Guarantee",
        description: "Full refund within 7 days",
        color: "text-purple-500",
        bgColor: "bg-purple-50 dark:bg-purple-950",
    },
    {
        icon: Star,
        title: "Top Rated",
        description: "4.9★ from 10,000+ reviews",
        color: "text-orange-500",
        bgColor: "bg-orange-50 dark:bg-orange-950",
    },
];

const containerVariants = {
    hidden: { opacity: 0 },
    visible: {
        opacity: 1,
        transition: {
            staggerChildren: 0.1,
        },
    },
};

const itemVariants = {
    hidden: { opacity: 0, y: 20 },
    visible: {
        opacity: 1,
        y: 0,
        transition: {
            duration: 0.5,
        },
    },
};

export function TrustSection() {
    return (
        <section className="py-16 bg-white dark:bg-slate-900 border-y border-slate-200 dark:border-slate-800">
            <div className="container mx-auto px-4">
                <motion.div
                    variants={containerVariants}
                    initial="hidden"
                    whileInView="visible"
                    viewport={{ once: true, margin: "-100px" }}
                    className="grid grid-cols-2 md:grid-cols-4 gap-6 lg:gap-8"
                >
                    {trustItems.map((item, index) => (
                        <motion.div
                            key={index}
                            variants={itemVariants}
                            className="flex flex-col items-center text-center group"
                        >
                            <div
                                className={`${item.bgColor} p-4 rounded-2xl mb-4 group-hover:scale-110 transition-transform duration-300`}
                            >
                                <item.icon className={`w-8 h-8 ${item.color}`} />
                            </div>
                            <h3 className="font-semibold text-slate-900 dark:text-white mb-1">
                                {item.title}
                            </h3>
                            <p className="text-sm text-slate-600 dark:text-slate-400">
                                {item.description}
                            </p>
                        </motion.div>
                    ))}
                </motion.div>
            </div>
        </section>
    );
}
