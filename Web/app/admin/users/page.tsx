"use client";

import { useState, useEffect, useCallback } from "react";
import {
    Search,
    MoreHorizontal,
    UserCheck,
    UserX,
    Mail,
    Shield,
    Eye,
    Loader2,
    RefreshCw,
} from "lucide-react";

import { PAGINATION, MESSAGES } from "@/constants";
import { formatNumber, formatRelativeTime } from "@/utils";

import { AdminLayout } from "@/components/layout/admin-layout";
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuSeparator,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";
import { Skeleton } from "@/components/ui/skeleton";
import { toast } from "sonner";

import { adminService, AdminUser, AdminStats } from "@/services/admin.service";

const USER_STATUS_COLORS: Record<string, string> = {
    active: "bg-green-500/10 text-green-600",
    suspended: "bg-red-500/10 text-red-600",
    inactive: "bg-zinc-500/10 text-zinc-600",
};

const USER_ROLE_COLORS: Record<string, string> = {
    Admin: "bg-purple-500/10 text-purple-600",
    Seller: "bg-blue-500/10 text-blue-600",
    Buyer: "bg-zinc-500/10 text-zinc-600",
};

function getUserStatus(user: AdminUser): string {
    if (user.isSuspended) return "suspended";
    if (user.isActive) return "active";
    return "inactive";
}

function getInitials(name: string): string {
    return name
        .split(" ")
        .map((n) => n[0])
        .join("")
        .toUpperCase()
        .slice(0, 2);
}

function StatCard({ 
    title, 
    value, 
    colorClass,
    isLoading 
}: { 
    title: string; 
    value: number; 
    colorClass?: string;
    isLoading: boolean;
}) {
    return (
        <Card>
            <CardContent className="pt-6">
                {isLoading ? (
                    <Skeleton className="h-8 w-20 mb-1" />
                ) : (
                    <div className={`text-2xl font-bold ${colorClass || ""}`}>
                        {formatNumber(value)}
                    </div>
                )}
                <p className="text-sm text-zinc-500">{title}</p>
            </CardContent>
        </Card>
    );
}

function UserTableRow({ 
    user, 
    onSuspend, 
    onActivate 
}: { 
    user: AdminUser; 
    onSuspend: (user: AdminUser) => void;
    onActivate: (user: AdminUser) => void;
}) {
    const status = getUserStatus(user);
    const primaryRole = user.roles[0] || "User";
    
    return (
        <TableRow>
            <TableCell>
                <div className="flex items-center gap-3">
                    <Avatar>
                        <AvatarFallback>
                            {getInitials(user.fullName || user.username)}
                        </AvatarFallback>
                    </Avatar>
                    <div>
                        <p className="font-medium">{user.fullName || user.username}</p>
                        <p className="text-sm text-zinc-500">{user.email}</p>
                    </div>
                </div>
            </TableCell>
            <TableCell>
                <Badge className={USER_ROLE_COLORS[primaryRole] || USER_ROLE_COLORS.Buyer}>
                    {primaryRole}
                </Badge>
            </TableCell>
            <TableCell>
                <Badge className={USER_STATUS_COLORS[status]}>
                    {status.charAt(0).toUpperCase() + status.slice(1)}
                </Badge>
            </TableCell>
            <TableCell className="text-zinc-500">
                {formatRelativeTime(user.createdAt)}
            </TableCell>
            <TableCell className="text-zinc-500 text-sm">
                {user.lastLoginAt 
                    ? formatRelativeTime(user.lastLoginAt)
                    : "Never"
                }
            </TableCell>
            <TableCell className="text-right">
                <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                        <Button variant="ghost" size="icon">
                            <MoreHorizontal className="h-4 w-4" />
                        </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end">
                        <DropdownMenuItem>
                            <Eye className="h-4 w-4 mr-2" />
                            View Profile
                        </DropdownMenuItem>
                        <DropdownMenuItem>
                            <Mail className="h-4 w-4 mr-2" />
                            Send Email
                        </DropdownMenuItem>
                        <DropdownMenuSeparator />
                        {user.isActive && !user.isSuspended ? (
                            <DropdownMenuItem 
                                className="text-red-600"
                                onClick={() => onSuspend(user)}
                            >
                                <UserX className="h-4 w-4 mr-2" />
                                Suspend User
                            </DropdownMenuItem>
                        ) : (
                            <DropdownMenuItem 
                                className="text-green-600"
                                onClick={() => onActivate(user)}
                            >
                                <UserCheck className="h-4 w-4 mr-2" />
                                Activate User
                            </DropdownMenuItem>
                        )}
                        {!user.roles.includes("Admin") && (
                            <DropdownMenuItem>
                                <Shield className="h-4 w-4 mr-2" />
                                Make Admin
                            </DropdownMenuItem>
                        )}
                    </DropdownMenuContent>
                </DropdownMenu>
            </TableCell>
        </TableRow>
    );
}

