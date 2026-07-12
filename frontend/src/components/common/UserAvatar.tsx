import { getInitials } from '@/lib/auth';
import { cn } from '@/lib/utils';

interface UserAvatarProps {
    name?: string | null;
    className?: string;
    size?: 'xs' | 'sm' | 'md' | 'lg';
}

const sizeClasses = {
    xs: 'w-5 h-5 text-[8px]',
    sm: 'w-7 h-7 text-[10px]',
    md: 'w-9 h-9 text-xs',
    lg: 'w-14 h-14 text-lg',
};

/** Colored initials avatar — deterministic hue from the name. */
const UserAvatar = ({ name, className, size = 'sm' }: UserAvatarProps) => {
    const label = name ?? '?';
    let hash = 0;
    for (let i = 0; i < label.length; i++) hash = (hash * 31 + label.charCodeAt(i)) % 360;
    return (
        <span
            className={cn(
                'inline-flex items-center justify-center rounded-full font-bold text-white ring-2 ring-white shrink-0',
                sizeClasses[size],
                className
            )}
            style={{ backgroundColor: `hsl(${hash}, 62%, 48%)` }}
            title={label}
        >
            {getInitials(label)}
        </span>
    );
};

export default UserAvatar;
