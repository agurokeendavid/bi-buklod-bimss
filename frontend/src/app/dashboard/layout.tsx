"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import type { ReactNode } from "react";
import { useAuth } from "@/lib/auth-context";
import { AppSidebar } from "@/components/app-sidebar";
import { AppHeader } from "@/components/app-header";

export default function DashboardLayout({ children }: { children: ReactNode }) {
  const router = useRouter();
  const { accessToken, isLoading } = useAuth();
  const [isMobileNavOpen, setIsMobileNavOpen] = useState(false);

  useEffect(() => {
    if (!isLoading && !accessToken) {
      router.replace("/login");
    }
  }, [isLoading, accessToken, router]);

  if (isLoading || !accessToken) {
    return null;
  }

  return (
    <div className="flex flex-1 md:flex-row">
      <AppSidebar isMobileOpen={isMobileNavOpen} onMobileOpenChange={setIsMobileNavOpen} />
      <div className="flex flex-1 flex-col bg-app-bg">
        <AppHeader onOpenMobileNav={() => setIsMobileNavOpen(true)} />
        <main className="w-full flex-1 px-[18px] pt-5 pb-12 md:px-[22px]">{children}</main>
      </div>
    </div>
  );
}
