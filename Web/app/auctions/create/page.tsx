'use client';

import Link from 'next/link';
import { MainLayout } from '@/components/layout/main-layout';
import { CreateAuctionForm } from '@/features/auction/create-auction-form';
import { RequireAuth } from '@/features/auth';
import {
    Breadcrumb,
    BreadcrumbItem,
    BreadcrumbLink,
    BreadcrumbList,
    BreadcrumbPage,
    BreadcrumbSeparator,
} from '@/components/ui/breadcrumb';

export default function CreateAuctionPage() {
    return (
        <RequireAuth>
            <MainLayout>
                <div className="container py-8 max-w-4xl mx-auto">
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
                                    <Link href="/auctions">Auctions</Link>
                                </BreadcrumbLink>
                            </BreadcrumbItem>
                            <BreadcrumbSeparator />
                            <BreadcrumbItem>
                                <BreadcrumbPage>Create</BreadcrumbPage>
                            </BreadcrumbItem>
                        </BreadcrumbList>
                    </Breadcrumb>
                    <CreateAuctionForm />
                </div>
            </MainLayout>
        </RequireAuth>
    );
}
