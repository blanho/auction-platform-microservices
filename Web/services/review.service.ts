import apiClient from '@/lib/api/axios';

export interface Review {
    id: string;
    orderId?: string;
    auctionId: string;
    reviewerUsername: string;
    reviewedUsername: string;
    rating: number;
    title?: string;
    comment?: string;
    sellerResponse?: string;
    sellerResponseAt?: string;
    createdAt: string;
}

export interface CreateReviewDto {
    auctionId: string;
    orderId?: string;
    reviewedUserId: string;
    reviewedUsername: string;
    rating: number;
    title?: string;
    comment?: string;
}

export interface UserRatingSummary {
    username: string;
    averageRating: number;
    totalReviews: number;
}

const BASE_URL = '/auction/api/v1/reviews';

export const reviewService = {
    async createReview(dto: CreateReviewDto): Promise<Review> {
        const response = await apiClient.post<Review>(BASE_URL, dto);
        return response.data;
    },

    async getReview(id: string): Promise<Review> {
        const response = await apiClient.get<Review>(`${BASE_URL}/${id}`);
        return response.data;
    },

    async getReviewsForAuction(auctionId: string): Promise<Review[]> {
        const response = await apiClient.get<Review[]>(`${BASE_URL}/auction/${auctionId}`);
        return response.data;
    },

    async getReviewsForUser(username: string): Promise<Review[]> {
        const response = await apiClient.get<Review[]>(`${BASE_URL}/user/${username}`);
        return response.data;
    },

    async getReviewsByUser(username: string): Promise<Review[]> {
        const response = await apiClient.get<Review[]>(`${BASE_URL}/by/${username}`);
        return response.data;
    },

    async getUserRatingSummary(username: string): Promise<UserRatingSummary> {
        const response = await apiClient.get<UserRatingSummary>(`${BASE_URL}/user/${username}/summary`);
        return response.data;
    },

    async addSellerResponse(reviewId: string, response: string): Promise<Review> {
        const res = await apiClient.post<Review>(`${BASE_URL}/${reviewId}/response`, {
            response,
        });
        return res.data;
    },
};

export const getRatingColor = (rating: number) => {
    if (rating >= 4.5) return 'text-green-600';
    if (rating >= 4.0) return 'text-green-500';
    if (rating >= 3.0) return 'text-yellow-500';
    if (rating >= 2.0) return 'text-orange-500';
    return 'text-red-500';
};

export const getRatingLabel = (rating: number) => {
    if (rating >= 4.5) return 'Excellent';
    if (rating >= 4.0) return 'Very Good';
    if (rating >= 3.0) return 'Good';
    if (rating >= 2.0) return 'Fair';
    return 'Poor';
};
