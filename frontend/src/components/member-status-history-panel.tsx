import type { MemberStatusHistoryEntry, ReferenceDataItem } from "@/lib/types/member";
import { Badge } from "@/components/ui/badge";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";

export function MemberStatusHistoryPanel({
  history,
  statusReasons,
}: {
  history: MemberStatusHistoryEntry[];
  statusReasons: ReferenceDataItem[];
}) {
  return (
    <div className="flex flex-col gap-3">
      <p className="text-sm font-medium">Status history</p>

      {history.length > 0 ? (
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Transition</TableHead>
              <TableHead>Reason</TableHead>
              <TableHead>Remarks</TableHead>
              <TableHead>Date</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {history.map((entry) => (
              <TableRow key={entry.id}>
                <TableCell>
                  <div className="flex items-center gap-1.5">
                    {entry.fromStatus ? (
                      <>
                        <Badge variant="outline">{entry.fromStatus}</Badge>
                        <span className="text-muted-foreground">→</span>
                      </>
                    ) : null}
                    <Badge>{entry.toStatus}</Badge>
                  </div>
                </TableCell>
                <TableCell>
                  {entry.reasonId ? (statusReasons.find((item) => item.id === entry.reasonId)?.name ?? "—") : "—"}
                </TableCell>
                <TableCell>{entry.remarks ?? "—"}</TableCell>
                <TableCell>{new Date(entry.occurredAtUtc).toLocaleString()}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      ) : (
        <p className="text-sm text-muted-foreground">No status history yet.</p>
      )}
    </div>
  );
}
