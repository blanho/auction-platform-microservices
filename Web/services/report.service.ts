import apiClient from '@/lib/api/axios';

export interface Report {
    id: string;
    reporterUsername: string;
    reportedUsername: string;
    auctionId?: string;
    type: string;
    priority: string;
    reason: string;
    description?: string;
    status: string;
    resolution?: string;
    resolvedBy?: string;
    resolvedAt?: string;
    createdAt: string;
}

export interface PagedReportsResponse {
    reports: Report[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
}

export interface ReportStats {
    totalReports: number;
    pendingReports: number;
    underReviewReports: number;
    resolvedReports: number;
    dismissedReports: number;
    criticalReports: number;
    highPriorityReports: number;
}

export interface GetReportsParams {
    status?: string;
    type?: string;
    priority?: string;
    reportedUsername?: string;
    pageNumber?: number;
    pageSize?: number;
}

export interface CreateReportDto {
    reportedUsername: string;
    auctionId?: string;
    type: string;
    reason: string;
    description?: string;
}

export interface UpdateReportStatusDto {
    status: string;
    resolution?: string;
}

export const reportService = {
    async getReports(params: GetReportsParams = {}): Promise<PagedReportsResponse> {
        const response = await apiClient.get<PagedReportsResponse>('/utility/api/reports', {
            params: {
                status: params.status && params.status !== 'all' ? params.status : undefined,
                type: params.type && params.type !== 'all' ? params.type : undefined,
                priority: params.priority && params.priority !== 'all' ? params.priority : undefined,
                reportedUsername: params.reportedUsername || undefined,
                pageNumber: params.pageNumber || 1,
                pageSize: params.pageSize || 20,
            },
        });
        return response.data;
    },

    async getReport(id: string): Promise<Report> {
        const response = await apiClient.get<Report>(`/utility/api/reports/${id}`);
        return response.data;
    },

    async createReport(dto: CreateReportDto): Promise<Report> {
        const response = await apiClient.post<Report>('/utility/api/reports', dto);
        return response.data;
    },

    async updateReportStatus(id: string, dto: UpdateReportStatusDto): Promise<void> {
        await apiClient.put(`/utility/api/reports/${id}/status`, dto);
    },

    async deleteReport(id: string): Promise<void> {
        await apiClient.delete(`/utility/api/reports/${id}`);
    },
};
