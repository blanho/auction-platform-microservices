"use client";

import { useState, useEffect, useCallback } from "react";
import {
    Search,
    DollarSign,
    CheckCircle,
    XCircle,
    Clock,
    Eye,
    MoreHorizontal,
    ArrowUpRight,
    Loader2,
    RefreshCw,
} from "lucide-react";

import { MESSAGES } from "@/constants";
import { formatCurrency, formatRelativeTime, formatNumber } from "@/utils";

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
import { Skeleton } from "@/components/ui/skeleton";
import { toast } from "sonner";

import { adminPaymentService, AdminWithdrawal, PaymentStatistics } from "@/services/wallet.service";

const PAYMENT_STATUS_COLORS: Record<string, string> = {
    Pending: "bg-yellow-500/10 text-yellow-600",
    Approved: "bg-blue-500/10 text-blue-600",
    Completed: "bg-green-500/10 text-green-600",
    Rejected: "bg-red-500/10 text-red-600",
};

function getInitials(name: string): string {
    return name
        .split(/[\s@]+/)
        .map((n) => n[0])
        .join("")
        .toUpperCase()
        .slice(0, 2);
}

function StatCard({
    icon: Icon,
    iconColor,
    label,
    value,
    subtext,
    isLoading,
}: {
    icon: React.ElementType;
    iconColor: string;
    label: string;
    value: string | number;
    subtext: string;
    isLoading: boolean;
}) {
    return (
        <Card>
            <CardContent className="pt-6">
                <div className="flex items-center gap-2">
                    <Icon className={`h-5 w-5 ${iconColor}`} />
                    <span className="text-sm text-zinc-500">{label}</span>
                </div>
                {isLoading ? (
                    <Skeleton className="h-8 w-24 mt-2" />
                ) : (
                    <div className="text-2xl font-bold mt-2">{value}</div>
                )}
                <p className="text-sm text-zinc-500">{subtext}</p>
            </CardContent>
        </Card>
    );
}

function WithdrawalTableRow({
    withdrawal,
    onApprove,
    onReject,
}: {
    withdrawal: AdminWithdrawal;
    onApprove: (withdrawal: AdminWithdrawal) => void;
    onReject: (withdrawal: AdminWithdrawal) => void;
}) {
    const isPending = withdrawal.status === "Pending";
    
    return (
        <TableRow>
            <TableCell>
                <div className="flex items-center gap-3">
                    <Avatar className="h-8 w-8">
                        <AvatarFallback className="text-xs">
                            {getInitials(withdrawal.username)}
                        </AvatarFallback>
                    </Avatar>
                    <div>
                        <p className="font-medium">{withdrawal.username}</p>
                    </div>
                </div>
            </TableCell>
            <TableCell>
                <div className="flex items-center gap-2">
                    <ArrowUpRight className="h-4 w-4 text-blue-500" />
                    <span>Withdrawal</span>
                </div>
            </TableCell>
            <TableCell>
                <span className="font-semibold">
                    {formatCurrency(withdrawal.amount)}
                </span>
            </TableCell>
            <TableCell>
                <p className="text-sm">{withdrawal.paymentMethod || "Bank Transfer"}</p>
            </TableCell>
            <TableCell>
                <Badge className={PAYMENT_STATUS_COLORS[withdrawal.status] || PAYMENT_STATUS_COLORS.Pending}>
                    {withdrawal.status}
                </Badge>
            </TableCell>
            <TableCell className="text-zinc-500">
                {formatRelativeTime(withdrawal.requestedAt)}
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
                            View Details
                        </DropdownMenuItem>
                        {isPending && (
                            <>
                                <DropdownMenuSeparator />
                                <DropdownMenuItem
                                    className="text-green-600"
                                    onClick={() => onApprove(withdrawal)}
                                >
                                    <CheckCircle className="h-4 w-4 mr-2" />
                                    Approve
                                </DropdownMenuItem>
                                <DropdownMenuItem
                                    className="text-red-600"
                                    onClick={() => onReject(withdrawal)}
                                >
                                    <XCircle className="h-4 w-4 mr-2" />
                                    Reject
                                </DropdownMenuItem>
                            </>
                        )}
                    </DropdownMenuContent>
                </DropdownMenu>
            </TableCell>
        </TableRow>
    );
}

