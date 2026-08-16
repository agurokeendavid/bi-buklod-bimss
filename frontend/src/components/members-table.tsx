"use client";

import { useMemo, useState } from "react";
import Link from "next/link";
import {
  flexRender,
  getCoreRowModel,
  getFilteredRowModel,
  getPaginationRowModel,
  getSortedRowModel,
  useReactTable,
  type ColumnDef,
  type ColumnFiltersState,
  type RowSelectionState,
  type SortingState,
} from "@tanstack/react-table";
import { ArrowDown, ArrowUp, ArrowUpDown } from "lucide-react";
import { toast } from "sonner";
import { useAuth } from "@/lib/auth-context";
import type { MemberStatus, MemberSummary } from "@/lib/types/member";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Input } from "@/components/ui/input";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { memberStatusBadgeClassName } from "@/lib/member-status";
import { cn } from "@/lib/utils";

interface FilterPill {
  value: MemberStatus | "All";
  label: string;
}

const FILTER_PILLS: FilterPill[] = [
  { value: "All", label: "All members" },
  { value: "Active", label: "Active" },
  { value: "PendingVerification", label: "Pending verification" },
  { value: "Inactive", label: "Inactive" },
];

function initialsFor(member: MemberSummary): string {
  return `${member.firstName.charAt(0)}${member.lastName.charAt(0)}`.toUpperCase();
}

function SortableHeader({ label, sorted, onClick }: { label: string; sorted: false | "asc" | "desc"; onClick: () => void }) {
  const Icon = sorted === "asc" ? ArrowUp : sorted === "desc" ? ArrowDown : ArrowUpDown;
  return (
    <Button
      type="button"
      variant="ghost"
      size="sm"
      className="-ml-3 gap-1.5 text-xs font-medium"
      onClick={onClick}
    >
      {label}
      <Icon className="size-3.5 text-muted-foreground" />
    </Button>
  );
}

