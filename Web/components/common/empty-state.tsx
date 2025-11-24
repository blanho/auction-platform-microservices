import { ReactNode } from 'react';
import { cn } from '@/lib/utils';

interface EmptyStateProps {
    icon?: ReactNode;
    title: string;
    description?: string;
    action?: ReactNode;
    className?: string;
}

export function EmptyState({
    icon,
    title,
    description,
    action,
    className,
}: EmptyStateProps) {
    return (
        <div
            className={cn(
                'flex flex-col items-center justify-center rounded-lg border border-dashed p-12 text-center',
                className
            )}
        >
            {icon && <div className="mb-4 text-muted-foreground">{icon}</div>}
            <h3 className="mb-2 text-lg font-semibold">{title}</h3>
            {description && (
                <p className="mb-4 text-sm text-muted-foreground">{description}</p>
            )}
            {action && <div>{action}</div>}
        </div>
    );
}
