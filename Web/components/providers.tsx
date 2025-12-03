'use client';
import { ReactNode, useState } from 'react';
import { SessionProvider } from 'next-auth/react';
import { QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import { queryClient } from '@/lib/api/query-client';
import { Toaster } from '@/components/ui/sonner';

interface ProvidersProps {
    children: ReactNode;
}

export function Providers({ children }: ProvidersProps) {
    const [client] = useState(() => queryClient);

    return (
        <SessionProvider>
            <QueryClientProvider client={client}>
                {children}
                <Toaster position="top-right" richColors />
                {process.env.NEXT_PUBLIC_ENABLE_DEVTOOLS === 'true' && (
                    <ReactQueryDevtools initialIsOpen={false} />
                )}
            </QueryClientProvider>
        </SessionProvider>
    );
}
