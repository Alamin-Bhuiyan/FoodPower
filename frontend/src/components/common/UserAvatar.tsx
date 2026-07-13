import { useEffect, useState } from 'react';
import { getInitials } from '@/lib/auth';
import { BASE_URL } from '@/lib/config';
import { cn } from '@/lib/utils';

interface UserAvatarProps {
    name?: string | null;
    /** Relative path (e.g. "/resources/avatars/x.jpg") or absolute http(s) URL. Renders an image when set. */
    imageUrl?: string | null;
    className?: string;
    size?: 'xs' | 'sm' | 'md' | 'lg';
}

const sizeClasses = {
    xs: 'w-5 h-5 text-[8px]',
    sm: 'w-7 h-7 text-[10px]',
    md: 'w-9 h-9 text-xs',
    lg: 'w-14 h-14 text-lg',
};

const resolveUrl = (path: string): string =>
    /^https?:\/\//i.test(path) ? path : `${BASE_URL}${path.startsWith('/') ? '' : '/'}${path}`;

/** Colored initials avatar — deterministic hue from the name; renders a photo when `imageUrl` is set. */
const UserAvatar = ({ name, imageUrl, className, size = 'sm' }: UserAvatarProps) => {
    const label = name ?? '?';
    const [errored, setErrored] = useState(false);

    // Reset the error flag whenever the image source changes (e.g. after upload/remove).
    useEffect(() => {
        setErrored(false);
    }, [imageUrl]);

    let hash = 0;
    for (let i = 0; i < label.length; i++) hash = (hash * 31 + label.charCodeAt(i)) % 360;

    const showImage = !!imageUrl && !errored;

    if (showImage) {
        return (
            <img
                src={resolveUrl(imageUrl as string)}
                alt={label}
                title={label}
                onError={() => setErrored(true)}
                className={cn(
                    'inline-block rounded-full object-cover ring-2 ring-white shrink-0 bg-secondary',
                    sizeClasses[size],
                    className
                )}
            />
        );
    }

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
