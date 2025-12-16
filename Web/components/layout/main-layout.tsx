'use client';

import { ReactNode } from 'react';
import { Header } from './header';
import { Footer } from './footer';
import { SkipLink } from './skip-link';

interface MainLayoutProps {
    children: ReactNode;
}

export function MainLayout({ children }: MainLayoutProps) {
    return (
        <div className="flex min-h-screen flex-col">
            <SkipLink />
            <Header />
            <main id="main-content" className="flex-1 container mx-auto px-4 py-6 md:px-6 lg:px-8">
                {children}
            </main>
            <Footer />
        </div>
    );
}
