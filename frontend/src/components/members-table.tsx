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
  type SortingState,
} from "@tanstack/react-table";
import { ArrowDown, ArrowUp, ArrowUpDown } from "lucide-react";
import type { MemberStatus, MemberSummary } from "@/lib/types/member";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { memberStatusBadgeClassName } from "@/lib/member-status";

const STATUS_OPTIONS: { value: MemberStatus | "All"; label: string }[] = [
  { value: "All", label: "All statuses" },
  { value: "PendingVerification", label: "Pending Verification" },
  { value: "Active", label: "Active" },
  { value: "Inactive", label: "Inactive" },
];

function SortableHeader({ label, sorted, onClick }: { label: string; sorted: false | "asc" | "desc"; onClick: () => void }) {
  const Icon = sorted === "asc" ? ArrowUp : sorted === "desc" ? ArrowDown : ArrowUpDown;
  return (
    <Button
      type="button"
      variant="ghost"
      size="sm"
      className="-ml-3 gap-1.5 text-sm font-medium"
      onClick={onClick}
    >
      {label}
      <Icon className="size-3.5 text-muted-foreground" />
    </Button>
  );
}

const columns: ColumnDef<MemberSummary>[] = [
  {
    accessorKey: "lastName",
    header: ({ column }) => (
      <SortableHeader label="Last name" sorted={column.getIsSorted()} onClick={() => column.toggleSorting()} />
    ),
    cell: ({ row }) => (
      <Link href={`/dashboard/members/${row.original.id}`} className="font-medium text-primary hover:underline">
        {row.original.lastName}
      </Link>
    ),
  },
  {
    accessorKey: "firstName",
    header: ({ column }) => (
      <SortableHeader label="First name" sorted={column.getIsSorted()} onClick={() => column.toggleSorting()} />
    ),
  },
  {
    accessorKey: "middleName",
    header: "Middle name",
    cell: ({ row }) => row.original.middleName ?? "—",
  },
  {
    accessorKey: "employeeNumber",
    header: "Employee number",
    cell: ({ row }) => row.original.employeeNumber ?? "—",
  },
  {
    accessorKey: "status",
    header: ({ column }) => (
      <SortableHeader label="Status" sorted={column.getIsSorted()} onClick={() => column.toggleSorting()} />
    ),
    cell: ({ row }) => (
      <Badge variant="outline" className={memberStatusBadgeClassName[row.original.status]}>
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
];

export function MembersTable({
  members,
  initialStatusFilter,
}: {
  members: MemberSummary[];
  initialStatusFilter?: MemberStatus;
}) {
  const [globalFilter, setGlobalFilter] = useState("");
  const [statusFilter, setStatusFilter] = useState<MemberStatus | "All">(initialStatusFilter ?? "All");
  const [sorting, setSorting] = useState<SortingState>([]);

  // Must be memoized: a fresh array literal on every render (even an
  // equivalent empty `[]`) makes TanStack Table's controlled `state.
  // columnFilters` look like it changed every render, triggering a
  // re-render that creates another new array — an infinite loop that
  // hangs the tab.
  const columnFilters = useMemo<ColumnFiltersState>(
    () => (statusFilter === "All" ? [] : [{ id: "status", value: statusFilter }]),
    [statusFilter],
  );

  const table = useReactTable({
    data: members,
    columns,
    state: {
      sorting,
      globalFilter,
      columnFilters,
    },
    onSortingChange: setSorting,
    onGlobalFilterChange: setGlobalFilter,
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

  const filteredRowCount = table.getFilteredRowModel().rows.length;

  return (
    <div className="flex flex-col gap-4">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
        <Input
          placeholder="Search by name or employee number…"
          value={globalFilter}
          onChange={(event) => setGlobalFilter(event.target.value)}
          className="sm:max-w-xs"
        />
        <Select value={statusFilter} onValueChange={(value) => setStatusFilter((value as MemberStatus | "All") ?? "All")}>
          <SelectTrigger className="sm:w-56">
            <SelectValue>{(value) => STATUS_OPTIONS.find((option) => option.value === value)?.label ?? "All statuses"}</SelectValue>
          </SelectTrigger>
          <SelectContent>
            {STATUS_OPTIONS.map((option) => (
              <SelectItem key={option.value} value={option.value}>
                {option.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      <Table>
        <TableHeader>
          {table.getHeaderGroups().map((headerGroup) => (
            <TableRow key={headerGroup.id}>
              {headerGroup.headers.map((header) => (
                <TableHead key={header.id}>
                  {header.isPlaceholder ? null : flexRender(header.column.columnDef.header, header.getContext())}
                </TableHead>
              ))}
            </TableRow>
          ))}
        </TableHeader>
        <TableBody>
          {table.getRowModel().rows.length ? (
            table.getRowModel().rows.map((row) => (
              <TableRow key={row.id}>
                {row.getVisibleCells().map((cell) => (
                  <TableCell key={cell.id}>{flexRender(cell.column.columnDef.cell, cell.getContext())}</TableCell>
                ))}
              </TableRow>
            ))
          ) : (
            <TableRow>
              <TableCell colSpan={columns.length} className="text-center text-muted-foreground">
                {members.length === 0 ? "No members yet." : "No members match your search."}
              </TableCell>
            </TableRow>
          )}
        </TableBody>
      </Table>

      <div className="flex flex-col items-center justify-between gap-3 sm:flex-row">
        <p className="text-sm text-muted-foreground">
          Showing {table.getRowModel().rows.length} of {filteredRowCount} member{filteredRowCount === 1 ? "" : "s"}
          {filteredRowCount !== members.length ? ` (filtered from ${members.length})` : ""}
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
          <span className="text-sm text-muted-foreground">
            Page {table.getState().pagination.pageIndex + 1} of {Math.max(table.getPageCount(), 1)}
          </span>
          <Button type="button" variant="outline" size="sm" onClick={() => table.nextPage()} disabled={!table.getCanNextPage()}>
            Next
          </Button>
        </div>
      </div>
    </div>
  );
}
