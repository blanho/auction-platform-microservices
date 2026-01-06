"use client";

import { Card, CardContent, CardHeader, CardTitle, Input } from "@repo/ui";
import { formatDateTime } from "@repo/utils";
import { Search, FileText } from "lucide-react";
import { useState } from "react";

export default function AuditLogsPage() {
  const [search, setSearch] = useState("");

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-3xl font-bold">Audit Logs</h1>
        <p className="mt-2 text-muted-foreground">
          System activity and security logs
        </p>
      </div>

      <div className="flex items-center gap-4">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Search logs..."
            className="pl-9"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Recent Activity</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex flex-col items-center justify-center py-12">
            <FileText className="h-12 w-12 text-muted-foreground" />
            <h3 className="mt-4 text-lg font-semibold">No audit logs</h3>
            <p className="mt-2 text-sm text-muted-foreground">
              Connect to AnalyticsService to view audit logs
            </p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
