import {
  Component,
  ElementRef,
  ViewChild,
  AfterViewInit,
  OnDestroy,
  OnChanges,
  SimpleChanges,
  input,
  signal,
  inject,
  effect
} from '@angular/core';
import { EditorView, basicSetup } from 'codemirror';
import { EditorState } from '@codemirror/state';
import { json } from '@codemirror/lang-json';
import { javascript } from '@codemirror/lang-javascript';
import { oneDark } from '@codemirror/theme-one-dark';
import { ConversionDirection } from '../direction-toggle/direction-toggle';
import { ThemeService } from '../../services/theme';

@Component({
  selector: 'app-output-panel',
  standalone: true,
  templateUrl: './output-panel.html',
  styleUrl: './output-panel.scss'
})
export class OutputPanel implements AfterViewInit, OnDestroy, OnChanges {
  @ViewChild('outputHost') outputHost!: ElementRef<HTMLDivElement>;

  direction = input<ConversionDirection>('json-to-csharp');
  isLoading = input<boolean>(false);
  errorMessage = input<string>('');
  outputValue = input<string>('');

  copied = signal(false);
  hasOutput = () => !!this.outputValue();
  hasError = () => !!this.errorMessage();

  private editorView?: EditorView;
  private themeService = inject(ThemeService);

  constructor() {
    effect(() => {
      const _ = this.themeService.isDark(); // subscribe to theme signal
      if (this.editorView) {
        this.reinitEditor();
      }
    });
  }

  ngAfterViewInit() {
    this.initEditor('');
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['outputValue'] && this.editorView) {
      this.updateContent(this.outputValue());
    }
    if (changes['direction'] && !changes['direction'].firstChange) {
      this.reinitEditor();
    }
  }

  ngOnDestroy() {
    this.editorView?.destroy();
  }

  private initEditor(content: string) {
    const language = this.direction() === 'json-to-csharp'
      ? javascript({ typescript: true })
      : json();

    const themeExtension = this.themeService.isDark() ? [oneDark] : [];

    this.editorView = new EditorView({
      state: EditorState.create({
        doc: content,
        extensions: [
          basicSetup,
          language,
          ...themeExtension,
          EditorView.editable.of(false)
        ]
      }),
      parent: this.outputHost.nativeElement
    });
  }

  private reinitEditor() {
    const current = this.editorView?.state.doc.toString() ?? '';
    this.editorView?.destroy();
    this.initEditor(current);
  }

  private updateContent(value: string) {
    if (!this.editorView) return;
    this.editorView.dispatch({
      changes: { from: 0, to: this.editorView.state.doc.length, insert: value }
    });
  }

  copyToClipboard() {
    const content = this.outputValue();
    if (!content) return;
    navigator.clipboard.writeText(content).then(() => {
      this.copied.set(true);
      setTimeout(() => this.copied.set(false), 2000);
    });
  }
}