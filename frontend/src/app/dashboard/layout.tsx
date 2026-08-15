"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import type { ReactNode } from "react";
import { useAuth } from "@/lib/auth-context";
import { NavHeader } from "@/components/nav-header";

export default function DashboardLayout({ children }: { children: ReactNode }) {
  const router = useRouter();
  const { accessToken, isLoading } = useAuth();

  useEffect(() => {
    if (!isLoading && !accessToken) {
      router.replace("/login");
    }
  }, [isLoading, accessToken, router]);

  if (isLoading || !accessToken) {
    return null;
  }

  return (
    <div className="flex flex-1 flex-col">
      <NavHeader />
      <main className="mx-auto w-full max-w-6xl flex-1 p-4">{children}</main>
    </div>
  );
}
