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

interface SearchBarProps {
    onSearch: (params: {
        query?: string;
        sortBy?: string;
        status?: string;
    }) => void;
}

export function SearchBar({ onSearch }: SearchBarProps) {
    const [query, setQuery] = useState('');
    const [sortBy, setSortBy] = useState<string>();
    const [status, setStatus] = useState<string>();

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        onSearch({ query, sortBy, status });
    };

    return (
        <form onSubmit={handleSubmit} className="space-y-4">
            <div className="flex gap-2">
                <Input
                    type="text"
                    placeholder="Search auctions..."
                    value={query}
                    onChange={(e) => setQuery(e.target.value)}
                    className="flex-1"
                />
                <Button type="submit">Search</Button>
            </div>
            <div className="flex gap-4">
                <Select
                    value={sortBy}
                    onValueChange={(value) => setSortBy(value)}
                >
                    <SelectTrigger className="w-[180px]">
                        <SelectValue placeholder="Sort by" />
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem value="new">Newest</SelectItem>
                        <SelectItem value="endingSoon">Ending Soon</SelectItem>
                        <SelectItem value="make">Make</SelectItem>
                    </SelectContent>
                </Select>

                <Select
                    value={status}
                    onValueChange={(value) => setStatus(value)}
                >
                    <SelectTrigger className="w-[180px]">
                        <SelectValue placeholder="Filter by" />
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem value="live">Live</SelectItem>
                        <SelectItem value="endingSoon">Ending Soon</SelectItem>
                        <SelectItem value="finished">Finished</SelectItem>
                    </SelectContent>
                </Select>
            </div>
        </form>
    );
}
