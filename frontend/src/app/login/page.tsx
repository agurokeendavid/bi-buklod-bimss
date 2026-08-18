"use client";

import { useState, type FormEvent } from "react";
import Image from "next/image";
import { useRouter } from "next/navigation";
import { Eye, EyeOff, OctagonAlert } from "lucide-react";
import { useAuth } from "@/lib/auth-context";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

export default function LoginPage() {
  const router = useRouter();
  const { login } = useAuth();
  const [userName, setUserName] = useState("");
  const [password, setPassword] = useState("");
  const [isPasswordVisible, setIsPasswordVisible] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      await login(userName, password);
      router.push("/dashboard");
    } catch {
      // Generic message only — never confirm/deny whether the username
      // exists, matching Bimss.Api's own AuthController convention.
      setError("Invalid username or password.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="grid flex-1 grid-cols-1 lg:grid-cols-[1.05fr_1fr]">
      {/* Left panel — government/organization identity, purpose, DPA notice. No fabricated
          live stats (unauthenticated page, no public endpoint) and no role selector
          (production reads the role from JWT claims — see docs/design/BIMSS-UI-SPEC.md §5.1). */}
      <div className="relative hidden flex-col justify-between overflow-hidden bg-[#0b3b6f] px-14 py-13 text-white lg:flex">
        {/* Building photo, layered under the navy overlay — docs/design/README.md's
            "Assets" section. Sits behind the decorative circles and all content. */}
        <div
          aria-hidden="true"
          className="pointer-events-none absolute inset-0 bg-cover bg-center"
          style={{
            backgroundImage:
              "linear-gradient(180deg, rgba(11,59,111,.92), rgba(11,59,111,.86) 45%, rgba(11,59,111,.95)), url(/immigration-bg.jpg)",
          }}
        />
        <div aria-hidden="true" className="pointer-events-none absolute inset-0 overflow-hidden">
          <div className="absolute -right-28 -bottom-28 size-[440px] rounded-full border border-white/[.13]" />
          <div className="absolute -right-16 -bottom-16 size-[290px] rounded-full border border-white/10" />
        </div>

        <div className="relative flex items-center gap-3">
          <Image src="/bi-seal.png" alt="" width={46} height={46} className="shrink-0 object-contain" priority />
          <div className="flex flex-col leading-tight">
            <span className="text-[10.5px] font-semibold tracking-[.16em] text-white/72 uppercase">
              Republic of the Philippines
            </span>
            <span className="text-[14.5px] font-semibold">Bureau of Immigration · Buklod ng Kawani</span>
          </div>
        </div>

        <div className="relative flex max-w-[450px] flex-col gap-4">
          <span className="text-[10.5px] font-semibold tracking-[.2em] text-white/70 uppercase">BIMSS · Release 1.0</span>
          <h1 className="text-[41px] leading-[1.09] font-bold tracking-tighter">
            Buklod Integrated Membership and Services System
          </h1>
          <p className="text-[14.5px] leading-[1.7] text-white/82">
            One secured record per member, replacing fragmented and manual recordkeeping.
            Membership, contributions, and member services are handled in a single verified
            workflow.
          </p>
        </div>

        {/* Spacer — the mockup's stat row (active members / fund balance / offices
            covered) sat here but is dropped: this is an unauthenticated page with no
            public endpoint to source real numbers from. */}
        <div aria-hidden="true" />
      </div>

      {/* Right panel — sign-in form. */}
      <div className="flex flex-1 items-center justify-center bg-background p-4">
        <div className="flex w-full max-w-[376px] flex-col gap-6">
          <div className="flex flex-col gap-1 lg:hidden">
            <span className="text-sm font-semibold text-primary">BIMSS</span>
          </div>
          <div className="flex flex-col gap-1">
            <h2 className="text-2xl font-semibold tracking-tight">Sign in</h2>
            <p className="text-sm text-muted-foreground">
              Use your BI employee number or Buklod membership ID.
            </p>
          </div>

          <form className="flex flex-col gap-4" onSubmit={handleSubmit}>
            <div className="flex flex-col gap-2">
              <Label htmlFor="userName">Employee / Membership ID</Label>
              <Input
                id="userName"
                name="userName"
                autoComplete="username"
                required
                className="tabular-nums"
                value={userName}
                onChange={(event) => setUserName(event.target.value)}
              />
            </div>
            <div className="flex flex-col gap-2">
              <Label htmlFor="password">Password</Label>
              <div className="relative">
                <Input
                  id="password"
                  name="password"
                  type={isPasswordVisible ? "text" : "password"}
                  autoComplete="current-password"
                  required
                  className="pr-11"
                  value={password}
                  onChange={(event) => setPassword(event.target.value)}
                />
                <button
                  type="button"
                  onClick={() => setIsPasswordVisible((visible) => !visible)}
                  className="absolute inset-y-0 right-0 flex w-11 items-center justify-center text-muted-foreground hover:text-foreground"
                  aria-label={isPasswordVisible ? "Hide password" : "Show password"}
                >
                  {isPasswordVisible ? <EyeOff className="size-5" /> : <Eye className="size-5" />}
                </button>
              </div>
            </div>
            {error ? (
              <Alert variant="destructive">
                <OctagonAlert />
                <AlertDescription>{error}</AlertDescription>
              </Alert>
            ) : null}
            <Button type="submit" className="mt-2 h-10 w-full" disabled={isSubmitting}>
              {isSubmitting ? "Signing in…" : "Sign in"}
            </Button>
          </form>

          <p className="text-[11.5px] leading-[1.65] text-muted-foreground">
            All access is logged. Personal data is processed under the Data Privacy Act of 2012
            (RA 10173). Unauthorized use is subject to administrative and criminal liability.
          </p>
        </div>
      </div>
    </div>
  );
}
