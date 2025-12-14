'use client';

import { useState, useEffect, useCallback } from 'react';
import { RefreshCw, Search, Filter, Eye, Calendar, User, Server } from 'lucide-react';
import Link from 'next/link';

import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
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

import { auditService } from '@/services/audit.service';
import { AuditLog, AuditAction, AuditActionLabels, PagedAuditLogs } from '@/types/audit';
import { formatRelativeTime } from '@/utils';

function getActionBadgeVariant(action: AuditAction): "default" | "secondary" | "destructive" | "outline" {
    switch (action) {
        case AuditAction.Created:
            return "default";
        case AuditAction.Updated:
            return "secondary";
        case AuditAction.Deleted:
        case AuditAction.SoftDeleted:
            return "destructive";
        case AuditAction.Restored:
            return "outline";
        default:
            return "secondary";
    }
}

function TableSkeleton() {
    return (
        <TableRow>
            <TableCell><Skeleton className="h-4 w-24" /></TableCell>
            <TableCell><Skeleton className="h-4 w-20" /></TableCell>
            <TableCell><Skeleton className="h-4 w-16" /></TableCell>
            <TableCell><Skeleton className="h-4 w-24" /></TableCell>
            <TableCell><Skeleton className="h-4 w-20" /></TableCell>
            <TableCell><Skeleton className="h-4 w-28" /></TableCell>
            <TableCell><Skeleton className="h-8 w-8" /></TableCell>
        </TableRow>
    );
}

