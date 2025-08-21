import { ReactNode } from 'react';
import { cn } from '../../lib/utils';
import { Skeleton } from '../ui/skeleton';

interface StatCardProps {
  title: string;
  value?: string | number;
  description?: string;
  icon?: ReactNode;
  loading?: boolean;
  className?: string;
}

export function StatCard({ title, value, description, icon, loading, className }: StatCardProps) {
  return (
    <div className={cn('rounded border bg-white p-4 shadow-sm flex flex-col gap-2', className)}>
      <div className="flex items-center justify-between">
        <span className="text-sm font-medium text-gray-600">{title}</span>
        {icon && <span className="text-gray-400">{icon}</span>}
      </div>
      <div className="text-2xl font-semibold min-h-[2.5rem]">
        {loading ? <Skeleton className="h-8 w-24" /> : value ?? '-'}
      </div>
      {description && (
        <div className="text-xs text-gray-500 min-h-[1rem]">{loading ? <Skeleton className="h-3 w-32" /> : description}</div>
      )}
    </div>
  );
}
