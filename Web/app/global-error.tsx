"use client";

import { Button } from "@/components/ui/button";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faCircleExclamation, faRotate } from "@fortawesome/free-solid-svg-icons";

interface GlobalErrorPageProps {
    error: Error & { digest?: string };
    reset: () => void;
}

export default function GlobalErrorPage({ error, reset }: GlobalErrorPageProps) {
    return (
        <html lang="en">
            <body className="min-h-screen flex items-center justify-center bg-slate-950 px-4">
                <div className="max-w-md w-full text-center space-y-6">
                    <div className="flex justify-center">
                        <div className="w-20 h-20 rounded-full bg-red-900/20 flex items-center justify-center">
                            <FontAwesomeIcon icon={faCircleExclamation} className="w-10 h-10 text-red-400" />
                        </div>
                    </div>

                    <div className="space-y-2">
                        <h1 className="text-2xl font-bold text-white">
                            Critical Error
                        </h1>
                        <p className="text-slate-400">
                            A critical error occurred. Please refresh the page or try again later.
                        </p>
                    </div>

                    <Button
                        onClick={reset}
                        className="bg-white text-slate-900 hover:bg-slate-100"
                    >
                        <FontAwesomeIcon icon={faRotate} className="w-4 h-4 mr-2" />
                        Try Again
                    </Button>

                    {error.digest && (
                        <p className="text-xs text-slate-500">
                            Error ID: {error.digest}
                        </p>
                    )}
                </div>
            </body>
        </html>
    );
}