export default function AdminAuditLogsPage() {
    const [auditLogs, setAuditLogs] = useState<PagedAuditLogs | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');
    const [entityTypeFilter, setEntityTypeFilter] = useState<string>('all');
    const [actionFilter, setActionFilter] = useState<string>('all');
    const [page, setPage] = useState(1);
    const [selectedLog, setSelectedLog] = useState<AuditLog | null>(null);
    const [isDetailsOpen, setIsDetailsOpen] = useState(false);

    const fetchAuditLogs = useCallback(async () => {
        setIsLoading(true);
        try {
            const data = await auditService.getAuditLogs({
                page,
                pageSize: 20,
                entityType: entityTypeFilter !== 'all' ? entityTypeFilter : undefined,
                action: actionFilter !== 'all' ? Number(actionFilter) as AuditAction : undefined,
            });
            setAuditLogs(data);
        } catch {
            toast.error('Failed to load audit logs');
        } finally {
            setIsLoading(false);
        }
    }, [page, entityTypeFilter, actionFilter]);

    useEffect(() => {
        fetchAuditLogs();
    }, [fetchAuditLogs]);

    const filteredLogs = auditLogs?.items.filter(log => {
        if (!searchTerm) return true;
        const search = searchTerm.toLowerCase();
        return (
            log.entityType.toLowerCase().includes(search) ||
            log.entityId.toLowerCase().includes(search) ||
            log.username?.toLowerCase().includes(search) ||
            log.serviceName.toLowerCase().includes(search)
        );
    }) ?? [];

    const handleViewDetails = (log: AuditLog) => {
        setSelectedLog(log);
        setIsDetailsOpen(true);
    };

    const parseJsonSafe = (json: string | undefined): Record<string, unknown> | null => {
        if (!json) return null;
        try {
            return JSON.parse(json);
        } catch {
            return null;
        }
    };

    return (
        <AdminLayout>
            <div className="p-6 lg:p-8 space-y-6">
                <Breadcrumb>
                    <BreadcrumbList>
                        <BreadcrumbItem>
                            <BreadcrumbLink asChild>
                                <Link href="/admin">Admin</Link>
                            </BreadcrumbLink>
                        </BreadcrumbItem>
                        <BreadcrumbSeparator />
                        <BreadcrumbItem>
                            <BreadcrumbPage>Audit Logs</BreadcrumbPage>
                        </BreadcrumbItem>
                    </BreadcrumbList>
                </Breadcrumb>

                <div className="flex items-center justify-between">
                    <div>
                        <h1 className="text-3xl font-bold tracking-tight">Audit Logs</h1>
                        <p className="text-muted-foreground">
                            Track all system changes and activities
                        </p>
                    </div>
                    <Button
                        variant="outline"
                        size="icon"
                        onClick={fetchAuditLogs}
                        disabled={isLoading}
                    >
                        <RefreshCw className={`h-4 w-4 ${isLoading ? 'animate-spin' : ''}`} />
                        <span className="sr-only">Refresh</span>
                    </Button>
                </div>

                <Card>
                    <CardHeader>
                        <CardTitle className="text-lg">Filters</CardTitle>
                    </CardHeader>
                    <CardContent>
                        <div className="flex flex-wrap gap-4">
                            <div className="flex-1 min-w-[200px]">
                                <div className="relative">
                                    <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                                    <Input
                                        placeholder="Search by entity, user, service..."
                                        value={searchTerm}
                                        onChange={(e) => setSearchTerm(e.target.value)}
                                        className="pl-10"
                                    />
                                </div>
                            </div>
                            <Select value={entityTypeFilter} onValueChange={setEntityTypeFilter}>
                                <SelectTrigger className="w-[180px]">
                                    <SelectValue placeholder="Entity Type" />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="all">All Entity Types</SelectItem>
                                    <SelectItem value="Auction">Auction</SelectItem>
                                    <SelectItem value="Bid">Bid</SelectItem>
                                    <SelectItem value="Category">Category</SelectItem>
                                    <SelectItem value="User">User</SelectItem>
                                    <SelectItem value="Report">Report</SelectItem>
                                    <SelectItem value="Payment">Payment</SelectItem>
                                </SelectContent>
                            </Select>
                            <Select value={actionFilter} onValueChange={setActionFilter}>
                                <SelectTrigger className="w-[150px]">
                                    <SelectValue placeholder="Action" />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="all">All Actions</SelectItem>
                                    <SelectItem value="1">Created</SelectItem>
                                    <SelectItem value="2">Updated</SelectItem>
                                    <SelectItem value="3">Deleted</SelectItem>
                                    <SelectItem value="4">Soft Deleted</SelectItem>
                                    <SelectItem value="5">Restored</SelectItem>
                                </SelectContent>
                            </Select>
                        </div>
                    </CardContent>
                </Card>

                <Card>
                    <CardHeader>
                        <CardTitle>Activity Log</CardTitle>
                        <CardDescription>
                            {auditLogs ? `${auditLogs.totalCount} total records` : 'Loading...'}
                        </CardDescription>
                    </CardHeader>
                    <CardContent>
                        <Table>
                            <TableHeader>
                                <TableRow>
                                    <TableHead>Entity Type</TableHead>
                                    <TableHead>Entity ID</TableHead>
                                    <TableHead>Action</TableHead>
                                    <TableHead>User</TableHead>
                                    <TableHead>Service</TableHead>
                                    <TableHead>Timestamp</TableHead>
                                    <TableHead className="w-[50px]"></TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                {isLoading ? (
                                    <>
                                        <TableSkeleton />
                                        <TableSkeleton />
                                        <TableSkeleton />
                                        <TableSkeleton />
                                        <TableSkeleton />
                                    </>
                                ) : filteredLogs.length === 0 ? (
                                    <TableRow>
                                        <TableCell colSpan={7} className="text-center py-8 text-muted-foreground">
                                            No audit logs found
                                        </TableCell>
                                    </TableRow>
                                ) : (
                                    filteredLogs.map((log) => (
                                        <TableRow key={log.id}>
                                            <TableCell className="font-medium">{log.entityType}</TableCell>
                                            <TableCell className="font-mono text-xs">
                                                {log.entityId.slice(0, 8)}...
                                            </TableCell>
                                            <TableCell>
                                                <Badge variant={getActionBadgeVariant(log.action)}>
                                                    {AuditActionLabels[log.action] || log.actionName}
                                                </Badge>
                                            </TableCell>
                                            <TableCell>{log.username || 'System'}</TableCell>
                                            <TableCell>
                                                <Badge variant="outline">{log.serviceName}</Badge>
                                            </TableCell>
                                            <TableCell className="text-muted-foreground">
                                                {formatRelativeTime(log.timestamp)}
                                            </TableCell>
                                            <TableCell>
                                                <Button
                                                    variant="ghost"
                                                    size="icon"
                                                    onClick={() => handleViewDetails(log)}
                                                >
                                                    <Eye className="h-4 w-4" />
                                                </Button>
                                            </TableCell>
                                        </TableRow>
                                    ))
                                )}
                            </TableBody>
                        </Table>

                        {auditLogs && auditLogs.totalPages > 1 && (
                            <div className="flex items-center justify-between mt-4">
                                <p className="text-sm text-muted-foreground">
                                    Page {auditLogs.page} of {auditLogs.totalPages}
                                </p>
                                <div className="flex gap-2">
                                    <Button
                                        variant="outline"
                                        size="sm"
                                        disabled={page === 1}
                                        onClick={() => setPage(p => p - 1)}
                                    >
                                        Previous
                                    </Button>
                                    <Button
                                        variant="outline"
                                        size="sm"
                                        disabled={page === auditLogs.totalPages}
                                        onClick={() => setPage(p => p + 1)}
                                    >
                                        Next
                                    </Button>
                                </div>
                            </div>
                        )}
                    </CardContent>
                </Card>

                <Dialog open={isDetailsOpen} onOpenChange={setIsDetailsOpen}>
                    <DialogContent className="max-w-2xl max-h-[80vh] overflow-y-auto">
                        <DialogHeader>
                            <DialogTitle>Audit Log Details</DialogTitle>
                            <DialogDescription>
                                Complete information about this activity
                            </DialogDescription>
                        </DialogHeader>
                        {selectedLog && (
                            <div className="space-y-4">
                                <div className="grid grid-cols-2 gap-4">
                                    <div className="space-y-1">
                                        <p className="text-sm font-medium text-muted-foreground">Entity Type</p>
                                        <p className="font-medium">{selectedLog.entityType}</p>
                                    </div>
                                    <div className="space-y-1">
                                        <p className="text-sm font-medium text-muted-foreground">Entity ID</p>
                                        <p className="font-mono text-sm">{selectedLog.entityId}</p>
                                    </div>
                                    <div className="space-y-1">
                                        <p className="text-sm font-medium text-muted-foreground">Action</p>
                                        <Badge variant={getActionBadgeVariant(selectedLog.action)}>
                                            {AuditActionLabels[selectedLog.action] || selectedLog.actionName}
                                        </Badge>
                                    </div>
                                    <div className="space-y-1">
                                        <p className="text-sm font-medium text-muted-foreground">User</p>
                                        <div className="flex items-center gap-2">
                                            <User className="h-4 w-4" />
                                            {selectedLog.username || 'System'}
                                        </div>
                                    </div>
                                    <div className="space-y-1">
                                        <p className="text-sm font-medium text-muted-foreground">Service</p>
                                        <div className="flex items-center gap-2">
                                            <Server className="h-4 w-4" />
                                            {selectedLog.serviceName}
                                        </div>
                                    </div>
                                    <div className="space-y-1">
                                        <p className="text-sm font-medium text-muted-foreground">Timestamp</p>
                                        <div className="flex items-center gap-2">
                                            <Calendar className="h-4 w-4" />
                                            {new Date(selectedLog.timestamp).toLocaleString()}
                                        </div>
                                    </div>
                                </div>

                                {selectedLog.ipAddress && (
                                    <div className="space-y-1">
                                        <p className="text-sm font-medium text-muted-foreground">IP Address</p>
                                        <p className="font-mono text-sm">{selectedLog.ipAddress}</p>
                                    </div>
                                )}

                                {selectedLog.correlationId && (
                                    <div className="space-y-1">
                                        <p className="text-sm font-medium text-muted-foreground">Correlation ID</p>
                                        <p className="font-mono text-sm">{selectedLog.correlationId}</p>
                                    </div>
                                )}

                                {selectedLog.changedProperties && selectedLog.changedProperties.length > 0 && (
                                    <div className="space-y-1">
                                        <p className="text-sm font-medium text-muted-foreground">Changed Properties</p>
                                        <div className="flex flex-wrap gap-2">
                                            {selectedLog.changedProperties.map((prop) => (
                                                <Badge key={prop} variant="secondary">{prop}</Badge>
                                            ))}
                                        </div>
                                    </div>
                                )}

                                {selectedLog.oldValues && (
                                    <div className="space-y-1">
                                        <p className="text-sm font-medium text-muted-foreground">Old Values</p>
                                        <pre className="bg-muted p-3 rounded-lg text-xs overflow-x-auto">
                                            {JSON.stringify(parseJsonSafe(selectedLog.oldValues), null, 2)}
                                        </pre>
                                    </div>
                                )}

                                {selectedLog.newValues && (
                                    <div className="space-y-1">
                                        <p className="text-sm font-medium text-muted-foreground">New Values</p>
                                        <pre className="bg-muted p-3 rounded-lg text-xs overflow-x-auto">
                                            {JSON.stringify(parseJsonSafe(selectedLog.newValues), null, 2)}
                                        </pre>
                                    </div>
                                )}
                            </div>
                        )}
                    </DialogContent>
                </Dialog>
            </div>
        </AdminLayout>
    );
}
