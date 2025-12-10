import apiClient from "@/lib/api/axios";
import { AuditLog, AuditLogQueryParams, PagedAuditLogs } from "@/types/audit";
import { API_ENDPOINTS } from "@/constants/api";

export const auditService = {
  /**
   * Get paginated audit logs with optional filters
   */
  getAuditLogs: async (
    params?: AuditLogQueryParams
  ): Promise<PagedAuditLogs> => {
    const { data } = await apiClient.get<PagedAuditLogs>(
      API_ENDPOINTS.AUDIT_LOGS,
      { params }
    );
    return data;
  },

  /**
   * Get audit history for a specific entity
   */
  getEntityAuditHistory: async (
    entityType: string,
    entityId: string
  ): Promise<AuditLog[]> => {
    const { data } = await apiClient.get<AuditLog[]>(
      API_ENDPOINTS.AUDIT_LOGS_BY_ENTITY(entityType, entityId)
    );
    return data;
  },

  /**
   * Get a single audit log by ID
   */
  getAuditLogById: async (id: string): Promise<AuditLog> => {
    const { data } = await apiClient.get<AuditLog>(
      API_ENDPOINTS.AUDIT_LOG_BY_ID(id)
    );
    return data;
  }
} as const;
