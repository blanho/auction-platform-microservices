// Footer component
'use client';

import Link from 'next/link';

export function Footer() {
    return (
        <footer className="border-t py-6 md:py-0">
            <div className="container mx-auto flex flex-col items-center justify-between gap-4 px-4 md:h-16 md:flex-row md:px-6 lg:px-8">
                <p className="text-center text-sm leading-loose text-muted-foreground md:text-left">
                    © {new Date().getFullYear()} AuctionHub. Built with Next.js & .NET Microservices.
                </p>
                <div className="flex items-center gap-4">
                    <Link
                        href="/about"
                        className="text-sm text-muted-foreground hover:text-primary"
                    >
                        About
                    </Link>
                    <Link
                        href="/terms"
                        className="text-sm text-muted-foreground hover:text-primary"
                    >
                        Terms
                    </Link>
                    <Link
                        href="/privacy"
                        className="text-sm text-muted-foreground hover:text-primary"
                    >
                        Privacy
                    </Link>
                </div>
            </div>
        </footer>
    );
}
