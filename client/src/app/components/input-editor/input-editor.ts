import {
  Component,
  ElementRef,
  ViewChild,
  AfterViewInit,
  OnDestroy,
  OnChanges,
  SimpleChanges,
  input,
  output,
  inject,
  effect
} from '@angular/core';
import { EditorView, basicSetup } from 'codemirror';
import { EditorState } from '@codemirror/state';
import { json } from '@codemirror/lang-json';
import { javascript } from '@codemirror/lang-javascript';
import { oneDark } from '@codemirror/theme-one-dark';
import { Subject, debounceTime, takeUntil } from 'rxjs';
import { ThemeService } from '../../services/theme';
import { ConversionDirection } from '../direction-toggle/direction-toggle';

@Component({
  selector: 'app-input-editor',
  standalone: true,
  templateUrl: './input-editor.html',
  styleUrl: './input-editor.scss'
})
export class InputEditor implements AfterViewInit, OnDestroy, OnChanges {
  @ViewChild('editorHost') editorHost!: ElementRef<HTMLDivElement>;

  direction = input<ConversionDirection>('json-to-csharp');
  autoConvert = input<boolean>(false);
  valueChange = output<string>();
  convert = output<string>();

  private editorView?: EditorView;
  private themeService = inject(ThemeService);
  private destroy$ = new Subject<void>();
  private inputSubject = new Subject<string>();

  isEmpty = () => !this.editorView ||
    this.editorView.state.doc.toString().trim().length === 0;

  constructor() {
    effect(() => {
      const _ = this.themeService.isDark();
      if (this.editorView) {
        this.reinitEditor();
      }
    });
  }

  ngAfterViewInit() {
    this.initEditor();

    this.inputSubject.pipe(
      debounceTime(800),
      takeUntil(this.destroy$)
    ).subscribe(value => {
      // Only auto-emit if autoConvert is enabled
      if (value.trim() && this.autoConvert()) {
        this.convert.emit(value);
      }
    });
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['direction'] && !changes['direction'].firstChange) {
      this.reinitEditor();
    }
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
    this.editorView?.destroy();
  }

  // In InputEditor — update initEditor():
  private initEditor() {
    const language = this.direction() === 'json-to-csharp'
      ? json()
      : javascript({ typescript: true });

    const themeExtension = this.themeService.isDark() ? [oneDark] : [];

    this.editorView = new EditorView({
      state: EditorState.create({
        doc: '',
        extensions: [
          basicSetup,
          language,
          ...themeExtension,
          EditorView.updateListener.of(update => {
            if (update.docChanged) {
              const value = update.state.doc.toString();
              this.valueChange.emit(value);
              this.inputSubject.next(value);
            }
          })
        ]
      }),
      parent: this.editorHost.nativeElement
    });
  }

  private reinitEditor() {
    const current = this.editorView?.state.doc.toString() ?? '';
    this.editorView?.destroy();
    this.initEditor();
    if (current) this.setValue(current);
  }

  setValue(value: string) {
    if (!this.editorView) return;
    this.editorView.dispatch({
      changes: { from: 0, to: this.editorView.state.doc.length, insert: value }
    });
  }

  clearEditor() {
    this.setValue('');
    this.valueChange.emit('');
  }

  triggerConversion() {
    const value = this.editorView?.state.doc.toString() ?? '';
    if (value.trim()) this.convert.emit(value);
  }
}