export default function AdminPaymentsPage() {
    const [withdrawals, setWithdrawals] = useState<AdminWithdrawal[]>([]);
    const [statistics, setStatistics] = useState<PaymentStatistics | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState("");

    const fetchWithdrawals = useCallback(async () => {
        setIsLoading(true);
        try {
            const data = await adminPaymentService.getPendingWithdrawals();
            setWithdrawals(data);
        } catch {
            toast.error(MESSAGES.ERROR.GENERIC);
        } finally {
            setIsLoading(false);
        }
    }, []);

    const fetchStatistics = useCallback(async () => {
        try {
            const stats = await adminPaymentService.getStatistics();
            setStatistics(stats);
        } catch {
        }
    }, []);

    useEffect(() => {
        fetchWithdrawals();
        fetchStatistics();
    }, [fetchWithdrawals, fetchStatistics]);

    const handleApprove = async (withdrawal: AdminWithdrawal) => {
        try {
            await adminPaymentService.approveWithdrawal(withdrawal.id);
            toast.success(MESSAGES.SUCCESS.WITHDRAWAL_APPROVED);
            fetchWithdrawals();
            fetchStatistics();
        } catch {
            toast.error(MESSAGES.ERROR.GENERIC);
        }
    };

    const handleReject = async (withdrawal: AdminWithdrawal) => {
        try {
            await adminPaymentService.rejectWithdrawal(withdrawal.id, "Rejected by admin");
            toast.success(MESSAGES.SUCCESS.WITHDRAWAL_REJECTED);
            fetchWithdrawals();
            fetchStatistics();
        } catch {
            toast.error(MESSAGES.ERROR.GENERIC);
        }
    };

    const filteredWithdrawals = withdrawals.filter((w) =>
        w.username.toLowerCase().includes(searchTerm.toLowerCase())
    );

    const isStatsLoading = statistics === null;

    return (
        <AdminLayout
            title="Payments"
            description="Manage withdrawal requests and payments"
        >
            {/* Stats */}
            <div className="grid gap-4 md:grid-cols-3 mb-6">
                <StatCard
                    icon={Clock}
                    iconColor="text-yellow-500"
                    label="Pending Withdrawals"
                    value={formatCurrency(statistics?.pendingWithdrawalsTotal || 0)}
                    subtext={`${formatNumber(statistics?.pendingWithdrawalsCount || 0)} requests`}
                    isLoading={isStatsLoading}
                />
                <StatCard
                    icon={ArrowUpRight}
                    iconColor="text-green-500"
                    label="Pending Count"
                    value={formatNumber(statistics?.pendingWithdrawalsCount || 0)}
                    subtext="Awaiting approval"
                    isLoading={isStatsLoading}
                />
                <StatCard
                    icon={DollarSign}
                    iconColor="text-amber-500"
                    label="Pending Amount"
                    value={formatCurrency(statistics?.pendingWithdrawalsTotal || 0)}
                    subtext="Total pending"
                    isLoading={isStatsLoading}
                />
            </div>

            {/* Filters */}
            <Card className="mb-6">
                <CardContent className="pt-6">
                    <div className="flex flex-col sm:flex-row gap-4">
                        <div className="relative flex-1">
                            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-zinc-500" />
                            <Input
                                placeholder="Search by username..."
                                value={searchTerm}
                                onChange={(e) => setSearchTerm(e.target.value)}
                                className="pl-9"
                            />
                        </div>
                        <Button
                            variant="outline"
                            size="icon"
                            onClick={() => fetchWithdrawals()}
                            disabled={isLoading}
                        >
                            <RefreshCw className={`h-4 w-4 ${isLoading ? "animate-spin" : ""}`} />
                        </Button>
                    </div>
                </CardContent>
            </Card>

            {/* Withdrawals Table */}
            <Card>
                <CardHeader>
                    <CardTitle>Pending Withdrawals</CardTitle>
                    <CardDescription>
                        {formatNumber(filteredWithdrawals.length)} pending requests
                    </CardDescription>
                </CardHeader>
                <CardContent>
                    {isLoading ? (
                        <div className="flex justify-center py-8">
                            <Loader2 className="h-8 w-8 animate-spin text-zinc-400" />
                        </div>
                    ) : filteredWithdrawals.length === 0 ? (
                        <div className="text-center py-8 text-zinc-500">
                            No pending withdrawals
                        </div>
                    ) : (
                        <Table>
                            <TableHeader>
                                <TableRow>
                                    <TableHead>User</TableHead>
                                    <TableHead>Type</TableHead>
                                    <TableHead>Amount</TableHead>
                                    <TableHead>Method</TableHead>
                                    <TableHead>Status</TableHead>
                                    <TableHead>Requested</TableHead>
                                    <TableHead className="text-right">Actions</TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                {filteredWithdrawals.map((withdrawal) => (
                                    <WithdrawalTableRow
                                        key={withdrawal.id}
                                        withdrawal={withdrawal}
                                        onApprove={handleApprove}
                                        onReject={handleReject}
                                    />
                                ))}
                            </TableBody>
                        </Table>
                    )}
                </CardContent>
            </Card>
        </AdminLayout>
    );
}
