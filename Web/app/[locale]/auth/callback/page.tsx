'use client';

import { useEffect, useState } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { signIn } from 'next-auth/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faSpinner, faCheckCircle, faExclamationCircle } from '@fortawesome/free-solid-svg-icons';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Suspense } from 'react';

function CallbackContent() {
    const router = useRouter();
    const searchParams = useSearchParams();
    const [status, setStatus] = useState<'loading' | 'success' | 'error'>('loading');
    const [message, setMessage] = useState('Completing sign in...');

    useEffect(() => {
        const handleCallback = async () => {
            const error = searchParams.get('error');
            const success = searchParams.get('success');
            const provider = searchParams.get('provider');
            const username = searchParams.get('username');
            const returnUrl = searchParams.get('returnUrl') || '/';

            if (error) {
                setStatus('error');
                setMessage(decodeURIComponent(error.replace(/\+/g, ' ')));
                return;
            }

            if (success === 'true' && provider && username) {
                setStatus('success');
                setMessage(`Signed in with ${provider}! Redirecting...`);

                setTimeout(() => {
                    router.push(returnUrl);
                }, 1500);
            } else {
                setStatus('error');
                setMessage('Invalid callback parameters');
            }
        };

        handleCallback();
    }, [searchParams, router]);

    return (
        <div className="flex min-h-screen items-center justify-center bg-background p-4">
            <Card className="w-full max-w-md">
                <CardHeader className="text-center">
                    <div className="mx-auto mb-4">
                        {status === 'loading' && (
                            <FontAwesomeIcon icon={faSpinner} className="h-12 w-12 text-primary animate-spin" />
                        )}
                        {status === 'success' && (
                            <FontAwesomeIcon icon={faCheckCircle} className="h-12 w-12 text-green-500" />
                        )}
                        {status === 'error' && (
                            <FontAwesomeIcon icon={faExclamationCircle} className="h-12 w-12 text-red-500" />
                        )}
                    </div>
                    <CardTitle className="text-2xl">
                        {status === 'loading' && 'Signing In'}
                        {status === 'success' && 'Welcome!'}
                        {status === 'error' && 'Sign In Failed'}
                    </CardTitle>
                    <CardDescription>{message}</CardDescription>
                </CardHeader>
                {status === 'error' && (
                    <CardContent className="text-center">
                        <Button onClick={() => router.push('/auth/signin')} variant="outline">
                            Back to Sign In
                        </Button>
                    </CardContent>
                )}
            </Card>
        </div>
    );
}

export default function CallbackPage() {
    return (
        <Suspense fallback={
            <div className="flex min-h-screen items-center justify-center">
                <FontAwesomeIcon icon={faSpinner} className="h-8 w-8 animate-spin" />
            </div>
        }>
            <CallbackContent />
        </Suspense>
    );
}
