'use client';

import { useState, useEffect, useCallback } from 'react';
import { RefreshCw, Search, Filter, Send, Bell, Users, CheckCircle, Clock } from 'lucide-react';
import Link from 'next/link';

import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from '@/components/ui/table';
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from '@/components/ui/select';
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogHeader,
    DialogTitle,
    DialogFooter,
} from '@/components/ui/dialog';
import {
    Breadcrumb,
    BreadcrumbItem,
    BreadcrumbLink,
    BreadcrumbList,
    BreadcrumbPage,
    BreadcrumbSeparator,
} from '@/components/ui/breadcrumb';
import { AdminLayout } from '@/components/layout/admin-layout';
import { toast } from 'sonner';

import { notificationService } from '@/services/notification.service';
import { 
    Notification, 
    PagedNotifications, 
    NotificationStats, 
    NotificationType, 
    NotificationStatus,
    BroadcastNotificationDto 
} from '@/types/notification';
import { formatRelativeTime } from '@/utils';

function getStatusBadgeVariant(status: string): "default" | "secondary" | "destructive" | "outline" {
    switch (status) {
        case NotificationStatus.Unread:
            return "default";
        case NotificationStatus.Read:
            return "secondary";
        case NotificationStatus.Dismissed:
            return "outline";
        default:
            return "secondary";
    }
}

function getTypeBadgeVariant(type: string): "default" | "secondary" | "destructive" | "outline" {
    if (type.includes('Bid') || type.includes('Won')) return "default";
    if (type.includes('Auction')) return "secondary";
    if (type.includes('Broadcast') || type.includes('System')) return "destructive";
    return "outline";
}

function TableSkeleton() {
    return (
        <TableRow>
            <TableCell><Skeleton className="h-4 w-24" /></TableCell>
            <TableCell><Skeleton className="h-4 w-20" /></TableCell>
            <TableCell><Skeleton className="h-4 w-32" /></TableCell>
            <TableCell><Skeleton className="h-4 w-40" /></TableCell>
            <TableCell><Skeleton className="h-4 w-16" /></TableCell>
            <TableCell><Skeleton className="h-4 w-24" /></TableCell>
        </TableRow>
    );
}

function StatCard({ title, value, icon: Icon, description }: { 
    title: string; 
    value: number | string; 
    icon: React.ElementType;
    description?: string;
}) {
    return (
        <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">{title}</CardTitle>
                <Icon className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
                <div className="text-2xl font-bold">{value}</div>
                {description && (
                    <p className="text-xs text-muted-foreground">{description}</p>
                )}
            </CardContent>
        </Card>
    );
}

