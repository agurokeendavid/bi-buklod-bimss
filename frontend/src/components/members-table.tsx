"use client";

import Link from "next/link";
import { flexRender, getCoreRowModel, useReactTable, type ColumnDef } from "@tanstack/react-table";
import type { MemberSummary } from "@/lib/types/member";
import { Badge } from "@/components/ui/badge";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";

const statusBadgeVariant: Record<MemberSummary["status"], "default" | "secondary" | "outline"> = {
  PendingVerification: "secondary",
  Active: "default",
  Inactive: "outline",
};

const columns: ColumnDef<MemberSummary>[] = [
  {
    accessorKey: "lastName",
    header: "Last name",
    cell: ({ row }) => (
      <Link href={`/dashboard/members/${row.original.id}`} className="font-medium text-primary hover:underline">
        {row.original.lastName}
      </Link>
    ),
  },
  {
    accessorKey: "firstName",
    header: "First name",
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
    header: "Status",
    cell: ({ row }) => <Badge variant={statusBadgeVariant[row.original.status]}>{row.original.status}</Badge>,
  },
];

export function MembersTable({ members }: { members: MemberSummary[] }) {
  const table = useReactTable({
    data: members,
    columns,
    getCoreRowModel: getCoreRowModel(),
  });

  return (
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
              No members yet.
            </TableCell>
          </TableRow>
        )}
      </TableBody>
    </Table>
  );
}