function toCsvValue(value: string): string {
  return /[",\n]/.test(value) ? `"${value.replace(/"/g, '""')}"` : value;
}

export function MembersTable({
  members,
  initialStatusFilter,
}: {
  members: MemberSummary[];
  initialStatusFilter?: MemberStatus;
}) {
  const { fetchWithAuth } = useAuth();
  const [localMembers, setLocalMembers] = useState(members);
  const [globalFilter, setGlobalFilter] = useState("");
  const [statusFilter, setStatusFilter] = useState<MemberStatus | "All">(initialStatusFilter ?? "All");
  const [sorting, setSorting] = useState<SortingState>([]);
  const [rowSelection, setRowSelection] = useState<RowSelectionState>({});
  const [isVerifying, setIsVerifying] = useState(false);

  const statusCounts = useMemo(() => {
    const counts: Record<MemberStatus, number> = { Active: 0, PendingVerification: 0, Inactive: 0 };
    for (const member of localMembers) {
      counts[member.status] += 1;
    }
    return counts;
  }, [localMembers]);

  // Must be memoized: a fresh array literal on every render (even an
  // equivalent empty `[]`) makes TanStack Table's controlled `state.
  // columnFilters` look like it changed every render, triggering a
  // re-render that creates another new array — an infinite loop that
  // hangs the tab.
  const columnFilters = useMemo<ColumnFiltersState>(
    () => (statusFilter === "All" ? [] : [{ id: "status", value: statusFilter }]),
    [statusFilter],
  );

  const columns = useMemo<ColumnDef<MemberSummary>[]>(
    () => [
      {
        id: "select",
        header: ({ table }) => (
          <Checkbox
            aria-label="Select all rows on this page"
            checked={table.getIsAllPageRowsSelected()}
            indeterminate={table.getIsSomePageRowsSelected() && !table.getIsAllPageRowsSelected()}
            onCheckedChange={(checked) => table.toggleAllPageRowsSelected(checked)}
          />
        ),
        cell: ({ row }) => (
          <Checkbox
            aria-label={`Select ${row.original.firstName} ${row.original.lastName}`}
            checked={row.getIsSelected()}
            onCheckedChange={(checked) => row.toggleSelected(checked)}
          />
        ),
        enableSorting: false,
      },
      {
        id: "member",
        accessorFn: (row) => `${row.lastName}, ${row.firstName}`,
        header: ({ column }) => (
          <SortableHeader label="Member" sorted={column.getIsSorted()} onClick={() => column.toggleSorting()} />
        ),
        cell: ({ row }) => (
          <div className="flex items-center gap-2.5">
            <div className="flex size-[30px] shrink-0 items-center justify-center rounded-full bg-primary-subtle text-[11px] font-medium text-primary">
              {initialsFor(row.original)}
            </div>
            <div className="flex min-w-0 flex-col">
              <Link
                href={`/dashboard/members/${row.original.id}`}
                className="truncate text-[13px] font-medium text-foreground hover:text-primary hover:underline"
              >
                {row.original.lastName}, {row.original.firstName}
                {row.original.middleName ? ` ${row.original.middleName.charAt(0)}.` : ""}
              </Link>
            </div>
          </div>
        ),
      },
      {
        accessorKey: "employeeNumber",
        header: "Employee number",
        cell: ({ row }) => <span className="tabular-nums">{row.original.employeeNumber ?? "—"}</span>,
      },
      {
        accessorKey: "status",
        header: ({ column }) => (
          <SortableHeader label="Status" sorted={column.getIsSorted()} onClick={() => column.toggleSorting()} />
        ),
        cell: ({ row }) => (
          <Badge variant="outline" className={cn("rounded-full text-[11.5px]", memberStatusBadgeClassName[row.original.status])}>
            {row.original.status}
          </Badge>
        ),
        filterFn: (row, columnId, filterValue: string) => {
          if (!filterValue || filterValue === "All") {
            return true;
          }
          return row.getValue(columnId) === filterValue;
        },
      },
    ],
    [],
  );

  const table = useReactTable({
    data: localMembers,
    columns,
    getRowId: (row) => row.id,
    state: {
      sorting,
      globalFilter,
      columnFilters,
      rowSelection,
    },
    onSortingChange: setSorting,
    onGlobalFilterChange: setGlobalFilter,
    onRowSelectionChange: setRowSelection,
    enableRowSelection: true,
    globalFilterFn: (row, _columnId, filterValue: string) => {
      const search = filterValue.toLowerCase();
      const member = row.original;
      return (
        member.lastName.toLowerCase().includes(search) ||
        member.firstName.toLowerCase().includes(search) ||
        (member.employeeNumber?.toLowerCase().includes(search) ?? false)
      );
    },
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    initialState: { pagination: { pageSize: 10 } },
  });

  const filteredRows = table.getFilteredRowModel().rows;
  const selectedRows = table.getSelectedRowModel().rows;
  const selectedPending = selectedRows.filter((row) => row.original.status === "PendingVerification");

  async function handleVerifySelected() {
    setIsVerifying(true);
    let succeeded = 0;
    let failed = 0;

    try {
      for (const row of selectedPending) {
        const response = await fetchWithAuth(`/api/members/${row.original.id}/verify`, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ remarks: null }),
        });

        if (response.ok) {
          succeeded += 1;
          setLocalMembers((current) =>
            current.map((member) => (member.id === row.original.id ? { ...member, status: "Active" } : member)),
          );
        } else {
          failed += 1;
        }
      }
    } finally {
      setIsVerifying(false);
      setRowSelection({});
      if (succeeded > 0) {
        toast.success(`Verified ${succeeded} member${succeeded === 1 ? "" : "s"}.`);
      }
      if (failed > 0) {
        toast.error(`Failed to verify ${failed} member${failed === 1 ? "" : "s"}.`);
      }
    }
  }

  function handleExportCsv() {
    const header = ["Last name", "First name", "Middle name", "Employee number", "Status"];
    const rows = filteredRows.map((row) => {
      const member = row.original;
      return [member.lastName, member.firstName, member.middleName ?? "", member.employeeNumber ?? "", member.status].map(
        toCsvValue,
      );
    });
    const csv = [header, ...rows].map((line) => line.join(",")).join("\n");
    const blob = new Blob([csv], { type: "text/csv;charset=utf-8;" });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;
    link.download = "bimss-members.csv";
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
  }

  return (
    <div className="flex flex-col gap-3.5">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex flex-wrap gap-2">
          {FILTER_PILLS.map((pill) => {
            const count = pill.value === "All" ? localMembers.length : statusCounts[pill.value];
            const isActive = statusFilter === pill.value;
            return (
              <button
                key={pill.value}
                type="button"
                onClick={() => setStatusFilter(pill.value)}
                className={cn(
                  "rounded-full px-3.5 py-1.5 text-xs font-medium transition-colors",
                  isActive
                    ? "bg-primary text-primary-foreground"
                    : "border border-border bg-background text-foreground hover:border-primary/50",
                )}
              >
                {pill.label} · {count}
              </button>
            );
          })}
        </div>
        <div className="flex items-center gap-2">
          <Input
            placeholder="Search by name or employee number…"
            value={globalFilter}
            onChange={(event) => setGlobalFilter(event.target.value)}
            className="w-56"
          />
          <Button type="button" variant="outline" size="sm" onClick={handleExportCsv}>
            Export CSV
          </Button>
        </div>
      </div>

      <div className="flex items-center gap-3 rounded-lg bg-app-bg px-3 py-2">
        <Checkbox
          aria-label="Select all rows on this page"
          checked={table.getIsAllPageRowsSelected()}
          indeterminate={table.getIsSomePageRowsSelected() && !table.getIsAllPageRowsSelected()}
          onCheckedChange={(checked) => table.toggleAllPageRowsSelected(checked)}
        />
        {selectedRows.length > 0 ? (
          <>
            <span className="text-xs font-medium">{selectedRows.length} selected</span>
            <Button
              type="button"
              size="sm"
              variant="outline"
              disabled={selectedPending.length === 0 || isVerifying}
              onClick={handleVerifySelected}
            >
              {isVerifying ? "Verifying…" : `Verify selected${selectedPending.length > 0 ? ` (${selectedPending.length})` : ""}`}
            </Button>
          </>
        ) : null}
        <span className="ml-auto text-xs text-muted-foreground">
          Showing {table.getRowModel().rows.length} of {filteredRows.length}
          {filteredRows.length !== localMembers.length ? ` (filtered from ${localMembers.length})` : ""}
        </span>
      </div>

      <Table>
        <TableHeader>
          {table.getHeaderGroups().map((headerGroup) => (
            <TableRow key={headerGroup.id} className="hover:bg-transparent">
              {headerGroup.headers.map((header) => (
                <TableHead key={header.id} className="text-xs font-medium text-muted-foreground">
                  {header.isPlaceholder ? null : flexRender(header.column.columnDef.header, header.getContext())}
                </TableHead>
              ))}
            </TableRow>
          ))}
        </TableHeader>
        <TableBody>
          {table.getRowModel().rows.length ? (
            table.getRowModel().rows.map((row) => (
              <TableRow key={row.id} data-state={row.getIsSelected() ? "selected" : undefined} className="text-[13px]">
                {row.getVisibleCells().map((cell) => (
                  <TableCell key={cell.id}>{flexRender(cell.column.columnDef.cell, cell.getContext())}</TableCell>
                ))}
              </TableRow>
            ))
          ) : (
            <TableRow>
              <TableCell colSpan={columns.length} className="text-center text-muted-foreground">
                {localMembers.length === 0 ? "No members yet." : "No members match your search."}
              </TableCell>
            </TableRow>
          )}
        </TableBody>
      </Table>

      <div className="flex flex-col items-center justify-between gap-3 sm:flex-row">
        <p className="text-xs text-muted-foreground">
          Page {table.getState().pagination.pageIndex + 1} of {Math.max(table.getPageCount(), 1)}
        </p>
        <div className="flex items-center gap-2">
          <Button
            type="button"
            variant="outline"
            size="sm"
            onClick={() => table.previousPage()}
            disabled={!table.getCanPreviousPage()}
          >
            Previous
          </Button>
          <Button type="button" variant="outline" size="sm" onClick={() => table.nextPage()} disabled={!table.getCanNextPage()}>
            Next
          </Button>
        </div>
      </div>
    </div>
  );
}
