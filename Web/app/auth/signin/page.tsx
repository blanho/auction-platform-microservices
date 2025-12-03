"use client";

import { signIn } from "next-auth/react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { useSearchParams } from "next/navigation";

export default function SignInPage() {
    const searchParams = useSearchParams();
    const callbackUrl = searchParams.get("callbackUrl") || "/";
    const error = searchParams.get("error");
    const registered = searchParams.get("registered");

    const handleSignIn = async () => {
        await signIn("id-server", { callbackUrl });
    };

    return (
        <div className="flex min-h-screen items-center justify-center bg-background p-4">
            <Card className="w-full max-w-md">
                <CardHeader className="space-y-1">
                    <CardTitle className="text-2xl font-bold">Sign In</CardTitle>
                    <CardDescription>
                        Sign in to your auction account
                    </CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                    {registered && (
                        <div className="rounded-md bg-green-500/15 p-3 text-sm text-green-600 dark:text-green-400">
                            Registration successful! You can now sign in.
                        </div>
                    )}
                    {error && (
                        <div className="rounded-md bg-destructive/15 p-3 text-sm text-destructive">
                            {error === "OAuthSignin" && "Error occurred during sign in"}
                            {error === "OAuthCallback" && "Error occurred during callback"}
                            {error === "OAuthCreateAccount" && "Could not create account"}
                            {error === "EmailCreateAccount" && "Could not create email account"}
                            {error === "Callback" && "Error in callback"}
                            {error === "OAuthAccountNotLinked" && "Account already linked to another provider"}
                            {error === "EmailSignin" && "Check your email address"}
                            {error === "CredentialsSignin" && "Sign in failed. Check your credentials"}
                            {error === "SessionRequired" && "Please sign in to access this page"}
                            {error === "default" && "Unable to sign in"}
                        </div>
                    )}
                    <Button
                        onClick={handleSignIn}
                        className="w-full"
                        size="lg"
                    >
                        Sign in with Identity Server
                    </Button>

                    <div className="text-center text-sm text-muted-foreground">
                        Don&apos;t have an account?{" "}
                        <Button
                            variant="link"
                            className="p-0 h-auto font-semibold"
                            onClick={() => window.location.href = "/auth/register"}
                        >
                            Register
                        </Button>
                    </div>
                </CardContent>
            </Card>
        </div>
    );
}
