"use client";

import { useState, useEffect, useCallback } from "react";
import {
    Search,
    Flag,
    AlertTriangle,
    CheckCircle,
    XCircle,
    Eye,
    MessageSquare,
    MoreHorizontal,
    Loader2,
    RefreshCw,
} from "lucide-react";

import { PAGINATION, MESSAGES } from "@/constants";
import { formatRelativeTime, formatNumber } from "@/utils";

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

import { reportService, Report } from "@/services/report.service";

const REPORT_STATUS_COLORS: Record<string, string> = {
    Pending: "bg-yellow-500/10 text-yellow-600",
    UnderReview: "bg-blue-500/10 text-blue-600",
    Resolved: "bg-green-500/10 text-green-600",
    Dismissed: "bg-zinc-500/10 text-zinc-600",
};

const PRIORITY_COLORS: Record<string, string> = {
    Critical: "bg-red-600/10 text-red-700",
    High: "bg-red-500/10 text-red-600",
    Medium: "bg-amber-500/10 text-amber-600",
    Low: "bg-green-500/10 text-green-600",
};

function getTypeIcon(type: string) {
    switch (type.toLowerCase()) {
        case "fraud":
            return <AlertTriangle className="h-4 w-4 text-red-500" />;
        case "counterfeit":
            return <Flag className="h-4 w-4 text-amber-500" />;
        case "nondelivery":
            return <XCircle className="h-4 w-4 text-blue-500" />;
        default:
            return <Eye className="h-4 w-4 text-purple-500" />;
    }
}

function getInitials(name: string): string {
    return name
        .split(/[\s@]+/)
        .map((n) => n[0])
        .join("")
        .toUpperCase()
        .slice(0, 2);
}

interface Stats {
    total: number;
    pending: number;
    review: number;
    resolved: number;
}

