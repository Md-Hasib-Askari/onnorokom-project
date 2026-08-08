"use client"

import { useTheme } from "next-themes"
import { Toaster as Sonner, type ToasterProps } from "sonner"
import { CircleCheckIcon, InfoIcon, TriangleAlertIcon, OctagonXIcon, Loader2Icon } from "lucide-react"

const Toaster = ({ ...props }: ToasterProps) => {
  const { theme = "system" } = useTheme()

  return (
    <Sonner
      theme={theme as ToasterProps["theme"]}
      className="toaster group"
      icons={{
        success: (
          <CircleCheckIcon className="size-4" />
        ),
        info: (
          <InfoIcon className="size-4" />
        ),
        warning: (
          <TriangleAlertIcon className="size-4" />
        ),
        error: (
          <OctagonXIcon className="size-4" />
        ),
        loading: (
          <Loader2Icon className="size-4 animate-spin" />
        ),
      }}
      style={
        {
          "--normal-bg": "var(--popover)",
          "--normal-text": "var(--popover-foreground)",
          "--normal-border": "var(--border)",
          "--border-radius": "var(--radius)",
          "--success-bg": "color-mix(in oklch, var(--success) 16%, var(--popover))",
          "--success-border": "color-mix(in oklch, var(--success) 40%, var(--popover))",
          "--warning-bg": "color-mix(in oklch, var(--warning) 16%, var(--popover))",
          "--warning-border": "color-mix(in oklch, var(--warning) 40%, var(--popover))",
          "--error-bg": "color-mix(in oklch, var(--destructive) 16%, var(--popover))",
          "--error-border": "color-mix(in oklch, var(--destructive) 40%, var(--popover))",
          "--info-bg": "color-mix(in oklch, var(--info) 16%, var(--popover))",
          "--info-border": "color-mix(in oklch, var(--info) 40%, var(--popover))",
        } as React.CSSProperties
      }
      toastOptions={{
        classNames: {
          toast: "cn-toast",
          success: "!bg-[var(--success-bg)] !text-success !border-[var(--success-border)]",
          warning: "!bg-[var(--warning-bg)] !text-warning !border-[var(--warning-border)]",
          error: "!bg-[var(--error-bg)] !text-destructive !border-[var(--error-border)]",
          info: "!bg-[var(--info-bg)] !text-info !border-[var(--info-border)]",
        },
      }}
      {...props}
    />
  )
}

export { Toaster }
