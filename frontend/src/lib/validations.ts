import * as z from "zod"

// Messages are i18n keys — render with t(errors.<field>.message) so they follow the active language.
export const loginSchema = z.object({
    email: z.string().email("validation.emailInvalid"),
    password: z.string().min(1, "validation.passwordRequired"),
})

export const registerSchema = z.object({
    fullName: z.string().min(1, "validation.fullNameRequired"),
    email: z.string().email("validation.emailInvalid"),
    password: z.string().min(8, "validation.passwordMin"),
    confirmPassword: z.string().min(1, "validation.confirmPasswordRequired"),
}).refine((data) => data.password === data.confirmPassword, {
    message: "validation.passwordsMismatch",
    path: ["confirmPassword"],
})

export const forgetPasswordSchema = z.object({
    email: z.string().email("validation.emailInvalid"),
})

export const resetPasswordSchema = z.object({
    password: z.string().min(8, "validation.passwordMin"),
    confirmPassword: z.string().min(1, "validation.confirmPasswordRequired"),
}).refine((data) => data.password === data.confirmPassword, {
    message: "validation.passwordsMismatch",
    path: ["confirmPassword"],
})

export const changePasswordSchema = z.object({
    oldPassword: z.string().min(1, "validation.currentPasswordRequired"),
    newPassword: z.string().min(8, "validation.passwordMin"),
    confirmPassword: z.string().min(1, "validation.confirmPasswordRequired"),
}).refine((data) => data.newPassword === data.confirmPassword, {
    message: "validation.passwordsMismatch",
    path: ["confirmPassword"],
})

export const menuItemSchema = z.object({
    name: z.string().min(1, "validation.menuNameRequired"),
    description: z.string().optional(),
})

export const catererSchema = z.object({
    name: z.string().min(1, "validation.catererNameRequired"),
    phone: z.string().optional(),
    price_per_lunch: z.preprocess((val) => Number(val), z.number().min(1, "validation.priceRequired")),
})

export type LoginFormValues = z.infer<typeof loginSchema>
export type RegisterFormValues = z.infer<typeof registerSchema>
export type ForgetPasswordFormValues = z.infer<typeof forgetPasswordSchema>
export type ResetPasswordFormValues = z.infer<typeof resetPasswordSchema>
export type ChangePasswordFormValues = z.infer<typeof changePasswordSchema>
export type MenuItemFormValues = z.infer<typeof menuItemSchema>
export type CatererFormValues = z.infer<typeof catererSchema>
