"use client";

import { useState, useEffect, Suspense } from "react";
import { signIn } from "next-auth/react";
import { useRouter, useSearchParams } from "next/navigation";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Skeleton } from "@/components/ui/skeleton";
import { AlertCircle, Loader2, CheckCircle2 } from "lucide-react";


function SignInLoading() {
    return (
        <div className="flex min-h-screen items-center justify-center bg-background p-4">
            <Card className="w-full max-w-md">
                <CardHeader className="space-y-1">
                    <Skeleton className="h-8 w-24" />
                    <Skeleton className="h-4 w-64" />
                </CardHeader>
                <CardContent className="space-y-4">
                    <div className="space-y-2">
                        <Skeleton className="h-4 w-20" />
                        <Skeleton className="h-10 w-full" />
                    </div>
                    <div className="space-y-2">
                        <Skeleton className="h-4 w-20" />
                        <Skeleton className="h-10 w-full" />
                    </div>
                    <Skeleton className="h-10 w-full" />
                </CardContent>
            </Card>
        </div>
    );
}

function SignInContent() {
    const router = useRouter();
    const searchParams = useSearchParams();
    const callbackUrl = searchParams.get("callbackUrl") || "/";
    const registered = searchParams.get("registered");
    const reset = searchParams.get("reset");

    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [loading, setLoading] = useState(false);
    const [oauthLoading, setOauthLoading] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [externalProviders, setExternalProviders] = useState<string[]>([]);

    const identityServerUrl = process.env.NEXT_PUBLIC_IDENTITY_SERVER_URL || "http://localhost:5001";

    useEffect(() => {
        const fetchProviders = async () => {
            try {
                const response = await fetch(`${identityServerUrl}/api/account/external-providers`);
                if (response.ok) {
                    const result = await response.json();
                    setExternalProviders(result.data || []);
                }
            } catch {
                console.error("Failed to fetch external providers");
            }
        };
        fetchProviders();
    }, [identityServerUrl]);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!username || !password) {
            setError("Please enter both username and password");
            return;
        }

        setLoading(true);
        setError(null);

        try {
            const result = await signIn("id-server", {
                username,
                password,
                redirect: false,
                callbackUrl
            });

            if (result?.error) {
                setError("Invalid username or password");
            } else if (result?.ok) {
                router.push(callbackUrl);
            }
        } catch {
            setError("An error occurred during sign in");
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="flex min-h-screen items-center justify-center bg-background p-4">
            <Card className="w-full max-w-md">
                <CardHeader className="space-y-1">
                    <CardTitle className="text-2xl font-bold">Sign In</CardTitle>
                    <CardDescription>
                        Enter your credentials to access your account
                    </CardDescription>
                </CardHeader>
                <CardContent>
                    <form onSubmit={handleSubmit} className="space-y-4">
                        {registered && (
                            <Alert className="bg-green-500/15 border-green-500/50">
                                <AlertDescription className="text-green-600 dark:text-green-400">
                                    Registration successful! Please check your email to confirm your account.
                                </AlertDescription>
                            </Alert>
                        )}

                        {reset && (
                            <Alert className="bg-green-500/15 border-green-500/50">
                                <CheckCircle2 className="h-4 w-4 text-green-600" />
                                <AlertDescription className="text-green-600 dark:text-green-400">
                                    Password reset successful! You can now sign in with your new password.
                                </AlertDescription>
                            </Alert>
                        )}

                        {error && (
                            <Alert variant="destructive">
                                <AlertCircle className="h-4 w-4" />
                                <AlertDescription>{error}</AlertDescription>
                            </Alert>
                        )}

                        <div className="space-y-2">
                            <Label htmlFor="username">Username</Label>
                            <Input
                                id="username"
                                type="text"
                                placeholder="Enter your username"
                                value={username}
                                onChange={(e) => setUsername(e.target.value)}
                                disabled={loading}
                                required
                            />
                        </div>

                        <div className="space-y-2">
                            <div className="flex justify-between items-center">
                                <Label htmlFor="password">Password</Label>
                                <Link 
                                    href="/auth/forgot-password" 
                                    className="text-sm text-primary hover:underline"
                                >
                                    Forgot password?
                                </Link>
                            </div>
                            <Input
                                id="password"
                                type="password"
                                placeholder="Enter your password"
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                                disabled={loading}
                                required
                            />
                        </div>

                        <Button
                            type="submit"
                            className="w-full"
                            size="lg"
                            disabled={loading}
                        >
                            {loading ? (
                                <>
                                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                                    Signing in...
                                </>
                            ) : (
                                "Sign In"
                            )}
                        </Button>

                        {externalProviders.length > 0 && (
                            <>
                                <div className="relative">
                                    <div className="absolute inset-0 flex items-center">
                                        <span className="w-full border-t" />
                                    </div>
                                    <div className="relative flex justify-center text-xs uppercase">
                                        <span className="bg-card px-2 text-muted-foreground">
                                            Or continue with
                                        </span>
                                    </div>
                                </div>

                                <div className="grid gap-2">
                                    {externalProviders.includes("Google") && (
                                        <Button
                                            type="button"
                                            variant="outline"
                                            onClick={() => handleOAuthLogin("Google")}
                                            disabled={!!oauthLoading}
                                        >
                                            {oauthLoading === "Google" ? (
                                                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                                            ) : (
                                                <svg className="mr-2 h-4 w-4" viewBox="0 0 24 24">
                                                    <path
                                                        fill="currentColor"
                                                        d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"
                                                    />
                                                    <path
                                                        fill="currentColor"
                                                        d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"
                                                    />
                                                    <path
                                                        fill="currentColor"
                                                        d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"
                                                    />
                                                    <path
                                                        fill="currentColor"
                                                        d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"
                                                    />
                                                </svg>
                                            )}
                                            Google
                                        </Button>
                                    )}
                                    {externalProviders.includes("Facebook") && (
                                        <Button
                                            type="button"
                                            variant="outline"
                                            onClick={() => handleOAuthLogin("Facebook")}
                                            disabled={!!oauthLoading}
                                        >
                                            {oauthLoading === "Facebook" ? (
                                                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                                            ) : (
                                                <svg className="mr-2 h-4 w-4" viewBox="0 0 24 24">
                                                    <path
                                                        fill="currentColor"
                                                        d="M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z"
                                                    />
                                                </svg>
                                            )}
                                            Facebook
                                        </Button>
                                    )}
                                </div>
                            </>
                        )}

                        <div className="text-center text-sm text-muted-foreground">
                            Don&apos;t have an account?{" "}
                            <Button
                                variant="link"
                                className="p-0 h-auto font-semibold"
                                onClick={() => router.push("/auth/register")}
                                type="button"
                            >
                                Register
                            </Button>
                        </div>
                    </form>
                </CardContent>
            </Card>
        </div>
    );

    function handleOAuthLogin(provider: string) {
        setOauthLoading(provider);
        window.location.href = `${identityServerUrl}/api/account/external-login?provider=${provider}&returnUrl=${encodeURIComponent(callbackUrl)}`;
    }
}

export default function SignInPage() {
    return (
        <Suspense fallback={<SignInLoading />}>
            <SignInContent />
        </Suspense>
    );
}
