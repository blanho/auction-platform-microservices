"use client";

import { motion, Variants, HTMLMotionProps } from "framer-motion";
import { ReactNode } from "react";

export const fadeInUp: Variants = {
    hidden: { opacity: 0, y: 20 },
    visible: { 
        opacity: 1, 
        y: 0,
        transition: { duration: 0.5, ease: "easeOut" }
    },
};

export const fadeInDown: Variants = {
    hidden: { opacity: 0, y: -20 },
    visible: { 
        opacity: 1, 
        y: 0,
        transition: { duration: 0.5, ease: "easeOut" }
    },
};

export const fadeInLeft: Variants = {
    hidden: { opacity: 0, x: -20 },
    visible: { 
        opacity: 1, 
        x: 0,
        transition: { duration: 0.5, ease: "easeOut" }
    },
};

export const fadeInRight: Variants = {
    hidden: { opacity: 0, x: 20 },
    visible: { 
        opacity: 1, 
        x: 0,
        transition: { duration: 0.5, ease: "easeOut" }
    },
};

export const scaleIn: Variants = {
    hidden: { opacity: 0, scale: 0.95 },
    visible: { 
        opacity: 1, 
        scale: 1,
        transition: { duration: 0.4, ease: "easeOut" }
    },
};

export const staggerContainer: Variants = {
    hidden: { opacity: 0 },
    visible: {
        opacity: 1,
        transition: {
            staggerChildren: 0.1,
            delayChildren: 0.1,
        },
    },
};

export const staggerItem: Variants = {
    hidden: { opacity: 0, y: 20 },
    visible: { 
        opacity: 1, 
        y: 0,
        transition: { duration: 0.4, ease: "easeOut" }
    },
};

interface AnimatedSectionProps extends Omit<HTMLMotionProps<"section">, "children"> {
    children: ReactNode;
    className?: string;
    delay?: number;
}

export function AnimatedSection({ 
    children, 
    className = "", 
    delay = 0,
    ...props 
}: AnimatedSectionProps) {
    return (
        <motion.section
            initial="hidden"
            whileInView="visible"
            viewport={{ once: true, margin: "-100px" }}
            variants={{
                hidden: { opacity: 0 },
                visible: { 
                    opacity: 1,
                    transition: { delay, duration: 0.5 }
                },
            }}
            className={className}
            {...props}
        >
            {children}
        </motion.section>
    );
}

interface AnimatedDivProps extends Omit<HTMLMotionProps<"div">, "children"> {
    children: ReactNode;
    className?: string;
    variant?: "fadeInUp" | "fadeInDown" | "fadeInLeft" | "fadeInRight" | "scaleIn";
    delay?: number;
}

const variantMap = {
    fadeInUp,
    fadeInDown,
    fadeInLeft,
    fadeInRight,
    scaleIn,
};

export function AnimatedDiv({ 
    children, 
    className = "", 
    variant = "fadeInUp",
    delay = 0,
    ...props 
}: AnimatedDivProps) {
    const selectedVariant = variantMap[variant];
    
    return (
        <motion.div
            initial="hidden"
            whileInView="visible"
            viewport={{ once: true, margin: "-50px" }}
            variants={{
                hidden: selectedVariant.hidden,
                visible: {
                    ...selectedVariant.visible,
                    transition: {
                        ...(typeof selectedVariant.visible === "object" && "transition" in selectedVariant.visible 
                            ? selectedVariant.visible.transition 
                            : {}),
                        delay,
                    },
                },
            }}
            className={className}
            {...props}
        >
            {children}
        </motion.div>
    );
}

interface StaggerContainerProps extends Omit<HTMLMotionProps<"div">, "children"> {
    children: ReactNode;
    className?: string;
    staggerDelay?: number;
}

export function StaggerContainer({ 
    children, 
    className = "",
    staggerDelay = 0.1,
    ...props 
}: StaggerContainerProps) {
    return (
        <motion.div
            initial="hidden"
            whileInView="visible"
            viewport={{ once: true, margin: "-50px" }}
            variants={{
                hidden: { opacity: 0 },
                visible: {
                    opacity: 1,
                    transition: {
                        staggerChildren: staggerDelay,
                        delayChildren: 0.1,
                    },
                },
            }}
            className={className}
            {...props}
        >
            {children}
        </motion.div>
    );
}

interface StaggerItemProps extends Omit<HTMLMotionProps<"div">, "children"> {
    children: ReactNode;
    className?: string;
}

export function StaggerItem({ children, className = "", ...props }: StaggerItemProps) {
    return (
        <motion.div
            variants={staggerItem}
            className={className}
            {...props}
        >
            {children}
        </motion.div>
    );
}

interface CountUpProps {
    value: number;
    prefix?: string;
    suffix?: string;
    duration?: number;
    className?: string;
    decimals?: number;
}

export function CountUp({ 
    value, 
    prefix = "", 
    suffix = "", 
    duration = 2,
    className = "",
    decimals = 0,
}: CountUpProps) {
    return (
        <motion.span
            className={className}
            initial={{ opacity: 0 }}
            whileInView={{ opacity: 1 }}
            viewport={{ once: true }}
        >
            <motion.span
                initial={{ opacity: 0 }}
                whileInView={{ opacity: 1 }}
                viewport={{ once: true }}
            >
                {prefix}
                <motion.span
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                    transition={{ duration: 0.3 }}
                >
                    <CountUpNumber value={value} duration={duration} decimals={decimals} />
                </motion.span>
                {suffix}
            </motion.span>
        </motion.span>
    );
}

function CountUpNumber({ value, duration, decimals }: { value: number; duration: number; decimals: number }) {
    return (
        <motion.span
            initial={{ opacity: 0 }}
            whileInView={{ 
                opacity: 1,
            }}
            viewport={{ once: true }}
            transition={{ duration }}
        >
            {value >= 1000 
                ? value.toLocaleString(undefined, { maximumFractionDigits: 0 }) 
                : value.toFixed(decimals)}
        </motion.span>
    );
}

export function PulsingDot({ className = "" }: { className?: string }) {
    return (
        <span className={`relative flex h-2 w-2 ${className}`}>
            <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-green-400 opacity-75" />
            <span className="relative inline-flex rounded-full h-2 w-2 bg-green-500" />
        </span>
    );
}

export function LiveIndicator({ className = "" }: { className?: string }) {
    return (
        <span className={`relative flex h-2.5 w-2.5 ${className}`}>
            <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-red-400 opacity-75" />
            <span className="relative inline-flex rounded-full h-2.5 w-2.5 bg-red-500" />
        </span>
    );
}
