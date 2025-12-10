"use client";

import { motion } from "framer-motion";
import { Search, Gavel, Trophy } from "lucide-react";

const steps = [
    {
        icon: Search,
        title: "Browse Items",
        description: "Explore thousands of unique items from verified sellers worldwide.",
        color: "from-blue-500 to-cyan-500",
        step: "01",
    },
    {
        icon: Gavel,
        title: "Place Your Bid",
        description: "Set your maximum bid and watch the real-time auction unfold.",
        color: "from-purple-500 to-pink-500",
        step: "02",
    },
    {
        icon: Trophy,
        title: "Win & Checkout",
        description: "Secure your item with our protected payment system.",
        color: "from-orange-500 to-red-500",
        step: "03",
    },
];

const containerVariants = {
    hidden: { opacity: 0 },
    visible: {
        opacity: 1,
        transition: {
            staggerChildren: 0.2,
        },
    },
};

const itemVariants = {
    hidden: { opacity: 0, y: 30 },
    visible: {
        opacity: 1,
        y: 0,
        transition: {
            duration: 0.6,
        },
    },
};

export function HowItWorksSection() {
    return (
        <section id="how-it-works" className="py-20 bg-white dark:bg-slate-950">
            <div className="container mx-auto px-4">
                {/* Header */}
                <div className="text-center mb-16">
                    <motion.div
                        initial={{ opacity: 0, y: 20 }}
                        whileInView={{ opacity: 1, y: 0 }}
                        viewport={{ once: true }}
                        transition={{ duration: 0.6 }}
                    >
                        <h2 className="text-3xl md:text-4xl font-bold text-slate-900 dark:text-white mb-4">
                            How It Works
                        </h2>
                        <p className="text-slate-600 dark:text-slate-400 max-w-2xl mx-auto">
                            Start bidding in minutes with our simple 3-step process. No complicated setup required.
                        </p>
                    </motion.div>
                </div>

                {/* Steps */}
                <motion.div
                    variants={containerVariants}
                    initial="hidden"
                    whileInView="visible"
                    viewport={{ once: true, margin: "-100px" }}
                    className="grid md:grid-cols-3 gap-8 lg:gap-12"
                >
                    {steps.map((step, index) => (
                        <motion.div
                            key={index}
                            variants={itemVariants}
                            className="relative"
                        >
                            {/* Connector line */}
                            {index < steps.length - 1 && (
                                <div className="hidden md:block absolute top-20 left-[60%] w-[80%] h-0.5 bg-gradient-to-r from-slate-300 to-transparent dark:from-slate-700" />
                            )}

                            <div className="flex flex-col items-center text-center">
                                {/* Step number */}
                                <div className="text-6xl font-bold text-slate-100 dark:text-slate-800 mb-4">
                                    {step.step}
                                </div>

                                {/* Icon */}
                                <motion.div
                                    whileHover={{ scale: 1.1, rotate: 5 }}
                                    className={`w-20 h-20 rounded-2xl bg-gradient-to-br ${step.color} flex items-center justify-center mb-6 shadow-lg`}
                                >
                                    <step.icon className="w-10 h-10 text-white" />
                                </motion.div>

                                {/* Content */}
                                <h3 className="text-xl font-bold text-slate-900 dark:text-white mb-3">
                                    {step.title}
                                </h3>
                                <p className="text-slate-600 dark:text-slate-400 max-w-xs">
                                    {step.description}
                                </p>
                            </div>
                        </motion.div>
                    ))}
                </motion.div>

                {/* Decorative elements */}
                <div className="flex justify-center mt-16">
                    <motion.div
                        initial={{ opacity: 0, scale: 0.8 }}
                        whileInView={{ opacity: 1, scale: 1 }}
                        viewport={{ once: true }}
                        className="px-8 py-4 rounded-full bg-gradient-to-r from-purple-100 to-blue-100 dark:from-purple-900/30 dark:to-blue-900/30 border border-purple-200 dark:border-purple-800"
                    >
                        <span className="text-slate-700 dark:text-slate-300">
                            🎉 Join <span className="font-bold text-purple-600 dark:text-purple-400">50,000+</span> happy bidders today!
                        </span>
                    </motion.div>
                </div>
            </div>
        </section>
    );
}
