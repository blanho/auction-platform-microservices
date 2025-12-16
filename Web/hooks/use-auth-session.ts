"use client";

import { useSession } from "next-auth/react";
import { useMemo } from "react";

interface AuthUser {
    id: string;
    name?: string | null;
    email?: string | null;
    image?: string | null;
    role?: string;
}

interface UseAuthSessionResult {
    user: AuthUser | null;
    accessToken: string | null;
    isAuthenticated: boolean;
    isLoading: boolean;
    isAdmin: boolean;
}

export function useAuthSession(): UseAuthSessionResult {
    const { data: session, status } = useSession();

    return useMemo(() => {
        const isLoading = status === "loading";
        const isAuthenticated = status === "authenticated" && !!session?.user;
        const user = isAuthenticated ? (session.user as AuthUser) : null;
        const accessToken = session?.accessToken ?? null;
        const isAdmin = user?.role === "admin" || user?.role === "Admin";

        return {
            user,
            accessToken,
            isAuthenticated,
            isLoading,
            isAdmin,
        };
    }, [session, status]);
}