export default function AdminNotificationsPage() {
    const [notifications, setNotifications] = useState<PagedNotifications | null>(null);
    const [stats, setStats] = useState<NotificationStats | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');
    const [typeFilter, setTypeFilter] = useState<string>('all');
    const [statusFilter, setStatusFilter] = useState<string>('all');
    const [page, setPage] = useState(1);
    const [isBroadcastOpen, setIsBroadcastOpen] = useState(false);
    const [isSending, setIsSending] = useState(false);
    const [broadcastForm, setBroadcastForm] = useState<BroadcastNotificationDto>({
        type: NotificationType.Broadcast,
        title: '',
        message: '',
        targetRole: undefined
    });

    const fetchNotifications = useCallback(async () => {
        setIsLoading(true);
        try {
            const data = await notificationService.getAllNotifications({
                pageNumber: page,
                pageSize: 20,
                type: typeFilter !== 'all' ? typeFilter : undefined,
                status: statusFilter !== 'all' ? statusFilter : undefined,
            });
            setNotifications(data);
        } catch {
            toast.error('Failed to load notifications');
        } finally {
            setIsLoading(false);
        }
    }, [page, typeFilter, statusFilter]);

    const fetchStats = useCallback(async () => {
        try {
            const data = await notificationService.getNotificationStats();
            setStats(data);
        } catch {
            console.error('Failed to load notification stats');
        }
    }, []);

    useEffect(() => {
        fetchNotifications();
        fetchStats();
    }, [fetchNotifications, fetchStats]);

    const handleBroadcast = async () => {
        if (!broadcastForm.title || !broadcastForm.message) {
            toast.error('Please fill in all required fields');
            return;
        }

        setIsSending(true);
        try {
            await notificationService.broadcastNotification(broadcastForm);
            toast.success('Notification broadcast successfully');
            setIsBroadcastOpen(false);
            setBroadcastForm({
                type: NotificationType.Broadcast,
                title: '',
                message: '',
                targetRole: undefined
            });
            fetchNotifications();
            fetchStats();
        } catch {
            toast.error('Failed to broadcast notification');
        } finally {
            setIsSending(false);
        }
    };

    const filteredNotifications = notifications?.items.filter(notification => {
        if (!searchTerm) return true;
        const search = searchTerm.toLowerCase();
        return (
            notification.title.toLowerCase().includes(search) ||
            notification.message.toLowerCase().includes(search) ||
            notification.userId.toLowerCase().includes(search)
        );
    }) ?? [];

    return (
        <AdminLayout>
            <div className="flex-1 space-y-6 p-6">
                <Breadcrumb>
                    <BreadcrumbList>
                        <BreadcrumbItem>
                            <BreadcrumbLink asChild>
                                <Link href="/admin">Admin</Link>
                            </BreadcrumbLink>
                        </BreadcrumbItem>
                        <BreadcrumbSeparator />
                        <BreadcrumbItem>
                            <BreadcrumbPage>Notifications</BreadcrumbPage>
                        </BreadcrumbItem>
                    </BreadcrumbList>
                </Breadcrumb>

                <div className="flex items-center justify-between">
                    <div>
                        <h1 className="text-3xl font-bold tracking-tight">Notification Management</h1>
                        <p className="text-muted-foreground">
                            View all notifications and broadcast messages to users
                        </p>
                    </div>
                    <div className="flex gap-2">
                        <Button 
                            variant="outline" 
                            onClick={() => { fetchNotifications(); fetchStats(); }}
                            disabled={isLoading}
                        >
                            <RefreshCw className={`mr-2 h-4 w-4 ${isLoading ? 'animate-spin' : ''}`} />
                            Refresh
                        </Button>
                        <Button onClick={() => setIsBroadcastOpen(true)}>
                            <Send className="mr-2 h-4 w-4" />
                            Broadcast
                        </Button>
                    </div>
                </div>

                <div className="grid gap-4 md:grid-cols-4">
                    <StatCard 
                        title="Total Notifications" 
                        value={stats?.totalNotifications ?? 0} 
                        icon={Bell}
                    />
                    <StatCard 
                        title="Unread" 
                        value={stats?.unreadNotifications ?? 0} 
                        icon={Clock}
                        description="Pending notifications"
                    />
                    <StatCard 
                        title="Today" 
                        value={stats?.todayCount ?? 0} 
                        icon={CheckCircle}
                        description="Sent today"
                    />
                    <StatCard 
                        title="Types" 
                        value={Object.keys(stats?.byType ?? {}).length} 
                        icon={Users}
                        description="Notification categories"
                    />
                </div>

                <Card>
                    <CardHeader>
                        <CardTitle>All Notifications</CardTitle>
                        <CardDescription>
                            View and manage all system notifications
                        </CardDescription>
                    </CardHeader>
                    <CardContent>
                        <div className="flex flex-col sm:flex-row gap-4 mb-6">
                            <div className="relative flex-1">
                                <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                                <Input
                                    placeholder="Search notifications..."
                                    value={searchTerm}
                                    onChange={(e) => setSearchTerm(e.target.value)}
                                    className="pl-9"
                                />
                            </div>
                            <div className="flex gap-2">
                                <Select value={typeFilter} onValueChange={setTypeFilter}>
                                    <SelectTrigger className="w-[150px]">
                                        <Filter className="mr-2 h-4 w-4" />
                                        <SelectValue placeholder="Type" />
                                    </SelectTrigger>
                                    <SelectContent>
                                        <SelectItem value="all">All Types</SelectItem>
                                        {Object.values(NotificationType).map((type) => (
                                            <SelectItem key={type} value={type}>{type}</SelectItem>
                                        ))}
                                    </SelectContent>
                                </Select>
                                <Select value={statusFilter} onValueChange={setStatusFilter}>
                                    <SelectTrigger className="w-[150px]">
                                        <SelectValue placeholder="Status" />
                                    </SelectTrigger>
                                    <SelectContent>
                                        <SelectItem value="all">All Status</SelectItem>
                                        {Object.values(NotificationStatus).map((status) => (
                                            <SelectItem key={status} value={status}>{status}</SelectItem>
                                        ))}
                                    </SelectContent>
                                </Select>
                            </div>
                        </div>

                        <div className="rounded-md border">
                            <Table>
                                <TableHeader>
                                    <TableRow>
                                        <TableHead>User</TableHead>
                                        <TableHead>Type</TableHead>
                                        <TableHead>Title</TableHead>
                                        <TableHead>Message</TableHead>
                                        <TableHead>Status</TableHead>
                                        <TableHead>Created</TableHead>
                                    </TableRow>
                                </TableHeader>
                                <TableBody>
                                    {isLoading ? (
                                        Array.from({ length: 5 }).map((_, i) => (
                                            <TableSkeleton key={i} />
                                        ))
                                    ) : filteredNotifications.length === 0 ? (
                                        <TableRow>
                                            <TableCell colSpan={6} className="h-24 text-center">
                                                No notifications found
                                            </TableCell>
                                        </TableRow>
                                    ) : (
                                        filteredNotifications.map((notification) => (
                                            <TableRow key={notification.id}>
                                                <TableCell className="font-mono text-xs">
                                                    {notification.userId === 'broadcast' 
                                                        ? <Badge variant="outline">Broadcast</Badge>
                                                        : notification.userId.slice(0, 8) + '...'
                                                    }
                                                </TableCell>
                                                <TableCell>
                                                    <Badge variant={getTypeBadgeVariant(notification.type)}>
                                                        {notification.type}
                                                    </Badge>
                                                </TableCell>
                                                <TableCell className="font-medium max-w-[200px] truncate">
                                                    {notification.title}
                                                </TableCell>
                                                <TableCell className="max-w-[300px] truncate text-muted-foreground">
                                                    {notification.message}
                                                </TableCell>
                                                <TableCell>
                                                    <Badge variant={getStatusBadgeVariant(notification.status)}>
                                                        {notification.status}
                                                    </Badge>
                                                </TableCell>
                                                <TableCell className="text-muted-foreground">
                                                    {formatRelativeTime(notification.createdAt)}
                                                </TableCell>
                                            </TableRow>
                                        ))
                                    )}
                                </TableBody>
                            </Table>
                        </div>

                        {notifications && notifications.totalPages > 1 && (
                            <div className="flex items-center justify-between mt-4">
                                <p className="text-sm text-muted-foreground">
                                    Page {notifications.pageNumber} of {notifications.totalPages} ({notifications.totalCount} total)
                                </p>
                                <div className="flex gap-2">
                                    <Button
                                        variant="outline"
                                        size="sm"
                                        onClick={() => setPage(p => Math.max(1, p - 1))}
                                        disabled={page === 1}
                                    >
                                        Previous
                                    </Button>
                                    <Button
                                        variant="outline"
                                        size="sm"
                                        onClick={() => setPage(p => p + 1)}
                                        disabled={page >= notifications.totalPages}
                                    >
                                        Next
                                    </Button>
                                </div>
                            </div>
                        )}
                    </CardContent>
                </Card>

                <Dialog open={isBroadcastOpen} onOpenChange={setIsBroadcastOpen}>
                    <DialogContent className="sm:max-w-[500px]">
                        <DialogHeader>
                            <DialogTitle>Broadcast Notification</DialogTitle>
                            <DialogDescription>
                                Send a notification to all users or a specific role
                            </DialogDescription>
                        </DialogHeader>
                        <div className="grid gap-4 py-4">
                            <div className="grid gap-2">
                                <Label htmlFor="type">Notification Type</Label>
                                <Select 
                                    value={broadcastForm.type}
                                    onValueChange={(value) => setBroadcastForm(prev => ({ 
                                        ...prev, 
                                        type: value as NotificationType 
                                    }))}
                                >
                                    <SelectTrigger>
                                        <SelectValue />
                                    </SelectTrigger>
                                    <SelectContent>
                                        <SelectItem value={NotificationType.Broadcast}>Broadcast</SelectItem>
                                        <SelectItem value={NotificationType.SystemNotification}>System Notification</SelectItem>
                                    </SelectContent>
                                </Select>
                            </div>
                            <div className="grid gap-2">
                                <Label htmlFor="title">Title *</Label>
                                <Input
                                    id="title"
                                    value={broadcastForm.title}
                                    onChange={(e) => setBroadcastForm(prev => ({ ...prev, title: e.target.value }))}
                                    placeholder="Notification title"
                                />
                            </div>
                            <div className="grid gap-2">
                                <Label htmlFor="message">Message *</Label>
                                <Textarea
                                    id="message"
                                    value={broadcastForm.message}
                                    onChange={(e) => setBroadcastForm(prev => ({ ...prev, message: e.target.value }))}
                                    placeholder="Notification message"
                                    rows={3}
                                />
                            </div>
                            <div className="grid gap-2">
                                <Label htmlFor="role">Target Role (Optional)</Label>
                                <Select 
                                    value={broadcastForm.targetRole ?? 'all'}
                                    onValueChange={(value) => setBroadcastForm(prev => ({ 
                                        ...prev, 
                                        targetRole: value === 'all' ? undefined : value
                                    }))}
                                >
                                    <SelectTrigger>
                                        <SelectValue placeholder="All users" />
                                    </SelectTrigger>
                                    <SelectContent>
                                        <SelectItem value="all">All Users</SelectItem>
                                        <SelectItem value="admin">Admins Only</SelectItem>
                                        <SelectItem value="user">Regular Users Only</SelectItem>
                                    </SelectContent>
                                </Select>
                            </div>
                        </div>
                        <DialogFooter>
                            <Button variant="outline" onClick={() => setIsBroadcastOpen(false)}>
                                Cancel
                            </Button>
                            <Button onClick={handleBroadcast} disabled={isSending}>
                                {isSending ? (
                                    <>
                                        <RefreshCw className="mr-2 h-4 w-4 animate-spin" />
                                        Sending...
                                    </>
                                ) : (
                                    <>
                                        <Send className="mr-2 h-4 w-4" />
                                        Send Broadcast
                                    </>
                                )}
                            </Button>
                        </DialogFooter>
                    </DialogContent>
                </Dialog>
            </div>
        </AdminLayout>
    );
}
