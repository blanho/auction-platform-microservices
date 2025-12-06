"use client";

import { useState, useCallback } from "react";
import { auditService } from "@/services/audit.service";
import {
  AuditLog,
  AuditLogQueryParams,
  PagedAuditLogs
} from "@/types/audit";

interface UseAuditLogsReturn {
  auditLogs: AuditLog[];
  pagedResult: PagedAuditLogs | null;
  isLoading: boolean;
  error: string | null;
  fetchAuditLogs: (params?: AuditLogQueryParams) => Promise<PagedAuditLogs>;
  fetchEntityHistory: (entityType: string, entityId: string) => Promise<AuditLog[]>;
  fetchAuditLogById: (id: string) => Promise<AuditLog>;
  clearError: () => void;
}

export function useAuditLogs(): UseAuditLogsReturn {
  const [auditLogs, setAuditLogs] = useState<AuditLog[]>([]);
  const [pagedResult, setPagedResult] = useState<PagedAuditLogs | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchAuditLogs = useCallback(async (params?: AuditLogQueryParams) => {
    setIsLoading(true);
    setError(null);
    try {
      const data = await auditService.getAuditLogs(params);
      setAuditLogs(data.items);
      setPagedResult(data);
      return data;
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Failed to fetch audit logs";
      setError(message);
      throw err;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const fetchEntityHistory = useCallback(
    async (entityType: string, entityId: string) => {
      setIsLoading(true);
      setError(null);
      try {
        const data = await auditService.getEntityAuditHistory(entityType, entityId);
        setAuditLogs(data);
        return data;
      } catch (err) {
        const message =
          err instanceof Error ? err.message : "Failed to fetch entity history";
        setError(message);
        throw err;
      } finally {
        setIsLoading(false);
      }
    },
    []
  );

  const fetchAuditLogById = useCallback(async (id: string) => {
    setIsLoading(true);
    setError(null);
    try {
      const data = await auditService.getAuditLogById(id);
      return data;
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Failed to fetch audit log";
      setError(message);
      throw err;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const clearError = useCallback(() => {
    setError(null);
  }, []);

  return {
    auditLogs,
    pagedResult,
    isLoading,
    error,
    fetchAuditLogs,
    fetchEntityHistory,
    fetchAuditLogById,
    clearError
  };
}
