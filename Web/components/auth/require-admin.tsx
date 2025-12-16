"use client";

import { useRouter } from "next/navigation";
import { useEffect } from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faSpinner, faShieldHalved } from "@fortawesome/free-solid-svg-icons";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { useAuthSession } from "@/hooks/use-auth-session";

interface RequireAdminProps {
  children: React.ReactNode;
  fallback?: React.ReactNode;
}

export function RequireAdmin({ children, fallback }: RequireAdminProps) {
  const { isAuthenticated, isLoading, isAdmin } = useAuthSession();
  const router = useRouter();

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.push(`/auth/signin?callbackUrl=${encodeURIComponent(window.location.pathname)}`);
    }
  }, [isLoading, isAuthenticated, router]);

  if (isLoading) {
    return (
      <div className="flex min-h-[400px] items-center justify-center">
        <FontAwesomeIcon icon={faSpinner} className="h-8 w-8 animate-spin text-muted-foreground" />
      </div>
    );
  }

  if (!isAuthenticated) {
    return null;
  }

  if (!isAdmin) {
    if (fallback) {
      return <>{fallback}</>;
    }

    return (
      <div className="container py-8">
        <Alert variant="destructive">
          <FontAwesomeIcon icon={faShieldHalved} className="h-4 w-4" />
          <AlertTitle>Access Denied</AlertTitle>
          <AlertDescription className="mt-2">
            <p>You do not have permission to access this page.</p>
            <p className="mt-2">This area is restricted to administrators only.</p>
            <Button
              variant="outline"
              className="mt-4"
              onClick={() => router.push("/")}
            >
              Go to Home
            </Button>
          </AlertDescription>
        </Alert>
      </div>
    );
  }

  return <>{children}</>;
}

export function useIsAdmin() {
  const { isAdmin } = useAuthSession();
  return isAdmin;
}
