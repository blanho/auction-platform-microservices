import { Button } from "@/components/ui/button";
import { FileQuestion, Home, ArrowLeft } from "lucide-react";
import Link from "next/link";

export default function NotFoundPage() {
    return (
        <div className="min-h-screen flex items-center justify-center bg-zinc-50 dark:bg-zinc-950 px-4">
            <div className="max-w-md w-full text-center space-y-6">
                <div className="flex justify-center">
                    <div className="w-20 h-20 rounded-full bg-amber-100 dark:bg-amber-900/20 flex items-center justify-center">
                        <FileQuestion className="w-10 h-10 text-amber-600 dark:text-amber-400" />
                    </div>
                </div>

                <div className="space-y-2">
                    <h1 className="text-6xl font-bold text-zinc-900 dark:text-white">
                        404
                    </h1>
                    <h2 className="text-xl font-semibold text-zinc-800 dark:text-zinc-200">
                        Page Not Found
                    </h2>
                    <p className="text-zinc-600 dark:text-zinc-400">
                        The page you&apos;re looking for doesn&apos;t exist or has been moved.
                    </p>
                </div>

                <div className="flex flex-col sm:flex-row gap-3 justify-center">
                    <Button variant="default" asChild>
                        <Link href="/">
                            <Home className="w-4 h-4 mr-2" />
                            Go Home
                        </Link>
                    </Button>
                    <Button variant="outline" asChild>
                        <Link href="/auctions">
                            <ArrowLeft className="w-4 h-4 mr-2" />
                            Browse Auctions
                        </Link>
                    </Button>
                </div>
            </div>
        </div>
    );
}
