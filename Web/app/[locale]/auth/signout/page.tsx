"use client";

import { useEffect } from "react";
import { signOut } from "next-auth/react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Loader2 } from "lucide-react";

export default function SignOutPage() {
    useEffect(() => {
        signOut({ callbackUrl: "/" });
    }, []);

    return (
        <div className="flex min-h-screen items-center justify-center bg-background p-4">
            <Card className="w-full max-w-md">
                <CardHeader className="space-y-1">
                    <CardTitle className="text-2xl font-bold text-center">Signing Out</CardTitle>
                </CardHeader>
                <CardContent className="flex justify-center py-6">
                    <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
                </CardContent>
            </Card>
        </div>
    );
}
