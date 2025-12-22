'use client';

import { useState, useEffect, Suspense } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import Link from 'next/link';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Loader2, CheckCircle2, Eye, EyeOff, ArrowLeft } from 'lucide-react';

import { Button } from '@/components/ui/button';
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Alert, AlertDescription } from '@/components/ui/alert';
import {
    Form,
    FormControl,
    FormField,
    FormItem,
    FormLabel,
    FormMessage,
    FormDescription,
} from '@/components/ui/form';

import { resetPasswordSchema, ResetPasswordFormValues } from '@/lib/validations/auth';
import { showErrorToast } from '@/utils';

const IDENTITY_SERVER_URL = process.env.NEXT_PUBLIC_IDENTITY_SERVER_URL || 'http://localhost:5001';

function ResetPasswordForm() {
    const router = useRouter();
    const searchParams = useSearchParams();

    const [isSubmitting, setIsSubmitting] = useState(false);
    const [success, setSuccess] = useState(false);
    const [showPassword, setShowPassword] = useState(false);
    const [showConfirmPassword, setShowConfirmPassword] = useState(false);
    const [invalidLink, setInvalidLink] = useState(false);

    const form = useForm<ResetPasswordFormValues>({
        resolver: zodResolver(resetPasswordSchema),
        defaultValues: {
            email: '',
            token: '',
            newPassword: '',
            confirmPassword: '',
        },
    });

    useEffect(() => {
        const email = searchParams.get('email') || '';
        const token = searchParams.get('token') || '';

        if (!email || !token) {
            setInvalidLink(true);
        } else {
            form.setValue('email', email);
            form.setValue('token', token);
        }
    }, [searchParams, form]);

    const onSubmit = async (values: ResetPasswordFormValues) => {
        setIsSubmitting(true);

        try {
            const response = await fetch(`${IDENTITY_SERVER_URL}/api/account/reset-password`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    email: values.email,
                    token: values.token,
                    newPassword: values.newPassword,
                }),
            });

            const result = await response.json();

            if (!response.ok) {
                const errorMessage = result.message || 'Password reset failed';
                const errors = result.errors ? result.errors.join(', ') : '';
                throw new Error(errors || errorMessage);
            }

            setSuccess(true);
            setTimeout(() => {
                router.push('/auth/signin?reset=true');
            }, 3000);
        } catch (error) {
            showErrorToast(error);
        } finally {
            setIsSubmitting(false);
        }
    };

    if (success) {
        return (
            <div className="flex min-h-screen items-center justify-center bg-background p-4">
                <Card className="w-full max-w-md">
                    <CardHeader className="space-y-1">
                        <div className="flex items-center justify-center mb-4">
                            <div className="rounded-full bg-green-100 p-3">
                                <CheckCircle2 className="h-8 w-8 text-green-600" />
                            </div>
                        </div>
                        <CardTitle className="text-2xl font-bold text-center">
                            Password Reset Successful!
                        </CardTitle>
                        <CardDescription className="text-center">
                            Your password has been reset. Redirecting to sign in...
                        </CardDescription>
                    </CardHeader>
                </Card>
            </div>
        );
    }

    if (invalidLink) {
        return (
            <div className="flex min-h-screen items-center justify-center bg-background p-4">
                <Card className="w-full max-w-md">
                    <CardHeader className="space-y-1">
                        <CardTitle className="text-2xl font-bold">Invalid Link</CardTitle>
                        <CardDescription>
                            This password reset link is invalid or has expired.
                        </CardDescription>
                    </CardHeader>
                    <CardContent>
                        <Alert variant="destructive" className="mb-4">
                            <AlertDescription>
                                Please request a new password reset link.
                            </AlertDescription>
                        </Alert>
                        <Link href="/auth/forgot-password">
                            <Button className="w-full">Request New Link</Button>
                        </Link>
                    </CardContent>
                </Card>
            </div>
        );
    }

    return (
        <div className="flex min-h-screen items-center justify-center bg-background p-4">
            <Card className="w-full max-w-md">
                <CardHeader className="space-y-1">
                    <CardTitle className="text-2xl font-bold">Reset Password</CardTitle>
                    <CardDescription>Enter your new password below.</CardDescription>
                </CardHeader>
                <CardContent>
                    <Form {...form}>
                        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
                            <FormField
                                control={form.control}
                                name="newPassword"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>New Password</FormLabel>
                                        <FormControl>
                                            <div className="relative">
                                                <Input
                                                    type={showPassword ? 'text' : 'password'}
                                                    placeholder="••••••••"
                                                    disabled={isSubmitting}
                                                    {...field}
                                                />
                                                <button
                                                    type="button"
                                                    onClick={() => setShowPassword(!showPassword)}
                                                    className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground"
                                                >
                                                    {showPassword ? (
                                                        <EyeOff className="h-4 w-4" />
                                                    ) : (
                                                        <Eye className="h-4 w-4" />
                                                    )}
                                                </button>
                                            </div>
                                        </FormControl>
                                        <FormDescription>
                                            Must be at least 6 characters with uppercase, lowercase,
                                            and a number.
                                        </FormDescription>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />

                            <FormField
                                control={form.control}
                                name="confirmPassword"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>Confirm Password</FormLabel>
                                        <FormControl>
                                            <div className="relative">
                                                <Input
                                                    type={showConfirmPassword ? 'text' : 'password'}
                                                    placeholder="••••••••"
                                                    disabled={isSubmitting}
                                                    {...field}
                                                />
                                                <button
                                                    type="button"
                                                    onClick={() =>
                                                        setShowConfirmPassword(!showConfirmPassword)
                                                    }
                                                    className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground"
                                                >
                                                    {showConfirmPassword ? (
                                                        <EyeOff className="h-4 w-4" />
                                                    ) : (
                                                        <Eye className="h-4 w-4" />
                                                    )}
                                                </button>
                                            </div>
                                        </FormControl>
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />

                            <Button
                                type="submit"
                                className="w-full"
                                disabled={isSubmitting}
                            >
                                {isSubmitting ? (
                                    <>
                                        <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                                        Resetting...
                                    </>
                                ) : (
                                    'Reset Password'
                                )}
                            </Button>

                            <Link href="/auth/signin" className="block">
                                <Button variant="ghost" className="w-full">
                                    <ArrowLeft className="mr-2 h-4 w-4" />
                                    Back to Sign In
                                </Button>
                            </Link>
                        </form>
                    </Form>
                </CardContent>
            </Card>
        </div>
    );
}

export default function ResetPasswordPage() {
    return (
        <Suspense
            fallback={
                <div className="flex min-h-screen items-center justify-center bg-background p-4">
                    <Card className="w-full max-w-md">
                        <CardHeader>
                            <CardTitle>Loading...</CardTitle>
                        </CardHeader>
                    </Card>
                </div>
            }
        >
            <ResetPasswordForm />
        </Suspense>
    );
}
