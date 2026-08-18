import type { ReactNode } from "react";
import { CardHeader } from "@/components/ui/card";
import { cn } from "@/lib/utils";

/**
 * Shared multi-step wizard header for BIMSS modules.
 * Pattern: Card > WizardHeader (title + optional meta + step segments) > CardContent (current step's FormSection) > FormFooter (see forms/record-form.tsx) with Back/Continue/Submit driven by the page's own step state.
 * See docs/design/BIMSS-UI-SPEC.md section 5.5 and dashboard/members/new/page.tsx for the reference usage.
 */
export function WizardHeader({
  title,
  meta,
  steps,
  currentStep,
}: {
  title: string;
  meta?: string;
  steps: string[];
  currentStep: number;
}) {
  return (
    <CardHeader className="flex flex-col items-stretch gap-4">
      <div className="flex flex-wrap items-baseline justify-between gap-x-4 gap-y-1">
        <div>
          <h2 className="text-[14.5px] font-semibold">{title}</h2>
          {meta ? <p className="mt-1 text-[12.5px] text-muted-foreground">{meta}</p> : null}
        </div>
        <span className="text-[12.5px] font-medium text-muted-foreground">
          Step {currentStep + 1} of {steps.length}
        </span>
      </div>
      <ol
        className="grid gap-x-2 gap-y-1.5"
        style={{ gridTemplateColumns: `repeat(${steps.length}, minmax(0, 1fr))` }}
      >
        {steps.map((label, index) => {
          const isDoneOrCurrent = index <= currentStep;
          const isCurrent = index === currentStep;
          return (
            <li key={label} className="flex flex-col gap-1.5">
              <div className={cn("h-1 rounded-full", isDoneOrCurrent ? "bg-primary" : "bg-border")} />
              <span className={cn("text-[11.5px]", isCurrent ? "font-semibold text-foreground" : "text-muted-foreground")}>
                {label}
              </span>
            </li>
          );
        })}
      </ol>
    </CardHeader>
  );
}

export function WizardStepBody({ children, className }: { children: ReactNode; className?: string }) {
  return <div className={cn("grid grid-cols-1 gap-x-[18px] gap-y-4 sm:grid-cols-2", className)}>{children}</div>;
}
