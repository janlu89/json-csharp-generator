import {
  Component,
  inject,
  signal,
  ViewChild,
  input,
  output,
  effect
} from '@angular/core';
import { InputEditor } from '../input-editor/input-editor';
import { OutputPanel } from '../output-panel/output-panel';
import { OptionsPanel } from '../options-panel/options-panel';
import { HistoryPanel, HistoryEntry } from '../history-panel/history-panel';
import { ConversionService, DEFAULT_OPTIONS, GenerationOptions } from '../../services/conversion';
import { ConversionDirection } from '../direction-toggle/direction-toggle';

@Component({
  selector: 'app-split-pane',
  standalone: true,
  imports: [InputEditor, OutputPanel, OptionsPanel, HistoryPanel],
  templateUrl: './split-pane.html',
  styleUrl: './split-pane.scss'
})
export class SplitPane {
  @ViewChild(InputEditor) inputEditor!: InputEditor;

  direction = input<ConversionDirection>('json-to-csharp');

  isLoading = signal(false);
  outputValue = signal('');
  errorMessage = signal('');
  currentInput = signal('');
  options = signal<GenerationOptions>({ ...DEFAULT_OPTIONS });
  autoConvert = signal(false);
  history = signal<HistoryEntry[]>([]);
  directionChange = output<ConversionDirection>();

  private suppressNextConvert = false;
  private conversionService = inject(ConversionService);

  constructor() {
    effect(() => {
      const _ = this.direction();
      this.suppressNextConvert = true;
      this.outputValue.set('');
      this.errorMessage.set('');
      this.currentInput.set('');
      setTimeout(() => {
        this.inputEditor?.clearEditor();
        setTimeout(() => this.suppressNextConvert = false, 900);
      });
    });
  }

  onInputChange(value: string) {
  this.currentInput.set(value);
  if (this.errorMessage()) this.errorMessage.set('');
  if (!value.trim()) {
    this.outputValue.set('');
    this.errorMessage.set('');
  }
}

  onConvert(value: string) {
    if (this.suppressNextConvert) return;
    if (!value.trim()) return;
    this.runConversion(value);
  }

  onOptionsChange(options: GenerationOptions) {
    this.options.set(options);
  }

  onAutoConvertChange(autoConvert: boolean) {
    this.autoConvert.set(autoConvert);
  }

  onHistorySelected(entry: HistoryEntry) {
  if (entry.direction !== this.direction()) {
    this.directionChange.emit(entry.direction);
    setTimeout(() => {
      this.outputValue.set(entry.output);
      this.inputEditor?.setValue(entry.input);
    }, 50);
  } else {
    this.outputValue.set(entry.output);
    this.inputEditor?.setValue(entry.input);
  }
}

  private runConversion(input: string) {
    this.isLoading.set(true);
    this.errorMessage.set('');
    this.outputValue.set('');

    const request$ = this.direction() === 'json-to-csharp'
      ? this.conversionService.jsonToCsharp(input, this.options())
      : this.conversionService.csharpToJson(input);

    request$.subscribe({
      next: result => {
        this.isLoading.set(false);
        if (result.success) {
          this.outputValue.set(result.output ?? '');
          this.addToHistory(input, result.output ?? '');
        } else {
          this.errorMessage.set(result.errorMessage ?? 'Conversion failed.');
        }
      },
      error: err => {
        this.isLoading.set(false);
        this.errorMessage.set(
          err.error?.errorMessage ??
          err.error?.error ??
          'Could not reach the API. Is the server running?'
        );
      }
    });
  }

  private addToHistory(input: string, output: string) {
    const entry: HistoryEntry = {
      id: crypto.randomUUID(),
      direction: this.direction(),
      input,
      output,
      timestamp: new Date()
    };

    // Cap history at 20 entries — oldest entries fall off the bottom
    this.history.update(entries => [entry, ...entries].slice(0, 20));
  }
}