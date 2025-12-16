"use client";

import { ReactNode } from "react";
import { SessionProvider as NextAuthSessionProvider } from "next-auth/react";
import { ThemeProvider } from "next-themes";
import { Toaster } from "@/components/ui/sonner";
import { NotificationProvider } from "@/context/notification.context";
import { QueryProvider } from "./query-provider";

interface ProvidersProps {
    children: ReactNode;
}

export function Providers({ children }: ProvidersProps) {
    return (
        <NextAuthSessionProvider
            refetchInterval={5 * 60}
            refetchOnWindowFocus={false}
        >
            <QueryProvider>
                <ThemeProvider
                    attribute="class"
                    defaultTheme="system"
                    enableSystem
                    disableTransitionOnChange
                >
                    <NotificationProvider>
                        {children}
                    </NotificationProvider>
                    <Toaster />
                </ThemeProvider>
            </QueryProvider>
        </NextAuthSessionProvider>
    );
}
