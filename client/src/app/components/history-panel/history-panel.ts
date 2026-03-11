import { Component, input, output, signal, computed } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ConversionDirection } from '../direction-toggle/direction-toggle';

export interface HistoryEntry {
  id: string;
  direction: ConversionDirection;
  input: string;
  output: string;
  timestamp: Date;
}

@Component({
  selector: 'app-history-panel',
  standalone: true,
  imports: [DatePipe],
  templateUrl: './history-panel.html',
  styleUrl: './history-panel.scss'
})
export class HistoryPanel {
  entries = input<HistoryEntry[]>([]);
  entrySelected = output<HistoryEntry>();

  isOpen = signal(false);

  hasEntries = computed(() => this.entries().length > 0);

  togglePanel() {
    if (this.hasEntries()) this.isOpen.set(!this.isOpen());
  }

  selectEntry(entry: HistoryEntry) {
    this.entrySelected.emit(entry);
  }

  getPreview(input: string): string {
    const oneLine = input.replace(/\s+/g, ' ').trim();
    return oneLine.length > 60 ? oneLine.slice(0, 60) + '...' : oneLine;
  }

  getDirectionLabel(direction: ConversionDirection): string {
    return direction === 'json-to-csharp' ? 'JSON → C#' : 'C# → JSON';
  }
}