export default function AdminUsersPage() {
    const [users, setUsers] = useState<AdminUser[]>([]);
    const [stats, setStats] = useState<AdminStats | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState("");
    const [roleFilter, setRoleFilter] = useState("all");
    const [statusFilter, setStatusFilter] = useState("all");
    const [totalCount, setTotalCount] = useState(0);

    const fetchUsers = useCallback(async () => {
        setIsLoading(true);
        try {
            const isActiveFilter = statusFilter === "active" ? true : 
                                   statusFilter === "inactive" ? false : undefined;
            const isSuspendedFilter = statusFilter === "suspended" ? true : undefined;
            
            const response = await adminService.getUsers({
                search: searchTerm || undefined,
                role: roleFilter !== "all" ? roleFilter : undefined,
                isActive: isActiveFilter,
                isSuspended: isSuspendedFilter,
                pageSize: PAGINATION.DEFAULT_PAGE_SIZE,
            });
            setUsers(response.users);
            setTotalCount(response.totalCount);
        } catch {
            toast.error(MESSAGES.ERROR.GENERIC);
        } finally {
            setIsLoading(false);
        }
    }, [searchTerm, roleFilter, statusFilter]);

    const fetchStats = useCallback(async () => {
        try {
            const statsData = await adminService.getStats();
            setStats(statsData);
        } catch {
        }
    }, []);

    useEffect(() => {
        fetchUsers();
        fetchStats();
    }, [fetchUsers, fetchStats]);

    const handleSuspendUser = async (user: AdminUser) => {
        try {
            await adminService.suspendUser(user.id, "Suspended by admin");
            toast.success(`User ${user.username} suspended`);
            fetchUsers();
            fetchStats();
        } catch {
            toast.error(MESSAGES.ERROR.GENERIC);
        }
    };

    const handleActivateUser = async (user: AdminUser) => {
        try {
            await adminService.activateUser(user.id);
            toast.success(`User ${user.username} activated`);
            fetchUsers();
            fetchStats();
        } catch {
            toast.error(MESSAGES.ERROR.GENERIC);
        }
    };

    const isStatsLoading = stats === null;

    return (
        <AdminLayout>
            <div className="p-6 lg:p-8 space-y-6">
                <div className="flex items-center justify-between">
                    <div>
                        <h1 className="text-3xl font-bold tracking-tight">User Management</h1>
                        <p className="text-muted-foreground">View and manage platform users</p>
                    </div>
                    <Button 
                        variant="outline" 
                        size="icon"
                        onClick={() => fetchUsers()}
                        disabled={isLoading}
                    >
                        <RefreshCw className={`h-4 w-4 ${isLoading ? "animate-spin" : ""}`} />
                    </Button>
                </div>

                {/* Stats */}
                <div className="grid gap-4 md:grid-cols-4">
                    <StatCard 
                        title="Total Users" 
                        value={stats?.totalUsers || 0} 
                        isLoading={isStatsLoading}
                    />
                    <StatCard 
                        title="Active Users" 
                        value={stats?.activeUsers || 0} 
                        colorClass="text-green-500"
                        isLoading={isStatsLoading}
                    />
                    <StatCard 
                        title="New This Month" 
                        value={stats?.newUsersThisMonth || 0} 
                        colorClass="text-blue-500"
                        isLoading={isStatsLoading}
                    />
                    <StatCard 
                        title="Suspended" 
                        value={stats?.suspendedUsers || 0} 
                        colorClass="text-red-500"
                        isLoading={isStatsLoading}
                    />
                </div>

                {/* Filters */}
                <Card>
                    <CardContent className="pt-6">
                        <div className="flex flex-col sm:flex-row gap-4">
                            <div className="relative flex-1">
                                <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-zinc-500" />
                                <Input
                                    placeholder="Search users by name or email..."
                                    value={searchTerm}
                                    onChange={(e) => setSearchTerm(e.target.value)}
                                    className="pl-9"
                                />
                            </div>
                            <Select value={roleFilter} onValueChange={setRoleFilter}>
                                <SelectTrigger className="w-[150px]">
                                    <SelectValue placeholder="Role" />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="all">All Roles</SelectItem>
                                    <SelectItem value="Admin">Admin</SelectItem>
                                    <SelectItem value="Seller">Seller</SelectItem>
                                    <SelectItem value="Buyer">Buyer</SelectItem>
                                </SelectContent>
                            </Select>
                            <Select value={statusFilter} onValueChange={setStatusFilter}>
                                <SelectTrigger className="w-[150px]">
                                    <SelectValue placeholder="Status" />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="all">All Status</SelectItem>
                                    <SelectItem value="active">Active</SelectItem>
                                    <SelectItem value="inactive">Inactive</SelectItem>
                                    <SelectItem value="suspended">Suspended</SelectItem>
                                </SelectContent>
                            </Select>
                        </div>
                    </CardContent>
                </Card>

                {/* Users Table */}
                <Card>
                    <CardHeader>
                        <CardTitle>Users</CardTitle>
                        <CardDescription>
                            {formatNumber(totalCount)} users found
                        </CardDescription>
                    </CardHeader>
                    <CardContent>
                        {isLoading ? (
                            <div className="flex justify-center py-8">
                                <Loader2 className="h-8 w-8 animate-spin text-zinc-400" />
                            </div>
                        ) : users.length === 0 ? (
                            <div className="text-center py-8 text-zinc-500">
                                {MESSAGES.EMPTY.SEARCH_RESULTS}
                            </div>
                        ) : (
                            <Table>
                                <TableHeader>
                                    <TableRow>
                                        <TableHead>User</TableHead>
                                        <TableHead>Role</TableHead>
                                        <TableHead>Status</TableHead>
                                        <TableHead>Joined</TableHead>
                                        <TableHead>Last Active</TableHead>
                                        <TableHead className="text-right">Actions</TableHead>
                                    </TableRow>
                                </TableHeader>
                                <TableBody>
                                    {users.map((user) => (
                                        <UserTableRow 
                                            key={user.id} 
                                            user={user}
                                            onSuspend={handleSuspendUser}
                                            onActivate={handleActivateUser}
                                        />
                                    ))}
                                </TableBody>
                            </Table>
                        )}
                    </CardContent>
                </Card>
            </div>
        </AdminLayout>
    );
}
