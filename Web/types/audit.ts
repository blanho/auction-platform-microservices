export enum AuditAction {
  Created = 1,
  Updated = 2,
  Deleted = 3,
  SoftDeleted = 4,
  Restored = 5
}

export const AuditActionLabels: Record<AuditAction, string> = {
  [AuditAction.Created]: "Created",
  [AuditAction.Updated]: "Updated",
  [AuditAction.Deleted]: "Deleted",
  [AuditAction.SoftDeleted]: "Soft Deleted",
  [AuditAction.Restored]: "Restored"
};

export interface AuditLog {
  id: string;
  entityId: string;
  entityType: string;
  action: AuditAction;
  actionName: string;
  oldValues?: string;
  newValues?: string;
  changedProperties?: string[];
  userId: string;
  username?: string;
  serviceName: string;
  correlationId?: string;
  ipAddress?: string;
  timestamp: string;
}

export interface AuditLogQueryParams {
  entityId?: string;
  entityType?: string;
  userId?: string;
  serviceName?: string;
  action?: AuditAction;
  fromDate?: string;
  toDate?: string;
  page?: number;
  pageSize?: number;
}

export interface PagedAuditLogs {
  items: AuditLog[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ParsedAuditChanges {
  property: string;
  oldValue: string | null;
  newValue: string | null;
}

export function parseAuditChanges(log: AuditLog): ParsedAuditChanges[] {
  const changes: ParsedAuditChanges[] = [];

  if (!log.changedProperties?.length) return changes;

  try {
    const oldValues = log.oldValues ? JSON.parse(log.oldValues) : {};
    const newValues = log.newValues ? JSON.parse(log.newValues) : {};

    for (const prop of log.changedProperties) {
      changes.push({
        property: prop,
        oldValue:
          oldValues[prop] !== undefined ? String(oldValues[prop]) : null,
        newValue: newValues[prop] !== undefined ? String(newValues[prop]) : null
      });
    }
  } catch {
    // If parsing fails, return empty changes
  }

  return changes;
}
