import type { ReactNode } from "react";
import { cn } from "@/lib/utils";

/**
 * Shared layout shell for create-record forms across BIMSS modules.
 * Pattern: Card header (title + description) -> one or more FormSections -> FormFooter.
 * See docs/design/BIMSS-UI-SPEC.md section 5 and dashboard/members/new/page.tsx for the reference usage.
 */
export function FormSection({
  title,
  description,
  children,
  className,
}: {
  title: string;
  description?: string;
  children: ReactNode;
  className?: string;
}) {
  return (
    <section className={cn("flex flex-col gap-4", className)}>
      <div>
        <h2 className="text-[14.5px] font-semibold">{title}</h2>
        {description ? <p className="mt-1 text-[12.5px] text-muted-foreground">{description}</p> : null}
      </div>
      <div className="grid grid-cols-1 gap-x-[18px] gap-y-4 sm:grid-cols-2">{children}</div>
    </section>
  );
}

export function FormFooter({ children, className }: { children: ReactNode; className?: string }) {
  return <div className={cn("flex flex-wrap items-center gap-2.5 border-t pt-[18px]", className)}>{children}</div>;
}

export function RequiredMark() {
  return (
    <span aria-hidden="true" className="text-destructive">
      {" "}
      *
    </span>
  );
}

export function FieldError({ message }: { message?: string }) {
  if (!message) {
    return null;
  }
  return (
    <p role="alert" className="text-sm text-destructive">
      {message}
    </p>
  );
}
