'use client';

import { useState } from 'react';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from '@/components/ui/select';
import { SearchFilterBy, SearchOrderBy } from '@/types/search';

interface SearchBarProps {
    onSearch: (params: {
        searchTerm: string;
        orderBy?: SearchOrderBy;
        filterBy?: SearchFilterBy;
    }) => void;
}

export function SearchBar({ onSearch }: SearchBarProps) {
    const [searchTerm, setSearchTerm] = useState('');
    const [orderBy, setOrderBy] = useState<SearchOrderBy>();
    const [filterBy, setFilterBy] = useState<SearchFilterBy>();

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        onSearch({ searchTerm, orderBy, filterBy });
    };

    return (
        <form onSubmit={handleSubmit} className="space-y-4">
            <div className="flex gap-2">
                <Input
                    type="text"
                    placeholder="Search auctions..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    className="flex-1"
                />
                <Button type="submit">Search</Button>
            </div>
            <div className="flex gap-4">
                <Select
                    value={orderBy}
                    onValueChange={(value) => setOrderBy(value as SearchOrderBy)}
                >
                    <SelectTrigger className="w-[180px]">
                        <SelectValue placeholder="Sort by" />
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem value={SearchOrderBy.New}>Newest</SelectItem>
                        <SelectItem value={SearchOrderBy.EndingSoon}>Ending Soon</SelectItem>
                        <SelectItem value={SearchOrderBy.Make}>Make</SelectItem>
                    </SelectContent>
                </Select>

                <Select
                    value={filterBy}
                    onValueChange={(value) => setFilterBy(value as SearchFilterBy)}
                >
                    <SelectTrigger className="w-[180px]">
                        <SelectValue placeholder="Filter by" />
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem value={SearchFilterBy.Live}>Live</SelectItem>
                        <SelectItem value={SearchFilterBy.EndingSoon}>Ending Soon</SelectItem>
                        <SelectItem value={SearchFilterBy.Finished}>Finished</SelectItem>
                    </SelectContent>
                </Select>
            </div>
        </form>
    );
}
