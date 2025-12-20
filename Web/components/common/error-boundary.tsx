"use client";

import { Component, ReactNode } from "react";
import { Button } from "@/components/ui/button";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faExclamationTriangle, faRotateRight, faHome } from "@fortawesome/free-solid-svg-icons";
import Link from "next/link";

interface ErrorBoundaryProps {
    children: ReactNode;
    fallback?: ReactNode;
    onError?: (error: Error, errorInfo: React.ErrorInfo) => void;
}

interface ErrorBoundaryState {
    hasError: boolean;
    error: Error | null;
}

export class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
    constructor(props: ErrorBoundaryProps) {
        super(props);
        this.state = { hasError: false, error: null };
    }

    static getDerivedStateFromError(error: Error): ErrorBoundaryState {
        return { hasError: true, error };
    }

    componentDidCatch(error: Error, errorInfo: React.ErrorInfo): void {
        console.error("ErrorBoundary caught an error:", error, errorInfo);
        this.props.onError?.(error, errorInfo);
    }

    handleReset = (): void => {
        this.setState({ hasError: false, error: null });
    };

    render(): ReactNode {
        if (this.state.hasError) {
            if (this.props.fallback) {
                return this.props.fallback;
            }

            return (
                <div className="min-h-[400px] flex items-center justify-center p-8">
                    <div className="text-center max-w-md">
                        <div className="w-16 h-16 mx-auto mb-6 rounded-full bg-red-100 dark:bg-red-900/30 flex items-center justify-center">
                            <FontAwesomeIcon 
                                icon={faExclamationTriangle} 
                                className="w-8 h-8 text-red-600 dark:text-red-400" 
                            />
                        </div>
                        <h2 className="text-2xl font-bold text-slate-900 dark:text-white mb-2">
                            Something went wrong
                        </h2>
                        <p className="text-slate-600 dark:text-slate-400 mb-6">
                            We encountered an unexpected error. Please try again or return to the homepage.
                        </p>
                        {process.env.NODE_ENV === "development" && this.state.error && (
                            <pre className="mb-6 p-4 bg-slate-100 dark:bg-slate-800 rounded-lg text-left text-xs overflow-auto max-h-32">
                                {this.state.error.message}
                            </pre>
                        )}
                        <div className="flex gap-3 justify-center">
                            <Button
                                variant="outline"
                                onClick={this.handleReset}
                                className="gap-2"
                            >
                                <FontAwesomeIcon icon={faRotateRight} className="w-4 h-4" />
                                Try Again
                            </Button>
                            <Button asChild className="gap-2">
                                <Link href="/">
                                    <FontAwesomeIcon icon={faHome} className="w-4 h-4" />
                                    Go Home
                                </Link>
                            </Button>
                        </div>
                    </div>
                </div>
            );
        }

        return this.props.children;
    }
}

interface RouteErrorBoundaryProps {
    children: ReactNode;
}

export function RouteErrorBoundary({ children }: RouteErrorBoundaryProps) {
    return (
        <ErrorBoundary
            onError={(error, errorInfo) => {
                if (process.env.NODE_ENV === "production") {
                    console.error("Route error:", { error: error.message, componentStack: errorInfo.componentStack });
                }
            }}
        >
            {children}
        </ErrorBoundary>
    );
}
