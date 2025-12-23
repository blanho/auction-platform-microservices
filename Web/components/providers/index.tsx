"use client";

import { ReactNode } from "react";
import { SessionProvider as NextAuthSessionProvider } from "next-auth/react";
import { ThemeProvider } from "next-themes";
import { Toaster } from "@/components/ui/sonner";
import { UndoToastContainer } from "@/components/ui/undo-toast";
import { NotificationProvider } from "@/context/notification.context";
import { WatchlistProvider } from "@/context/watchlist.context";
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
                        <WatchlistProvider>
                            {children}
                        </WatchlistProvider>
                    </NotificationProvider>
                    <Toaster />
                    <UndoToastContainer />
                </ThemeProvider>
            </QueryProvider>
        </NextAuthSessionProvider>
    );
}

