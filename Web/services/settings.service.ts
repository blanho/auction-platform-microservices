import apiClient from '@/lib/api/axios';
import { API_ENDPOINTS } from '@/constants/api';

export type SettingCategory = 
    | 'General'
    | 'Auction'
    | 'Notification'
    | 'Security'
    | 'Email'
    | 'Payment';

export interface PlatformSetting {
    id: string;
    key: string;
    value: string;
    description?: string;
    category: string;
    dataType?: string;
    isSystem: boolean;
    updatedAt: string;
    updatedBy?: string;
}

export interface CreateSettingDto {
    key: string;
    value: string;
    description?: string;
    category: SettingCategory;
    dataType?: string;
    validationRules?: string;
}

export interface UpdateSettingDto {
    value: string;
}

export interface SettingsMap {
    [key: string]: string;
}

interface ApiResponse<T> {
    isSuccess: boolean;
    data: T;
    errorMessage?: string;
}

export const settingsService = {
    async getSettings(category?: SettingCategory): Promise<PlatformSetting[]> {
        const params = category ? { category } : {};
        const response = await apiClient.get<ApiResponse<PlatformSetting[]>>(
            API_ENDPOINTS.ADMIN.SETTINGS,
            { params }
        );
        return response.data.data;
    },

    async getSettingByKey(key: string): Promise<PlatformSetting> {
        const response = await apiClient.get<ApiResponse<PlatformSetting>>(
            API_ENDPOINTS.ADMIN.SETTING_BY_KEY(key)
        );
        return response.data.data;
    },

    async getSettingById(id: string): Promise<PlatformSetting> {
        const response = await apiClient.get<ApiResponse<PlatformSetting>>(
            API_ENDPOINTS.ADMIN.SETTING_BY_ID(id)
        );
        return response.data.data;
    },

    async createSetting(dto: CreateSettingDto): Promise<PlatformSetting> {
        const response = await apiClient.post<ApiResponse<PlatformSetting>>(
            API_ENDPOINTS.ADMIN.SETTINGS,
            dto
        );
        return response.data.data;
    },

    async updateSetting(id: string, dto: UpdateSettingDto): Promise<PlatformSetting> {
        const response = await apiClient.put<ApiResponse<PlatformSetting>>(
            API_ENDPOINTS.ADMIN.SETTING_BY_ID(id),
            dto
        );
        return response.data.data;
    },

    async deleteSetting(id: string): Promise<void> {
        await apiClient.delete(API_ENDPOINTS.ADMIN.SETTING_BY_ID(id));
    },

    async bulkUpdateSettings(settings: { key: string; value: string }[]): Promise<void> {
        await apiClient.put(API_ENDPOINTS.ADMIN.SETTINGS_BULK, { settings });
    },

    settingsToMap(settings: PlatformSetting[]): SettingsMap {
        return settings.reduce((acc, setting) => {
            acc[setting.key] = setting.value;
            return acc;
        }, {} as SettingsMap);
    },

    mapToSettings(settingsMap: SettingsMap, existingSettings: PlatformSetting[]): { key: string; value: string }[] {
        return Object.entries(settingsMap)
            .filter(([key, value]) => {
                const existing = existingSettings.find(s => s.key === key);
                return existing && existing.value !== value;
            })
            .map(([key, value]) => ({ key, value }));
    }
};