function StatCard({
    title,
    value,
    colorClass,
    isActive,
    onClick,
    isLoading,
}: {
    title: string;
    value: number;
    colorClass?: string;
    isActive: boolean;
    onClick: () => void;
    isLoading: boolean;
}) {
    return (
        <Card
            className={`cursor-pointer transition-colors ${isActive ? "border-amber-500" : "hover:border-zinc-300"}`}
            onClick={onClick}
        >
            <CardContent className="pt-6">
                {isLoading ? (
                    <Skeleton className="h-8 w-16 mb-1" />
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

function ReportTableRow({
    report,
    onResolve,
    onDismiss,
}: {
    report: Report;
    onResolve: (report: Report) => void;
    onDismiss: (report: Report) => void;
}) {
    const statusKey = report.status.replace(/\s+/g, "");
    
    return (
        <TableRow>
            <TableCell>
                <div className="flex items-center gap-2">
                    {getTypeIcon(report.type)}
                    <span className="font-medium">{report.type}</span>
                </div>
            </TableCell>
            <TableCell>
                <div>
                    <p className="font-medium">{report.reason}</p>
                    <p className="text-xs text-zinc-500 truncate max-w-[200px]">
                        {report.description || "No description"}
                    </p>
                </div>
            </TableCell>
            <TableCell>
                <div className="flex items-center gap-2">
                    <Avatar className="h-8 w-8">
                        <AvatarFallback className="text-xs">
                            {getInitials(report.reportedUsername)}
                        </AvatarFallback>
                    </Avatar>
                    <span>{report.reportedUsername}</span>
                </div>
            </TableCell>
            <TableCell>
                <Badge className={PRIORITY_COLORS[report.priority] || PRIORITY_COLORS.Medium}>
                    {report.priority}
                </Badge>
            </TableCell>
            <TableCell>
                <Badge className={REPORT_STATUS_COLORS[statusKey] || REPORT_STATUS_COLORS.Pending}>
                    {report.status}
                </Badge>
            </TableCell>
            <TableCell className="text-zinc-500">
                {formatRelativeTime(report.createdAt)}
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
                        <DropdownMenuItem>
                            <MessageSquare className="h-4 w-4 mr-2" />
                            Contact Reporter
                        </DropdownMenuItem>
                        <DropdownMenuSeparator />
                        <DropdownMenuItem
                            className="text-green-600"
                            onClick={() => onResolve(report)}
                        >
                            <CheckCircle className="h-4 w-4 mr-2" />
                            Mark Resolved
                        </DropdownMenuItem>
                        <DropdownMenuItem
                            className="text-red-600"
                            onClick={() => onDismiss(report)}
                        >
                            <XCircle className="h-4 w-4 mr-2" />
                            Dismiss Report
                        </DropdownMenuItem>
                    </DropdownMenuContent>
                </DropdownMenu>
            </TableCell>
        </TableRow>
    );
}

export default function AdminReportsPage() {
    const [reports, setReports] = useState<Report[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState("");
    const [statusFilter, setStatusFilter] = useState("all");
    const [activeTab, setActiveTab] = useState("all");
    const [totalCount, setTotalCount] = useState(0);
    const [stats, setStats] = useState<Stats>({ total: 0, pending: 0, review: 0, resolved: 0 });

    const fetchReports = useCallback(async () => {
        setIsLoading(true);
        try {
            const statusParam = activeTab === "pending" ? "Pending" :
                               activeTab === "review" ? "UnderReview" :
                               activeTab === "resolved" ? "Resolved" :
                               statusFilter !== "all" ? statusFilter : undefined;

            const response = await reportService.getReports({
                status: statusParam,
                reportedUsername: searchTerm || undefined,
                pageSize: PAGINATION.DEFAULT_PAGE_SIZE,
            });
            
            setReports(response.reports);
            setTotalCount(response.totalCount);
        } catch {
            toast.error(MESSAGES.ERROR.GENERIC);
        } finally {
            setIsLoading(false);
        }
    }, [activeTab, statusFilter, searchTerm]);

    const fetchStats = useCallback(async () => {
        try {
            const [allReports, pendingReports, reviewReports, resolvedReports] = await Promise.all([
                reportService.getReports({ pageSize: 1 }),
                reportService.getReports({ status: "Pending", pageSize: 1 }),
                reportService.getReports({ status: "UnderReview", pageSize: 1 }),
                reportService.getReports({ status: "Resolved", pageSize: 1 }),
            ]);
            
            setStats({
                total: allReports.totalCount,
                pending: pendingReports.totalCount,
                review: reviewReports.totalCount,
                resolved: resolvedReports.totalCount,
            });
        } catch {
        }
    }, []);

    useEffect(() => {
        fetchReports();
        fetchStats();
    }, [fetchReports, fetchStats]);

    const handleResolve = async (report: Report) => {
        try {
            await reportService.updateReportStatus(report.id, {
                status: "Resolved",
                resolution: "Resolved by admin",
            });
            toast.success(MESSAGES.SUCCESS.REPORT_RESOLVED);
            fetchReports();
            fetchStats();
        } catch {
            toast.error(MESSAGES.ERROR.GENERIC);
        }
    };

    const handleDismiss = async (report: Report) => {
        try {
            await reportService.updateReportStatus(report.id, {
                status: "Dismissed",
                resolution: "Dismissed by admin",
            });
            toast.success("Report dismissed");
            fetchReports();
            fetchStats();
        } catch {
            toast.error(MESSAGES.ERROR.GENERIC);
        }
    };

    const isStatsLoading = stats.total === 0 && isLoading;

    return (
        <AdminLayout
            title="Reports"
            description="Review and manage fraud reports"
        >
            {/* Stats */}
            <div className="grid gap-4 md:grid-cols-4 mb-6">
                <StatCard
                    title="Total Reports"
                    value={stats.total}
                    isActive={activeTab === "all"}
                    onClick={() => setActiveTab("all")}
                    isLoading={isStatsLoading}
                />
                <StatCard
                    title="Pending Review"
                    value={stats.pending}
                    colorClass="text-yellow-500"
                    isActive={activeTab === "pending"}
                    onClick={() => setActiveTab("pending")}
                    isLoading={isStatsLoading}
                />
                <StatCard
                    title="Under Review"
                    value={stats.review}
                    colorClass="text-blue-500"
                    isActive={activeTab === "review"}
                    onClick={() => setActiveTab("review")}
                    isLoading={isStatsLoading}
                />
                <StatCard
                    title="Resolved"
                    value={stats.resolved}
                    colorClass="text-green-500"
                    isActive={activeTab === "resolved"}
                    onClick={() => setActiveTab("resolved")}
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
                                placeholder="Search by reported user..."
                                value={searchTerm}
                                onChange={(e) => setSearchTerm(e.target.value)}
                                className="pl-9"
                            />
                        </div>
                        <Select value={statusFilter} onValueChange={setStatusFilter}>
                            <SelectTrigger className="w-[180px]">
                                <SelectValue placeholder="Status" />
                            </SelectTrigger>
                            <SelectContent>
                                <SelectItem value="all">All Status</SelectItem>
                                <SelectItem value="Pending">Pending</SelectItem>
                                <SelectItem value="UnderReview">Under Review</SelectItem>
                                <SelectItem value="Resolved">Resolved</SelectItem>
                                <SelectItem value="Dismissed">Dismissed</SelectItem>
                            </SelectContent>
                        </Select>
                        <Button
                            variant="outline"
                            size="icon"
                            onClick={() => fetchReports()}
                            disabled={isLoading}
                        >
                            <RefreshCw className={`h-4 w-4 ${isLoading ? "animate-spin" : ""}`} />
                        </Button>
                    </div>
                </CardContent>
            </Card>

            {/* Reports Table */}
            <Card>
                <CardHeader>
                    <CardTitle>Fraud Reports</CardTitle>
                    <CardDescription>
                        {formatNumber(totalCount)} reports found
                    </CardDescription>
                </CardHeader>
                <CardContent>
                    {isLoading ? (
                        <div className="flex justify-center py-8">
                            <Loader2 className="h-8 w-8 animate-spin text-zinc-400" />
                        </div>
                    ) : reports.length === 0 ? (
                        <div className="text-center py-8 text-zinc-500">
                            {MESSAGES.EMPTY.REPORTS}
                        </div>
                    ) : (
                        <Table>
                            <TableHeader>
                                <TableRow>
                                    <TableHead>Type</TableHead>
                                    <TableHead>Reason</TableHead>
                                    <TableHead>Reported User</TableHead>
                                    <TableHead>Priority</TableHead>
                                    <TableHead>Status</TableHead>
                                    <TableHead>Date</TableHead>
                                    <TableHead className="text-right">Actions</TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                {reports.map((report) => (
                                    <ReportTableRow
                                        key={report.id}
                                        report={report}
                                        onResolve={handleResolve}
                                        onDismiss={handleDismiss}
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
