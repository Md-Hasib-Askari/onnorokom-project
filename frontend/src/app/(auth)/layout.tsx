import Link from "next/link";
import { GraduationCapIcon } from "lucide-react";
import { ModeToggle } from "@/components/mode-toggle";
import { ROUTES } from "@/lib/routes";

export default function AuthLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="relative grid min-h-svh lg:grid-cols-2">
      <ModeToggle className="absolute top-6 right-6 z-10" />
      <div className="relative hidden flex-col justify-between overflow-hidden bg-sidebar p-10 text-sidebar-foreground lg:flex">
        <div
          className="pointer-events-none absolute inset-0"
          style={{
            backgroundImage:
              "radial-gradient(circle at 15% 20%, color-mix(in oklch, var(--sidebar-primary), transparent 82%), transparent 55%), radial-gradient(circle at 85% 75%, color-mix(in oklch, var(--sidebar-primary), transparent 88%), transparent 50%)",
          }}
        />
        <Link href={ROUTES.home} className="relative flex items-center gap-2.5 text-lg font-semibold">
          <span className="flex size-9 items-center justify-center rounded-lg bg-sidebar-primary text-sidebar-primary-foreground">
            <GraduationCapIcon className="size-5" />
          </span>
          Onnorokom
        </Link>
        <div className="relative space-y-3 pb-16">
          <p className="text-3xl font-semibold text-balance">
            Assignments and submissions, organized for the whole school.
          </p>
          <p className="max-w-sm text-sm text-sidebar-foreground/60">
            One workspace for admins, teachers, and students to track coursework from
            assignment to grade.
          </p>
        </div>
        <p className="relative text-xs text-sidebar-foreground/40">
          &copy; {new Date().getFullYear()} Onnorokom Assignment System
        </p>
      </div>
      <div className="flex flex-col items-center justify-center gap-6 bg-muted/40 p-6 md:p-10">
        <div className="flex w-full max-w-sm flex-col gap-6">
          <Link
            href={ROUTES.home}
            className="flex items-center gap-2.5 self-center text-lg font-semibold lg:hidden"
          >
            <span className="flex size-9 items-center justify-center rounded-lg bg-primary text-primary-foreground">
              <GraduationCapIcon className="size-5" />
            </span>
            Onnorokom
          </Link>
          {children}
        </div>
      </div>
    </div>
  );
}
