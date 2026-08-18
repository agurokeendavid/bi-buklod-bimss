"use client";

import { useEffect, useState, type FormEvent } from "react";
import { useParams, useRouter } from "next/navigation";
import { toast } from "sonner";
import { OctagonAlert } from "lucide-react";
import { useAuth } from "@/lib/auth-context";
import { parseFieldErrors } from "@/lib/api-errors";
import type { MemberDetail, ReferenceDataItem, UpdateMemberRequest } from "@/lib/types/member";
import { FormSection, FormFooter, RequiredMark, FieldError } from "@/components/forms/record-form";
import { WizardHeader } from "@/components/forms/wizard";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Card, CardContent } from "@/components/ui/card";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Breadcrumbs } from "@/components/breadcrumbs";

const NO_SUFFIX = "__none__";
const WIZARD_STEPS = ["Personal information", "Employment information"];
const STEP_0_FIELDS = new Set([
  "lastName",
  "firstName",
  "middleName",
  "suffixId",
  "dateOfBirth",
  "placeOfBirth",
  "civilStatusId",
  "joiningReason",
]);

export default function EditMemberPage() {
  const params = useParams<{ id: string }>();
  const router = useRouter();
  const { fetchWithAuth } = useAuth();

  const [civilStatuses, setCivilStatuses] = useState<ReferenceDataItem[]>([]);
  const [suffixes, setSuffixes] = useState<ReferenceDataItem[]>([]);
  const [officeUnits, setOfficeUnits] = useState<ReferenceDataItem[]>([]);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [notFound, setNotFound] = useState(false);
  const [isLoaded, setIsLoaded] = useState(false);

  const [employeeNumber, setEmployeeNumber] = useState("");
  const [lastName, setLastName] = useState("");
  const [firstName, setFirstName] = useState("");
  const [middleName, setMiddleName] = useState("");
  const [suffixId, setSuffixId] = useState<string>(NO_SUFFIX);
  const [dateOfBirth, setDateOfBirth] = useState("");
  const [placeOfBirth, setPlaceOfBirth] = useState("");
  const [civilStatusId, setCivilStatusId] = useState("");
  const [joiningReason, setJoiningReason] = useState("");
  const [positionDesignation, setPositionDesignation] = useState("");
  const [officeUnitId, setOfficeUnitId] = useState("");
  const [permanentAppointmentDate, setPermanentAppointmentDate] = useState("");

  const [step, setStep] = useState(0);
  const [isDirty, setIsDirty] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    let cancelled = false;

    async function loadData() {
      const [memberResponse, civilStatusesResponse, suffixesResponse, officeUnitsResponse] = await Promise.all([
        fetchWithAuth(`/api/members/${params.id}`),
        fetchWithAuth("/api/reference-data/civil-statuses"),
        fetchWithAuth("/api/reference-data/suffixes"),
        fetchWithAuth("/api/reference-data/office-units"),
      ]);

      if (cancelled) {
        return;
      }

      if (memberResponse.status === 404) {
        setNotFound(true);
        return;
      }

      if (!memberResponse.ok || !civilStatusesResponse.ok || !suffixesResponse.ok || !officeUnitsResponse.ok) {
        setLoadError("Failed to load member data.");
        return;
      }

      const member = (await memberResponse.json()) as MemberDetail;
      setCivilStatuses((await civilStatusesResponse.json()) as ReferenceDataItem[]);
      setSuffixes((await suffixesResponse.json()) as ReferenceDataItem[]);
      setOfficeUnits((await officeUnitsResponse.json()) as ReferenceDataItem[]);

      setEmployeeNumber(member.employeeNumber ?? "");
      setLastName(member.lastName);
      setFirstName(member.firstName);
      setMiddleName(member.middleName ?? "");
      setSuffixId(member.suffixId ?? NO_SUFFIX);
      setDateOfBirth(member.dateOfBirth);
      setPlaceOfBirth(member.placeOfBirth);
      setCivilStatusId(member.civilStatusId);
      setJoiningReason(member.joiningReason ?? "");
      setPositionDesignation(member.positionDesignation ?? "");
      setOfficeUnitId(member.officeUnitId ?? "");
      setPermanentAppointmentDate(member.permanentAppointmentDate ?? "");
      setIsLoaded(true);
    }

    void loadData();

    return () => {
      cancelled = true;
    };
  }, [fetchWithAuth, params.id]);

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

  function set<T>(setter: (value: T) => void, field: string) {
    return (value: T) => {
      setIsDirty(true);
      clearFieldError(field);
      setter(value);
    };
  }

  function handleBack() {
    if (isDirty && !window.confirm("Discard your changes to this member record?")) {
      return;
    }
    router.push(`/dashboard/members/${params.id}`);
  }

  // Same reasoning as the new-member wizard: Personal information's required
  // fields aren't in the DOM once step 1 is showing, so they need an explicit
  // check before advancing (and a way back if the API rejects one on submit).
  function validateStep0(): boolean {
    const missing: Record<string, string> = {};
    if (!lastName.trim()) missing.lastName = "Last name is required.";
    if (!firstName.trim()) missing.firstName = "First name is required.";
    if (!dateOfBirth) missing.dateOfBirth = "Date of birth is required.";
    if (!placeOfBirth.trim()) missing.placeOfBirth = "Place of birth is required.";
    if (!civilStatusId) missing.civilStatusId = "Civil status is required.";

    if (Object.keys(missing).length > 0) {
      setFieldErrors((current) => ({ ...current, ...missing }));
      return false;
    }
    return true;
  }

  function handleContinue() {
    if (validateStep0()) {
      setStep(1);
    }
  }

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    // Defensive: step 0's "Continue" is type="button", but some browsers
    // still fire a native form submit right after it advances the step
    // (observed even with no submit()/requestSubmit() call in the JS call
    // stack). Refuse to persist anything unless the wizard is genuinely on
    // its last step, regardless of what triggered this submit event.
    if (step !== WIZARD_STEPS.length - 1) {
      return;
    }
    setSubmitError(null);
    setFieldErrors({});
    setIsSubmitting(true);

    try {
      const request: UpdateMemberRequest = {
        lastName,
        firstName,
        middleName: middleName || null,
        suffixId: suffixId === NO_SUFFIX ? null : suffixId,
        dateOfBirth,
        placeOfBirth,
        civilStatusId,
        joiningReason: joiningReason || null,
        positionDesignation,
        officeUnitId,
        permanentAppointmentDate: permanentAppointmentDate || null,
      };

      const response = await fetchWithAuth(`/api/members/${params.id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(request),
      });

      if (response.status === 400) {
        const errors = await parseFieldErrors(response);
        setFieldErrors(errors);
        setSubmitError("Please fix the highlighted fields.");
        if (Object.keys(errors).some((field) => STEP_0_FIELDS.has(field))) {
          setStep(0);
        }
        return;
      }

      if (!response.ok) {
        setSubmitError(`Failed to update member (${response.status}).`);
        return;
      }

      setIsDirty(false);
      toast.success("Member updated.");
      router.push(`/dashboard/members/${params.id}`);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="flex flex-col gap-4">
      <Breadcrumbs
        items={[
          { label: "Dashboard", href: "/dashboard" },
          { label: "Members", href: "/dashboard/members" },
          ...(isLoaded ? [{ label: `${lastName}, ${firstName}`, href: `/dashboard/members/${params.id}` }] : []),
          { label: "Edit" },
        ]}
      />

      <Card className="rounded-xl shadow-none">
        <WizardHeader
          title="Edit member"
          meta="BI Employee Number cannot be changed here. Fields marked * are required."
          steps={WIZARD_STEPS}
          currentStep={step}
        />
      </Card>

      <Card className="rounded-xl shadow-none">
        <CardContent>
          {notFound ? (
            <p className="text-sm text-muted-foreground">Member not found.</p>
          ) : loadError ? (
            <p className="text-sm text-destructive">{loadError}</p>
          ) : !isLoaded ? (
            <p className="text-sm text-muted-foreground">Loading…</p>
          ) : (
            step === 0 ? (
            // Deliberately a <div>, not a <form>: on some browsers, clicking
            // this step's "Continue" (type="button") was observed to also
            // trigger a native form submit once the DOM advanced to step 1 —
            // even with no submit()/requestSubmit() call anywhere in the JS
            // call stack, and reproducing with extensions disabled. Only
            // wrapping the final step in a real <form> removes any element
            // for that native behavior to act on.
            <div className="flex flex-col gap-8">
              <FormSection
                title="Personal information"
                description="Legal identity as it will appear on the membership record."
              >
                <div className="flex flex-col gap-2">
                  <Label htmlFor="lastName">
                    Last name
                    <RequiredMark />
                  </Label>
                  <Input
                    id="lastName"
                    required
                    aria-invalid={!!fieldErrors.lastName}
                    value={lastName}
                    onChange={(event) => set(setLastName, "lastName")(event.target.value)}
                  />
                  <FieldError message={fieldErrors.lastName} />
                </div>
                <div className="flex flex-col gap-2">
                  <Label htmlFor="firstName">
                    First name
                    <RequiredMark />
                  </Label>
                  <Input
                    id="firstName"
                    required
                    aria-invalid={!!fieldErrors.firstName}
                    value={firstName}
                    onChange={(event) => set(setFirstName, "firstName")(event.target.value)}
                  />
                  <FieldError message={fieldErrors.firstName} />
                </div>
                <div className="flex flex-col gap-2">
                  <Label htmlFor="middleName">Middle name</Label>
                  <Input
                    id="middleName"
                    value={middleName}
                    onChange={(event) => set(setMiddleName, "middleName")(event.target.value)}
                  />
                  <FieldError message={fieldErrors.middleName} />
                </div>
                <div className="flex flex-col gap-2">
                  <Label htmlFor="suffix">Suffix</Label>
                  <Select value={suffixId} onValueChange={(value) => set(setSuffixId, "suffixId")(value ?? NO_SUFFIX)}>
                    <SelectTrigger id="suffix">
                      <SelectValue placeholder="None">
                        {(value) => suffixes.find((item) => item.id === value)?.name ?? "None"}
                      </SelectValue>
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value={NO_SUFFIX}>None</SelectItem>
                      {suffixes.map((item) => (
                        <SelectItem key={item.id} value={item.id}>
                          {item.name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <FieldError message={fieldErrors.suffixId} />
                </div>
                <div className="flex flex-col gap-2">
                  <Label htmlFor="dateOfBirth">
                    Date of birth
                    <RequiredMark />
                  </Label>
                  <Input
                    id="dateOfBirth"
                    type="date"
                    required
                    aria-invalid={!!fieldErrors.dateOfBirth}
                    value={dateOfBirth}
                    onChange={(event) => set(setDateOfBirth, "dateOfBirth")(event.target.value)}
                  />
                  <FieldError message={fieldErrors.dateOfBirth} />
                </div>
                <div className="flex flex-col gap-2">
                  <Label htmlFor="placeOfBirth">
                    Place of birth
                    <RequiredMark />
                  </Label>
                  <Input
                    id="placeOfBirth"
                    required
                    aria-invalid={!!fieldErrors.placeOfBirth}
                    value={placeOfBirth}
                    onChange={(event) => set(setPlaceOfBirth, "placeOfBirth")(event.target.value)}
                  />
                  <FieldError message={fieldErrors.placeOfBirth} />
                </div>
                <div className="flex flex-col gap-2">
                  <Label htmlFor="civilStatus">
                    Civil status
                    <RequiredMark />
                  </Label>
                  <Select
                    value={civilStatusId}
                    onValueChange={(value) => set(setCivilStatusId, "civilStatusId")(value ?? "")}
                    required
                  >
                    <SelectTrigger id="civilStatus">
                      <SelectValue placeholder="Select civil status">
                        {(value) => civilStatuses.find((item) => item.id === value)?.name ?? "Select civil status"}
                      </SelectValue>
                    </SelectTrigger>
                    <SelectContent>
                      {civilStatuses.map((item) => (
                        <SelectItem key={item.id} value={item.id}>
                          {item.name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <FieldError message={fieldErrors.civilStatusId} />
                </div>
                <div className="flex flex-col gap-2 sm:col-span-2">
                  <Label htmlFor="joiningReason">Reason for joining Buklod</Label>
                  <Textarea
                    id="joiningReason"
                    value={joiningReason}
                    onChange={(event) => set(setJoiningReason, "joiningReason")(event.target.value)}
                  />
                  <FieldError message={fieldErrors.joiningReason} />
                </div>
              </FormSection>

              {submitError ? (
                <Alert variant="destructive">
                  <OctagonAlert />
                  <AlertDescription>{submitError}</AlertDescription>
                </Alert>
              ) : null}

              <FormFooter>
                <Button type="button" variant="outline" onClick={handleBack}>
                  Cancel
                </Button>
                <div className="flex-1" />
                {/* Not wired to real draft persistence yet — see the same note on
                    the new-member wizard's footer. */}
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => toast.info("Saving drafts isn't available yet — coming in a later phase.")}
                >
                  Save as draft
                </Button>
                <Button type="button" onClick={handleContinue}>
                  Continue to {WIZARD_STEPS[1]}
                </Button>
              </FormFooter>
            </div>
            ) : (
            <form className="flex flex-col gap-8" onSubmit={handleSubmit}>
              <FormSection
                title="Employment information"
                description="BI Employee Number cannot be changed here."
              >
                <div className="flex flex-col gap-2">
                  <Label htmlFor="employeeNumber">BI employee number</Label>
                  <Input id="employeeNumber" value={employeeNumber} disabled />
                </div>
                <div className="flex flex-col gap-2">
                  <Label htmlFor="positionDesignation">
                    Position / designation
                    <RequiredMark />
                  </Label>
                  <Input
                    id="positionDesignation"
                    required
                    aria-invalid={!!fieldErrors.positionDesignation}
                    value={positionDesignation}
                    onChange={(event) => set(setPositionDesignation, "positionDesignation")(event.target.value)}
                  />
                  <FieldError message={fieldErrors.positionDesignation} />
                </div>
                <div className="flex flex-col gap-2">
                  <Label htmlFor="officeUnit">
                    Office unit
                    <RequiredMark />
                  </Label>
                  <Select
                    value={officeUnitId}
                    onValueChange={(value) => set(setOfficeUnitId, "officeUnitId")(value ?? "")}
                    required
                  >
                    <SelectTrigger id="officeUnit">
                      <SelectValue placeholder="Select office unit">
                        {(value) => officeUnits.find((item) => item.id === value)?.name ?? "Select office unit"}
                      </SelectValue>
                    </SelectTrigger>
                    <SelectContent>
                      {officeUnits.map((item) => (
                        <SelectItem key={item.id} value={item.id}>
                          {item.name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <FieldError message={fieldErrors.officeUnitId} />
                </div>
                <div className="flex flex-col gap-2">
                  <Label htmlFor="permanentAppointmentDate">Permanent appointment date</Label>
                  <Input
                    id="permanentAppointmentDate"
                    type="date"
                    value={permanentAppointmentDate}
                    onChange={(event) => set(setPermanentAppointmentDate, "permanentAppointmentDate")(event.target.value)}
                  />
                  <FieldError message={fieldErrors.permanentAppointmentDate} />
                </div>
              </FormSection>

              {submitError ? (
                <Alert variant="destructive">
                  <OctagonAlert />
                  <AlertDescription>{submitError}</AlertDescription>
                </Alert>
              ) : null}

              <FormFooter>
                <Button type="button" variant="outline" onClick={() => setStep(0)}>
                  Back
                </Button>
                <div className="flex-1" />
                {/* Not wired to real draft persistence yet — see the same note on
                    the new-member wizard's footer. */}
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => toast.info("Saving drafts isn't available yet — coming in a later phase.")}
                >
                  Save as draft
                </Button>
                <Button type="submit" disabled={isSubmitting}>
                  {isSubmitting ? "Saving…" : "Save changes"}
                </Button>
              </FormFooter>
            </form>
            )
          )}
        </CardContent>
      </Card>
    </div>
  );
}
