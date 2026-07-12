import React from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import LanguageToggle from '@/components/common/LanguageToggle';

interface AuthLayoutProps {
    title: string;
    subtitle?: React.ReactNode;
    children: React.ReactNode;
}

/** Full-screen mobile-first auth wrapper (no tab bar). */
const AuthLayout = ({ title, subtitle, children }: AuthLayoutProps) => {
    const { t } = useTranslation();
    return (
        <div className="min-h-dvh flex flex-col max-w-md mx-auto px-6 pt-[calc(var(--safe-top)+2.5rem)] pb-[calc(var(--safe-bottom)+2rem)]">
            <div className="flex items-start justify-between mb-8">
                <Link to="/login" className="flex items-center gap-3">
                    <span className="w-12 h-12 rounded-2xl bg-primary text-primary-foreground flex items-center justify-center text-2xl shadow-[0_4px_14px_rgba(249,115,22,0.35)]">
                        🍱
                    </span>
                    <div>
                        <div className="font-extrabold text-lg leading-tight text-foreground">FoodPower</div>
                        <div className="text-[11px] text-muted-foreground">{t('app.tagline')}</div>
                    </div>
                </Link>
                <LanguageToggle className="mt-1" />
            </div>

            <div className="flex-1">
                <h1 className="text-2xl font-extrabold text-foreground">{title}</h1>
                {subtitle && <p className="mt-1.5 text-sm text-muted-foreground">{subtitle}</p>}
                <div className="mt-8">{children}</div>
            </div>
        </div>
    );
};

export default AuthLayout;