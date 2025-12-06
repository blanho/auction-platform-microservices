'use client';

import { useState, useEffect, useCallback } from 'react';
import { Plus, Loader2, Upload, Download, RefreshCw } from 'lucide-react';
import Link from 'next/link';

import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { MainLayout } from '@/components/layout/main-layout';
import { RequireAdmin } from '@/components/auth/require-admin';
import { AuctionDataTable } from '@/features/auction/auction-data-table';
import { ImportAuctionsDialog } from '@/features/auction/import-auctions-dialog';
import { ExportAuctionsDialog } from '@/features/auction/export-auctions-dialog';
import {
    Breadcrumb,
    BreadcrumbItem,
    BreadcrumbLink,
    BreadcrumbList,
    BreadcrumbPage,
    BreadcrumbSeparator,
} from '@/components/ui/breadcrumb';

import apiClient from '@/lib/api/axios';
import { Auction } from '@/types/auction';
import { ApiResponse } from '@/types';
import { API_ENDPOINTS } from '@/constants/api';

function AdminAuctionsContent() {
    const [auctions, setAuctions] = useState<Auction[] | null>(null);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<unknown>(null);

    const fetchAuctions = useCallback(async () => {
        setIsLoading(true);
        try {
            const { data } = await apiClient.get<ApiResponse<Auction[]>>(
                API_ENDPOINTS.AUCTIONS
            );
            setAuctions(data.data);
            setError(null);
        } catch (err) {
            setError(err);
        } finally {
            setIsLoading(false);
        }
    }, []);

    useEffect(() => {
        fetchAuctions();
    }, [fetchAuctions]);

    return (
        <MainLayout>
            <div className="container py-8">
                <Breadcrumb className="mb-6">
                    <BreadcrumbList>
                        <BreadcrumbItem>
                            <BreadcrumbLink asChild>
                                <Link href="/">Home</Link>
                            </BreadcrumbLink>
                        </BreadcrumbItem>
                        <BreadcrumbSeparator />
                        <BreadcrumbItem>
                            <BreadcrumbLink asChild>
                                <Link href="/admin">Admin</Link>
                            </BreadcrumbLink>
                        </BreadcrumbItem>
                        <BreadcrumbSeparator />
                        <BreadcrumbItem>
                            <BreadcrumbPage>Auctions Management</BreadcrumbPage>
                        </BreadcrumbItem>
                    </BreadcrumbList>
                </Breadcrumb>

                <div className="mb-8 flex items-center justify-between">
                    <div>
                        <h1 className="text-3xl font-bold">Auctions Management</h1>
                        <p className="text-muted-foreground">
                            Manage all auctions in the system
                        </p>
                    </div>
                    <div className="flex items-center gap-2">
                        <Button
                            variant="outline"
                            size="icon"
                            onClick={fetchAuctions}
                            disabled={isLoading}
                        >
                            <RefreshCw className={`h-4 w-4 ${isLoading ? 'animate-spin' : ''}`} />
                            <span className="sr-only">Refresh</span>
                        </Button>
                        <ImportAuctionsDialog
                            onSuccess={fetchAuctions}
                            trigger={
                                <Button variant="outline">
                                    <Upload className="mr-2 h-4 w-4" />
                                    Import
                                </Button>
                            }
                        />
                        <ExportAuctionsDialog
                            trigger={
                                <Button variant="outline">
                                    <Download className="mr-2 h-4 w-4" />
                                    Export
                                </Button>
                            }
                        />
                        <Button asChild>
                            <Link href="/auctions/create">
                                <Plus className="mr-2 h-4 w-4" />
                                Create Auction
                            </Link>
                        </Button>
                    </div>
                </div>

                {isLoading && !auctions && (
                    <div className="flex justify-center py-12">
                        <Loader2 className="h-8 w-8 animate-spin" />
                    </div>
                )}

                {!!error && (
                    <Alert variant="destructive">
                        <AlertDescription>
                            Failed to load auctions. Please try again.
                        </AlertDescription>
                    </Alert>
                )}

                {!error && auctions && (
                    <>
                        {auctions.length === 0 ? (
                            <Card>
                                <CardHeader>
                                    <CardTitle>No auctions yet</CardTitle>
                                </CardHeader>
                                <CardContent>
                                    <p className="text-muted-foreground mb-4">
                                        There are no auctions in the system. Create the first auction to get started!
                                    </p>
                                    <Button asChild>
                                        <Link href="/auctions/create">
                                            <Plus className="mr-2 h-4 w-4" />
                                            Create First Auction
                                        </Link>
                                    </Button>
                                </CardContent>
                            </Card>
                        ) : (
                            <AuctionDataTable 
                                data={auctions} 
                                onActionComplete={fetchAuctions}
                            />
                        )}
                    </>
                )}
            </div>
        </MainLayout>
    );
}

export default function AdminAuctionsPage() {
    return (
        <RequireAdmin>
            <AdminAuctionsContent />
        </RequireAdmin>
    );
}
