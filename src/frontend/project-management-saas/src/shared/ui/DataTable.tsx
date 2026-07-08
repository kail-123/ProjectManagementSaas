import {
  Box,
  Button,
  Checkbox,
  FormControlLabel,
  IconButton,
  InputAdornment,
  Menu,
  MenuItem,
  Paper,
  Skeleton,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TablePagination,
  TableRow,
  TableSortLabel,
  TextField,
  Toolbar,
  Tooltip,
  Typography,
  alpha,
  useTheme
} from '@mui/material';
import { Columns3, Download, Filter, MoreHorizontal, Pencil, Plus, Search, Trash2 } from 'lucide-react';
import { useMemo, useState } from 'react';
import type { DragEvent, MouseEvent, ReactNode } from 'react';
import { ConfirmationDialog } from './ConfirmationDialog';
import { EmptyState, ErrorState } from './StateBlocks';

export interface DataTableColumn<TRow> {
  field: string;
  headerName: string;
  minWidth?: number;
  hiddenByDefault?: boolean;
  sortable?: boolean;
  render?: (row: TRow) => ReactNode;
  exportValue?: (row: TRow) => string | number | boolean | null | undefined;
}

export interface DataTableQuery {
  pageNumber: number;
  pageSize: number;
  search?: string;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

interface DataTableProps<TRow extends { id: string }> {
  title: string;
  rows: TRow[];
  totalCount: number;
  columns: DataTableColumn<TRow>[];
  query: DataTableQuery;
  loading: boolean;
  exportFileName: string;
  getRowLabel: (row: TRow) => string;
  onQueryChange: (query: DataTableQuery) => void;
  onCreate?: () => void;
  onEdit?: (row: TRow) => void;
  onDelete?: (row: TRow) => void;
  onBulkDelete?: (ids: string[]) => void;
  renderRowActions?: (row: TRow) => ReactNode;
  canExport?: boolean;
  error?: boolean;
  onRetry?: () => void;
}

export function DataTable<TRow extends { id: string }>({
  title,
  rows,
  totalCount,
  columns,
  query,
  loading,
  exportFileName,
  getRowLabel,
  onQueryChange,
  onCreate,
  onEdit,
  onDelete,
  onBulkDelete,
  renderRowActions,
  canExport = true,
  error = false,
  onRetry
}: DataTableProps<TRow>) {
  const theme = useTheme();
  const [selectedIds, setSelectedIds] = useState<string[]>([]);
  const [columnAnchor, setColumnAnchor] = useState<HTMLElement | null>(null);
  const [exportAnchor, setExportAnchor] = useState<HTMLElement | null>(null);
  const [filterAnchor, setFilterAnchor] = useState<HTMLElement | null>(null);
  const [rowActionAnchor, setRowActionAnchor] = useState<HTMLElement | null>(null);
  const [rowContextMenu, setRowContextMenu] = useState<{ mouseX: number; mouseY: number; row: TRow } | null>(null);
  const [activeRow, setActiveRow] = useState<TRow | null>(null);
  const [visibleColumns, setVisibleColumns] = useState<Record<string, boolean>>(() =>
    Object.fromEntries(columns.map((column) => [column.field, !column.hiddenByDefault]))
  );
  const [columnOrder, setColumnOrder] = useState<string[]>(() => columns.map((column) => column.field));
  const [draggedField, setDraggedField] = useState<string | null>(null);
  const [pendingBulkDelete, setPendingBulkDelete] = useState(false);
  const [pendingDeleteRow, setPendingDeleteRow] = useState<TRow | null>(null);

  const orderedColumns = useMemo(
    () => columnOrder
      .map((field) => columns.find((column) => column.field === field))
      .filter((column): column is DataTableColumn<TRow> => Boolean(column)),
    [columnOrder, columns]
  );
  const activeColumns = useMemo(
    () => orderedColumns.filter((column) => visibleColumns[column.field] !== false),
    [orderedColumns, visibleColumns]
  );
  const allSelected = rows.length > 0 && rows.every((row) => selectedIds.includes(row.id));

  const handleSort = (field: string) => {
    const nextDirection = query.sortBy === field && query.sortDirection === 'asc' ? 'desc' : 'asc';
    onQueryChange({ ...query, sortBy: field, sortDirection: nextDirection, pageNumber: 1 });
  };

  const toggleSelected = (id: string) => {
    setSelectedIds((current) => (current.includes(id) ? current.filter((value) => value !== id) : [...current, id]));
  };

  const handleColumnDrop = (event: DragEvent<HTMLTableCellElement>, targetField: string) => {
    event.preventDefault();
    if (!draggedField || draggedField === targetField) {
      return;
    }

    setColumnOrder((current) => {
      const next = current.filter((field) => field !== draggedField);
      const targetIndex = next.indexOf(targetField);
      next.splice(targetIndex, 0, draggedField);
      return next;
    });
    setDraggedField(null);
  };

  const exportRows = () => {
    return rows.map((row) =>
      activeColumns.map((column) => {
        const value = column.exportValue ? column.exportValue(row) : getPrimitiveExportValue(column.render?.(row));
        return value ?? '';
      })
    );
  };

  const exportExcel = () => {
    const header = activeColumns.map((column) => `<th>${escapeHtml(column.headerName)}</th>`).join('');
    const body = exportRows()
      .map((row) => `<tr>${row.map((value) => `<td>${escapeHtml(String(value))}</td>`).join('')}</tr>`)
      .join('');
    const workbook = `<html><head><meta charset="UTF-8"></head><body><table><thead><tr>${header}</tr></thead><tbody>${body}</tbody></table></body></html>`;
    downloadBlob(`${exportFileName}.xls`, 'application/vnd.ms-excel;charset=utf-8', workbook);
  };

  const exportCsv = () => {
    const header = activeColumns.map((column) => csvEscape(column.headerName)).join(',');
    const body = exportRows().map((row) => row.map((value) => csvEscape(String(value))).join(',')).join('\n');
    downloadBlob(`${exportFileName}.csv`, 'text/csv;charset=utf-8', `${header}\n${body}`);
  };

  const exportPdf = async () => {
    const [{ default: jsPDF }, { default: autoTable }] = await Promise.all([
      import('jspdf'),
      import('jspdf-autotable')
    ]);
    const doc = new jsPDF({ orientation: 'landscape' });
    doc.text(title, 14, 14);
    autoTable(doc, {
      head: [activeColumns.map((column) => column.headerName)],
      body: exportRows().map((row) => row.map((value) => String(value))),
      startY: 20
    });
    doc.save(`${exportFileName}.pdf`);
  };

  return (
    <Paper
      sx={{
        width: '100%',
        overflow: 'hidden',
        border: '1px solid',
        borderColor: 'divider',
        borderRadius: 3,
        bgcolor: 'background.paper'
      }}
    >
      <Toolbar sx={{ gap: 1.25, flexWrap: 'wrap', minHeight: 72, p: 2 }}>
        <Box sx={{ flexGrow: 1, minWidth: 180 }}>
          <Typography variant="h6" sx={{ fontWeight: 850 }}>{title}</Typography>
          <Typography variant="caption" color="text.secondary">{totalCount.toLocaleString()} records</Typography>
        </Box>
        <TextField
          size="small"
          placeholder="Quick search"
          value={query.search ?? ''}
          onChange={(event) => onQueryChange({ ...query, search: event.target.value, pageNumber: 1 })}
          slotProps={{
            input: {
              startAdornment: (
                <InputAdornment position="start">
                  <Search size={16} />
                </InputAdornment>
              )
            }
          }}
          sx={{ minWidth: { xs: '100%', sm: 280 }, maxWidth: { sm: 340 } }}
        />
        <Tooltip title="Advanced filters">
          <IconButton onClick={(event) => setFilterAnchor(event.currentTarget)}>
            <Filter size={18} />
          </IconButton>
        </Tooltip>
        <Tooltip title="Columns">
          <IconButton onClick={(event) => setColumnAnchor(event.currentTarget)}>
            <Columns3 size={18} />
          </IconButton>
        </Tooltip>
        {canExport ? (
          <Tooltip title="Export">
            <IconButton onClick={(event) => setExportAnchor(event.currentTarget)}>
              <Download size={18} />
            </IconButton>
          </Tooltip>
        ) : null}
        {onBulkDelete ? (
          <Button
            color="error"
            variant="outlined"
            startIcon={<Trash2 size={16} />}
            disabled={selectedIds.length === 0}
            onClick={() => setPendingBulkDelete(true)}
          >
            Bulk Delete
          </Button>
        ) : null}
        {onCreate ? (
          <Button variant="contained" startIcon={<Plus size={16} />} onClick={onCreate}>
            New
          </Button>
        ) : null}
      </Toolbar>

      {error ? (
        <Box sx={{ p: 2 }}>
          <ErrorState title="Unable to load records" description="The server did not return data for this view." onRetry={onRetry} />
        </Box>
      ) : (
        <>
          <TableContainer sx={{ maxHeight: 'calc(100vh - 330px)', minHeight: 260 }}>
            <Table stickyHeader size="small" sx={{ tableLayout: 'auto' }}>
              <TableHead>
                <TableRow>
                  {onBulkDelete ? (
                    <TableCell padding="checkbox">
                      <Checkbox
                        checked={allSelected}
                        indeterminate={selectedIds.length > 0 && !allSelected}
                        onChange={() => setSelectedIds(allSelected ? [] : rows.map((row) => row.id))}
                      />
                    </TableCell>
                  ) : null}
                  {activeColumns.map((column) => (
                    <TableCell
                      key={column.field}
                      draggable
                      onDragStart={() => setDraggedField(column.field)}
                      onDragOver={(event) => event.preventDefault()}
                      onDrop={(event) => handleColumnDrop(event, column.field)}
                      sx={{
                        minWidth: column.minWidth,
                        resize: 'horizontal',
                        overflow: 'auto',
                        bgcolor: 'background.paper',
                        borderBottom: '1px solid',
                        borderColor: 'divider'
                      }}
                    >
                      {column.sortable === false ? (
                        column.headerName
                      ) : (
                        <TableSortLabel
                          active={query.sortBy === column.field}
                          direction={query.sortBy === column.field ? query.sortDirection ?? 'asc' : 'asc'}
                          onClick={() => handleSort(column.field)}
                        >
                          {column.headerName}
                        </TableSortLabel>
                      )}
                    </TableCell>
                  ))}
                  {(onEdit || onDelete || renderRowActions) && <TableCell align="right">Actions</TableCell>}
                </TableRow>
              </TableHead>
              <TableBody>
                {loading
                  ? Array.from({ length: Math.min(query.pageSize, 8) }).map((_, index) => (
                      <TableRow key={index}>
                        {onBulkDelete ? <TableCell><Skeleton /></TableCell> : null}
                        {activeColumns.map((column) => (
                          <TableCell key={column.field}><Skeleton /></TableCell>
                        ))}
                        {(onEdit || onDelete || renderRowActions) && <TableCell><Skeleton /></TableCell>}
                      </TableRow>
                    ))
                  : rows.map((row) => (
                      <TableRow
                        key={row.id}
                        hover
                        tabIndex={0}
                        selected={selectedIds.includes(row.id)}
                        onContextMenu={(event) => {
                          if (!onEdit && !onDelete) {
                            return;
                          }

                          event.preventDefault();
                          setRowContextMenu({ mouseX: event.clientX + 2, mouseY: event.clientY - 6, row });
                        }}
                        sx={{
                          '&:focus-visible': {
                            bgcolor: alpha(theme.palette.primary.main, 0.08)
                          }
                        }}
                      >
                        {onBulkDelete ? (
                          <TableCell padding="checkbox">
                            <Checkbox checked={selectedIds.includes(row.id)} onChange={() => toggleSelected(row.id)} />
                          </TableCell>
                        ) : null}
                        {activeColumns.map((column) => (
                          <TableCell key={column.field}>{column.render ? column.render(row) : ''}</TableCell>
                        ))}
                        {(onEdit || onDelete || renderRowActions) ? (
                          <TableCell align="right">
                            <Stack direction="row" spacing={0.5} sx={{ justifyContent: 'flex-end' }}>
                              {renderRowActions?.(row)}
                              {onEdit || onDelete ? (
                                <Tooltip title="More actions">
                                  <IconButton
                                    size="small"
                                    onClick={(event: MouseEvent<HTMLElement>) => {
                                      setActiveRow(row);
                                      setRowActionAnchor(event.currentTarget);
                                    }}
                                  >
                                    <MoreHorizontal size={18} />
                                  </IconButton>
                                </Tooltip>
                              ) : null}
                            </Stack>
                          </TableCell>
                        ) : null}
                      </TableRow>
                    ))}
              </TableBody>
            </Table>
          </TableContainer>

          {!loading && rows.length === 0 ? (
            <Box sx={{ p: 2 }}>
              <EmptyState title="No records found" description="Try a different search or create the first record for this view." />
            </Box>
          ) : null}

          <TablePagination
            component="div"
            count={totalCount}
            page={query.pageNumber - 1}
            rowsPerPage={query.pageSize}
            rowsPerPageOptions={[10, 25, 50, 100]}
            onPageChange={(_, page) => onQueryChange({ ...query, pageNumber: page + 1 })}
            onRowsPerPageChange={(event) => onQueryChange({ ...query, pageSize: Number(event.target.value), pageNumber: 1 })}
          />
        </>
      )}

      <Menu
        anchorEl={filterAnchor}
        open={Boolean(filterAnchor)}
        onClose={() => setFilterAnchor(null)}
        slotProps={{ paper: { sx: { width: 320, p: 1 } } }}
      >
        <Box sx={{ px: 1, py: 1 }}>
          <Typography variant="subtitle2" sx={{ mb: 1 }}>Filters</Typography>
          <TextField
            size="small"
            label="Search"
            value={query.search ?? ''}
            onChange={(event) => onQueryChange({ ...query, search: event.target.value, pageNumber: 1 })}
          />
        </Box>
        <MenuItem
          disabled={!query.search}
          onClick={() => {
            onQueryChange({ ...query, search: '', pageNumber: 1 });
            setFilterAnchor(null);
          }}
        >
          Clear filters
        </MenuItem>
      </Menu>

      <Menu anchorEl={columnAnchor} open={Boolean(columnAnchor)} onClose={() => setColumnAnchor(null)}>
        {orderedColumns.map((column) => (
          <MenuItem key={column.field}>
            <FormControlLabel
              control={
                <Checkbox
                  checked={visibleColumns[column.field] !== false}
                  onChange={(event) => setVisibleColumns((current) => ({ ...current, [column.field]: event.target.checked }))}
                />
              }
              label={column.headerName}
            />
          </MenuItem>
        ))}
      </Menu>

      <Menu anchorEl={exportAnchor} open={Boolean(exportAnchor)} onClose={() => setExportAnchor(null)}>
        <MenuItem onClick={() => { exportExcel(); setExportAnchor(null); }}>Export Excel</MenuItem>
        <MenuItem onClick={() => { exportCsv(); setExportAnchor(null); }}>Export CSV</MenuItem>
        <MenuItem onClick={() => { void exportPdf(); setExportAnchor(null); }}>Export PDF</MenuItem>
      </Menu>

      <Menu
        anchorEl={rowActionAnchor}
        open={Boolean(rowActionAnchor)}
        onClose={() => {
          setRowActionAnchor(null);
          setActiveRow(null);
        }}
      >
        {activeRow && onEdit ? (
          <MenuItem onClick={() => { onEdit(activeRow); setRowActionAnchor(null); }}>
            <ListItemIconCompat><Pencil size={16} /></ListItemIconCompat>
            Edit
          </MenuItem>
        ) : null}
        {activeRow && onDelete ? (
          <MenuItem onClick={() => { setPendingDeleteRow(activeRow); setRowActionAnchor(null); }} sx={{ color: 'error.main' }}>
            <ListItemIconCompat><Trash2 size={16} /></ListItemIconCompat>
            Delete
          </MenuItem>
        ) : null}
      </Menu>

      <Menu
        open={rowContextMenu !== null}
        onClose={() => setRowContextMenu(null)}
        anchorReference="anchorPosition"
        anchorPosition={rowContextMenu ? { top: rowContextMenu.mouseY, left: rowContextMenu.mouseX } : undefined}
      >
        {rowContextMenu && onEdit ? (
          <MenuItem onClick={() => { onEdit(rowContextMenu.row); setRowContextMenu(null); }}>
            Edit {getRowLabel(rowContextMenu.row)}
          </MenuItem>
        ) : null}
        {rowContextMenu && onDelete ? (
          <MenuItem onClick={() => { setPendingDeleteRow(rowContextMenu.row); setRowContextMenu(null); }} sx={{ color: 'error.main' }}>
            Delete
          </MenuItem>
        ) : null}
      </Menu>

      <ConfirmationDialog
        open={pendingBulkDelete}
        title="Delete selected records"
        description={`Delete ${selectedIds.length} selected record(s)?`}
        confirmLabel="Delete"
        confirmColor="error"
        onCancel={() => setPendingBulkDelete(false)}
        onConfirm={() => {
          onBulkDelete?.(selectedIds);
          setSelectedIds([]);
          setPendingBulkDelete(false);
        }}
      />
      <ConfirmationDialog
        open={pendingDeleteRow !== null}
        title="Delete record"
        description={pendingDeleteRow ? `Delete ${getRowLabel(pendingDeleteRow)}?` : ''}
        confirmLabel="Delete"
        confirmColor="error"
        onCancel={() => setPendingDeleteRow(null)}
        onConfirm={() => {
          if (pendingDeleteRow) {
            onDelete?.(pendingDeleteRow);
          }
          setPendingDeleteRow(null);
        }}
      />
    </Paper>
  );
}

function ListItemIconCompat({ children }: { children: ReactNode }) {
  return <Box sx={{ width: 28, display: 'inline-flex', color: 'inherit' }}>{children}</Box>;
}

function escapeHtml(value: string) {
  return value
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#039;');
}

function csvEscape(value: string) {
  return `"${value.replaceAll('"', '""')}"`;
}

function getPrimitiveExportValue(value: ReactNode) {
  return typeof value === 'string' || typeof value === 'number' || typeof value === 'boolean' ? value : '';
}

function downloadBlob(fileName: string, type: string, content: string) {
  const blob = new Blob([content], { type });
  const link = document.createElement('a');
  link.href = URL.createObjectURL(blob);
  link.download = fileName;
  link.click();
  URL.revokeObjectURL(link.href);
}
