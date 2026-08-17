"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import type { ReactNode } from "react";
import { useAuth } from "@/lib/auth-context";
import { MemberHeader } from "@/components/member-header";

// Member self-service shell — its own route group, distinct from
// /dashboard's officer-facing admin layout (no sidebar, no membership
// register access). Authorization for any actual member data lives
// server-side on each endpoint (Permission.Membership.ViewSelf/ManageSelf,
// added starting BIMSS-040) — this layout only gates on being signed in,
// the same client-side check /dashboard/layout.tsx already uses; a
// client-side check is never itself the authorization boundary (AGENTS.md).
export default function MemberLayout({ children }: { children: ReactNode }) {
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
    <div className="flex min-h-screen flex-col bg-app-bg">
      <MemberHeader />
      <main className="mx-auto w-full max-w-[760px] flex-1 px-[18px] pt-6 pb-12 md:px-[22px]">{children}</main>
    </div>
  );
}
