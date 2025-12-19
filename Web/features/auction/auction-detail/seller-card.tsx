"use client";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
    faUser,
    faStar,
    faCheckCircle,
} from "@fortawesome/free-solid-svg-icons";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

interface SellerCardProps {
    sellerUsername: string;
    rating?: number;
    isVerified?: boolean;
}

export function SellerCard({
    sellerUsername,
    rating = 4.8,
    isVerified = true,
}: SellerCardProps) {
    return (
        <Card className="border-0 shadow-lg bg-white/80 dark:bg-slate-900/80 backdrop-blur-xl rounded-2xl">
            <CardHeader className="pb-3">
                <CardTitle className="text-sm font-semibold flex items-center gap-2">
                    <FontAwesomeIcon icon={faUser} className="w-4 h-4 text-purple-500" />
                    Seller
                </CardTitle>
            </CardHeader>
            <CardContent>
                <div className="flex items-center gap-4">
                    <div className="w-14 h-14 rounded-full bg-linear-to-br from-purple-500 to-blue-600 flex items-center justify-center text-white font-bold text-xl shadow-lg shadow-purple-500/25">
                        {sellerUsername.charAt(0).toUpperCase()}
                    </div>
                    <div>
                        <p className="font-semibold text-slate-900 dark:text-white">{sellerUsername}</p>
                        <div className="flex items-center gap-2 text-sm text-slate-500 mt-1">
                            <FontAwesomeIcon icon={faStar} className="w-3 h-3 text-amber-400" />
                            <span className="font-medium">{rating}</span>
                            {isVerified && (
                                <>
                                    <span className="text-slate-300 dark:text-slate-600">•</span>
                                    <span className="flex items-center gap-1 text-emerald-600 dark:text-emerald-400">
                                        <FontAwesomeIcon icon={faCheckCircle} className="w-3 h-3" />
                                        Verified
                                    </span>
                                </>
                            )}
                        </div>
                    </div>
                </div>
            </CardContent>
        </Card>
    );
}
