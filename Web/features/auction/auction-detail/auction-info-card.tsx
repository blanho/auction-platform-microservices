"use client";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
    faTachometerAlt,
    faPalette,
    faCalendarAlt,
    faTag,
    faFire,
} from "@fortawesome/free-solid-svg-icons";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

interface AuctionInfoCardProps {
    title: string;
    subtitle?: string;
    isFeatured?: boolean;
    yearManufactured?: number | null;
    categoryName?: string;
    attributes?: Record<string, string>;
}

export function AuctionInfoCard({
    title,
    subtitle,
    isFeatured,
    yearManufactured,
    categoryName,
    attributes = {},
}: AuctionInfoCardProps) {
    return (
        <Card className="border-0 shadow-lg bg-white/80 dark:bg-slate-900/80 backdrop-blur-xl rounded-2xl">
            <CardHeader className="pb-2">
                <div className="flex items-start justify-between">
                    <div>
                        <CardTitle className="text-2xl font-bold bg-linear-to-r from-slate-900 to-slate-600 dark:from-white dark:to-slate-300 bg-clip-text text-transparent">
                            {title}
                        </CardTitle>
                        {subtitle && <CardDescription className="text-base mt-1">{subtitle}</CardDescription>}
                    </div>
                    {isFeatured && (
                        <Badge className="bg-linear-to-r from-amber-500 to-orange-500 text-white border-0">
                            <FontAwesomeIcon icon={faFire} className="w-3 h-3 mr-1" />
                            Featured
                        </Badge>
                    )}
                </div>
            </CardHeader>
            <CardContent>
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                    {attributes.mileage && (
                        <div className="flex items-center gap-3 p-3 rounded-xl bg-slate-50 dark:bg-slate-800/50">
                            <div className="w-10 h-10 rounded-full bg-linear-to-br from-blue-500 to-cyan-500 flex items-center justify-center">
                                <FontAwesomeIcon icon={faTachometerAlt} className="w-4 h-4 text-white" />
                            </div>
                            <div>
                                <p className="text-xs text-slate-500">Mileage</p>
                                <p className="font-semibold text-slate-900 dark:text-white">{attributes.mileage}</p>
                            </div>
                        </div>
                    )}
                    {attributes.color && (
                        <div className="flex items-center gap-3 p-3 rounded-xl bg-slate-50 dark:bg-slate-800/50">
                            <div className="w-10 h-10 rounded-full bg-linear-to-br from-purple-500 to-pink-500 flex items-center justify-center">
                                <FontAwesomeIcon icon={faPalette} className="w-4 h-4 text-white" />
                            </div>
                            <div>
                                <p className="text-xs text-slate-500">Color</p>
                                <p className="font-semibold text-slate-900 dark:text-white">{attributes.color}</p>
                            </div>
                        </div>
                    )}
                    <div className="flex items-center gap-3 p-3 rounded-xl bg-slate-50 dark:bg-slate-800/50">
                        <div className="w-10 h-10 rounded-full bg-linear-to-br from-emerald-500 to-green-500 flex items-center justify-center">
                            <FontAwesomeIcon icon={faCalendarAlt} className="w-4 h-4 text-white" />
                        </div>
                        <div>
                            <p className="text-xs text-slate-500">Year</p>
                            <p className="font-semibold text-slate-900 dark:text-white">{yearManufactured || "N/A"}</p>
                        </div>
                    </div>
                    {categoryName && (
                        <div className="flex items-center gap-3 p-3 rounded-xl bg-slate-50 dark:bg-slate-800/50">
                            <div className="w-10 h-10 rounded-full bg-linear-to-br from-amber-500 to-orange-500 flex items-center justify-center">
                                <FontAwesomeIcon icon={faTag} className="w-4 h-4 text-white" />
                            </div>
                            <div>
                                <p className="text-xs text-slate-500">Category</p>
                                <p className="font-semibold text-slate-900 dark:text-white">{categoryName}</p>
                            </div>
                        </div>
                    )}
                </div>
            </CardContent>
        </Card>
    );
}
