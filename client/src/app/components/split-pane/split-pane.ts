import {
  Component,
  inject,
  signal,
  ViewChild,
  input,
  effect
} from '@angular/core';
import { InputEditor } from '../input-editor/input-editor';
import { OutputPanel } from '../output-panel/output-panel';
import { OptionsPanel } from '../options-panel/options-panel';
import { ConversionService, DEFAULT_OPTIONS, GenerationOptions } from '../../services/conversion';
import { ConversionDirection } from '../direction-toggle/direction-toggle';

@Component({
  selector: 'app-split-pane',
  standalone: true,
  imports: [InputEditor, OutputPanel, OptionsPanel],
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
}