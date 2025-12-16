"use client";

import { useEffect, useState, useCallback } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { Badge } from "@/components/ui/badge";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Skeleton } from "@/components/ui/skeleton";
import {
    Collapsible,
    CollapsibleContent,
    CollapsibleTrigger,
} from "@/components/ui/collapsible";
import { auditService } from "@/services/audit.service";
import {
    AuditLog,
    AuditAction,
    parseAuditChanges,
    ParsedAuditChanges
} from "@/types/audit";
import { formatDistanceToNow, format } from "date-fns";
import { cn } from "@/lib/utils";
import {
    History,
    Plus,
    Pencil,
    Trash2,
    RotateCcw,
    ChevronDown,
    User,
    Clock,
    ArrowRight
} from "lucide-react";

interface AuditHistoryProps {
    entityType: string;
    entityId: string;
    maxHeight?: string;
    showDetails?: boolean;
}

const DEFAULT_MAX_HEIGHT = "400px";
const SKELETON_COUNT = 3;

interface ActionConfig {
    label: string;
    icon: React.ComponentType<{ className?: string }>;
    className: string;
    bgClassName: string;
}

const ACTION_CONFIG: Record<AuditAction, ActionConfig> = {
    [AuditAction.Created]: {
        label: "Created",
        icon: Plus,
        className: "text-green-600",
        bgClassName: "bg-green-100 dark:bg-green-900"
    },
    [AuditAction.Updated]: {
        label: "Updated",
        icon: Pencil,
        className: "text-blue-600",
        bgClassName: "bg-blue-100 dark:bg-blue-900"
    },
    [AuditAction.Deleted]: {
        label: "Deleted",
        icon: Trash2,
        className: "text-red-600",
        bgClassName: "bg-red-100 dark:bg-red-900"
    },
    [AuditAction.SoftDeleted]: {
        label: "Deactivated",
        icon: Trash2,
        className: "text-orange-600",
        bgClassName: "bg-orange-100 dark:bg-orange-900"
    },
    [AuditAction.Restored]: {
        label: "Restored",
        icon: RotateCcw,
        className: "text-purple-600",
        bgClassName: "bg-purple-100 dark:bg-purple-900"
    }
};

function getActionConfig(action: AuditAction): ActionConfig {
    return ACTION_CONFIG[action] || ACTION_CONFIG[AuditAction.Updated];
}

function LoadingSkeleton() {
    return (
        <div className="space-y-3">
            {Array.from({ length: SKELETON_COUNT }).map((_, i) => (
                <div key={i} className="flex items-start gap-3 p-3 rounded-lg border">
                    <Skeleton className="h-10 w-10 rounded-full" />
                    <div className="flex-1 space-y-2">
                        <Skeleton className="h-4 w-32" />
                        <Skeleton className="h-3 w-48" />
                    </div>
                    <Skeleton className="h-6 w-20" />
                </div>
            ))}
        </div>
    );
}

interface ErrorStateProps {
    message: string;
}

function ErrorState({ message }: ErrorStateProps) {
    return (
        <div className="text-center py-8 text-red-500">
            <p>{message}</p>
        </div>
    );
}

function EmptyState() {
    return (
        <motion.div
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            className="text-center py-8 text-gray-500 dark:text-gray-400"
        >
            <motion.div
                initial={{ scale: 0.8 }}
                animate={{ scale: 1 }}
                transition={{ delay: 0.1 }}
            >
                <History className="h-10 w-10 mx-auto mb-2 opacity-50" />
            </motion.div>
            <p className="text-sm">No audit history available</p>
        </motion.div>
    );
}

interface ChangeItemProps {
    change: ParsedAuditChanges;
}

function ChangeItem({ change }: ChangeItemProps) {
    return (
        <div className="flex items-center gap-2 text-xs py-1">
            <span className="font-medium text-gray-600 dark:text-gray-400 min-w-[100px]">
                {change.property}:
            </span>
            {change.oldValue && (
                <span className="text-red-600 dark:text-red-400 line-through">
                    {change.oldValue}
                </span>
            )}
            {change.oldValue && change.newValue && (
                <ArrowRight className="h-3 w-3 text-gray-400" />
            )}
            {change.newValue && (
                <span className="text-green-600 dark:text-green-400">
                    {change.newValue}
                </span>
            )}
        </div>
    );
}

interface AuditItemProps {
    log: AuditLog;
    showDetails: boolean;
}

