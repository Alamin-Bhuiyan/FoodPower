import { useTranslation } from 'react-i18next';
import { cn } from '@/lib/utils';

interface LanguageToggleProps {
    className?: string;
}

/** Small EN/বাং segmented toggle. Persists via the i18next localStorage detector cache. */
const LanguageToggle = ({ className }: LanguageToggleProps) => {
    const { i18n, t } = useTranslation();
    const isBn = (i18n.resolvedLanguage ?? i18n.language ?? 'en').startsWith('bn');

    return (
        <div
            role="group"
            aria-label={t('common.language')}
            className={cn('inline-flex items-center rounded-full border border-border bg-secondary/60 p-0.5', className)}
        >
            <button
                type="button"
                onClick={() => i18n.changeLanguage('en')}
                aria-pressed={!isBn}
                className={cn(
                    'px-2 py-0.5 rounded-full text-[11px] font-semibold leading-5 transition-colors',
                    !isBn ? 'bg-card text-foreground shadow-sm' : 'text-muted-foreground'
                )}
            >
                EN
            </button>
            <button
                type="button"
                onClick={() => i18n.changeLanguage('bn')}
                aria-pressed={isBn}
                className={cn(
                    'px-2 py-0.5 rounded-full text-[11px] font-semibold leading-5 transition-colors',
                    isBn ? 'bg-card text-foreground shadow-sm' : 'text-muted-foreground'
                )}
            >
                বাং
            </button>
        </div>
    );
};

export default LanguageToggle;
