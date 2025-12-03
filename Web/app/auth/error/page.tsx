"use client";

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { useSearchParams, useRouter } from "next/navigation";

export default function AuthErrorPage() {
    const searchParams = useSearchParams();
    const router = useRouter();
    const error = searchParams.get("error");

    const errorMessages: Record<string, { title: string; description: string }> = {
        Configuration: {
            title: "Configuration Error",
            description: "There is a problem with the server configuration.",
        },
        AccessDenied: {
            title: "Access Denied",
            description: "You do not have permission to access this resource.",
        },
        Verification: {
            title: "Verification Error",
            description: "The verification token has expired or has already been used.",
        },
        RefreshAccessTokenError: {
            title: "Session Expired",
            description: "Your session has expired. Please sign in again.",
        },
        Default: {
            title: "Authentication Error",
            description: "An error occurred during authentication. Please try again.",
        },
    };

    const errorInfo = errorMessages[error || "Default"] || errorMessages.Default;

    return (
        <div className="flex min-h-screen items-center justify-center bg-background p-4">
            <Card className="w-full max-w-md">
                <CardHeader className="space-y-1">
                    <CardTitle className="text-2xl font-bold text-destructive">
                        {errorInfo.title}
                    </CardTitle>
                    <CardDescription>
                        {errorInfo.description}
                    </CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                    <div className="space-y-2">
                        <Button
                            onClick={() => router.push("/auth/signin")}
                            className="w-full"
                        >
                            Try Again
                        </Button>
                        <Button
                            onClick={() => router.push("/")}
                            variant="outline"
                            className="w-full"
                        >
                            Go Home
                        </Button>
                    </div>
                </CardContent>
            </Card>
        </div>
    );
}
