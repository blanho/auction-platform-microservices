"use client";

import { useState, useEffect, Suspense } from "react";
import { useSearchParams } from "next/navigation";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { AlertCircle, CheckCircle2, Loader2, Mail } from "lucide-react";

type ConfirmationStatus = "loading" | "success" | "error" | "invalid";

function ConfirmEmailContent() {
    const searchParams = useSearchParams();
    
    const [status, setStatus] = useState<ConfirmationStatus>("loading");
    const [message, setMessage] = useState<string>("");
    const [resendLoading, setResendLoading] = useState(false);
    const [resendSuccess, setResendSuccess] = useState(false);

    const identityServerUrl = process.env.NEXT_PUBLIC_IDENTITY_SERVER_URL || "http://localhost:5001";

    const userId = searchParams.get("userId") || "";
    const token = searchParams.get("token") || "";
    const email = searchParams.get("email") || "";

    useEffect(() => {
        if (!userId || !token) {
            setStatus("invalid");
            setMessage("Invalid confirmation link. Please request a new confirmation email.");
            return;
        }

        const confirmEmail = async () => {
            try {
                const response = await fetch(`${identityServerUrl}/api/account/confirm-email`, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify({ userId, token })
                });

                const result = await response.json();

                if (!response.ok) {
                    throw new Error(result.message || "Email confirmation failed");
                }

                setStatus("success");
                setMessage("Your email has been confirmed successfully!");
            } catch (err) {
                setStatus("error");
                setMessage(err instanceof Error ? err.message : "Email confirmation failed. The link may be expired or invalid.");
            }
        };

        confirmEmail();
    }, [userId, token, identityServerUrl]);

    const handleResendConfirmation = async () => {
        if (!email) {
            setMessage("Email address not available. Please go to sign in and request a new confirmation.");
            return;
        }

        setResendLoading(true);
        
        try {
            const response = await fetch(`${identityServerUrl}/api/account/resend-confirmation`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({ email })
            });

            if (!response.ok) {
                const result = await response.json();
                throw new Error(result.message || "Failed to resend confirmation email");
            }

            setResendSuccess(true);
        } catch (err) {
            setMessage(err instanceof Error ? err.message : "Failed to resend confirmation email");
        } finally {
            setResendLoading(false);
        }
    };

    const renderContent = () => {
        switch (status) {
            case "loading":
                return (
                    <>
                        <div className="flex items-center justify-center mb-4">
                            <Loader2 className="h-12 w-12 text-primary animate-spin" />
                        </div>
                        <CardTitle className="text-2xl font-bold text-center">Confirming Your Email</CardTitle>
                        <CardDescription className="text-center">
                            Please wait while we verify your email address...
                        </CardDescription>
                    </>
                );

            case "success":
                return (
                    <>
                        <div className="flex items-center justify-center mb-4">
                            <div className="rounded-full bg-green-100 p-3">
                                <CheckCircle2 className="h-8 w-8 text-green-600" />
                            </div>
                        </div>
                        <CardTitle className="text-2xl font-bold text-center">Email Confirmed!</CardTitle>
                        <CardDescription className="text-center">
                            {message}
                        </CardDescription>
                    </>
                );

            case "error":
            case "invalid":
                return (
                    <>
                        <div className="flex items-center justify-center mb-4">
                            <div className="rounded-full bg-red-100 p-3">
                                <AlertCircle className="h-8 w-8 text-red-600" />
                            </div>
                        </div>
                        <CardTitle className="text-2xl font-bold text-center">Confirmation Failed</CardTitle>
                        <CardDescription className="text-center">
                            {message}
                        </CardDescription>
                    </>
                );
        }
    };

    const renderActions = () => {
        if (status === "loading") {
            return null;
        }

        if (status === "success") {
            return (
                <CardContent className="pt-0">
                    <Link href="/auth/signin" className="block">
                        <Button className="w-full">
                            Continue to Sign In
                        </Button>
                    </Link>
                </CardContent>
            );
        }

        if (resendSuccess) {
            return (
                <CardContent className="pt-0">
                    <Alert className="mb-4">
                        <Mail className="h-4 w-4" />
                        <AlertDescription>
                            A new confirmation email has been sent. Please check your inbox.
                        </AlertDescription>
                    </Alert>
                    <Link href="/auth/signin" className="block">
                        <Button variant="outline" className="w-full">
                            Back to Sign In
                        </Button>
                    </Link>
                </CardContent>
            );
        }

        return (
            <CardContent className="pt-0 space-y-3">
                {email && (
                    <Button 
                        onClick={handleResendConfirmation} 
                        className="w-full"
                        disabled={resendLoading}
                    >
                        {resendLoading ? (
                            <>
                                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                                Sending...
                            </>
                        ) : (
                            "Resend Confirmation Email"
                        )}
                    </Button>
                )}
                <Link href="/auth/signin" className="block">
                    <Button variant="outline" className="w-full">
                        Back to Sign In
                    </Button>
                </Link>
            </CardContent>
        );
    };

    return (
        <div className="flex min-h-screen items-center justify-center bg-background p-4">
            <Card className="w-full max-w-md">
                <CardHeader className="space-y-1">
                    {renderContent()}
                </CardHeader>
                {renderActions()}
            </Card>
        </div>
    );
}

export default function ConfirmEmailPage() {
    return (
        <Suspense fallback={
            <div className="flex min-h-screen items-center justify-center bg-background p-4">
                <Card className="w-full max-w-md">
                    <CardHeader className="space-y-1">
                        <div className="flex items-center justify-center mb-4">
                            <Loader2 className="h-12 w-12 text-primary animate-spin" />
                        </div>
                        <CardTitle className="text-2xl font-bold text-center">Loading...</CardTitle>
                    </CardHeader>
                </Card>
            </div>
        }>
            <ConfirmEmailContent />
        </Suspense>
    );
}
