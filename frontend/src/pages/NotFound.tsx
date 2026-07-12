import { Link, useLocation } from 'react-router-dom';
import { useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { UtensilsCrossed } from 'lucide-react';
import { Button } from '@/components/ui/button';

const NotFound = () => {
    const { t } = useTranslation();
    const location = useLocation();

    useEffect(() => {
        console.error('404 Error: User attempted to access non-existent route:', location.pathname);
    }, [location.pathname]);

    return (
        <div className="min-h-dvh max-w-md mx-auto flex flex-col items-center justify-center px-6 text-center">
            <span className="w-16 h-16 rounded-3xl bg-primary/10 text-primary flex items-center justify-center mb-4">
                <UtensilsCrossed className="h-8 w-8" />
            </span>
            <h1 className="text-4xl font-extrabold mb-1">404</h1>
            <p className="text-sm text-muted-foreground mb-6">{t('notFound.message')}</p>
            <Button asChild className="h-12 rounded-xl px-8 font-semibold">
                <Link to="/">{t('notFound.backHome')}</Link>
            </Button>
        </div>
    );
};

export default NotFound;