function AuditItem({ log, showDetails }: AuditItemProps) {
    const [isOpen, setIsOpen] = useState(false);
    const config = getActionConfig(log.action);
    const Icon = config.icon;
    const changes = parseAuditChanges(log);
    const hasChanges = changes.length > 0;

    return (
        <motion.div
            initial={{ opacity: 0, x: -10 }}
            animate={{ opacity: 1, x: 0 }}
            whileHover={{ scale: 1.01 }}
            transition={{ duration: 0.2 }}
            className="rounded-lg border border-slate-200 dark:border-slate-800 transition-colors"
        >
            <Collapsible open={isOpen} onOpenChange={setIsOpen}>
                <div className="flex items-start gap-3 p-3">
                    <div className={cn(
                        "h-10 w-10 rounded-full flex items-center justify-center shrink-0",
                        config.bgClassName
                    )}>
                        <Icon className={cn("h-5 w-5", config.className)} />
                    </div>

                    <div className="flex-1 min-w-0">
                        <div className="flex items-center gap-2 flex-wrap">
                            <Badge
                                variant="secondary"
                                className={cn("text-xs", config.className)}
                            >
                                {config.label}
                            </Badge>
                            {log.username && (
                                <span className="text-sm text-gray-600 dark:text-gray-400 flex items-center gap-1">
                                    <User className="h-3 w-3" />
                                    {log.username}
                                </span>
                            )}
                        </div>

                        <div className="flex items-center gap-2 mt-1 text-xs text-gray-500 dark:text-gray-400">
                            <Clock className="h-3 w-3" />
                            <span title={format(new Date(log.timestamp), "PPpp")}>
                                {formatDistanceToNow(new Date(log.timestamp), { addSuffix: true })}
                            </span>
                            <span className="text-gray-300 dark:text-gray-600">•</span>
                            <span className="text-gray-400 dark:text-gray-500">{log.serviceName}</span>
                        </div>

                        {showDetails && hasChanges && (
                            <CollapsibleTrigger asChild>
                                <button className="flex items-center gap-1 mt-2 text-xs text-blue-600 dark:text-blue-400 hover:text-blue-800 dark:hover:text-blue-300 transition-colors">
                                    <ChevronDown className={cn(
                                        "h-4 w-4 transition-transform",
                                        isOpen && "rotate-180"
                                    )} />
                                    {isOpen ? "Hide" : "Show"} {changes.length} change{changes.length !== 1 ? "s" : ""}
                                </button>
                            </CollapsibleTrigger>
                        )}
                    </div>
                </div>

                {showDetails && hasChanges && (
                    <CollapsibleContent>
                        <motion.div
                            initial={{ opacity: 0 }}
                            animate={{ opacity: 1 }}
                            className="px-3 pb-3 pt-0 ml-[52px] border-t border-slate-100 dark:border-slate-800 mt-2"
                        >
                            <div className="pt-2 space-y-1">
                                {changes.map((change, idx) => (
                                    <ChangeItem key={idx} change={change} />
                                ))}
                            </div>
                        </motion.div>
                    </CollapsibleContent>
                )}
            </Collapsible>
        </motion.div>
    );
}

export function AuditHistory({
    entityType,
    entityId,
    maxHeight = DEFAULT_MAX_HEIGHT,
    showDetails = true
}: AuditHistoryProps) {
    const [logs, setLogs] = useState<AuditLog[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const fetchHistory = useCallback(async () => {
        setIsLoading(true);
        setError(null);
        try {
            const data = await auditService.getEntityAuditHistory(entityType, entityId);
            setLogs(data);
        } catch {
            setError("Failed to load audit history");
        } finally {
            setIsLoading(false);
        }
    }, [entityType, entityId]);

    useEffect(() => {
        if (entityId) {
            fetchHistory();
        }
    }, [fetchHistory, entityId]);

    if (isLoading) {
        return <LoadingSkeleton />;
    }

    if (error) {
        return <ErrorState message={error} />;
    }

    if (logs.length === 0) {
        return <EmptyState />;
    }

    return (
        <ScrollArea style={{ maxHeight }} className="pr-4">
            <motion.div
                initial="hidden"
                animate="visible"
                variants={{
                    hidden: { opacity: 0 },
                    visible: {
                        opacity: 1,
                        transition: { staggerChildren: 0.05 }
                    }
                }}
                className="space-y-2"
            >
                <AnimatePresence>
                    {logs.map((log) => (
                        <motion.div
                            key={log.id}
                            variants={{
                                hidden: { opacity: 0, y: 10 },
                                visible: { opacity: 1, y: 0 }
                            }}
                        >
                            <AuditItem
                                log={log}
                                showDetails={showDetails}
                            />
                        </motion.div>
                    ))}
                </AnimatePresence>
            </motion.div>
        </ScrollArea>
    );
}
