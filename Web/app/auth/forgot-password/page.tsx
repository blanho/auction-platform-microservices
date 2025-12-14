'use client';

import { useState } from 'react';
import Link from 'next/link';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Loader2, Mail, ArrowLeft } from 'lucide-react';

import { Button } from '@/components/ui/button';
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import {
    Form,
    FormControl,
    FormField,
    FormItem,
    FormLabel,
    FormMessage,
} from '@/components/ui/form';

import { forgotPasswordSchema, ForgotPasswordFormValues } from '@/lib/validations/auth';
import { showErrorToast } from '@/utils';

const IDENTITY_SERVER_URL = process.env.NEXT_PUBLIC_IDENTITY_SERVER_URL || 'http://localhost:5001';

export default function ForgotPasswordPage() {
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [sentEmail, setSentEmail] = useState<string | null>(null);

    const form = useForm<ForgotPasswordFormValues>({
        resolver: zodResolver(forgotPasswordSchema),
        defaultValues: {
            email: '',
        },
    });

    const onSubmit = async (values: ForgotPasswordFormValues) => {
        setIsSubmitting(true);

        try {
            const response = await fetch(`${IDENTITY_SERVER_URL}/api/account/forgot-password`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email: values.email }),
            });

            const result = await response.json();

            if (!response.ok) {
                throw new Error(result.message || 'Request failed');
            }

            setSentEmail(values.email);
        } catch (error) {
            showErrorToast(error);
        } finally {
            setIsSubmitting(false);
        }
    };

    if (sentEmail) {
        return (
            <div className="flex min-h-screen items-center justify-center bg-background p-4">
                <Card className="w-full max-w-md">
                    <CardHeader className="space-y-1">
                        <div className="flex items-center justify-center mb-4">
                            <div className="rounded-full bg-green-100 p-3">
                                <Mail className="h-8 w-8 text-green-600" />
                            </div>
                        </div>
                        <CardTitle className="text-2xl font-bold text-center">
                            Check Your Email
                        </CardTitle>
                        <CardDescription className="text-center">
                            If an account exists with <strong>{sentEmail}</strong>, you
                            will receive a password reset link shortly.
                        </CardDescription>
                    </CardHeader>
                    <CardContent className="space-y-4">
                        <p className="text-sm text-muted-foreground text-center">
                            Didn&apos;t receive the email? Check your spam folder or try
                            again with a different email address.
                        </p>
                        <div className="flex flex-col gap-2">
                            <Button
                                variant="outline"
                                onClick={() => {
                                    setSentEmail(null);
                                    form.reset();
                                }}
                                className="w-full"
                            >
                                Try Another Email
                            </Button>
                            <Link href="/auth/signin" className="w-full">
                                <Button variant="ghost" className="w-full">
                                    <ArrowLeft className="mr-2 h-4 w-4" />
                                    Back to Sign In
                                </Button>
                            </Link>
                        </div>
                    </CardContent>
                </Card>
            </div>
        );
    }

    return (
        <div className="flex min-h-screen items-center justify-center bg-background p-4">
            <Card className="w-full max-w-md">
                <CardHeader className="space-y-1">
                    <CardTitle className="text-2xl font-bold">Forgot Password</CardTitle>
                    <CardDescription>
                        Enter your email address and we&apos;ll send you a link to reset
                        your password.
                    </CardDescription>
                </CardHeader>
                <CardContent>
                    <Form {...form}>
                        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
                            <FormField
                                control={form.control}
                                name="email"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>Email</FormLabel>
                                        <FormControl>
                                            <Input
                                                type="email"
                                                placeholder="you@example.com"
                                                disabled={isSubmitting}
                                                {...field}
                                            />
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
                                        Sending...
                                    </>
                                ) : (
                                    'Send Reset Link'
                                )}
                            </Button>

                            <div className="text-center text-sm">
                                Remember your password?{' '}
                                <Link
                                    href="/auth/signin"
                                    className="text-primary hover:underline"
                                >
                                    Sign in
                                </Link>
                            </div>
                        </form>
                    </Form>
                </CardContent>
            </Card>
        </div>
    );
}
