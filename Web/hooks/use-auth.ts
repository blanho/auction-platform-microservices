"use client";

import { useSession } from "next-auth/react";
import { useRouter } from "next/navigation";
import { useEffect } from "react";

export function useAuth(required: boolean = true) {
  const { data: session, status } = useSession();
  const router = useRouter();

  useEffect(() => {
    if (required && status === "unauthenticated") {
      router.push(
        `/auth/signin?callbackUrl=${encodeURIComponent(
          window.location.pathname
        )}`
      );
    }
  }, [required, status, router]);

  return {
    session,
    status,
    isAuthenticated: status === "authenticated",
    isLoading: status === "loading"
  };
}
