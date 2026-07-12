import React from 'react'
import classNames from '@/utils/classNames'

interface FormItemProps {
    children: React.ReactNode
    label?: React.ReactNode
    invalid?: boolean
    errorMessage?: React.ReactNode
    className?: string
}

export const FormItem = ({
    children,
    label,
    invalid,
    errorMessage,
    className,
}: FormItemProps) => {
    return (
        <div className={classNames('mb-4', className)}>
            {label && <div className="mb-1">{label}</div>}
            {children}
            {invalid && errorMessage && (
                <div className="mt-1">{errorMessage}</div>
            )}
        </div>
    )
}
