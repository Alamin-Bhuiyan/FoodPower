import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import LanguageDetector from 'i18next-browser-languagedetector';
import en from './locales/en.json';
import bn from './locales/bn.json';

i18n
    .use(LanguageDetector)
    .use(initReactI18next)
    .init({
        resources: {
            en: { translation: en },
            bn: { translation: bn },
        },
        fallbackLng: 'en',
        supportedLngs: ['en', 'bn'],
        nonExplicitSupportedLngs: true,
        detection: {
            order: ['localStorage', 'navigator'],
            caches: ['localStorage'],
            lookupLocalStorage: 'foodpower_lang',
        },
        interpolation: {
            escapeValue: false, // React already escapes
        },
        returnNull: false,
    });

/** Keep <html lang> in sync so the browser picks the right fonts/hyphenation. */
const applyHtmlLang = (lng: string) => {
    document.documentElement.lang = lng?.startsWith('bn') ? 'bn' : 'en';
};
applyHtmlLang(i18n.resolvedLanguage ?? i18n.language ?? 'en');
i18n.on('languageChanged', applyHtmlLang);

/** Is the active UI language Bangla? */
export const isBangla = (): boolean => (i18n.resolvedLanguage ?? i18n.language ?? 'en').startsWith('bn');

/** BCP-47 locale for date formatting driven by the active UI language. */
export const dateLocale = (): string => (isBangla() ? 'bn-BD' : 'en-GB');

export default i18n;
