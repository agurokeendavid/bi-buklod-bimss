"use client";

import { useEffect, useState, type FormEvent } from "react";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { OctagonAlert } from "lucide-react";
import { useAuth } from "@/lib/auth-context";
import { parseFieldErrors } from "@/lib/api-errors";
import type { CreateMemberRequest, ReferenceDataItem } from "@/lib/types/member";
import { Breadcrumbs } from "@/components/breadcrumbs";
import { FormSection, FormFooter, RequiredMark, FieldError } from "@/components/forms/record-form";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Skeleton } from "@/components/ui/skeleton";
import { Textarea } from "@/components/ui/textarea";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";

const NO_SUFFIX = "__none__";

export default function NewMemberPage() {
  const router = useRouter();
  const { fetchWithAuth } = useAuth();

  const [civilStatuses, setCivilStatuses] = useState<ReferenceDataItem[]>([]);
  const [suffixes, setSuffixes] = useState<ReferenceDataItem[]>([]);
  const [officeUnits, setOfficeUnits] = useState<ReferenceDataItem[]>([]);
  const [isLoadingReferenceData, setIsLoadingReferenceData] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);

  const [lastName, setLastName] = useState("");
  const [firstName, setFirstName] = useState("");
  const [middleName, setMiddleName] = useState("");
  const [suffixId, setSuffixId] = useState<string>(NO_SUFFIX);
  const [dateOfBirth, setDateOfBirth] = useState("");
  const [placeOfBirth, setPlaceOfBirth] = useState("");
  const [civilStatusId, setCivilStatusId] = useState("");
  const [joiningReason, setJoiningReason] = useState("");
  const [employeeNumber, setEmployeeNumber] = useState("");
  const [positionDesignation, setPositionDesignation] = useState("");
  const [officeUnitId, setOfficeUnitId] = useState("");
  const [permanentAppointmentDate, setPermanentAppointmentDate] = useState("");

  const [isDirty, setIsDirty] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    let cancelled = false;

    async function loadReferenceData() {
      const [civilStatusesResponse, suffixesResponse, officeUnitsResponse] = await Promise.all([
        fetchWithAuth("/api/reference-data/civil-statuses"),
        fetchWithAuth("/api/reference-data/suffixes"),
        fetchWithAuth("/api/reference-data/office-units"),
      ]);

      if (cancelled) {
        return;
      }

      if (!civilStatusesResponse.ok || !suffixesResponse.ok || !officeUnitsResponse.ok) {
        setLoadError("Failed to load reference data.");
        setIsLoadingReferenceData(false);
        return;
      }

      setCivilStatuses((await civilStatusesResponse.json()) as ReferenceDataItem[]);
      setSuffixes((await suffixesResponse.json()) as ReferenceDataItem[]);
      setOfficeUnits((await officeUnitsResponse.json()) as ReferenceDataItem[]);
      setIsLoadingReferenceData(false);
    }

    void loadReferenceData();

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

  function set<T>(setter: (value: T) => void, field: string) {
    return (value: T) => {
      setIsDirty(true);
      clearFieldError(field);
      setter(value);
    };
  }

  function handleCancel() {
    if (isDirty && !window.confirm("Discard this member record? Your changes will be lost.")) {
      return;
    }
    router.push("/dashboard/members");
  }

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setSubmitError(null);
    setFieldErrors({});
    setIsSubmitting(true);

    try {
      const request: CreateMemberRequest = {
        lastName,
        firstName,
        middleName: middleName || null,
        suffixId: suffixId === NO_SUFFIX ? null : suffixId,
        dateOfBirth,
        placeOfBirth,
        civilStatusId,
        joiningReason: joiningReason || null,
        employeeNumber,
        positionDesignation,
        officeUnitId,
        permanentAppointmentDate: permanentAppointmentDate || null,
      };

      const response = await fetchWithAuth("/api/members", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(request),
      });

      if (response.status === 409) {
        setSubmitError("That employee number is already registered to another member.");
        return;
      }

      if (response.status === 400) {
        const errors = await parseFieldErrors(response);
        setFieldErrors(errors);
        setSubmitError("Please fix the highlighted fields.");
        return;
      }

      if (!response.ok) {
        setSubmitError(`Failed to create member (${response.status}).`);
        return;
      }

      const body = (await response.json()) as { id: string };
      setIsDirty(false);
      toast.success("Member created.");
      router.push(`/dashboard/members/${body.id}`);
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
          { label: "New member" },
        ]}
      />

      <Card className="mx-auto w-full max-w-[980px] rounded-xl shadow-none">
        <CardHeader>
          <CardTitle className="text-[14.5px] font-semibold">Create member</CardTitle>
          <CardDescription>
            Core identity and employment information. BI Employee Number is mandatory and unique. Fields marked
            <RequiredMark /> are required.
          </CardDescription>
        </CardHeader>
        <CardContent>
          {loadError ? (
            <Alert variant="destructive">
              <OctagonAlert />
              <AlertDescription>{loadError}</AlertDescription>
            </Alert>
          ) : isLoadingReferenceData ? (
            <div className="flex flex-col gap-8">
              <div className="grid grid-cols-1 gap-x-[18px] gap-y-4 sm:grid-cols-2">
                {Array.from({ length: 6 }).map((_, index) => (
                  <Skeleton key={index} className="h-[62px]" />
                ))}
              </div>
              <div className="grid grid-cols-1 gap-x-[18px] gap-y-4 sm:grid-cols-2">
                {Array.from({ length: 4 }).map((_, index) => (
                  <Skeleton key={index} className="h-[62px]" />
                ))}
              </div>
            </div>
          ) : (
            <form className="flex flex-col gap-8" onSubmit={handleSubmit}>
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

              <FormSection
                title="Employment information"
                description="Verified against BI personnel records where applicable."
              >
                <div className="flex flex-col gap-2">
                  <Label htmlFor="employeeNumber">
                    BI employee number
                    <RequiredMark />
                  </Label>
                  <Input
                    id="employeeNumber"
                    required
                    aria-invalid={!!fieldErrors.employeeNumber}
                    value={employeeNumber}
                    onChange={(event) => set(setEmployeeNumber, "employeeNumber")(event.target.value)}
                  />
                  <FieldError message={fieldErrors.employeeNumber} />
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
                <Button type="button" variant="outline" onClick={handleCancel}>
                  Cancel
                </Button>
                <div className="flex-1" />
                <Button type="submit" disabled={isSubmitting}>
                  {isSubmitting ? "Creating…" : "Create member"}
                </Button>
              </FormFooter>
            </form>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
