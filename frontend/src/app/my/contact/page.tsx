"use client";

import { useEffect, useState, type FormEvent } from "react";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { OctagonAlert } from "lucide-react";
import { useAuth } from "@/lib/auth-context";
import { parseFieldErrors } from "@/lib/api-errors";
import type { MyContact, UpdateMyContactRequest } from "@/lib/types/member";
import { FormSection, FormFooter, RequiredMark, FieldError } from "@/components/forms/record-form";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Skeleton } from "@/components/ui/skeleton";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

export default function MemberContactPage() {
  const router = useRouter();
  const { fetchWithAuth } = useAuth();

  const [loadError, setLoadError] = useState<string | null>(null);
  const [notLinked, setNotLinked] = useState(false);
  const [isLoaded, setIsLoaded] = useState(false);

  const [landline, setLandline] = useState("");
  const [mobileNumber, setMobileNumber] = useState("");
  const [email, setEmail] = useState("");
  const [presentAddress, setPresentAddress] = useState("");
  const [permanentAddress, setPermanentAddress] = useState("");

  const [isDirty, setIsDirty] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    let cancelled = false;

    async function loadContact() {
      const response = await fetchWithAuth("/api/my/contact");
      if (cancelled) {
        return;
      }

      if (response.status === 404) {
        setNotLinked(true);
        return;
      }

      if (!response.ok) {
        setLoadError(`Failed to load your contact info (${response.status}).`);
        return;
      }

      const contact = (await response.json()) as MyContact;
      setLandline(contact.landline ?? "");
      setMobileNumber(contact.mobileNumber ?? "");
      setEmail(contact.email ?? "");
      setPresentAddress(contact.presentAddress ?? "");
      setPermanentAddress(contact.permanentAddress ?? "");
      setIsLoaded(true);
    }

    void loadContact();

    return () => {
      cancelled = true;
    };
  }, [fetchWithAuth]);

  useEffect(() => {
    if (!isDirty) {
      return;
    }

    function handleBeforeUnload(event: BeforeUnloadEvent) {
      event.preventDefault();
    }

    window.addEventListener("beforeunload", handleBeforeUnload);
    return () => window.removeEventListener("beforeunload", handleBeforeUnload);
  }, [isDirty]);

  function clearFieldError(field: string) {
    setFieldErrors((current) => {
      if (!(field in current)) {
        return current;
      }
      const next = { ...current };
      delete next[field];
      return next;
    });
  }

  function set(setter: (value: string) => void, field: string) {
    return (value: string) => {
      setIsDirty(true);
      clearFieldError(field);
      setter(value);
    };
  }

  function handleCancel() {
    if (isDirty && !window.confirm("Discard these changes?")) {
      return;
    }
    router.push("/my");
  }

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setSubmitError(null);
    setFieldErrors({});
    setIsSubmitting(true);

    try {
      const request: UpdateMyContactRequest = {
        landline: landline || null,
        mobileNumber,
        email,
        presentAddress: presentAddress || null,
        permanentAddress: permanentAddress || null,
      };

      const response = await fetchWithAuth("/api/my/contact", {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(request),
      });

      if (response.status === 400) {
        const problem = await response
          .clone()
          .json()
          .catch(() => null);
        const errors = await parseFieldErrors(response);
        setFieldErrors(errors);
        setSubmitError((problem?.detail as string | undefined) ?? "Please fix the highlighted fields.");
        return;
      }

      if (!response.ok) {
        setSubmitError(`Failed to update your contact info (${response.status}).`);
        return;
      }

      setIsDirty(false);
      toast.success("Your contact info was updated.");
      router.push("/my");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="mx-auto flex w-full max-w-[980px] flex-col">
      <Card className="rounded-xl shadow-none">
        <CardHeader>
          <CardTitle className="text-[14.5px] font-semibold">Update contact info</CardTitle>
          <CardDescription>
            Contact info takes effect immediately — no officer review needed. Fields marked
            <RequiredMark /> are required.
          </CardDescription>
        </CardHeader>
        <CardContent>
          {notLinked ? (
            <p className="text-sm text-muted-foreground">
              Your account isn&apos;t linked to a membership record yet. Contact a Membership Officer to set this up.
            </p>
          ) : loadError ? (
            <Alert variant="destructive">
              <OctagonAlert />
              <AlertDescription>{loadError}</AlertDescription>
            </Alert>
          ) : !isLoaded ? (
            <div className="grid grid-cols-1 gap-x-[18px] gap-y-4 sm:grid-cols-2">
              {Array.from({ length: 5 }).map((_, index) => (
                <Skeleton key={index} className="h-[62px]" />
              ))}
            </div>
          ) : (
            <form className="flex flex-col gap-8" onSubmit={handleSubmit}>
              <FormSection title="Contact information">
                <div className="flex flex-col gap-2">
                  <Label htmlFor="landline">Landline</Label>
                  <Input
                    id="landline"
                    aria-invalid={!!fieldErrors.landline}
                    value={landline}
                    onChange={(event) => set(setLandline, "landline")(event.target.value)}
                  />
                  <FieldError message={fieldErrors.landline} />
                </div>
                <div className="flex flex-col gap-2">
                  <Label htmlFor="mobileNumber">
                    Mobile number
                    <RequiredMark />
                  </Label>
                  <Input
                    id="mobileNumber"
                    required
                    aria-invalid={!!fieldErrors.mobileNumber}
                    value={mobileNumber}
                    onChange={(event) => set(setMobileNumber, "mobileNumber")(event.target.value)}
                  />
                  <FieldError message={fieldErrors.mobileNumber} />
                </div>
                <div className="flex flex-col gap-2 sm:col-span-2">
                  <Label htmlFor="email">
                    Email
                    <RequiredMark />
                  </Label>
                  <Input
                    id="email"
                    type="email"
                    required
                    aria-invalid={!!fieldErrors.email}
                    value={email}
                    onChange={(event) => set(setEmail, "email")(event.target.value)}
                  />
                  <FieldError message={fieldErrors.email} />
                </div>
                <div className="flex flex-col gap-2">
                  <Label htmlFor="presentAddress">Present address</Label>
                  <Input
                    id="presentAddress"
                    aria-invalid={!!fieldErrors.presentAddress}
                    value={presentAddress}
                    onChange={(event) => set(setPresentAddress, "presentAddress")(event.target.value)}
                  />
                  <FieldError message={fieldErrors.presentAddress} />
                </div>
                <div className="flex flex-col gap-2">
                  <Label htmlFor="permanentAddress">Permanent address</Label>
                  <Input
                    id="permanentAddress"
                    aria-invalid={!!fieldErrors.permanentAddress}
                    value={permanentAddress}
                    onChange={(event) => set(setPermanentAddress, "permanentAddress")(event.target.value)}
                  />
                  <FieldError message={fieldErrors.permanentAddress} />
                </div>
              </FormSection>

              {submitError ? (
                <Alert variant="destructive">
                  <OctagonAlert />
                  <AlertDescription>{submitError}</AlertDescription>
                </Alert>
              ) : null}

              <FormFooter>
                <Button type="button" variant="outline" onClick={handleCancel}>
                  Cancel
                </Button>
                <div className="flex-1" />
                <Button type="submit" disabled={isSubmitting}>
                  {isSubmitting ? "Saving…" : "Save changes"}
                </Button>
              </FormFooter>
            </form>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
