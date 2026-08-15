"use client";

import { useEffect, useState, type FormEvent } from "react";
import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import { toast } from "sonner";
import { useAuth } from "@/lib/auth-context";
import type { MemberDetail, ReferenceDataItem, UpdateMemberRequest } from "@/lib/types/member";
import { Button, buttonVariants } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { cn } from "@/lib/utils";

const NO_SUFFIX = "__none__";

function RequiredMark() {
  return (
    <span aria-hidden="true" className="text-destructive">
      {" "}
      *
    </span>
  );
}

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

  const [submitError, setSubmitError] = useState<string | null>(null);
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

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setSubmitError(null);
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

      if (!response.ok) {
        setSubmitError(`Failed to update member (${response.status}).`);
        return;
      }

      toast.success("Member updated.");
      router.push(`/dashboard/members/${params.id}`);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="flex flex-col gap-4">
      <Link
        href={`/dashboard/members/${params.id}`}
        className={cn(buttonVariants({ variant: "outline", size: "sm" }), "w-fit")}
      >
        ← Back to member
      </Link>

      <Card>
        <CardHeader>
          <CardTitle>Edit member</CardTitle>
          <CardDescription>
            Core identity and employment information. BI Employee Number cannot be changed here. Fields marked
            <RequiredMark /> are required.
          </CardDescription>
        </CardHeader>
        <CardContent>
          {notFound ? (
            <p className="text-sm text-muted-foreground">Member not found.</p>
          ) : loadError ? (
            <p className="text-sm text-destructive">{loadError}</p>
          ) : !isLoaded ? (
            <p className="text-sm text-muted-foreground">Loading…</p>
          ) : (
            <form className="flex flex-col gap-4" onSubmit={handleSubmit}>
              <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
                <div className="flex flex-col gap-2">
                  <Label htmlFor="employeeNumber">BI employee number</Label>
                  <Input id="employeeNumber" value={employeeNumber} disabled />
                </div>
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
                {isSubmitting ? "Saving…" : "Save changes"}
              </Button>
            </form>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
