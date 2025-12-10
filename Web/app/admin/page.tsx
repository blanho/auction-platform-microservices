'use client';

import Link from 'next/link';
import { useSession } from 'next-auth/react';
import {
    Gavel,
    Users,
    Settings,
    ChevronRight
} from 'lucide-react';

import { MainLayout } from '@/components/layout/main-layout';
import { RequireAdmin } from '@/components/auth/require-admin';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import {
    Breadcrumb,
    BreadcrumbItem,
    BreadcrumbLink,
    BreadcrumbList,
    BreadcrumbPage,
    BreadcrumbSeparator,
} from '@/components/ui/breadcrumb';

const adminLinks = [
    {
        title: 'Auctions Management',
        description: 'Create, edit, delete, import and export auctions',
        href: '/admin/auctions',
        icon: Gavel,
    },
    {
        title: 'Users Management',
        description: 'Manage user accounts and permissions',
        href: '/admin/users',
        icon: Users,
        disabled: true,
    },
    {
        title: 'Settings',
        description: 'Configure system settings',
        href: '/admin/settings',
        icon: Settings,
        disabled: true,
    },
];

function AdminDashboardContent() {
    const { data: session } = useSession();

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
                            <BreadcrumbPage>Admin Dashboard</BreadcrumbPage>
                        </BreadcrumbItem>
                    </BreadcrumbList>
                </Breadcrumb>

                <div className="mb-8">
                    <h1 className="text-3xl font-bold">Admin Dashboard</h1>
                    <p className="text-muted-foreground">
                        Welcome back, {session?.user?.name}
                    </p>
                </div>

                <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
                    {adminLinks.map((link) => (
                        <Link
                            key={link.href}
                            href={link.disabled ? '#' : link.href}
                            className={link.disabled ? 'cursor-not-allowed' : ''}
                        >
                            <Card className={`h-full transition-colors ${link.disabled
                                    ? 'opacity-50'
                                    : 'hover:bg-accent hover:text-accent-foreground'
                                }`}>
                                <CardHeader>
                                    <div className="flex items-center justify-between">
                                        <link.icon className="h-8 w-8 text-primary" />
                                        {!link.disabled && (
                                            <ChevronRight className="h-5 w-5 text-muted-foreground" />
                                        )}
                                    </div>
                                    <CardTitle className="mt-4">{link.title}</CardTitle>
                                    <CardDescription>{link.description}</CardDescription>
                                </CardHeader>
                                {link.disabled && (
                                    <CardContent>
                                        <span className="text-xs text-muted-foreground">
                                            Coming soon
                                        </span>
                                    </CardContent>
                                )}
                            </Card>
                        </Link>
                    ))}
                </div>
            </div>
        </MainLayout>
    );
}

export default function AdminPage() {
    return (
        <RequireAdmin>
            <AdminDashboardContent />
        </RequireAdmin>
    );
}
