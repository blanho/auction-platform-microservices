import apiClient from "@/lib/api/axios";

export interface Brand {
    id: string;
    name: string;
    slug: string;
    logoUrl?: string;
    description?: string;
    displayOrder: number;
    isActive: boolean;
    isFeatured: boolean;
    auctionCount?: number;
}

export interface CreateBrandDto {
    name: string;
    logoUrl?: string;
    description?: string;
    displayOrder?: number;
    isFeatured?: boolean;
}

export interface UpdateBrandDto {
    name?: string;
    logoUrl?: string;
    description?: string;
    displayOrder?: number;
    isActive?: boolean;
    isFeatured?: boolean;
}

const BASE_URL = "/auction/api/v1/brands";

export const brandService = {
    async getBrands(params?: {
        activeOnly?: boolean;
        featuredOnly?: boolean;
        count?: number;
    }): Promise<Brand[]> {
        const response = await apiClient.get<Brand[]>(BASE_URL, { params });
        return response.data;
    },

    async getBrandById(id: string): Promise<Brand> {
        const response = await apiClient.get<Brand>(`${BASE_URL}/${id}`);
        return response.data;
    },

    async createBrand(dto: CreateBrandDto): Promise<Brand> {
        const response = await apiClient.post<Brand>(BASE_URL, dto);
        return response.data;
    },

    async updateBrand(id: string, dto: UpdateBrandDto): Promise<Brand> {
        const response = await apiClient.put<Brand>(`${BASE_URL}/${id}`, dto);
        return response.data;
    },

    async deleteBrand(id: string): Promise<void> {
        await apiClient.delete(`${BASE_URL}/${id}`);
    },
};
