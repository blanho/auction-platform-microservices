"use client";

import { useTheme } from "next-themes";
import { useEffect, useState } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { Moon, Sun, Monitor } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { cn } from "@/lib/utils";

interface ThemeToggleProps {
    variant?: "default" | "icon" | "expanded";
    className?: string;
}

export function ThemeToggle({ variant = "default", className }: ThemeToggleProps) {
    const { theme, setTheme, resolvedTheme } = useTheme();
    const [mounted, setMounted] = useState(false);

    useEffect(() => {
        setMounted(true);
    }, []);

    if (!mounted) {
        return (
            <Button variant="ghost" size="icon" className={cn("w-9 h-9", className)}>
                <div className="h-5 w-5 bg-muted rounded animate-pulse" />
            </Button>
        );
    }

    if (variant === "icon") {
        return (
            <Button
                variant="ghost"
                size="icon"
                onClick={() => setTheme(resolvedTheme === "dark" ? "light" : "dark")}
                className={cn(
                    "relative w-9 h-9 rounded-full overflow-hidden",
                    "hover:bg-purple-100 dark:hover:bg-purple-900/30",
                    className
                )}
            >
                <AnimatePresence mode="wait" initial={false}>
                    <motion.div
                        key={resolvedTheme}
                        initial={{ y: -20, opacity: 0, rotate: -90 }}
                        animate={{ y: 0, opacity: 1, rotate: 0 }}
                        exit={{ y: 20, opacity: 0, rotate: 90 }}
                        transition={{ duration: 0.2 }}
                    >
                        {resolvedTheme === "dark" ? (
                            <Moon className="h-5 w-5 text-purple-400" />
                        ) : (
                            <Sun className="h-5 w-5 text-amber-500" />
                        )}
                    </motion.div>
                </AnimatePresence>
                <span className="sr-only">Toggle theme</span>
            </Button>
        );
    }

    if (variant === "expanded") {
        const themes = [
            { value: "light", icon: Sun, label: "Light" },
            { value: "dark", icon: Moon, label: "Dark" },
            { value: "system", icon: Monitor, label: "System" },
        ];

        return (
            <div className={cn("flex items-center gap-1 p-1 rounded-full bg-muted", className)}>
                {themes.map(({ value, icon: Icon, label }) => (
                    <button
                        key={value}
                        onClick={() => setTheme(value)}
                        className={cn(
                            "flex items-center gap-1.5 px-3 py-1.5 rounded-full text-sm font-medium transition-all",
                            theme === value
                                ? "bg-background text-foreground shadow-sm"
                                : "text-muted-foreground hover:text-foreground"
                        )}
                    >
                        <Icon className="h-4 w-4" />
                        <span className="hidden sm:inline">{label}</span>
                    </button>
                ))}
            </div>
        );
    }

    return (
        <DropdownMenu>
            <DropdownMenuTrigger asChild>
                <Button
                    variant="ghost"
                    size="icon"
                    className={cn(
                        "relative w-9 h-9 rounded-full",
                        "hover:bg-purple-100 dark:hover:bg-purple-900/30",
                        className
                    )}
                >
                    <Sun className="h-5 w-5 rotate-0 scale-100 transition-all dark:-rotate-90 dark:scale-0 text-amber-500" />
                    <Moon className="absolute h-5 w-5 rotate-90 scale-0 transition-all dark:rotate-0 dark:scale-100 text-purple-400" />
                    <span className="sr-only">Toggle theme</span>
                </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end" className="min-w-[140px]">
                <DropdownMenuItem
                    onClick={() => setTheme("light")}
                    className={cn(theme === "light" && "bg-accent")}
                >
                    <Sun className="mr-2 h-4 w-4 text-amber-500" />
                    Light
                </DropdownMenuItem>
                <DropdownMenuItem
                    onClick={() => setTheme("dark")}
                    className={cn(theme === "dark" && "bg-accent")}
                >
                    <Moon className="mr-2 h-4 w-4 text-purple-400" />
                    Dark
                </DropdownMenuItem>
                <DropdownMenuItem
                    onClick={() => setTheme("system")}
                    className={cn(theme === "system" && "bg-accent")}
                >
                    <Monitor className="mr-2 h-4 w-4 text-slate-500" />
                    System
                </DropdownMenuItem>
            </DropdownMenuContent>
        </DropdownMenu>
    );
}
