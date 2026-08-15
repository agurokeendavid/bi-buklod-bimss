"use client";

import { useEffect, useState, type FormEvent } from "react";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { useAuth } from "@/lib/auth-context";
import type { CreateMemberRequest, ReferenceDataItem } from "@/lib/types/member";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";

const NO_SUFFIX = "__none__";

function RequiredMark() {
  return (
    <span aria-hidden="true" className="text-destructive">
      {" "}
      *
    </span>
  );
}

export default function NewMemberPage() {
  const router = useRouter();
  const { fetchWithAuth } = useAuth();

  const [civilStatuses, setCivilStatuses] = useState<ReferenceDataItem[]>([]);
  const [suffixes, setSuffixes] = useState<ReferenceDataItem[]>([]);
  const [officeUnits, setOfficeUnits] = useState<ReferenceDataItem[]>([]);
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

  const [submitError, setSubmitError] = useState<string | null>(null);
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
        return;
      }

      setCivilStatuses((await civilStatusesResponse.json()) as ReferenceDataItem[]);
      setSuffixes((await suffixesResponse.json()) as ReferenceDataItem[]);
      setOfficeUnits((await officeUnitsResponse.json()) as ReferenceDataItem[]);
    }

    void loadReferenceData();

    return () => {
      cancelled = true;
    };
  }, [fetchWithAuth]);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setSubmitError(null);
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

      if (!response.ok) {
        setSubmitError(`Failed to create member (${response.status}).`);
        return;
      }

      const body = (await response.json()) as { id: string };
      toast.success("Member created.");
      router.push(`/dashboard/members/${body.id}`);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>Create member</CardTitle>
        <CardDescription>
          Core identity and employment information. BI Employee Number is mandatory and unique. Fields marked
          <RequiredMark /> are required.
        </CardDescription>
      </CardHeader>
      <CardContent>
        {loadError ? (
          <p className="text-sm text-destructive">{loadError}</p>
        ) : (
          <form className="flex flex-col gap-4" onSubmit={handleSubmit}>
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
              <div className="flex flex-col gap-2">
                <Label htmlFor="lastName">
                  Last name
                  <RequiredMark />
                </Label>
                <Input id="lastName" required value={lastName} onChange={(event) => setLastName(event.target.value)} />
              </div>
              <div className="flex flex-col gap-2">
                <Label htmlFor="firstName">
                  First name
                  <RequiredMark />
                </Label>
                <Input id="firstName" required value={firstName} onChange={(event) => setFirstName(event.target.value)} />
              </div>
              <div className="flex flex-col gap-2">
                <Label htmlFor="middleName">Middle name</Label>
                <Input id="middleName" value={middleName} onChange={(event) => setMiddleName(event.target.value)} />
              </div>

              <div className="flex flex-col gap-2">
                <Label htmlFor="suffix">Suffix</Label>
                <Select value={suffixId} onValueChange={(value) => setSuffixId(value ?? NO_SUFFIX)}>
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
                  value={dateOfBirth}
                  onChange={(event) => setDateOfBirth(event.target.value)}
                />
              </div>
              <div className="flex flex-col gap-2">
                <Label htmlFor="placeOfBirth">
                  Place of birth
                  <RequiredMark />
                </Label>
                <Input
                  id="placeOfBirth"
                  required
                  value={placeOfBirth}
                  onChange={(event) => setPlaceOfBirth(event.target.value)}
                />
              </div>

              <div className="flex flex-col gap-2">
                <Label htmlFor="civilStatus">
                  Civil status
                  <RequiredMark />
                </Label>
                <Select value={civilStatusId} onValueChange={(value) => setCivilStatusId(value ?? "")} required>
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
              </div>
              <div className="flex flex-col gap-2">
                <Label htmlFor="employeeNumber">
                  BI employee number
                  <RequiredMark />
                </Label>
                <Input
                  id="employeeNumber"
                  required
                  value={employeeNumber}
                  onChange={(event) => setEmployeeNumber(event.target.value)}
                />
              </div>
              <div className="flex flex-col gap-2">
                <Label htmlFor="positionDesignation">
                  Position / designation
                  <RequiredMark />
                </Label>
                <Input
                  id="positionDesignation"
                  required
                  value={positionDesignation}
                  onChange={(event) => setPositionDesignation(event.target.value)}
                />
              </div>

              <div className="flex flex-col gap-2">
                <Label htmlFor="officeUnit">
                  Office unit
                  <RequiredMark />
                </Label>
                <Select value={officeUnitId} onValueChange={(value) => setOfficeUnitId(value ?? "")} required>
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
              </div>
              <div className="flex flex-col gap-2">
                <Label htmlFor="permanentAppointmentDate">Permanent appointment date</Label>
                <Input
                  id="permanentAppointmentDate"
                  type="date"
                  value={permanentAppointmentDate}
                  onChange={(event) => setPermanentAppointmentDate(event.target.value)}
                />
              </div>
            </div>

            <div className="flex flex-col gap-2">
              <Label htmlFor="joiningReason">Reason for joining Buklod</Label>
              <Textarea
                id="joiningReason"
                value={joiningReason}
                onChange={(event) => setJoiningReason(event.target.value)}
              />
            </div>

            {submitError ? (
              <p role="alert" className="text-sm text-destructive">
                {submitError}
              </p>
            ) : null}

            <Button type="submit" disabled={isSubmitting} className="w-fit">
              {isSubmitting ? "Creating…" : "Create member"}
            </Button>
          </form>
        )}
      </CardContent>
    </Card>
  );
}
